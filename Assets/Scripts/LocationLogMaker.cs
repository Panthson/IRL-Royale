using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;

/* attach this class to an object to cause the device to log location */
public class LocationLogMaker : MonoBehaviour
{
    ILocationProvider _locationProvider;
    LocationLogWriter logWriter;
    bool isAndroid;

    // Start is called before the first frame update
    void Start()
    {
        logWriter = new LocationLogWriter();
        _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;

        // only work if on Android
        var regex = new Regex(@"(?<=API-)-?\d+");
        Match match = regex.Match(SystemInfo.operatingSystem); // eg 'Android OS 8.1.0 / API-27 (OPM2.171019.029/4657601)'
        isAndroid = match.Success;

        if (isAndroid)
        {
            _locationProvider.OnLocationUpdated += LocationLogger_OnLocationUpdated;
            Debug.Log("writing log to " + Application.persistentDataPath);
        }
        else
        {
            Debug.Log("not on Android, so not taking logs");
        }
    }

// Update is called once per frame
void Update()
    {
        
    }

    void LocationLogger_OnLocationUpdated(Mapbox.Unity.Location.Location location)
    {
        // only log if on android
        if (isAndroid)
        {
            logWriter.Write(location);
        }
    }
}
