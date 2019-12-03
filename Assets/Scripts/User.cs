using Firebase.Database;
using Mapbox.Unity.Location;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    private readonly static string[] SEPARATOR = { ", ", "\n" };
    private const string ID = "id";
    private const string LOCATION = "location";
    private const string USERNAME = "username";
    private const string HEALTH = "health";

    public string username = null;
    public string id = null;
    public float health = 100f;
    public string lastAttackedBy;
    public string lobby = null;
    public Lobby parent;
    public DatabaseReference userData = null;
    private IEnumerator locationLerp;

    public void InitializeUser(string username, string location, 
        string lobby, string userID, DatabaseReference userData, Lobby parentLobby)
    {
        this.userData = userData;
        this.username = username;
        this.id = userID;
        this.lobby = lobby;
        parent = parentLobby;
        this.userData.ValueChanged += HandleDataChanged;
        locationLerp = SetLocation(location);
        StartCoroutine(locationLerp);
    }

    public IEnumerator SetLocation(string location)
    {
        string[] loc = location.Split(SEPARATOR, 2, 
            System.StringSplitOptions.None);
        float latitude = float.Parse(loc[0]);
        float longitude = float.Parse(loc[1]);
        Vector3 newPosition = LocationProviderFactory.Instance.mapManager.
            GeoToWorldPosition(new Mapbox.Utils.Vector2d(latitude, longitude));
        while (transform.localPosition != newPosition)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    // Handles Change in location of this user in the database
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
            if (args.Snapshot.Exists)
            {
                string location = args.Snapshot.Child(LOCATION).Value.ToString();
                // Resets Coroutine
                StopCoroutine(locationLerp);
                locationLerp = SetLocation(location);
                // Starts Coroutine to Move to location with new value
                StartCoroutine(locationLerp);
                health = float.Parse(args.Snapshot.Child(HEALTH).Value.ToString());
                if (health <= 0)
                {
                    lastAttackedBy = args.Snapshot.Child("lastAttackedBy").Value.ToString();
                    if (lastAttackedBy == LoginInfo.Uid)
                    {
                        Debug.Log("You killed " + username);
                        Death();
                    }
                }
            }
            else
            {
                Death();
            }
        }
    }

    public void Death()
    {
        parent.users.Remove(this);
        Destroy(this);
    }

    public void Disconnected()
    {
        Debug.Log(username + " Disconnected.");
        Death();
    }

    public void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Triggered");
    }

    public void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            userData.ValueChanged -= HandleDataChanged;
        }
    }

    /*public void OnApplicationQuit()
    {
        if (this)
            userData.ValueChanged -= HandleDataChanged;
    }*/
}
