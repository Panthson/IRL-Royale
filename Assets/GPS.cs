using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class GPS : MonoBehaviour
{
    public static GPS Instance { set; get; }

    public float oLat;
    public float oLon;

    public float latitude;
    public float longitude;

    public Text gps;
    public Text pos;
    public RawImage mapTexture;

    private string url = "";
    private IEnumerator mapCoroutine;
    private IEnumerator locationService;
    private bool loadingMap = false;
    public int zoom;
    public int mapWidth;
    public int mapHeight;
    public enum mapType { roadmap, satellite, hybrid, terrain};
    public mapType mapSelected;
    public int scale;

    private static DatabaseReference db;

    void Start()
    {
        locationService = StartLocationService();
        StartCoroutine(locationService);

        //FirebaseApp.DefaultInstance.SetEditorDatabaseUrl();
        string DBURL = "https://iroyale-1571440677136.firebaseio.com/";
        FirebaseDatabase t = FirebaseDatabase.GetInstance(DBURL);
        db = t.GetReference("");
        //db = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void Update()
    {
        
    }

    public void RestartService()
    {
        StopCoroutine(locationService);
        locationService = StartLocationService();
        StartCoroutine(locationService);
    }

    Vector3 CalcTile(float lat, float lng)
    {
        //pseudo
        //n = 2 ^ zoom
        //xtile = n * ((lon_deg + 180) / 360)
        //ytile = n * (1 - (log(tan(lat_rad) + sec(lat_rad)) / π)) / 2

        float n = Mathf.Pow(2, zoom);
        float xtile = n * ((lng + 180) / 360);
        float ytile = n * (1 - (Mathf.Log(Mathf.Tan(Mathf.Deg2Rad * lat) + (1f / Mathf.Cos(Mathf.Deg2Rad * lat))) / Mathf.PI)) / 2f;
        return new Vector3(xtile, ytile);

    }

    public Vector3 CalcPos (float lat, float lng)
    {
        float x = Mathf.Pow(2, zoom + 8)/360;
        float y = Mathf.Pow(2, zoom + 8) / ( 360 * Mathf.Cos(lat));
        return new Vector3(x, y);
    }

    public void UpdateMap(int zoomLvl)
    {
        zoom = zoomLvl;
        UpdateMap();
    }

    public void UpdateMap()
    {
        if (mapCoroutine != null)
        {
            StopCoroutine(mapCoroutine);
        }
        transform.localPosition = Vector3.zero;
        oLat = latitude;
        oLon = longitude;
        Debug.Log("Updating Map");
        mapCoroutine = GetGoogleMap(oLat, oLon);
        StartCoroutine(mapCoroutine);

        if (db == null)
            return;
        db.Child("lobbies").Child("0").Child("players").Child("0").Child("location").SetValueAsync(oLat + ", " + oLon + "");
        db.Child("lobbies").Child("0").Child("players").Child("0").Child("attack").SetValueAsync(UnityEngine.Random.Range(0,10));
        Debug.Log("Writnig to DB");
    }

    private IEnumerator GetGoogleMap(float lat, float lon)
    {
        loadingMap = true;
        url = "https://maps.googleapis.com/maps/api/staticmap?center=" + latitude + ',' + longitude + "&zoom=" + zoom + "&size="
            + mapWidth + 'x' + mapHeight + "&scale=" + scale + "&maptype=roadmap" + "&key=AIzaSyCxPXgnYG3SprUvxg13xd4EUoWcaFda6xU";
        WWW www = new WWW(url);
        yield return www;
        mapTexture.texture = www.texture;
        loadingMap = false;
        StopCoroutine(mapCoroutine);
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("User has not enabled GPS");
        }
        Input.location.Start(0.1f, 0.1f);
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.Log("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        UpdateMap();
        while (Input.location.status != LocationServiceStatus.Failed)
        {
            if (transform.position.x >= 500 || transform.position.x <= -500 || transform.position.y >= 500 || transform.position.y <= -500)
            {
                UpdateMap();
            }
            yield return new WaitForEndOfFrame();
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            gps.text = "Lat: " + latitude + "   Long: " + longitude;
            while (loadingMap == true)
            {
                yield return new WaitForEndOfFrame();
            }
            Vector3 newPos = CalcTile(oLat - latitude, oLon - longitude);
            pos.text = "X: " + -newPos.x + "Y: " + -newPos.y;
            transform.localPosition = new Vector3(-newPos.x, -newPos.y);
        }
    }
}
