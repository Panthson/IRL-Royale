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

public class DatabaseManager : MonoBehaviour
{
    // CONSTANTS
    private const string LOBBIES = "lobbies";
    private const string LOCATION = "location";
    private const string USERS = "users";
    private const string ROOT = "";

    public User[] users;

    private static DatabaseManager instance;

    public Mapbox.Examples.LocationStatus loc;

    private static DatabaseReference db;

    private FirebaseAuth auth;

    private static string playerNum = "";

    private static int totalChildren = -500;

    private static bool initialized = false;

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

    public IEnumerator PopulateUsers() {
        var task = db.Child(USERS).GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);

        foreach (DataSnapshot user in task.Result.Children) {
            Debug.Log("INSIDE POPULATEUSERS: " + user.Key + " " + (string)user.Value);
        }
        StartCoroutine(getSize());
    }

    private void InitializeFirebase()
    {

        FirebaseApp app = FirebaseApp.DefaultInstance;

        // db link
        app.SetEditorDatabaseUrl("https://iroyale-1571440677136.firebaseio.com/");

        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

        // user authentication
        auth = FirebaseAuth.DefaultInstance;
        auth.SignInWithEmailAndPasswordAsync("test@gmail.com", "testtest").ContinueWith(task => {
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

            db = FirebaseDatabase.DefaultInstance.RootReference;

            //Getting client id for FB using device id
            initialized = true;
            //StartCoroutine(getSize());
        });
    }

    string getCurrentLocation() {
        return loc.currLoc.LatitudeLongitude.x + ", " + loc.currLoc.LatitudeLongitude.y;
    }

    string getPlayerNum()
    {
        return playerNum;
    }

    private IEnumerator getSize() {
        var task = db.Child(USERS).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);
        if (task.IsCompleted)
            totalChildren = (int)task.Result.ChildrenCount;
        else if (task.IsFaulted)
            Debug.Log("ah shit");

        totalChildren = (int)task.Result.ChildrenCount;
        playerNum = totalChildren.ToString();
        Debug.Log("ah shit " + playerNum);
        
        initialized = true;
    }

    // Start is called before the first frame update
    public void Start()
    {

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            Debug.Log("UH");
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("DEPENDENCY");
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
        if (db == null)
            return;

        if (initialized == false) {
            Debug.Log("here we go again");
        }
        else {
            db.Child(USERS).Child("124").Child(LOCATION).SetValueAsync(getCurrentLocation());
            Debug.Log("updating");
        }
    }

    // on application quit, delete player from database
    void OnApplicationQuit()
    {
        db.Child(USERS).Child(getPlayerNum()).RemoveValueAsync();
        auth.SignOut();
    }
}