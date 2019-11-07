using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Mapbox;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;
using UnityEngine.UI;

public class DatabaseManager : MonoBehaviour
{
    // CONSTANTS
    private const string LOBBIES = "lobbies";
    private const string LOCATION = "location";
    private const string USERS = "users";
    private const string ROOT = "";

    public Text test;
    public User[] users;

    private static DatabaseManager instance;

    public Mapbox.Examples.LocationStatus loc;

    private static DatabaseReference db;

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

    private IEnumerator InitializeLocation() {
        yield return new WaitUntil(() => loc.initialized);

        test.text = "InitLoc";
        StartCoroutine(PopulateUsers());
    }

    public IEnumerator PopulateUsers() {
        var task = db.Child(USERS).GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);

        foreach (DataSnapshot user in task.Result.Children) {
            Debug.Log("INSIDE POPULATEUSERS: " + user.Key + " " + (string)user.Value);
        }
        test.text = "PopUsers";
        StartCoroutine(getSize());
    }
    
/*
    public void GetUser(string id)
    {
        db.Child(USERS).Child(id).Child(LOCATION).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.Log("ah shit");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                
            }

        });
    }
    */

    string getCurrentLocation() {
        return loc.currLoc.LatitudeLongitude.x + ", " + loc.currLoc.LatitudeLongitude.y;
    }

    string getPlayerNum()
    {
        return playerNum;
    }

    private IEnumerator getSize() {
        var task = db.Child(USERS).GetValueAsync();
        test.text = "getSize1";
        yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);
        test.text = "getSize2";
        if (task.IsCompleted)
            totalChildren = (int)task.Result.ChildrenCount;
        else if (task.IsFaulted)
            Debug.Log("ah shit");

        totalChildren = (int)task.Result.ChildrenCount;
        playerNum = totalChildren.ToString();
        Debug.Log("ah shit " + playerNum);
        
        initialized = true;
    }

    public void Test()
    {
        string DBURL = "https://iroyale-1571440677136.firebaseio.com/";
        FirebaseDatabase t = FirebaseDatabase.GetInstance(DBURL);
        db = t.GetReference(ROOT);
        test.text = "sTARTING";
        IEnumerator routine = InitializeLocation();
        StartCoroutine(routine);
    }

    // Start is called before the first frame update
    public void Start()
    {
        string DBURL = "https://iroyale-1571440677136.firebaseio.com/";
        FirebaseDatabase t = FirebaseDatabase.GetInstance(DBURL);
        db = t.GetReference(ROOT);
        test.text = "sTARTING";
        IEnumerator routine = InitializeLocation();
        StartCoroutine(routine);
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
            db.Child(USERS).Child(getPlayerNum()).Child(LOCATION).SetValueAsync(getCurrentLocation());
        }
    }

    // on application quit, delete player from database
    void OnApplicationQuit()
    {
        db.Child(USERS).Child(getPlayerNum()).RemoveValueAsync();
    }
}