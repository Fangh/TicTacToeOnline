using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Unity.Editor;
using Newtonsoft.Json;
using UnityEngine;

public class OnlineManager : MonoBehaviour
{
    public static OnlineManager Instance;
    public static event System.Action OnDependenciesChecked;
    public static event System.Action OnDatabaseDownloaded;

    [Header("References")]
    [SerializeField] private GamesList gamesList;
    [SerializeField] private GameObject connectingLabel;

    internal bool isConnected;

    private DatabaseReference onlineDatabase = null;

    private bool firebaseInitialized = false;
    private SDatabase localDatabase;

    #region Unity Methods

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                firebaseInitialized = true;
                OnDependenciesChecked?.Invoke();

                LoginToDatabase();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Create a new game and send it to the online database.
    /// </summary>
    /// <param name="_id">The ID of this game. Should be unique.</param>
    /// <param name="_playerID">The ID of the player who wants to create this game.</param>
    public void CreateGame(string _id, string _playerID)
    {
        if (!isConnected || onlineDatabase == null)
        {
            SimplePopup.Instance.Open("Cannot create", "You are not connected.");
            return;
        }
        if (string.IsNullOrEmpty(_playerID))
        {
            SimplePopup.Instance.Open("Cannot create", "Please choose a user name before create a new game.");
            return;
        }

        if (localDatabase.games.ContainsKey(_id))
        {
            SimplePopup.Instance.Open("Cannot create", "There is already a game with this name.");
            return;
        }
#if !UNITY_EDITOR
        if (!PlayerPrefs.HasKey("FCMToken"))
        {
            SimplePopup.Instance.Open("Cannot create", $"You don't have a token. Please connect to the internet to get one.");
            return;
        }
#endif

#if !UNITY_EDITOR
        SPlayer localPlayer = new SPlayer(_playerID, PlayerPrefs.GetString("FCMToken"));
#else
        SPlayer localPlayer = new SPlayer(_playerID, $"COMPUTERTOKEN_{SystemInfo.deviceUniqueIdentifier}");
#endif

        localDatabase.games.Add(_id, BoardManager.Instance.InitializeNewGame(_id, localPlayer));

        string JSON = JsonConvert.SerializeObject(localDatabase.games[_id]);
        //Debug.Log($"JSON = {JSON}");

        onlineDatabase.Child("games").Child(_id).SetRawJsonValueAsync(JSON).ContinueWithOnMainThread(task =>
        {
            SimplePopup.Instance.Open("Game created", "You are Player 1.");
            Debug.Log($"game {_id} has been created online");
            onlineDatabase.Child("games").Child(_id).ValueChanged += UpdateLocalGame;
        });
    }

