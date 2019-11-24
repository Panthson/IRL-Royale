using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Mapbox;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;
using Mapbox.Unity.Location;

public class DatabaseManager : MonoBehaviour
{
    // CONSTANTS
    private const string LOBBIES = "lobbies";
    private const string ID = "id";
    private const string LOCATION = "location";
    private const string USERNAME = "username";
    private const string USERS = "users";
    private const string ROOT = "";
    private string ANONYMOUS_USERNAME = "anonymous";
    private readonly static string[] SEPARATOR = { ", ", "\n" };

    // PRIVATE VARIABLES
    private static DatabaseManager instance;
    private FirebaseAuth Authenticator;

    // PUBLIC VARIABLES
    public DatabaseReference Database;
    public Mapbox.Examples.LocationStatus loc;
    public bool initialized = false;
    public List<User> users;
    public User userRef;

    private DataSnapshot usertree;
    private bool instantiateUsers = false;
    private bool doItAgain = true;

    // Gives a reference of DatabaseManager using DatabaseManager.Instance
    public static DatabaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DatabaseManager>();
            }
            return instance;
        }
    }

    // Start checks dependencies and runs InitializeFirebase()
    public void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (instantiateUsers && doItAgain)
        {
            instantiateUsers = false;
            InstantiateUsers();
        }
        if (initialized)
        {
            Player.player = Database.Child(USERS).Child(LoginInfo.Uid);
            Player.player.ValueChanged += Player.Instance.HandleHealthChanged;
        }
    }

    // Initializes the Database and Authenticator
    private void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;

        // db link
        app.SetEditorDatabaseUrl("https://iroyale-1571440677136.firebaseio.com/");

        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

        // user authentication
        Authenticator = FirebaseAuth.DefaultInstance;

        if (!LoginInfo.IsGuest)
            Authenticator.SignInWithEmailAndPasswordAsync(LoginInfo.Email,
                LoginInfo.Password).ContinueWith(task => {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                        return;
                    }


                    Firebase.Auth.FirebaseUser newUser = task.Result;
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);

                    Database = FirebaseDatabase.DefaultInstance.RootReference;

                    //Getting client id for FB using device id
                    //initialized = true;
                    GetUsers();
                });
        else
            FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e =
                  task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    return;
                }
                if (task.IsFaulted)
                {

                    Firebase.FirebaseException e =
                    task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    return;
                }

                Firebase.Auth.FirebaseUser newUser = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);

                Database = FirebaseDatabase.DefaultInstance.RootReference;

                PlayerData data = new PlayerData(ANONYMOUS_USERNAME, LoginInfo.Uid);

                string jsonData = JsonUtility.ToJson(data);

                Database.Child("users").Child(LoginInfo.Uid).
                      SetRawJsonValueAsync(jsonData).ContinueWith(task2 =>
                      {
                          GetUsers();
                      });
            });
    }

    public async void GetUsers()
    {
        DataSnapshot snapshot = null;
        Debug.Log("Getting Users");

        await Database.Child(USERS).GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError("Task Failed");
            }
            else if (task.IsCompleted)
            {
                snapshot = task.Result;

            }
        });

        if (snapshot != null)
        {
            usertree = snapshot;
            instantiateUsers = true;
        }
    }

    public void InstantiateUsers()
    {
        Debug.Log("instantiating users");
        foreach (DataSnapshot user in usertree.Children)
        {
            if (user.Key.Equals(LoginInfo.Uid))
                continue;

            User u = Instantiate(userRef, Vector3.zero, Quaternion.identity, transform);

            u.InitializeUser(user.Child(USERNAME).Value.ToString(),
                user.Child(ID).Value.ToString(), user.Child(LOCATION).Value.ToString(),
                Database.Child(USERS).Child(user.Key));

            users.Add(u);
        }

        initialized = true;
        doItAgain = false;
    }

    // on application quit, delete player from database
    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            if (Database != null)
            {
                if (LoginInfo.IsGuest)
                    Database.Child(USERS).Child(LoginInfo.Uid).RemoveValueAsync();
            }
            if (Authenticator != null)
            {
                Authenticator.SignOut();
            }
        } else
        {
                Start();
        }
    }

    /*
    void OnApplicationQuit()
    {
        if (Database != null)
        {
            if (LoginInfo.IsGuest)
                Database.Child(USERS).Child(LoginInfo.Uid).RemoveValueAsync();
        }
        if (Authenticator != null)
        {
            Authenticator.SignOut();
        }
    }*/
}