using Firebase.Messaging;
using UnityEngine;

public class FCMManager : MonoBehaviour
{
    public void Start()
    {
        /*FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;*/
    }

    public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        /*if (!PlayerPrefs.HasKey("FCMToken"))
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
        }*/
    }

    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        //Debug.Log("Received a new message from: " + e.Message.From);
    }
}
