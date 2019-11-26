using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfilePanel : MonoBehaviour
{
    private bool is_LogOut = false;

    public CanvasGroup profilePanel;

    public void Open()
    {
        profilePanel.alpha = 1;
        profilePanel.blocksRaycasts = true;
    }

    public void Close()
    {
        profilePanel.alpha = 0;
        profilePanel.blocksRaycasts = false;
    }

    public void LogOut()
    {
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
        is_LogOut = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (is_LogOut)
        {
               SceneManager.LoadScene("Home");
        }
    }
}
