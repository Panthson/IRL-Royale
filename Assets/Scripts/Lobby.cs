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
    private string usernames;
    public float radius;
    private int timer;
    public bool locationSet = false;
    public bool currentLobby = false;
    public bool joined = false;
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

    public string Usernames
    {
        get
        {
            return usernames;
        }
        set
        {
            usernames = value;
            if (currentLobby)
            {
                LobbyPanel.Instance.usersList.text = usernames;
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
        this.Usernames = usernames;
        // Radius
        this.radius = radius;
        // Timer
        Timer = timer;
        // Value Changed Listeners
        db.ValueChanged += HandleDataChanged;
        // Set Radius Size
        resizeRadius = SetRadiusSize(radius);
        if (isActiveAndEnabled)
            StartCoroutine(resizeRadius);
    }

    public void SetLocation(string location)
    {
        string[] loc = location.Split(SEPARATOR, 2,
            System.StringSplitOptions.None);
        //Debug.Log("WORKING WITH: " + lobbyName);
        float latitude = float.Parse(loc[0]);
        float longitude = float.Parse(loc[1]);
        // check if within range of player
        if (Mathf.Abs(latitude) - Mathf.Abs((float)Player.Instance.Loc.currLoc.LatitudeLongitude.x) > 0.001f ||
            Mathf.Abs(longitude) - Mathf.Abs((float)Player.Instance.Loc.currLoc.LatitudeLongitude.y) > 0.001f)
        {
            // delete this lobby because it is out of range
            DatabaseManager.Instance.lobbies.Remove(this);
            Destroy(gameObject);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
        Vector3 newPosition = LocationProviderFactory.Instance.mapManager.
            GeoToWorldPosition(new Mapbox.Utils.Vector2d(latitude, longitude));
        //Debug.Log(lobbyName + " Timer: " + Timer + " CurrentPosition: " + transform.position + " NewPosition: " + newPosition);
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
        LobbyPanel.Instance.openButton.gameObject.SetActive(true);
    }

    public async void RemoveLobbyPanel()
    {
        if (currentLobby)
        {
            await DatabaseManager.Instance.ExitLobby(lobbyId);
            LobbyPanel.Instance.lobby = null;
            currentLobby = false;
            LobbyPanel.Instance.usersList.text = "Open: Nothing";
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
            UpdateLobby(args.Snapshot);
        }
    }

    async void UpdateLobby(DataSnapshot snapshot)
    {
        Timer = Int32.Parse(snapshot.Child(TIMER).Value.ToString());
        isActive = Int32.Parse(snapshot.Child(ISACTIVE).Value.ToString());
        playerNum = Int32.Parse(snapshot.Child(PLAYERNUM).Value.ToString());
        string location = snapshot.Child(LOCATION).Value.ToString();
        SetLocation(location);
        radius = float.Parse(snapshot.Child(RADIUS).Value.ToString());
        StopCoroutine(resizeRadius);
        resizeRadius = SetRadiusSize(radius);
        StartCoroutine(resizeRadius);

        Usernames = "";
        foreach (DataSnapshot user in snapshot.Child(PLAYERS).Children)
        {
            Usernames += user.Value.ToString() + "\n";
        }
        int checkInProgress = Int32.Parse(snapshot.Child(INPROGRESS).Value.ToString());
        // If match is starting right now
        if (checkInProgress != inProgress)
        {
            inProgress = checkInProgress;
            // Match About to Start
            if (checkInProgress == 1)
            {
                // Lobby is In Progress
                if (joined)
                {
                    Debug.Log("Match Started");
                    // You are in this match and it has started
                    await DatabaseManager.Instance.GetUsers(snapshot.Child(PLAYERS), this);
                    LobbyPanel.Instance.OpenBattlePanel();
                    Player.Instance.CanAttack = true;
                    lobbyRange.circle.color = new Color(172, 0, 255, 50);
                }
                else
                {
                    Debug.Log("Match Started Without You");
                    // The match has started and you are not in
                    LobbyPanel.Instance.OpenMainPanel();
                    lobbyRange.circle.color = new Color(173, 23, 14);
                }
            }
            // Match Ended
            else
            {
                Debug.Log("Match Ended");
                Player.Instance.CanAttack = false;
                DatabaseManager.Instance.DeleteAllUsers(this);
                // Sets Back to Green
                lobbyRange.circle.color = new Color(68, 0, 255, 50);
                joined = false;
            }

            // Match now inProgress
        }
        // Match Going On
    }

    /*public async void TestGetUsers()
    {
        await DatabaseManager.Instance.GetUsers();
    }*/

    public IEnumerator SetRadiusSize(float radius)
    {
        while (LobbyRange != radius)
        {
            LobbyRange = Mathf.Lerp(LobbyRange, radius, Time.deltaTime * 1);
            yield return new WaitForEndOfFrame();
        }
    }

    /*public void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            db.ValueChanged -= HandleDataChanged;
        }
    }*/

    /*public void OnApplicationQuit()
    {
        db.ValueChanged -= HandleDataChanged;
    }*/
}
