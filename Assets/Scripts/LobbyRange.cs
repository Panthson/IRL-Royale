using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LobbyRange : MonoBehaviour
{
    public Lobby thisLobby;
    public SpriteRenderer circle;
    public async void OnTriggerEnter(Collider other)
    {
        while (!thisLobby.locationSet)
        {
            await Task.Delay(100);
        }
        if (other.CompareTag("Player"))
        {
            thisLobby.SetLobbyPanel();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            thisLobby.RemoveLobbyPanel();
        }
        
    }

    
}
