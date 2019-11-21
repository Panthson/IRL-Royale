﻿using System.Collections;
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

    private DatabaseReference databaseReference;

    private bool signed_in = false;

    private string DATA_URL = "https://iroyale-1571440677136.firebaseio.com/";

    public void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DATA_URL);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Update()
    {
        if (signed_in) {
            Debug.Log("switching scenes");
            SceneManager.LoadScene("MapBox");
        }
    }

    public void Login()
    {
        Debug.Log("button pressed");
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text, 
            passwordInput.text).ContinueWith((task =>
        {
            if (task.IsCanceled)
            {
                Firebase.FirebaseException e =
              task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError)e.ErrorCode);
                Debug.Log("task cancelled");
                return;
            }
            if (task.IsFaulted)
            {
              
                Firebase.FirebaseException e =
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError)e.ErrorCode);
                Debug.Log("task faulted");
                return;
            }
            if (task.IsCompleted)
            {
                Debug.Log("login successful");
                LoginInfo.Email = emailInput.text;
                LoginInfo.Password = passwordInput.text;
                LoginInfo.Uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                LoginInfo.IsGuest = false;

                signed_in = true;
            }
        })); 
    }

    void GetErrorMessage(AuthError errorCode)
    {
        string msg = "";
        msg = errorCode.ToString();
        Debug.Log(msg);
    }
}