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
using System;

public class DatabaseManager : MonoBehaviour
{
    // CONSTANTS
    private const string LOBBIES = "lobbies";
    private const string ID = "id";
    private const string LOCATION = "location";
    private const string USERNAME = "userName";
    private const string USERS = "users";
    private const string ROOT = "";
    private string ANONYMOUS_USERNAME = "anonymous";

    private const string ISACTIVE = "isActive";
    private const string LOBBYNAME = "lobbyName";
    private const string PLAYERNUM = "playerNum";
    private const string PLAYERS = "players";
    private const string RADIUS = "radius";
    private const string TIMER = "timer";

    private readonly static string[] SEPARATOR = { ", ", "\n" };

    // PRIVATE VARIABLES
    private static DatabaseManager instance;
    private FirebaseAuth Authenticator;
    private DataSnapshot userTree;
    private DataSnapshot lobbyTree;
    private bool getUsers = false;
    private bool getLobbies = false;

    // PUBLIC VARIABLES
    public DatabaseReference Database;
    public bool initialized = false;
    public List<User> users;
    public User userRef;
    public List<Lobby> lobbies;
    public Lobby lobbyRef;
    public Image healthBar;
    public CanvasGroup loadingScreen;

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

    public void StartLoad()
    {
        if (loadingScreen.blocksRaycasts) return;
        loadingScreen.alpha = 1;
        loadingScreen.blocksRaycasts = true;
    }

    public void EndLoad()
    {
        if (!loadingScreen.blocksRaycasts) return;
        loadingScreen.alpha = 0;
        loadingScreen.blocksRaycasts = false;
    }

    // Start checks dependencies and runs InitializeFirebase()
    public async void Start()
    {
        StartLoad();
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
        InitializeFirebase();
        StartCoroutine(CreateLobbies());
    }

    // Creates Lobby Objects after Firebase has initialized
    public IEnumerator CreateLobbies()
    {

        while (!initialized)
        {
            yield return new WaitForSeconds(1);
        }
        GetLobbies();
        while (!getLobbies)
        {
            yield return new WaitForEndOfFrame();
        }
        InstantiateLobbies();
        EndLoad();
    }

    // Initializes the Database and Authenticator
    // When initialized is true, this function has finished running.
    private void InitializeFirebase()
    {
        Debug.Log("Initializing Firebase");
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
                    initialized = true;
                    AddPlayer(newUser.DisplayName, LoginInfo.Uid);
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

                initialized = true;
                AddPlayer(ANONYMOUS_USERNAME, LoginInfo.Uid);
            });
    }

    // Adds Current Player to Database if initialized
    public async void AddPlayer(string userName, string id)
    {
        if (initialized)
        {
            Player.Instance.SetPlayer(userName, id);
            Player.SetDatabaseReference(Database.Child(USERS).Child(id));

            string jsonData = JsonUtility.ToJson(Player.Instance);

            await Database.Child("users").Child(LoginInfo.Uid).
                  SetRawJsonValueAsync(jsonData).ContinueWith(task2 =>
                  {

                  });
        }
    }

    // Gets Users from Lobby
    public async void GetUsers(string lobbyKey)
    {
        DataSnapshot snapshot = null;
        Debug.Log("Getting Users");

        await Database.Child(LOBBIES).Child(lobbyKey).GetValueAsync().ContinueWith(task => {
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
            userTree = snapshot;
            getUsers = true;
        }
    }

    // Gets Lobbies from Database
    public async void GetLobbies()
    {
        DataSnapshot snapshot = null;
        Debug.Log("Getting Lobbies");

        await Database.Child(LOBBIES).GetValueAsync().ContinueWith(task => {
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
            lobbyTree = snapshot;
            getLobbies = true;
        }
    }

    // Instantiates the User Objects in each Lobby
    // TODO
    // Currently it instantiates them in the scene on Start
    // Change this to be right before a match starts
    public void InstantiateUsers()
    {
        Debug.Log("Instantiating Users");
        foreach (DataSnapshot user in userTree.Children)
        {
            if (user.Key.Equals(LoginInfo.Uid))
                continue;
            if (user.Child(USERNAME).Value.ToString() != null)
            {
                User u = Instantiate(userRef, Vector3.zero, Quaternion.identity, transform);
                u.InitializeUser(user.Child(USERNAME).Value.ToString(),
                    user.Child(ID).Value.ToString(), user.Child(LOCATION).Value.ToString(),
                    Database.Child(USERS).Child(user.Key));
                users.Add(u);
            }
        }
    }

    // Instantiates the Lobby Objects in the scene saved in List<Lobby> lobbies
    public void InstantiateLobbies()
    {
        //Debug.Log("Instantiating Lobbies");
        foreach (DataSnapshot lobby in lobbyTree.Children)
        {
            if (lobby.Child(LOBBYNAME).Value.ToString() != null)
            {
                Lobby l = Instantiate(lobbyRef, Vector3.zero, Quaternion.identity, transform);
                l.lobbyRange.enabled = false;
                l.InitializeLobby(Int32.Parse(lobby.Child(ISACTIVE).Value.ToString()),
                    lobby.Child(LOCATION).Value.ToString(), lobby.Child(LOBBYNAME).Value.ToString(),
                    Int32.Parse(lobby.Child(PLAYERNUM).Value.ToString()),
                    lobby.Child(PLAYERS).Value.ToString(), float.Parse(lobby.Child(RADIUS).Value.ToString()),
                    Int32.Parse(lobby.Child(TIMER).Value.ToString()), Database.Child(LOBBIES).Child(lobby.Key));

                lobbies.Add(l);
                l.lobbyRange.enabled = true;
                Debug.Log(l.lobbyName);
            }
            
        }
    }

    public void DeleteAllUsers()
    {
        foreach(User user in users)
        {
            users.Remove(user);
            Destroy(user.gameObject);
        }
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
        }
        else
        {

        }
    }

    
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
    }
}