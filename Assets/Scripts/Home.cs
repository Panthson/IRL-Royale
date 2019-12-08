using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;

public class Home : MonoBehaviour
{
    private string ANONYMOUS_USERNAME = "anonymous";

    private DatabaseReference databaseReference;

    private bool signed_in = false;

    private string DATA_URL = "https://iroyale-1571440677136.firebaseio.com/";

    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DATA_URL);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        Input.location.Start();
    }

    public void GoToLogin()
    {
        SceneManager.LoadScene("Login");
    }

    public void GoToSignup()
    {
        SceneManager.LoadScene("Signup");
    }

    public void GoToHome()
    {
        SceneManager.LoadScene("Home");
    }

    public void LogInAnonymous()
    {
        //FirebaseUser newUser = await FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
        //Debug.LogFormat("Firebase user created successfully: {0} ({1})",
        //    newUser.DisplayName, newUser.UserId);

        LoginInfo.IsGuest = true;
        //LoginInfo.Uid = newUser.UserId;

        SceneManager.LoadScene("Mapbox");
    }

    void GetErrorMessage(AuthError errorCode)
    {
        string msg = "";
        msg = errorCode.ToString();
        Debug.Log(msg);
    }
}
