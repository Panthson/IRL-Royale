using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public CanvasGroup mainPanel;
    public CanvasGroup profilePanel;
    public CanvasGroup battlePanel;
    public CanvasGroup lobbyPanel;
    public CanvasGroup loadingScreen;
    private static CanvasManager instance;
    public static CanvasManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CanvasManager>();
            }
            return instance;
        }
    }

    public void Toggle(Panels panel)
    {
        CanvasGroup p = null;
        switch (panel)
        {
            case Panels.main:
                p = mainPanel;
                break;
            case Panels.profile:
                p = profilePanel;
                break;
            case Panels.battle:
                p = battlePanel;
                break;
            case Panels.lobby:
                p = lobbyPanel;
                break;
            case Panels.loading:
                p = loadingScreen;
                break;
            default:
                p = mainPanel;
                break;
        }
        if(p.alpha == 1)
        {
            p.alpha = 0;
            p.blocksRaycasts = false;
        }
        else
        {
            p.alpha = 1;
            p.blocksRaycasts = true;
        }
    }

    public enum Panels
    {
        main, profile, battle, lobby, loading
    }
}