    /// <summary>
    /// Join an online game.
    /// </summary>
    /// <param name="gameId">Id of the game to join.</param>
    /// <param name="playerID">The name of the player who is joining teh game.</param>
    public void JoinGame(string gameId, string playerID)
    {
        if (!isConnected || onlineDatabase == null)
        {
            SimplePopup.Instance.Open("Cannot join", "You are not connected.");
            return;
        }
        if (localDatabase.games.Count == 0)
        {
            SimplePopup.Instance.Open("Cannot join", "Online Database is not fully downloaded.");
            return;
        }
        if (string.IsNullOrEmpty(gameId))
        {
            SimplePopup.Instance.Open("Cannot join", "Please choose a game ID to join.");
            return;
        }
        if (string.IsNullOrEmpty(playerID))
        {
            SimplePopup.Instance.Open("Cannot join", "Please choose a user name before joining a game.");
            return;
        }
        if (!localDatabase.games.ContainsKey(gameId))
        {
            SimplePopup.Instance.Open("Cannot join", $"There is no game with id {gameId}");
            return;
        }
#if !UNITY_EDITOR
        if (!PlayerPrefs.HasKey("FCMToken"))
        {
            SimplePopup.Instance.Open("Cannot join", $"You don't have a token. Please connect to the internet to get one.");
            return;
        }
#endif

        SGame tempLocalGame = localDatabase.games[gameId];
        tempLocalGame.id = gameId;

#if !UNITY_EDITOR
        SPlayer localPlayer = new SPlayer(playerID, PlayerPrefs.GetString("FCMToken"));
#else
        SPlayer localPlayer = new SPlayer(playerID, $"COMPUTERTOKEN_{System.DateTime.Now}");
#endif

        if (tempLocalGame.winner != 0)
        {
            SimplePopup.Instance.Open("Game is finished", $"This game is already finished. The winner was Player {tempLocalGame.winner}.");
            return;
        }

        if (string.IsNullOrEmpty(tempLocalGame.player2.id))
        {
            tempLocalGame.player2 = new SPlayer(localPlayer.id, localPlayer.token);
            BoardManager.Instance.currentTeam = 2;
            SimplePopup.Instance.Open("Game joined", "You are Player 2.");
        }
        else
        {
            if (tempLocalGame.player1.id == playerID)
            {
                BoardManager.Instance.currentTeam = 1;
                SimplePopup.Instance.Open("Game joined", $"Welcome back {playerID} (Player 1).");
            }
            else if (tempLocalGame.player2.id == playerID)
            {
                BoardManager.Instance.currentTeam = 2;
                SimplePopup.Instance.Open("Game joined", $"Welcome back {playerID} (Player 2).");
            }
            else
            {
                SimplePopup.Instance.Open("Already 2 players", "Sorry there is no more free slot for you or you are not using the same username from the last time.");
                return;
            }
        }

        BoardManager.Instance.InitializeGameWithData(tempLocalGame.id, tempLocalGame.player1, tempLocalGame.player2, tempLocalGame.board, tempLocalGame.currentTurn);
        onlineDatabase.Child("games").Child(tempLocalGame.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(tempLocalGame)).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("There were an error sending the message");
            }
            else
            {
                Debug.Log("You have successfully told the server that you have joined the game");
                onlineDatabase.Child("games").Child(tempLocalGame.id).ValueChanged += UpdateLocalGame;
            }
        });

    }

    public void UpdateOnlineGame(SGame localGame)
    {
        onlineDatabase.Child("games").Child(localGame.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(localGame)).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Online {localGame.id} game is updated");
        });
    }

    public SGame[] GetAllGames()
    {
        SGame[] array = new SGame[localDatabase.games.Count];
        localDatabase.games.Values.CopyTo(array, 0);
        return array;
    }

    public SGame GetGameDataByID(string _id)
    {
        return localDatabase.games[_id];
    }

    public int GetNumberOfPlayerOfGame(string _id)
    {
        if (!localDatabase.games.ContainsKey(_id))
        {
            Debug.LogError($"There is no game with id {_id}");
            return -1;
        }
        int nb = 0;

        if (!string.IsNullOrEmpty(localDatabase.games[_id].player1.id))
            nb++;

        if (!string.IsNullOrEmpty(localDatabase.games[_id].player2.id))
            nb++;

        return nb;
    }

#endregion

#region Private Methods

    private void LoginToDatabase()
    {
        if (!firebaseInitialized)
        {
            Debug.LogError("Firebase is not initialized. can not connect.");
            return;
        }
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://tictactoe-123456.firebaseio.com/");
        onlineDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        onlineDatabase.GetValueAsync().ContinueWithOnMainThread(value =>
        {
            if (value.IsFaulted || value.IsCanceled)
            {
                Debug.LogError($"There were an error while trying to downloade the database : {value.Exception.Message}");
            }
            else if (value.IsCompleted)
            {
                isConnected = true;
                Debug.Log("OnlineDatabase = " + value.Result.GetRawJsonValue());
                localDatabase = JsonConvert.DeserializeObject<SDatabase>(value.Result.GetRawJsonValue());
                onlineDatabase.Child("games").ChildAdded += AddGameInLocalDatabase;
                onlineDatabase.Child("games").ChildRemoved += RemoveGameInLocalDatabase; ;

                foreach (var game in localDatabase.games)
                    gamesList.AddButton(game.Key);

                connectingLabel.SetActive(false);
                OnDatabaseDownloaded?.Invoke();
            }
        });
    }

    private void UpdateLocalGame(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log($"Updating local game with these data : \n {args.Snapshot.GetRawJsonValue()} ");
        SGame gameData = JsonConvert.DeserializeObject<SGame>(args.Snapshot.GetRawJsonValue());
        localDatabase.games[gameData.id] = gameData;
        BoardManager.Instance.UpdateBoard(gameData);
    }

    /// <summary>
    /// When a game has been deleted in the online database, update the local database
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveGameInLocalDatabase(object sender, ChildChangedEventArgs e)
    {
        if (localDatabase.games.ContainsKey(e.Snapshot.Key))
        {
            Debug.Log($"Game {e.Snapshot.Key} has been deleted from the online database. Updating localDatase...");
            localDatabase.games.Remove(e.Snapshot.Key);
        }
    }

    /// <summary>
    /// When a game has been added in the online datase, update the local database
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddGameInLocalDatabase(object sender, ChildChangedEventArgs e)
    {
        if (!localDatabase.games.ContainsKey(e.Snapshot.Key))
        {
            Debug.Log($"Game {e.Snapshot.Key} has been added from the online database. Updating localDatase...");
            localDatabase.games.Add(e.Snapshot.Key, JsonConvert.DeserializeObject<SGame>(e.Snapshot.GetRawJsonValue()));
        }
    }
#endregion
}
