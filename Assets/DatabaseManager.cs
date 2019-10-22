using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class DatabaseManager : MonoBehaviour
{
    public Mapbox.Examples.LocationStatus loc;

    private static DatabaseReference db;

    // Start is called before the first frame update
    void Start()
    {
        string DBURL = "https://iroyale-1571440677136.firebaseio.com/";
        FirebaseDatabase t = FirebaseDatabase.GetInstance(DBURL);
        db = t.GetReference("");
    }

    // Update is called once per frame
    void Update()
    {
        if (db == null)
            return;
        //db.Child("lobbies").Child("0").Child("players").Child("0").Child("location").SetValueAsync(loc.currLoc.LatitudeLongitude.x + ", " + loc.currLoc.LatitudeLongitude.y);
        //db.Child("lobbies").Child("0").Child("players").Child("0").Child("attack").SetValueAsync(UnityEngine.Random.Range(0, 10));
        db.Child("players").Child("0").Child("location").SetValueAsync(loc.currLoc.LatitudeLongitude.x + ", " + loc.currLoc.LatitudeLongitude.y);
        Debug.Log("Writnig to DB");
    }
}
