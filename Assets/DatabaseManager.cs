using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Mapbox;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;

public class DatabaseManager : MonoBehaviour
{
    // CONSTANTS
    private const string LOBBIES = "lobbies";
    private const string LOCATION = "location";
    private const string PLAYERS = "players";
    private const string PLAYERCOUNT = "playerCount";
    private const string ROOT = "";

    

    public Mapbox.Examples.LocationStatus loc;

    private static DatabaseReference db;

    private static string playerNum = "";

    private static int totalChildren = -500;

    private static bool initialized = false;

    //private IEnumerator 
    
    string getCurrentLocation()
    {
        return loc.currLoc.LatitudeLongitude.x + ", " + loc.currLoc.LatitudeLongitude.y;
    }

    string getPlayerNum()
    {
        return playerNum;
    }

    private IEnumerator getSize() {
        var task = db.Child(PLAYERS).GetValueAsync();

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

    /*
    int getLobbySize(FirebaseDatabase t)
    {
        int totalChildren = -2;

        db.Child(PLAYERS).GetValueAsync().ContinueWithOnMainThread(task => {
             if (task.IsFaulted)
             {
                 Debug.Log("ah shit");
             }
             else if (task.IsCompleted)
             {
                 totalChildren = (int)task.Result.ChildrenCount;
                 Debug.Log("ah shit " + totalChildren);
             }

         });

        Debug.Log("here we go again " + totalChildren);

        return totalChildren;
    }*/

    // Start is called before the first frame update
    void Start()
    {
        string DBURL = "https://iroyale-1571440677136.firebaseio.com/";
        FirebaseDatabase t = FirebaseDatabase.GetInstance(DBURL);
        db = t.GetReference(ROOT);
        /*
        int numPlayers;
        db.Child(PLAYERS).Child(PLAYERCOUNT).GetValueAsync().ContinueWith(task => {
          if (task.IsFaulted)
          {
                Debug.Log("get player count failed");
          }
          else if (task.IsCompleted)
          {
              DataSnapshot snapshot = task.Result;
              // Do something with snapshot...
          }
      });*/

        


        //playerNum = totalChildren.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (db == null)
            return;

        if (initialized == false)
        {
            StartCoroutine(getSize());
            
            Debug.Log("here we go again " + playerNum);
            
        }
        else
        {
            db.Child(PLAYERS).Child(getPlayerNum()).Child(LOCATION).SetValueAsync(getCurrentLocation());
            //Debug.Log("Writing to DB" + playerNum);
        }
    }

    // on application quit, delete player from database
    void OnApplicationQuit()
    {
        db.Child(PLAYERS).Child(getPlayerNum()).RemoveValueAsync();
    }
}

public class Player {
    private readonly static string[] SEPARATOR = { ", " };

    public string location;
    public float lat;
    public float lon;

    public Player() {
    }

    public Player(string location) {
        this.location = location;
        string[] loc = this.location.Split(SEPARATOR, 2, System.StringSplitOptions.RemoveEmptyEntries);
        this.lat = float.Parse(loc[0]);
        this.lon = float.Parse(loc[1]);
    }

    public string getLocation() {
        return location;
    }
}


public class YieldTask : CustomYieldInstruction
{
    public YieldTask(Task task)
    {
        Task = task;
    }

    public override bool keepWaiting => !Task.IsCompleted;

    public Task Task { get; }
}