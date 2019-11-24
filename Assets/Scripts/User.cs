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

    public string username = null;
    public string id = null;
    public string location = null;
    public DatabaseReference db = null;
    private bool initialized = false;

    //public string squadId;
    //public string lobbyId;
    //equipped weapon
    //armor
    //public float health;
    //weapons array
    //wins
    //losses
    //kills
    //deaths
    //icons

    public void InitializeUser(string username, string id, string location, 
        DatabaseReference db)
    {
        this.username = username;
        this.id = id;
        this.db = db;
        this.db.ValueChanged += HandleLocationChanged;
        SetLocation(location);
        initialized = true;
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

    void HandleLocationChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot


        location = args.Snapshot.Child(LOCATION).Value.ToString();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(initialized)
            SetLocation(this.location);
    }
}
