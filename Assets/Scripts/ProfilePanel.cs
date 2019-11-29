using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfilePanel : MonoBehaviour
{
    private string DATA_URL = "https://iroyale-1571440677136.firebaseio.com/";
    private const string USERS = "users";

    private bool is_LogOut = false;

    public DatabaseReference Database;
    public CanvasGroup profilePanel;

    public void Open()
    {
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
        if (Database != null)
        {
            if (LoginInfo.IsGuest)
                Database.Child(USERS).Child(LoginInfo.Uid).RemoveValueAsync();
        }
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
        is_LogOut = true;
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
            SceneManager.LoadScene("Home");
        }
    }
}
