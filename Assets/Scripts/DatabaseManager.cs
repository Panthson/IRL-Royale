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

public class DatabaseManager : MonoBehaviour {
    // CONSTANTS
    private const string LOBBIES = "lobbies";
    private const string ID = "id";
    private const string LOCATION = "location";
    private const string USERNAME = "username";
    private const string USERS = LOBBIES + "/0/users";
    private const string ROOT = "";
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
    public Image loading;

    private DataSnapshot usertree;
    private bool instantiateUsers = false;
    
    // Gives a reference of DatabaseManager using DatabaseManager.Instance
    public static DatabaseManager Instance {
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
    public void Start() {
        loading.enabled = true;
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
    void Update() {
        if (instantiateUsers) {
            instantiateUsers = false;
            InstantiateUsers();
        }
        if (initialized)
        {
            Database.Child(USERS).Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
                Child(LOCATION).SetValueAsync(GetCurrentLocation());
        }
    }

    // Initializes the Database and Authenticator
    private void InitializeFirebase() {
        FirebaseApp app = FirebaseApp.DefaultInstance;

        // db link
        app.SetEditorDatabaseUrl("https://iroyale-1571440677136.firebaseio.com/");

        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

        // user authentication
        Authenticator = FirebaseAuth.DefaultInstance;

        if (!LoginInfo.IsGuest)
            LoginAsUser();
        else
            LoginAsGuest();
    }

    private void LoginAsGuest(){
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

            GetUsers();
        });
    }

    private void LoginAsUser(){
        Authenticator.SignInWithEmailAndPasswordAsync(LoginInfo.Email,
                LoginInfo.Password).ContinueWith(task =>
                {
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
    }

    // Returns the String of latitude and longitude from mapbox
    string GetCurrentLocation() {
        return loc.currLoc.LatitudeLongitude.x + ", " + loc.currLoc.LatitudeLongitude.y;
    }

    /*
    private IEnumerator GetSize() {
        int totalChildren;
        var task = Database.Child(USERS).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);
        if (task.IsCompleted)
            totalChildren = (int)task.Result.ChildrenCount;
        else if (task.IsFaulted)
            Debug.Log("Task Failed");

        totalChildren = (int)task.Result.ChildrenCount;
        string playerNum = totalChildren.ToString();
        Debug.Log("User Count: " + playerNum);
        
        initialized = true;
        loading.enabled = false;
    }*/

    public async void GetUsers() {
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

    public void InstantiateUsers() {
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
    }

    // on application quit, delete player from database
    void OnApplicationPause() {
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