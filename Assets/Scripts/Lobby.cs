using Firebase.Database;
using Mapbox.Unity.Location;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    private readonly static string[] SEPARATOR = { ", ", "\n" };
    private const string LOCATION = "location";
    private const string ISACTIVE = "isActive";
    private const string PLAYERNUM = "playerNum";
    private const string PLAYERS = "players";
    private const string RADIUS = "radius";
    private const string TIMER = "timer";
    public string lobbyName;
    public int isActive;
    public int playerNum;
    public List<User> players;
    public float radius;
    public int timer;
    public bool locationSet = false;
    public bool currentLobby;
    private DatabaseReference db;
    public LobbyRange lobbyRange;
    private IEnumerator resizeRadius;
    private IEnumerator checkOpen;
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

    public void InitializeLobby(int isActive, string location, string lobbyName,
        int playerNum, string players, float radius, int timer, DatabaseReference db)
    {
        // Pulls the Database Reference
        this.db = db;
        // Sets the Name
        this.lobbyName = lobbyName;
        // Sets Active Game to In Progress
        this.isActive = isActive;
        // playerNum
        this.playerNum = playerNum;
        // Radius
        this.radius = radius;
        // Timer
        this.timer = timer;
        // Value Changed Listeners
        this.db.ValueChanged += HandleIsActiveChanged;
        this.db.ValueChanged += HandleLocationChanged;
        this.db.ValueChanged += HandlePlayersChanged;
        this.db.ValueChanged += HandleRadiusChanged;
        this.db.ValueChanged += HandleTimerChanged;
        // Set Radius Size
        resizeRadius = SetRadiusSize(radius);
        StartCoroutine(resizeRadius);
    }

    public void SetLocation(string location)
    {
        string[] loc = location.Split(SEPARATOR, 2,
            System.StringSplitOptions.None);
        float latitude = float.Parse(loc[0]);
        float longitude = float.Parse(loc[1]);
        if (Mathf.Abs(latitude) - Mathf.Abs((float)Player.Instance.Loc.currLoc.LatitudeLongitude.x) > 0.001f ||
            Mathf.Abs(longitude) - Mathf.Abs((float)Player.Instance.Loc.currLoc.LatitudeLongitude.y) > 0.001f)
        {
            //Debug.Log("Latitude: " + latitude + "\nLongitude: " + longitude);
            // delete this lobby because it is out of range
            DatabaseManager.Instance.lobbies.Remove(this);
            Destroy(gameObject);
            return;
        }
        Vector3 newPosition = LocationProviderFactory.Instance.mapManager.
            GeoToWorldPosition(new Mapbox.Utils.Vector2d(latitude, longitude));
        Debug.Log(lobbyName + " Timer: " + timer + " CurrentPosition: " + transform.position + " NewPosition: " + newPosition);
        if (Mathf.Abs(newPosition.x) > 2000 || Mathf.Abs(newPosition.z) > 2000)
        {
            // delete this lobby because it is out of range
            DatabaseManager.Instance.lobbies.Remove(this);
            Destroy(gameObject);
            return;
        }
        else
        {
            transform.localPosition = newPosition;
            locationSet = true;
        }
    }

    public void SetLobbyPanel()
    {
        LobbyPanel.Instance.InitializeLobby(this);
        currentLobby = true;
        LobbyPanel.Instance.openText.text = "Open: " + lobbyName;
        LobbyPanel.Instance.openButton.gameObject.SetActive(true);
    }

    public void RemoveLobbyPanel()
    {
        if (currentLobby)
        {
            LobbyPanel.Instance.lobby = null;
            currentLobby = false;
            LobbyPanel.Instance.openText.text = "Open: Nothing";
            LobbyPanel.Instance.Close();
            LobbyPanel.Instance.openButton.gameObject.SetActive(false);
        }
    }

    void HandleIsActiveChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        if (this)
            isActive = Int32.Parse(args.Snapshot.Child(ISACTIVE).Value.ToString());
    }

    void HandlePlayerNumChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        if (this)
            playerNum = Int32.Parse(args.Snapshot.Child(PLAYERNUM).Value.ToString());
    }

    void HandleLocationChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        string location = args.Snapshot.Child(LOCATION).Value.ToString();
        if (this)
            SetLocation(location);
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
        if (this)
        {

        }
    }

    void HandleRadiusChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        if (this)
        {
            radius = float.Parse(args.Snapshot.Child(RADIUS).Value.ToString());
            StopCoroutine(resizeRadius);
            resizeRadius = SetRadiusSize(radius);
            StartCoroutine(resizeRadius);
        }
    }

    void HandleTimerChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        if (this)
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

    public void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            db.ValueChanged -= HandleIsActiveChanged;
            db.ValueChanged -= HandleLocationChanged;
            db.ValueChanged -= HandlePlayersChanged;
            db.ValueChanged -= HandleRadiusChanged;
            db.ValueChanged -= HandleTimerChanged;
        }
    }

    public void OnApplicationQuit()
    {
        db.ValueChanged -= HandleIsActiveChanged;
        db.ValueChanged -= HandleLocationChanged;
        db.ValueChanged -= HandlePlayersChanged;
        db.ValueChanged -= HandleRadiusChanged;
        db.ValueChanged -= HandleTimerChanged;
    }
}
