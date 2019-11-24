using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player : MonoBehaviour
{
    public string username;
    public string id;
    public string location;

    public Mapbox.Examples.LocationStatus loc;
    private float health;
    public float Attack;
    public float Defense;
    public Range range;
    public Image HealthBar;
    public DatabaseReference player;

    private static Player instance;
    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Player>();
            }
            return instance;
        }
    }

    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            if (value >= 100)
            {
                health = 100;
            }
            else if (value <= 0)
            {
                health = 0;
            }
            else
            {
                health = value;
            }
            StartCoroutine(SetHealthBar(health/100));
        }
    }

    public float Range
    {
        get
        {
            return range.transform.localScale.x;
        }
        set
        {
            range.transform.localScale = new Vector3(value, value, 1f);

        }
    }

    public IEnumerator SetHealthBar(float health)
    {
        while (HealthBar.fillAmount != health)
        {
            if (HealthBar.fillAmount > .01f)
            {
                HealthBar.fillAmount -= .01f;

                if (HealthBar.fillAmount < .3f)
                {
                    // Under 30% health
                    if ((int)(HealthBar.fillAmount * 100f) % 3 == 0)
                    {
                        HealthBar.color = Color.white;
                    }
                    else
                    {
                        HealthBar.color = Color.red;
                    }
                }
            }
            else
            {
                health = 1f;
                HealthBar.color = Color.red;
            }
            HealthBar.fillAmount = Mathf.Lerp(HealthBar.fillAmount, health, Time.deltaTime * 1);
            yield return new WaitForEndOfFrame();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            
        }
    }

    // Returns the String of latitude and longitude from mapbox
    string GetCurrentLocation()
    {
        return loc.currLoc.LatitudeLongitude.x + ", " + loc.currLoc.LatitudeLongitude.y;
    }
}

[Serializable]
public class PlayerData
{
    public string username;
    public string id;
    public string location;

    public PlayerData() { }

    public PlayerData(string name, string id)
    {
        username = name;
        this.id = id;
        location = "0, 0";
    }



}
