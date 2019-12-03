using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    public CanvasGroup lobbyPanel;
    public CanvasGroup mainPanel;
    public CanvasGroup battlePanel;
    public Text lobbyName;
    public Button openButton;
    public Text usersList;
    public Text timerText;
    public Lobby lobby;
    public Button joinButton;
    public Button exitButton;
    public bool isOpen = false;
    public bool inProgress = false;

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
        usersList.text = lobby.Usernames;
    }

    public async void JoinLobby()
    {
        await DatabaseManager.Instance.JoinLobby(lobby.lobbyId);
        joinButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(true);
        lobby.joined = true;
    }

    public async void ExitLobby()
    {
        await DatabaseManager.Instance.ExitLobby(lobby.lobbyId);
        exitButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(true);
        lobby.joined = false;
    }

    public void Open()
    {
        //Debug.Log("Opening");
        lobbyPanel.alpha = 1;
        lobbyPanel.blocksRaycasts = true;
        isOpen = true;
    }

    public void Close()
    {
        //Debug.Log("Closing");
        isOpen = false;
        lobbyPanel.alpha = 0;
        lobbyPanel.blocksRaycasts = false;
    }

    public void OpenMainPanel()
    {
        if (inProgress)
        {
            inProgress = false;
        }
        else
        {
            return;
        }
        mainPanel.alpha = 1;
        mainPanel.blocksRaycasts = true;
        battlePanel.alpha = 0;
        battlePanel.blocksRaycasts = true;
    }

    public void OpenBattlePanel()
    {
        if (inProgress)
        {
            return;
        }
        else
        {
            inProgress = true;
        }
        battlePanel.alpha = 1;
        battlePanel.blocksRaycasts = true;
        mainPanel.alpha = 0;
        mainPanel.blocksRaycasts = false;
    }
}
