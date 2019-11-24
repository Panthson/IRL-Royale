using Firebase.Database;
using Mapbox.Unity.Location;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    private readonly static string[] SEPARATOR = { ", ", "\n" };
    private const string ID = "id";
    private const string LOCATION = "location";
    private const string ISACTIVE = "isActive";
    private const string PLAYERS = "players";
    private const string RADIUS = "radius";
    private const string TIMER = "timer";

    public int isActive;
    public string location;
    public string lobbyName;
    public int playerNum;
    public List<User> players;
    public float radius;
    public int timer;
    private DatabaseReference db;
    public LobbyRange lobbyRange;

    public float LobbyRange
    {
        get
        {
            return lobbyRange.transform.localScale.x;
        }
        set
        {
            lobbyRange.transform.localScale = new Vector3(value, value, 1f);

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeLobby(int isActive, string location, string lobbyName,
        int playerNum, string players, float radius, int timer, DatabaseReference db)
    {
        this.isActive = isActive;
        SetLocation(location);
        this.lobbyName = lobbyName;
        this.playerNum = playerNum;
        this.radius = radius;
        this.timer = timer;
        this.db = db;
        this.db.ValueChanged += HandleIsActiveChanged;
        this.db.ValueChanged += HandlePlayersChanged;
        this.db.ValueChanged += HandleRadiusChanged;
        this.db.ValueChanged += HandleTimerChanged;
    }

    public void SetLocation(string location)
    {
        string[] loc = location.Split(SEPARATOR, 2,
            System.StringSplitOptions.None);
        float latitude = float.Parse(loc[0]);
        float longitude = float.Parse(loc[1]);
        transform.localPosition = LocationProviderFactory.Instance.mapManager.
            GeoToWorldPosition(new Mapbox.Utils.Vector2d(latitude, longitude));
    }

    void HandleIsActiveChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        isActive = Int32.Parse(args.Snapshot.Child(ISACTIVE).Value.ToString());
    }

    void HandlePlayersChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        // loop and parse
    }

    void HandleRadiusChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        radius = float.Parse(args.Snapshot.Child(RADIUS).Value.ToString());
        StartCoroutine(SetRadiusSize(radius));
    }

    void HandleTimerChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        timer = Int32.Parse(args.Snapshot.Child(TIMER).Value.ToString());
    }

    public IEnumerator SetRadiusSize(float radius)
    {
        while (LobbyRange != radius)
        {
            LobbyRange = Mathf.Lerp(LobbyRange, radius, Time.deltaTime * 1);
            yield return new WaitForEndOfFrame();
        }
    }
}
