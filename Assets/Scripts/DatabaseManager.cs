using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Auth;
using Firebase.Functions;
using UnityEngine.UI;
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
    private FirebaseFunctions Functions;
    private FirebaseDatabase Database;

    // PUBLIC VARIABLES
    public bool initialized;
    
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
        Database = FirebaseDatabase.DefaultInstance;
        Functions = FirebaseFunctions.DefaultInstance;
        if (!LoginInfo.IsGuest)
        {
            FirebaseUser firebaseUser = await Authenticator.
                SignInWithEmailAndPasswordAsync(LoginInfo.Email, LoginInfo.Password);
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
        await GetLobbies();
        
    }

    // Adds Current Player to Database if initialized
    public async Task SetPlayer(string id)
    {
        Player.SetDatabaseReference(Database.GetReference(USERS).Child(id));
        DataSnapshot player = await Database.GetReference(USERS).Child(id).GetValueAsync();
        if (LoginInfo.IsGuest)
        {
            Player.Instance.username = ANONYMOUS_USERNAME;
        }
        else
        {
            Player.Instance.username = player.Child(USERNAME).Value.ToString();
        }
        string jsonData = JsonUtility.ToJson(Player.Instance);
        await Database.GetReference(USERS).Child(id).SetRawJsonValueAsync(jsonData);
        Player.Instance.StartUpdatingPlayer();
    }

    // Gets Users from Lobby
    public async Task GetUsers(DataSnapshot players, Lobby lobby)
    {
        DeleteAllUsers(lobby);
        
        List<string> userIds = new List<string>();
        foreach (DataSnapshot user in players.Children)
        {
            userIds.Add(user.Key);
        }

        foreach (string id in userIds)
        {
            Debug.Log("ID: " + id);
        }

        foreach (string id in userIds) 
        {
            Debug.Log("Instantiating user: " + id);
            if (id.Equals(LoginInfo.Uid))
            {
                Debug.Log("Skipping this user");
            }
            else
            {
                DataSnapshot userObject = await Database.GetReference(USERS).Child(id).GetValueAsync();
                User u = Instantiate(userRef, Vector3.zero, Quaternion.identity, transform);
                //Debug.Log("Username: " + userObject.Child(USERNAME).Value.ToString());
                u.InitializeUser(userObject.Child(USERNAME).Value.ToString(),
                    userObject.Child(LOCATION).Value.ToString(), userObject.Child(LOBBY).ToString(), 
                    id, Database.GetReference(USERS).Child(userObject.Key), lobby);
                //Debug.Log("ADDING " + u.username + " TO LIST");
                lobby.users.Add(u);
            }
        }
    }

    // Gets Lobbies from Database
    public async Task GetLobbies()
    {
        StartLoad();
        loadingText.text = "Getting Lobbies...";
        DeleteAllLobbies();
        await Task.Delay(TimeSpan.FromSeconds(1));
        DataSnapshot lobbyTree = await Database.GetReference(LOBBIES).GetValueAsync();
        foreach (DataSnapshot lobby in lobbyTree.Children)
        {
            if (lobby.Child(LOBBYNAME).Value.ToString() != null)
            {

                Lobby l = Instantiate(lobbyRef, Vector3.zero, Quaternion.identity, transform);
                l.lobbyRange.enabled = false;
                l.gameObject.SetActive(false);

                string usernames = "";
                foreach (DataSnapshot user in lobby.Child(PLAYERS).Children) {
                    usernames += user.Value.ToString() + "\n";
                }

                l.InitializeLobby(lobby.Key.ToString(), Int32.Parse(lobby.Child(ISACTIVE).Value.ToString()),
                    Int32.Parse(lobby.Child(INPROGRESS).Value.ToString()),
                    lobby.Child(LOCATION).Value.ToString(), lobby.Child(LOBBYNAME).Value.ToString(),
                    Int32.Parse(lobby.Child(PLAYERNUM).Value.ToString()),
                    usernames, float.Parse(lobby.Child(RADIUS).Value.ToString()),
                    Int32.Parse(lobby.Child(TIMER).Value.ToString()), Database.GetReference(LOBBIES).Child(lobby.Key));

                lobbies.Add(l);
                l.lobbyRange.enabled = true;
                Debug.Log(l.lobbyName);
            }

        }
        EndLoad();
    }
    
    public Task JoinLobby(string lobbyId)
    {
        var data = new Dictionary<string, object>();
        data["playerId"] = LoginInfo.Uid;
        data["username"] = LoginInfo.Username;
        data["lobbyId"] = lobbyId;

        var function = Functions.GetHttpsCallable("joinLobby");

        return function.CallAsync(data).ContinueWith((task) =>
        {
            return task.Result.Data;
        });
    }

    public Task ExitLobby(string lobbyId)
    {
        var data = new Dictionary<string, object>();
        data["playerId"] = LoginInfo.Uid;
        data["lobbyId"] = lobbyId;

        var function = Functions.GetHttpsCallable("exitLobby");

        return function.CallAsync(data).ContinueWith((task) =>
        {
            return task.Result.Data;
        });
    }

    private Task<string> ReduceHP(string enemyId)
    {
        // Create the arguments to the callable function.
        var data = new Dictionary<string, object>();
        data["enemyId"] = enemyId;
        data["attackerId"] = LoginInfo.Uid;

        // Call the function and extract the operation from the result.
        var function = Functions.GetHttpsCallable("reduceHP");

        Debug.Log("calling database function");
        return function.CallAsync(data).ContinueWith((task) => {
            return (string)task.Result.Data;
        });
    }

    public void SendAttackCall(HashSet<string> enemies)
    {
        foreach(string enemyId in enemies)
        {
            ReduceHP(enemyId);
        }
    }

    // Deletes all Users
    public void DeleteAllUsers(Lobby lobby)
    {
        if (lobby.users.Count == 0) return;
        foreach(User user in lobby.users)
        {
            Destroy(user.gameObject);
        }
        lobby.users.Clear();
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
                    Database.GetReference(USERS).Child(LoginInfo.Uid).RemoveValueAsync();
            }
        }
    }
    
    void OnApplicationQuit()
    {
        if (Database != null)
        {
            if (LoginInfo.IsGuest)
                Database.GetReference(USERS).Child(LoginInfo.Uid).RemoveValueAsync();
        }
    }
}