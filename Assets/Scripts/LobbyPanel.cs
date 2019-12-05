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
    public Text lobbyName2;
    private string lobbyNameValue;
    public Button openButton;
    public Text usersList;
    public Text timerText;
    public Text timerText2;
    public Text timerBattleText;
    public Text battleText;
    private string timerValue;
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

    void Update()
    {
        if (lobby)
        {
            if (lobby.joined)
                battleText.text = "Players Remaining: " + lobby.playerNum + '\n' + "Kills: " + Player.Instance.currentKills;
        }
    }

    public string TimerValue
    {
        get
        {
            return timerValue;
        }
        set
        {
            timerValue = value;
            timerText.text = timerValue;
            timerText2.text = timerValue;
            timerBattleText.text = timerValue;
            if (timerValue == "0") timerBattleText.gameObject.SetActive(false);
            else timerBattleText.gameObject.SetActive(true);
        }
    }

    public string LobbyNameValue
    {
        get
        {
            return lobbyNameValue;
        }
        set
        {
            lobbyNameValue = value;
            lobbyName.text = lobbyNameValue;
            lobbyName2.text = lobbyNameValue;
        }
    }

    public void InitializeLobby(Lobby lobby)
    {
        this.lobby = lobby;
        LobbyNameValue = lobby.lobbyName;
        usersList.text = lobby.Usernames;
    }

    public async void JoinLobby()
    {
        if (lobby.inProgress == 1) return;
        await DatabaseManager.Instance.JoinLobby(lobby.lobbyId);
        joinButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(true);
        lobby.joined = true;
    }

    public async void ExitLobby()
    {
        if (lobby.isActive == 1) return;
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
