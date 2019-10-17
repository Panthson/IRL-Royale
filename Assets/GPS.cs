using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GPS : MonoBehaviour
{
    public static GPS Instance { set; get; }

    public float latitude;
    public float longitude;

    public Text gps;
    public RawImage mapTexture;

    private string url = "";
    private IEnumerator mapCoroutine;
    private bool loadingMap = false;
    public int zoom;
    public int mapWidth;
    public int mapHeight;
    public enum mapType { roadmap, satellite, hybrid, terrain};
    public mapType mapSelected;
    public int scale;

    void Start()
    {
        StartCoroutine(StartLocationService());
        
    }

    void Update()
    {
        
    }

    public void UpdateMap()
    {
        Debug.Log("Updating Map");
        if (mapCoroutine != null)
        {
            StopCoroutine(mapCoroutine);
        }
        mapCoroutine = GetGoogleMap(latitude, longitude);
        StartCoroutine(mapCoroutine);
    }

    private IEnumerator GetGoogleMap(float lat, float lon)
    {
        loadingMap = true;
        url = "https://maps.googleapis.com/maps/api/staticmap?center=" + latitude + ',' + longitude + "&zoom=" + zoom + "&size="
            + mapWidth + 'x' + mapHeight + "&scale=" + scale + "&maptype=roadmap" + "&key=AIzaSyCxPXgnYG3SprUvxg13xd4EUoWcaFda6xU";
        WWW www = new WWW(url);
        yield return www;
        loadingMap = false;
        mapTexture.texture = www.texture;
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
        while (Input.location.status != LocationServiceStatus.Failed)
        {
            yield return new WaitForSeconds(3);
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            gps.text = "Lat: " + latitude + "   Long: " + longitude;
        }
        
        
    }
}
