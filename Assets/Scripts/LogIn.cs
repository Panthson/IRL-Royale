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

public class LogIn //: MonoBehaviour
{
  
    public Text emailInput, passwordInput, username;

    private Players data;

    private DatabaseReference databaseReference;

    private string DATA_URL = "https://iroyale-1571440677136.firebaseio.com/";

    public void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DATA_URL);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
       // LogIn.DontDestroyOnLoad(this.Re);
    }


    public void Login()
    {
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text, passwordInput.text).ContinueWith((task =>
        {
            if (task.IsCanceled)
            {
                Firebase.FirebaseException e =
              task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError)e.ErrorCode);
                return;
            }
            if (task.IsFaulted)
            {
              
                Firebase.FirebaseException e =
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError)e.ErrorCode);
                return;
            }
            if (task.IsCompleted)
            {
               // print("USER IS LOGIN");
            }
        }));
    }

    public void Logout()
    {
        if(FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }
    }

    public void LogInAnonymous()
    {
        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().
            ContinueWith((task =>
            {
                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e =
                  task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                    GetErrorMessage((AuthError)e.ErrorCode);
                    return;
                }
                if (task.IsFaulted)
                {

                    Firebase.FirebaseException e =
                    task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                    GetErrorMessage((AuthError)e.ErrorCode);
                    return;
                }
                if (task.IsCompleted)
                {
                 //   print("USER IS LOGIN ANO");
                   // SceneManager.LoadScene("Mapbox");
                }
            }));
      //  SceneManager.LoadScene("Mapbox");
    }

    public void Register(Text emailInput, Text passwordInput, Text username)
    {
        //SceneManager.LoadScene("NewUser");

        if(emailInput.text.Equals("") && passwordInput.text.Equals("") && username.text.Equals(""))
        {
          //  print("Please enter an email, password, and username to register");
            return;
        }
      
        //Update
        //data = new Players(username.text);

        // string jsonData = JsonUtility.ToJson(data);

        //databaseReference.Child("User" + Random.Range(0, 1000000)).
        //  SetRawJsonValueAsync(jsonData);
       

        //Old
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInput.text, passwordInput.text).ContinueWith((task =>
         {
             if (task.IsCanceled)
             {
                 Firebase.FirebaseException e =
               task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                 GetErrorMessage((AuthError)e.ErrorCode);
                 return;
             }

             if (task.IsFaulted)
             {
                 Firebase.FirebaseException e =
               task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                 GetErrorMessage((AuthError)e.ErrorCode);
                 return;
             }
             // Firebase user has been created.
             Firebase.Auth.FirebaseUser newUser = task.Result;
             Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                 newUser.DisplayName, newUser.UserId);
             /*
             if (task.IsCompleted)
             {
                 FirebaseAuth.DefaultInstance.GetHashCode();
               
               //  print("Registration COMPLETE");
             }

             */
             //FirebaseUser newUser = task.Result;
         }));
     //   if (FirebaseAuth.DefaultInstance.CurrentUser != null)
       //     print("user id: " + FirebaseAuth.DefaultInstance.CurrentUser.UserId);
    }

    void GetErrorMessage(AuthError errorCode)
    {
        string msg = "";
        msg = errorCode.ToString();
       // print(msg);
    }
}
