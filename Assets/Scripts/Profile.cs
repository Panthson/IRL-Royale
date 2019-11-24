using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Profile : MonoBehaviour
{
    private bool is_LogOut = false;

    public GameObject ProfilePanel;

    public void OpenPanel()
    {
        if (ProfilePanel != null)
        {
            bool isActive = ProfilePanel.activeSelf;
            ProfilePanel.SetActive(!isActive);
        }
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
