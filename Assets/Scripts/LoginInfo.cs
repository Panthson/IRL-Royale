﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LoginInfo
{
    private static string email = "test@gmail.com";
    private static string password = "testtest";
    private static string uid = "1QhRyxGhfYNG6YHY0IfQL5xLhNz2";
    private static bool isGuest = false;

    public static string Email
    {
        get
        {
            return email;
        }
        set
        {
            email = value;
        }
    }
    public static string Password
    {
        get
        {
            return password;
        }
        set
        {
            password = value;
        }
    }
    public static string Uid
    {
        get
        {
            return uid;
        }
        set
        {
            uid = value;
        }
    }

    public static bool IsGuest
    {
        get
        {
            return isGuest;
        }
        set
        {
            isGuest = value;
        }
    }
}
