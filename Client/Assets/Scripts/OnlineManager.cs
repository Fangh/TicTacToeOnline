using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Runtime.Remoting.Lifetime;
using System.Security.Policy;
using UnityEngine;

public class OnlineManager : MonoBehaviour
{
    public static OnlineManager Instance;
    public static event Action OnDependenciesChecked;
    public static event Action OnDatabaseDownloaded;

    [Header("References")]
    [SerializeField] private GamesList gamesList;
    [SerializeField] private GameObject connectingLabel;

    internal bool isConnected;

    private DatabaseReference onlineDatabase = null;

    private bool firebaseInitialized = false;
    private SDatabase localDatabase;
    private string currentGameId = null;
    private bool appIsExiting = false;

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

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            DisconnectFromGame(currentGameId);
        else
            ReconnectToGame(currentGameId);
    }

    private void OnApplicationQuit()
    {
        appIsExiting = true;
        DisconnectFromGame(currentGameId);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Create a new game and send it to the online database.
    /// </summary>
    /// <param name="_gameId">The ID of this game. Should be unique.</param>
    /// <param name="_playerID">The ID of the player who wants to create this game.</param>
    public void CreateGame(string _gameId, string _playerID)
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

        if (localDatabase.games.ContainsKey(_gameId))
        {
            SimplePopup.Instance.Open("Cannot create", "There is already a game with this name.");
            return;
        }
        if (!PlayerPrefs.HasKey("FCMToken"))
        {
            SimplePopup.Instance.Open("Cannot create", $"You don't have a token. Please connect to the internet to get one.");
            return;
        }
        SPlayer localPlayer = new SPlayer(_playerID, PlayerPrefs.GetString("FCMToken"));

        if (!string.IsNullOrEmpty(currentGameId))
            DisconnectFromGame(currentGameId);

        localDatabase.games.Add(_gameId, BoardManager.Instance.InitializeNewGame(_gameId, localPlayer));

        string JSON = JsonConvert.SerializeObject(localDatabase.games[_gameId]);
        //Debug.Log($"JSON = {JSON}");

        onlineDatabase.Child("games").Child(_gameId).SetRawJsonValueAsync(JSON).ContinueWithOnMainThread(task =>
        {
            currentGameId = _gameId;
            SimplePopup.Instance.Open("Game created", "You are Player 1.");
            Debug.Log($"game {_gameId} has been created online");
            onlineDatabase.Child("games").Child(_gameId).ValueChanged += UpdateLocalGame;
        });
    }

    /// <summary>
    /// Join an online game.
    /// </summary>
    /// <param name="_gameId">Id of the game to join.</param>
    /// <param name="_playerID">The name of the player who is joining teh game.</param>
    public void JoinGame(string _gameId, string _playerID)
    {
        //Checking if you can join
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
        if (string.IsNullOrEmpty(_gameId))
        {
            SimplePopup.Instance.Open("Cannot join", "Please choose a game ID to join.");
            return;
        }
        if (string.IsNullOrEmpty(_playerID))
        {
            SimplePopup.Instance.Open("Cannot join", "Please choose a user name before joining a game.");
            return;
        }
        if (!localDatabase.games.ContainsKey(_gameId))
        {
            SimplePopup.Instance.Open("Cannot join", $"There is no game with id {_gameId}");
            return;
        }
        if (!PlayerPrefs.HasKey("FCMToken"))
        {
            SimplePopup.Instance.Open("Cannot join", $"You don't have a token. Please connect to the internet to get one.");
            return;
        }

        SGame tempLocalGame = localDatabase.games[_gameId];
        tempLocalGame.id = _gameId;

        SPlayer localPlayer = new SPlayer(_playerID, PlayerPrefs.GetString("FCMToken"));

        if (tempLocalGame.winner != 0)
        {
            SimplePopup.Instance.Open("Game is finished", $"This game is already finished. The winner was Player {tempLocalGame.winner}.");
            return;
        }

        //Disconnect from last Game
        if (!string.IsNullOrEmpty(currentGameId))
            DisconnectFromGame(currentGameId);

        //Checking which player index you are.
        int playerIndex = WhichPlayerAmI(tempLocalGame);

        if (playerIndex == 0)
        {
            if (string.IsNullOrEmpty(tempLocalGame.player2.token)) //There is a place for you
            {
                tempLocalGame.player2 = new SPlayer(localPlayer.id, localPlayer.token);
                tempLocalGame.player2.isConnected = true;
                BoardManager.Instance.currentTeam = 2;
                SimplePopup.Instance.Open("Game joined", $"Hello {localPlayer.id} You are Player 2.");
            }
            else //Everything is taken
            {
                SimplePopup.Instance.Open("Already 2 players", "Sorry there is no more free slot for you or you are not using the same username from the last time.");
                return;
            }
        }
        else
        {
            if (playerIndex == 1)
            {
                tempLocalGame.player1.isConnected = true;
                tempLocalGame.player1.id = _playerID; //update with the new name
            }
            else
            {
                tempLocalGame.player2.isConnected = true;
                tempLocalGame.player2.id = _playerID;//update with the new name
            }

            BoardManager.Instance.currentTeam = playerIndex;
            SimplePopup.Instance.Open("Game joined", $"Welcome back {_playerID} (Player {playerIndex}).");
        }
        currentGameId = tempLocalGame.id;

        BoardManager.Instance.InitializeGameWithData(tempLocalGame.id, tempLocalGame.player1, tempLocalGame.player2, tempLocalGame.board, tempLocalGame.currentTurn);
        UpdateOnlineGame(tempLocalGame, () =>
        {
            onlineDatabase.Child("games").Child(tempLocalGame.id).ValueChanged += UpdateLocalGame;
        });

    }

    public void UpdateOnlineGame(SGame localGame, Action _callbackOnUpdated = null)
    {
        onlineDatabase.Child("games").Child(localGame.id).SetRawJsonValueAsync(JsonConvert.SerializeObject(localGame)).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("There were an error sending the message");
            }
            else
            {
                Debug.Log($"Online {localGame.id} game is updated");
                _callbackOnUpdated?.Invoke();
            }
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

        if (!string.IsNullOrEmpty(localDatabase.games[_id].player1.token))
            nb++;

        if (!string.IsNullOrEmpty(localDatabase.games[_id].player2.token))
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

                if (!string.IsNullOrEmpty(value.Result.GetRawJsonValue()))
                {
                    try
                    {
                        localDatabase = JsonConvert.DeserializeObject<SDatabase>(value.Result.GetRawJsonValue());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error : {e}");
                    }
                    onlineDatabase.Child("games").ChildAdded += AddGameInLocalDatabase;
                    onlineDatabase.Child("games").ChildRemoved += RemoveGameInLocalDatabase;

                    if (localDatabase.games.Count > 0)
                    {
                        foreach (var game in localDatabase.games)
                            gamesList.AddButton(game.Key);
                    }
                }

                connectingLabel.SetActive(false);
                OnDatabaseDownloaded?.Invoke();
            }
        });
    }

    private void UpdateLocalGame(object sender, ValueChangedEventArgs args)
    {
        if (appIsExiting)
            return;

        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log($"Updating local game with these data : \n {args.Snapshot.GetRawJsonValue()} ");
        if (string.IsNullOrEmpty(args.Snapshot.GetRawJsonValue()))
            return;

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

    /// <summary>
    /// Compare your FCMToken against game.player.id foreach players.
    /// </summary>
    /// <param name="_game">The game you want to check</param>
    /// <returns>Returns 0 if you are not is this game. 1 if you are player1, 2 if you are player2</returns>
    private int WhichPlayerAmI(SGame _game)
    {
        if (!string.IsNullOrEmpty(_game.player1.token))
        {
            if (_game.player1.token == PlayerPrefs.GetString("FCMToken"))
                return 1;
        }

        if (!string.IsNullOrEmpty(_game.player2.token))
        {
            if (_game.player2.token == PlayerPrefs.GetString("FCMToken"))
                return 2;
        }

        return 0;
    }

    private void DisconnectFromGame(string _gameId)
    {
        if (_gameId == null)
            return;

        SGame tempGame = localDatabase.games[_gameId];

        //Disconnect you from the last game you were.
        int playerLastIndex = WhichPlayerAmI(tempGame);
        if (playerLastIndex == 1)
            tempGame.player1.isConnected = false;
        else if (playerLastIndex == 2)
            tempGame.player2.isConnected = false;

        onlineDatabase.Child("games").Child(_gameId).ValueChanged -= UpdateLocalGame;
        Debug.Log($"Disconnecting player{playerLastIndex} from {_gameId}");

        UpdateOnlineGame(tempGame);
    }

    private void ReconnectToGame(string _gameId)
    {
        if (_gameId == null)
            return;

        SGame tempGame = localDatabase.games[_gameId];

        //Disconnect you from the last game you were.
        int playerLastIndex = WhichPlayerAmI(tempGame);
        if (playerLastIndex == 1)
            tempGame.player1.isConnected = true;
        else if (playerLastIndex == 2)
            tempGame.player2.isConnected = true;

        onlineDatabase.Child("games").Child(_gameId).ValueChanged += UpdateLocalGame;
        Debug.Log($"Reconnect player{playerLastIndex} from {_gameId}");

        UpdateOnlineGame(tempGame);
    }
    #endregion
}
