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
    private const string INPROGRESS = "inProgress";
    private const string PLAYERNUM = "playerNum";
    private const string PLAYERS = "players";
    private const string RADIUS = "radius";
    private const string TIMER = "timer";
    public string lobbyId;
    public string lobbyName;
    public int inProgress;
    public int isActive;
    public int playerNum;
    public List<User> users;
    public string usernames;
    public float radius;
    private int timer;
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
    public int Timer
    {
        get
        {
            return timer;
        }
        set
        {
            timer = value;
            if (currentLobby)
            {
                LobbyPanel.Instance.timerText.text = timer.ToString();
            }
        }
    }

    public void InitializeLobby(string lobbyId, int isActive, int inProgress, string location, 
        string lobbyName, int playerNum, string usernames, float radius, 
        int timer, DatabaseReference db)
    {
        // Gets the lobby ID
        this.lobbyId = lobbyId;
        // Pulls the Database Reference
        this.db = db;
        // Sets the Name
        this.lobbyName = lobbyName;
        // Sets Lobby to Active
        this.isActive = isActive;
        // Sets Game in Progress
        this.inProgress = inProgress;
        // playerNum
        this.playerNum = playerNum;
        // usernames
        this.usernames = usernames;
        // Radius
        this.radius = radius;
        // Timer
        Timer = timer;
        // Value Changed Listeners
        db.ValueChanged += HandleDataChanged;
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
        Debug.Log(lobbyName + " Timer: " + Timer + " CurrentPosition: " + transform.position + " NewPosition: " + newPosition);
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
        currentLobby = true;
        LobbyPanel.Instance.InitializeLobby(this);
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

    void HandleDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        if (this)
        {
            isActive = Int32.Parse(args.Snapshot.Child(ISACTIVE).Value.ToString());
            inProgress = Int32.Parse(args.Snapshot.Child(INPROGRESS).Value.ToString());
            playerNum = Int32.Parse(args.Snapshot.Child(PLAYERNUM).Value.ToString());
            
            string location = args.Snapshot.Child(LOCATION).Value.ToString();
            SetLocation(location);

            radius = float.Parse(args.Snapshot.Child(RADIUS).Value.ToString());
            StopCoroutine(resizeRadius);
            resizeRadius = SetRadiusSize(radius);
            StartCoroutine(resizeRadius);

            Timer = Int32.Parse(args.Snapshot.Child(TIMER).Value.ToString());

            usernames = "";
            foreach (DataSnapshot user in args.Snapshot.Child(PLAYERS).Children)
            {
                usernames += user.Value.ToString() + "\n";
            }
        }
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
            db.ValueChanged -= HandleDataChanged;
        }
    }

    public void OnApplicationQuit()
    {
        db.ValueChanged -= HandleDataChanged;
    }
}
