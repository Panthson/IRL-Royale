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

public class DatabaseManager : MonoBehaviour {
    // CONSTANTS
    private const string LOBBIES = "lobbies";
    private const string LOCATION = "location";
    private const string USERS = "users";
    private const string ROOT = "";

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
        Debug.Log("starting dbmanager");
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
        if (initialized)
        {
            Database.Child(USERS).Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).
                Child(LOCATION).SetValueAsync(GetCurrentLocation());
        }
    }

    // Initializes the Database and Authenticator
    private void InitializeFirebase() {
        Debug.Log(LoginInfo.Email);
        Debug.Log(LoginInfo.Password);
        Debug.Log(LoginInfo.Uid);

        FirebaseApp app = FirebaseApp.DefaultInstance;

        // db link
        app.SetEditorDatabaseUrl("https://iroyale-1571440677136.firebaseio.com/");

        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

        // user authentication
        Authenticator = FirebaseAuth.DefaultInstance;

        if(!LoginInfo.IsGuest)
            Authenticator.SignInWithEmailAndPasswordAsync(LoginInfo.Email, LoginInfo.Password).ContinueWith(task => {
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
                Debug.Log("Starting PopUsers");
                PopulateUsers();
            });
        else
            FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith((task =>
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

                //Getting client id for FB using device id
                //initialized = true;
                Debug.Log("Starting PopUsers");
                PopulateUsers();
            }));
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

    public async void PopulateUsers() {
        DataSnapshot snapshot = null;
        Debug.Log("Starting PopUsers");
        
        await Database.Child(USERS).GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError("Task Failed");
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Task Completed");
                snapshot = task.Result;
                
            }
        });

        if (snapshot != null)
        {
            foreach (DataSnapshot user in snapshot.Children)
            {
                Debug.Log("User: " + user.Key + " " + user.Child(LOCATION).Value);
                //User u = Instantiate(userRef, transform.position, transform.rotation, transform);
                //Debug.Log("Instantiated");
                //userRef.SetUser(user.Key, user.Key, user.Child(LOCATION).Value.ToString());
                //users.Add(userRef);
            }
        }
        //StartCoroutine(GetSize());

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