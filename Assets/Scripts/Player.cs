﻿using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player : MonoBehaviour
{
    private const string USERS = "users";
    private const string LOCATION = "location";
    private const string HEALTH = "health";
    private const string ATTACK = "attack";
    private const string KILLS = "kills";
    private const string DEATHS = "deaths";
    private Mapbox.Examples.LocationStatus loc;
    public string username;
    public float health = 100f;
    public float attack = 5f;
    public int kills;
    public int deaths;
    private Range range;
    private Image HealthBar;
    public static DatabaseReference db;

    public Mapbox.Examples.LocationStatus Loc
    {
        get
        {
            if (loc == null)
            {
                loc = GetComponent<Mapbox.Examples.LocationStatus>();
            }
            return loc;
        }
    }

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
            if (range == null)
            {
                range = FindObjectOfType<Range>();
            }
            return range.transform.localScale.x;
        }
        set
        {
            if (range == null)
            {
                range = FindObjectOfType<Range>();
            }
            range.transform.localScale = new Vector3(value, value, 1f);

        }
    }

    public IEnumerator SetHealthBar(float health)
    {
        if (HealthBar == null)
        {
            HealthBar = DatabaseManager.Instance.healthBar;
        }
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

    public static void SetDatabaseReference (DatabaseReference reference)
    {
        if (db == null)
        {
            db = reference;
            db.ValueChanged += Instance.HandleHealthChanged;
            db.ValueChanged += Instance.HandleKillsChanged;
            db.ValueChanged += Instance.HandleDeathsChanged;
        }
    }

    public void HandleHealthChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        if (this)
            Health = float.Parse(args.Snapshot.Child(HEALTH).Value.ToString());
    }

    public void HandleKillsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        if (this)
        {
            kills = int.Parse(args.Snapshot.Child(KILLS).Value.ToString());
        }
    }

    public void HandleDeathsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot

        if (this)
        {
            deaths = int.Parse(args.Snapshot.Child(DEATHS).Value.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        SetLocation();
    }

    // Returns the String of latitude and longitude from mapbox
    public async void SetLocation()
    {
        if (db == null) return;
        string location = Loc.currLoc.LatitudeLongitude.x + ", " + Loc.currLoc.LatitudeLongitude.y;
        await db.Child(LOCATION).SetValueAsync(location);
    }

    public void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            db.ValueChanged -= HandleHealthChanged;
            db.ValueChanged -= HandleKillsChanged;
            db.ValueChanged -= HandleDeathsChanged;
        }
        else
        {

        }
    }

    /*public void OnApplicationQuit()
    {
        if (this) {
            player.ValueChanged -= HandleHealthChanged;
            db.ValueChanged -= HandleKillsChanged;
            db.ValueChanged -= HandleDeathsChanged;
        }
    }*/
}
