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

public class SignUp : MonoBehaviour
{
    public InputField emailInput, passwordInput, confirmPasswordInput, username;

    private DatabaseReference databaseReference;

    private string DATA_URL = "https://iroyale-1571440677136.firebaseio.com/";

    //Update panel

    public GameObject comfPanel;

    public GameObject ErrorPanel;

    public GameObject errorText;

    private bool is_SignUp = false;

    private bool is_errorMessage = false;

    private string msg;

    // Start is called before the first frame update
    void Start() {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DATA_URL);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void CreateNewUser() {
        if (emailInput.text.Equals("") || passwordInput.text.Equals("") || confirmPasswordInput.text.Equals("")
            || username.text.Equals(""))
        {
            Debug.Log("All fields not filled in");
            OpenPanel("All fields not filled in");

            return;
        }

        if (!passwordInput.text.Equals(confirmPasswordInput.text))
        {
            Debug.Log("Password and confirm password not the same");
            OpenPanel("Password and confirm password not the same");
            return;
        }

        Register(emailInput.text, passwordInput.text, username.text);
    }

    public void GoToHome()
    {
        SceneManager.LoadScene("Home");
    }

    public void GoToLogin()
    {
        SceneManager.LoadScene("Login");
    }

    public async void Register(string emailInput, string passwordInput, string username)
    {
        if (emailInput.Equals("") && passwordInput.Equals("") && username.Equals(""))
        {
            return;
        }
        string id = "";
        await FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInput, 
            passwordInput).ContinueWith((task => {
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
            FirebaseUser newUser = task.Result;
            id = newUser.UserId;
            CreateNewUser(id);
        }));
    }

    async void CreateNewUser(string id)
    {
        DatabaseReference newUser = databaseReference.Child("users").Child(id);
        await newUser.Child("username").SetValueAsync(username.text);
        await newUser.Child("kills").SetValueAsync(0);
        await newUser.Child("deaths").SetValueAsync(0);
        is_SignUp = true;
    }

    void GetErrorMessage(AuthError errorCode)
    {
        msg = "";
        msg = errorCode.ToString();
        is_errorMessage = true;
        //OpenPanel(msg);
        Debug.Log(msg);
    }

    public void OpenConfirmationPanel()
    {
        if(comfPanel != null)
        {
            comfPanel.SetActive(true);
        }
    }

    public void OpenPanel(string msg)
    {
        if(comfPanel != null)
        {
            bool isActive = ErrorPanel.activeSelf;
            ErrorPanel.SetActive(!isActive);
            errorText.SetActive(true);
            errorText.GetComponent<Text>().text = msg;

        }
        if (is_errorMessage)
        {
            is_errorMessage = false;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (is_SignUp)
        {
            OpenConfirmationPanel();
        }
        if (is_errorMessage)
        {
            OpenPanel(msg);
        }
    }
}
