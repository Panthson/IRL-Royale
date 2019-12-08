using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfilePanel : MonoBehaviour
{
    private string DATA_URL = "https://iroyale-1571440677136.firebaseio.com/";
    private const string USERS = "users";

    public bool is_LogOut = false;

    public DatabaseReference Database;
    public CanvasGroup profilePanel;
    public Text Stats;

    private static ProfilePanel instance;
    public static ProfilePanel Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<ProfilePanel>();
            }

            return instance;
        }
    }

    public void Open()
    {
        Stats.text = LoginInfo.Username + "\nKills: " + Player.Instance.kills.ToString() +
            "\nDeaths: " + Player.Instance.deaths.ToString();
        profilePanel.alpha = 1;
        profilePanel.blocksRaycasts = true;
    }

    public void Close()
    {
        profilePanel.alpha = 0;
        profilePanel.blocksRaycasts = false;
    }

    public void LogOut()
    {
        is_LogOut = true;


        if (Database != null)
        {
            Player.Instance.RemoveListener();
        }

        Player.Instance.RemoveDatabaseReference();
        Player.Instance.SetDatabaseReference(null);

        if (LobbyPanel.Instance.lobby != null)
        {
            if (LobbyPanel.Instance.lobby.isActive == 1) {
                DatabaseManager.Instance.SetDeath();
            }
            else
                LobbyPanel.Instance.ExitLobby();
        }

        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene("Home");
    }

    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DATA_URL);
        Database = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {
        if (is_LogOut)
        {
            
        }
    }
}
