using Mapbox.Unity.Location;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    private readonly static string[] SEPARATOR = { ", " };
    public string username;
    public string id;
    public string location;
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

    public void SetUser(string username, string id, string location)
    {
        this.username = username;
        this.id = id;
        SetLocation(location);
    }

    public void SetLocation(string location)
    {
        string[] loc = this.location.Split(SEPARATOR, 2, System.StringSplitOptions.RemoveEmptyEntries);
        float latitude = float.Parse(loc[0]);
        float longitude = float.Parse(loc[1]);
        transform.localPosition = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(new Mapbox.Utils.Vector2d(latitude, longitude));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DatabaseManager.Instance.GetUser(id);
    }
}
