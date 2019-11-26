using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyRange : MonoBehaviour
{
    public Lobby thisLobby;
    public SpriteRenderer Circle;

    public SphereCollider Collider;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(SetCurrentLobby());
        }
    }

    public IEnumerator SetCurrentLobby()
    {
        yield return new WaitUntil(() => DatabaseManager.Instance.initialized);
        thisLobby.SetLobbyPanel();
    }

    public IEnumerator RemoveCurrentLobby()
    {
        yield return new WaitUntil(() => DatabaseManager.Instance.initialized);
        if (thisLobby.currentLobby == true)
        {
            thisLobby.currentLobby = false;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(RemoveCurrentLobby());
        }
        
    }

    
}
