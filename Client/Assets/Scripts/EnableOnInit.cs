using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableOnInit : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        GetComponent<Button>().interactable = false;
        OnlineManager.OnDatabaseDownloaded += DisableMe;
    }

    private void OnDisable()
    {
        OnlineManager.OnDatabaseDownloaded -= DisableMe;
    }

    private void DisableMe()
    {
        GetComponent<Button>().interactable = true;
    }
}
