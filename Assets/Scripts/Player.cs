using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine.UI;
using System;

[Serializable]
public class Player
{
    public string username;
    public string id;
    public string location;

    public Player() { }

    public Player(string name, string id)
    {
        username = name;
        this.id = id;
        location = "0, 0";
    }

}
