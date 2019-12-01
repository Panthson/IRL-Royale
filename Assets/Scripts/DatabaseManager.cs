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
    private const string USERNAME = "username";
    private const string KILLS = "kills";
    private const string DEATHS = "deaths";
    private const string USERS = "users";
    private const string LOBBY = "lobby";
    private const string ROOT = "";
    private string ANONYMOUS_USERNAME = "anonymous";

    private const string ISACTIVE = "isActive";
    private const string INPROGRESS = "inProgress";
    private const string LOBBYNAME = "lobbyName";
    private const string PLAYERNUM = "playerNum";
    private const string PLAYERS = "players";
    private const string RADIUS = "radius";
    private const string TIMER = "timer";

    private readonly static string[] SEPARATOR = { ", ", "\n" };

    // PRIVATE VARIABLES
    private static DatabaseManager instance;
    private FirebaseAuth Authenticator;

    // PUBLIC VARIABLES
    public bool initialized;
    public DatabaseReference Database;
    public List<User> users;
    public User userRef;
    public List<Lobby> lobbies;
    public Lobby lobbyRef;
    public Image healthBar;
    public CanvasGroup loadingScreen;
    public Text loadingText;

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

    // Start Loading Screen
    public void StartLoad()
    {
        if (loadingScreen.blocksRaycasts) return;
        loadingScreen.alpha = 1;
        loadingScreen.blocksRaycasts = true;
    }
    
    // End Loading Screen
    public void EndLoad()
    {
        if (!loadingScreen.blocksRaycasts) return;
        loadingScreen.alpha = 0;
        loadingScreen.blocksRaycasts = false;
    }

    // // Initializes the Database and Authenticator
    public async void Start()
    {
        StartLoad();
        // Checking Dependencies
        var dependencystatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        //Debug.Log("Initializing Firebase");
        loadingText.text = "Initializing Firebase...";
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // Link to Database
        app.SetEditorDatabaseUrl("https://iroyale-1571440677136.firebaseio.com/");
        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
        // user authentication
        Authenticator = FirebaseAuth.DefaultInstance;
        Database = FirebaseDatabase.DefaultInstance.RootReference;
        if (!LoginInfo.IsGuest)
        {
            FirebaseUser firebaseUser = await Authenticator.SignInWithEmailAndPasswordAsync(LoginInfo.Email, LoginInfo.Password);
            LoginInfo.Uid = firebaseUser.UserId;
        }
        else
        {
            FirebaseUser firebaseUser = await FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
            LoginInfo.Uid = firebaseUser.UserId;
            LoginInfo.Username = ANONYMOUS_USERNAME;
        }
        loadingText.text = "Awaiting Player...";
        await SetPlayer(LoginInfo.Uid);
        EndLoad();
        initialized = true;
        GetLobbies();
    }

    // Adds Current Player to Database if initialized
    public async Task SetPlayer(string id)
    {
        Player.SetDatabaseReference(Database.Child(USERS).Child(id));
        DataSnapshot player = await Database.Child(USERS).Child(id).GetValueAsync();
        if (LoginInfo.IsGuest)
        {
            Player.Instance.username = ANONYMOUS_USERNAME;
        }
        else
        {
            Player.Instance.username = player.Child(USERNAME).Value.ToString();
        }
        string jsonData = JsonUtility.ToJson(Player.Instance);
        await Database.Child(USERS).Child(id).SetRawJsonValueAsync(jsonData);
    }

    // Gets Users from Lobby
    public async void GetUsers(string lobbyKey)
    {
        DeleteAllUsers();
        Debug.Log("Getting Users");
        DataSnapshot userTree = await Database.Child(LOBBIES).Child(lobbyKey).GetValueAsync();
        foreach (DataSnapshot user in userTree.Children)
        {
            if (user.Key.Equals(LoginInfo.Uid))
                continue;
            if (user.Child(USERNAME).Value.ToString() != null)
            {
                User u = Instantiate(userRef, Vector3.zero, Quaternion.identity, transform);
                u.InitializeUser(user.Child(USERNAME).Value.ToString(),
                    user.Child(LOCATION).Value.ToString(), user.Child(LOBBY).ToString(), Database.Child(USERS).Child(user.Key));
                users.Add(u);
            }
        }
    }

    // Gets Lobbies from Database
    public async void GetLobbies()
    {
        StartLoad();
        loadingText.text = "Getting Lobbies...";
        DeleteAllLobbies();
        Debug.Log("Getting Lobbies");
        await Task.Delay(TimeSpan.FromSeconds(1));
        DataSnapshot lobbyTree = await Database.Child(LOBBIES).GetValueAsync();
        foreach (DataSnapshot lobby in lobbyTree.Children)
        {
            if (lobby.Child(LOBBYNAME).Value.ToString() != null)
            {
                Lobby l = Instantiate(lobbyRef, Vector3.zero, Quaternion.identity, transform);
                l.lobbyRange.enabled = false;
                l.InitializeLobby(lobby.Key.ToString(), Int32.Parse(lobby.Child(ISACTIVE).Value.ToString()),
                    Int32.Parse(lobby.Child(INPROGRESS).Value.ToString()),
                    lobby.Child(LOCATION).Value.ToString(), lobby.Child(LOBBYNAME).Value.ToString(),
                    Int32.Parse(lobby.Child(PLAYERNUM).Value.ToString()),
                    float.Parse(lobby.Child(RADIUS).Value.ToString()),
                    Int32.Parse(lobby.Child(TIMER).Value.ToString()), Database.Child(LOBBIES).Child(lobby.Key));

                lobbies.Add(l);
                l.lobbyRange.enabled = true;
                Debug.Log(l.lobbyName);
            }

        }
        EndLoad();
    }

    // Deletes all Users
    public void DeleteAllUsers()
    {
        if (users.Count == 0) return;
        foreach(User user in users)
        {
            Destroy(user.gameObject);
        }
        users.Clear();
    }

    // Deletes all Lobbies
    public void DeleteAllLobbies()
    {
        if (lobbies.Count == 0) return;
        foreach (Lobby lobby in lobbies)
        {
            Destroy(lobby.gameObject);
        }
        lobbies.Clear();
    }

    // on application quit, delete player from database if guest
    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            if (Database != null)
            {
                if (LoginInfo.IsGuest)
                    Database.Child(USERS).Child(LoginInfo.Uid).RemoveValueAsync();
            }
        }
    }
    
    void OnApplicationQuit()
    {
        if (Database != null)
        {
            if (LoginInfo.IsGuest)
                Database.Child(USERS).Child(LoginInfo.Uid).RemoveValueAsync();
        }
    }
}