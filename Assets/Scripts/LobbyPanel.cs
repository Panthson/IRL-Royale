﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    public CanvasGroup lobbyPanel;
    public Text lobbyName;
    public Button openButton;
    public Text openText;
    public Text timerText;
    public Lobby lobby;
    public bool isOpen = false;

    private static LobbyPanel instance;
    public static LobbyPanel Instance {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LobbyPanel>();
            }
            return instance;
        }
    }

    

    public void InitializeLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyName.text = lobby.lobbyName;
    }


    public void Open()
    {
        Debug.Log("Opening");
        lobbyPanel.alpha = 1;
        lobbyPanel.blocksRaycasts = true;
        isOpen = true;
    }

    public void Close()
    {
        Debug.Log("Closing");
        isOpen = false;
        lobbyPanel.alpha = 0;
        lobbyPanel.blocksRaycasts = false;
    }
}
