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
            Database.Child(USERS).Child("124").Child(LOCATION).SetValueAsync(GetCurrentLocation());
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
        Authenticator.SignInWithEmailAndPasswordAsync("test@gmail.com", "testtest").ContinueWith(task => {
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
            Debug.Log("Starting PopUsers");
            PopulateUsers();
        });
    }

    // Returns the String of latitude and longitude from mapbox
    string GetCurrentLocation() {
        return loc.currLoc.LatitudeLongitude.x + ", " + loc.currLoc.LatitudeLongitude.y;
    }

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
    }

    public async void PopulateUsers() {
        bool init = false;
        DataSnapshot snapshot = null;
        Debug.Log("Starting PopUSers");
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
    }

    // on application quit, delete player from database
    void OnApplicationPause() {
        if (Database != null)
        {
            Database.Child(USERS).Child("124").RemoveValueAsync();
        }
        if (Authenticator != null)
        {
            Authenticator.SignOut();
        }
    }
}