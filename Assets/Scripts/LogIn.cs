using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;


//private static DatabaseReference database;

public class LogIn : MonoBehaviour
{
  
    public InputField emailInput, passwordInput;
    public GameObject ErrorPanel;
    public Text errorText;
    private DatabaseReference databaseReference;

    private bool signed_in = false;

    private string DATA_URL = "https://iroyale-1571440677136.firebaseio.com/";

    public void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DATA_URL);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public async void Login()
    {
        Debug.Log("button pressed");
        FirebaseUser user = await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text, passwordInput.text).ContinueWith((task =>
        {
            if (task.IsCanceled)
            {
                Firebase.FirebaseException e =
              task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError)e.ErrorCode);
                return null;
            }

            if (task.IsFaulted)
            {
                Firebase.FirebaseException e =
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                GetErrorMessage((AuthError)e.ErrorCode);
                return null;
            }
            return task.Result;
        }));
        if (user == null) return;
        LoginInfo.Email = emailInput.text;
        LoginInfo.Password = passwordInput.text;
        LoginInfo.Uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        LoginInfo.IsGuest = false;
        SceneManager.LoadScene("MapBox");
    }

    void GetErrorMessage(AuthError errorCode)
    {
        string msg = "";
        msg = errorCode.ToString();
        OpenPanel(msg);
    }

    public void OpenPanel(string msg)
    {
        ErrorPanel.SetActive(true);
        errorText.text = msg;
    }

    public void ClosePanel()
    {
        ErrorPanel.SetActive(false);
    }
}
