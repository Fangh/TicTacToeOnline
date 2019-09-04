using Firebase.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FCMManager : MonoBehaviour
{
    public static FCMManager Instance;

    #region Private Methods

    private void Awake()
    {
        Instance = this;
        OnlineManager.OnDependenciesChecked += Register;
    }

    private void Register()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        Debug.Log("TokenReceived has been registered");
    }

    private void OnDestroy()
    {
        OnlineManager.OnDependenciesChecked -= Register;
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        if (!PlayerPrefs.HasKey("FCMToken"))
        {
            Debug.Log($"first time getting FCM Token {token.Token}. Saving it.");
            PlayerPrefs.SetString("FCMToken", token.Token);
            return;
        }

        if (PlayerPrefs.GetString("FCMToken") != token.Token)
        {
            Debug.Log($"Token received { token.Token} is different from token saved { PlayerPrefs.GetString("FCMToken") }");
        }
        else
        {
            Debug.Log($"Token received { token.Token} is the same from token saved");
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log($"Received a new message (type : {e.Message.MessageType} id:{e.Message.MessageId}) from: {e.Message.From}. \n content : {Newtonsoft.Json.JsonConvert.SerializeObject(e.Message.Data) } ");
    }


    private IEnumerator POST_SendNotif(string recipientToken, string title, string body)
    {
        if (BoardManager.Instance.currentTeam == 0)
        {
            Debug.LogError("You can't send a notif while not being in a game");
            yield return null;
        }

        SFireCloudMessage newMessage = new SFireCloudMessage(recipientToken, new SFireCloudNotification(title, body));
        string JSON = Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, object>() { { "message", newMessage } });
        UnityWebRequest request = new UnityWebRequest("https://fcm.googleapis.com/v1/projects/tictactoe-123456/messages:send", "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        //I add to use Postman to get the key : https://stackoverflow.com/questions/50399170/what-bearer-token-should-i-be-using-for-firebase-cloud-messaging-testing
        request.SetRequestHeader("Authorization", "Bearer ya29.GltaB8Ne9yc0uXd6KJW5YrU0RiO-F-eNds92YpkFd7dTIsRzRN2wzezInMIo9rgeP-IVcYTKI_ngG9pKhclrfvz9PV_unksGAk2fTF6Rd0d1GQWuvTcDRDQksTBx");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.ASCII.GetBytes(JSON));
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            SimplePopup.Instance.Open("Notification", $"There were an error while sending a notification : { request.error}");
            Debug.LogError($"There were an error while sending a notification : {request.error}");
        }
        else
        {
            SimplePopup.Instance.Open("Notification", $"notification has been send {request.downloadHandler.text}");
            Debug.Log($"notification has been send {request.downloadHandler.text}");
        }
    }

    #endregion


    public void SendNotification(ENotifType type)
    {
        SGame currentGame = BoardManager.Instance.currentGame;

        string otherName = BoardManager.Instance.currentTeam == 1 ? currentGame.player2.id : currentGame.player1.id;
        string gameName = currentGame.id;
        string winnerName = "Nobody";
        string recipientToken = BoardManager.Instance.currentTeam == 1 ? currentGame.player2.token : currentGame.player1.token;

        if (currentGame.winner == 1)
            winnerName = currentGame.player1.id;
        else if (currentGame.winner == 2)
            winnerName = currentGame.player2.id;

        switch (type)
        {
            case ENotifType.nextTurn:
                StartCoroutine(POST_SendNotif(recipientToken, "Next Turn", $"Eyh {otherName} it's your turn to play in {gameName}"));
                break;
            case ENotifType.winner:
                StartCoroutine(POST_SendNotif(recipientToken, "Game is over", $"{winnerName} is the winner of {gameName}"));
                break;
            default:
                break;
        }

    }
}

public struct SFireCloudMessage
{
    public string token;
    public SFireCloudNotification notification;

    public SFireCloudMessage(string _token, SFireCloudNotification _notification)
    {
        token = _token;
        notification = _notification;
    }
}

public struct SFireCloudNotification
{
    public string title;
    public string body;

    public SFireCloudNotification(string _title, string _body)
    {
        title = _title;
        body = _body;
    }
}

public enum ENotifType
{
    nextTurn,
    winner
}
