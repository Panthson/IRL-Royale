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
    public DatabaseReference userData = null;
    private IEnumerator locationLerp;

    public void InitializeUser(string username, string id, string location, 
        DatabaseReference userData)
    {
        this.username = username;
        this.id = id;
        this.userData = userData;
        this.userData.ValueChanged += HandleLocationChanged;
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
    void HandleLocationChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        string location = args.Snapshot.Child(LOCATION).Value.ToString();
        // Resets Coroutine
        StopCoroutine(locationLerp);
        locationLerp = SetLocation(location);
        // Starts Coroutine to Move to location with new value
        StartCoroutine(locationLerp);
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered");
    }
}
