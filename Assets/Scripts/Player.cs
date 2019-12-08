using Firebase.Database;
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
    private const string LAST_ATTACKED = "lastAttackedBy";
    private const string LOBBY = "lobby";
    private Mapbox.Examples.LocationStatus loc;
    public string username;
    public string lastAttackedBy = "";
    public string lobby = "";
    public float health = 100f;
    public float attack = 5f;
    public int kills = 0;
    private int currentKills = 0;
    public int deaths = 0;
    private bool canAttack = false;
    private Range range;
    private Image HealthBar;
    public DatabaseReference db;

    void Start()
    {
        range = GetComponentInChildren<Range>();
        HealthBar = DatabaseManager.Instance.healthBar;
    }

    public bool CanAttack
    {
        get
        {
            if (range == null) return false; 
            return range.canAttack;
        }
        set
        {
            if (range != null) 
                range.canAttack = value;
        }
    }

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
                //Debug.Log("You Died to " + lastAttackedBy);
                LobbyPanel.Instance.OpenLossPanel(lastAttackedBy);
                ResetHealth();
            }
            else
            {
                health = value;
            }
            LobbyPanel.Instance.HealthText.text = health.ToString();
            //StopAllCoroutines();
            //StartCoroutine(SetHealthBar());
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
                HealthBar.color = Color.red;
            }
            //Debug.Log(health / 100f);
            HealthBar.fillAmount = Mathf.Lerp(HealthBar.fillAmount, health / 100f, 0.5f);
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

    public int CurrentKills { get => currentKills; set => currentKills = value; }

    public IEnumerator SetHealthBar()
    {
        while (HealthBar.fillAmount != health/100f)
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
                HealthBar.color = Color.red;
            }
            HealthBar.fillAmount = Mathf.Lerp(HealthBar.fillAmount, health/100f, Time.deltaTime * 0.1f);
            yield return new WaitForEndOfFrame();
        }
    }

    public void SetDatabaseReference (DatabaseReference reference)
    {
        db = reference;
    }

    public async void RemoveDatabaseReference() {
        if (LoginInfo.IsGuest)
        {
            await db.RemoveValueAsync();
        }
    }

    public void StartUpdatingPlayer()
    {
        db.ValueChanged += Instance.HandleDataChanged;
    }

    public void HandleDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        };
        // Do something with the data in args.Snapshot
        if (this)
        {
            if (Health != 0)
            {
                Health = float.Parse(args.Snapshot.Child(HEALTH).Value.ToString());
            }
            kills = int.Parse(args.Snapshot.Child(KILLS).Value.ToString());
            deaths = int.Parse(args.Snapshot.Child(DEATHS).Value.ToString());
            lobby = args.Snapshot.Child(LOBBY).Value != null ?
                    args.Snapshot.Child(LOBBY).Value.ToString() : "";
            lastAttackedBy = args.Snapshot.Child(LAST_ATTACKED).Value != null ?
                    args.Snapshot.Child(LAST_ATTACKED).Value.ToString() : "";
            /*Debug.Log("Health: " + health
                    + "kills: " + kills
                    + "deaths: " + deaths
                    + "lobby: " + lobby
                    + "lastAttackedBy: " + lastAttackedBy);*/
        }
    }

    // TODO DEATHS
    public async void SetNewDeathValues(bool death)
    {
        //int newKills = kills + currentKills;
        //await db.Child(KILLS).SetValueAsync(newKills.ToString());
        if (death)
        {
            deaths++;
            await db.Child(DEATHS).SetValueAsync(deaths.ToString());
        }
    }

    public async void ResetHealth() {
        await db.Child(HEALTH).SetValueAsync(100);
    }

    // Update is called once per frame
    void Update()
    {
        if(!ProfilePanel.Instance.is_LogOut)
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
            db.ValueChanged -= HandleDataChanged;
        }
        else
        {
            
        }
    }

    public void RemoveListener() { 
        db.ValueChanged -= HandleDataChanged;
    }

    public void AddListener() {
        db.ValueChanged += HandleDataChanged;
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
