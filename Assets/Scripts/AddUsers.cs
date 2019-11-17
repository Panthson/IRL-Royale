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

public class AddUsers : MonoBehaviour
{
    private Players data;
    public Text emailInput, passwordInput, username;

    private DatabaseReference databaseReference;

    private string DATA_URL = "https://iroyale-1571440677136.firebaseio.com/";

    private LogIn log = new LogIn();

    // Start is called before the first frame update
    void Start() {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DATA_URL);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void CreateNewUser() {
        if (emailInput.text.Equals("") || passwordInput.text.Equals(""))
        {
            //  print("Please enter an email, password, and username to register");
            return;
        }

        log.Register(emailInput, passwordInput, username);
        //Update
        data = new Players(username.text, FirebaseAuth.DefaultInstance.CurrentUser.UserId);

        string jsonData = JsonUtility.ToJson(data);
        if (FirebaseAuth.DefaultInstance.CurrentUser != null && FirebaseAuth.DefaultInstance.CurrentUser.Email != "")
        {
            print("user id: " + FirebaseAuth.DefaultInstance.CurrentUser.UserId);
            databaseReference.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
              SetRawJsonValueAsync(jsonData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
