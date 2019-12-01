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

       
        _locationProvider.OnLocationUpdated += LocationLogger_OnLocationUpdated;
        Debug.Log("writing log to " + Application.persistentDataPath);
    }

// Update is called once per frame
void Update()
    {
        
    }

    void LocationLogger_OnLocationUpdated(Mapbox.Unity.Location.Location location)
    {
        logWriter.Write(location);
    }
}
