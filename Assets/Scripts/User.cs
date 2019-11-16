using Firebase.Database;
using Mapbox.Unity.Location;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    private readonly static string[] SEPARATOR = { ", ", "\n" };
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
        string[] loc = location.Split(SEPARATOR, 2, System.StringSplitOptions.None);
        float latitude = float.Parse(loc[0]);
        float longitude = float.Parse(loc[1]);
        transform.localPosition = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(new Mapbox.Utils.Vector2d(latitude, longitude));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    async void Update()
    {
        if (DatabaseManager.Instance.initialized)
        {
            DataSnapshot snapshot = null;
            await DatabaseManager.Instance.Database.Child("users").Child("123").Child("location").GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    // Handle the error...
                    Debug.LogError("Task Failed");
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Task Completed");
                    snapshot = task.Result;

                }
            });
            if (snapshot != null)
                SetLocation(snapshot.Value.ToString());
        }
        
    }
}
