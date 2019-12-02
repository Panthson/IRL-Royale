using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : MonoBehaviour
{
    public SpriteRenderer Circle;

    public SphereCollider Collider;

    private HashSet<string> enemies;

    void Start()
    {
        enemies = new HashSet<string>();
    }

    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    AttackEnemies();
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string enemyID = other.gameObject.GetComponent<User>().id;
            string enemyUsername = other.gameObject.GetComponent<User>().username;
            enemies.Add(enemyID);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string enemyID = other.gameObject.GetComponent<User>().id;
            string enemyUsername = other.gameObject.GetComponent<User>().username;
            enemies.Remove(enemyID);
        }
    }

    private void AttackEnemies()
    {
        foreach(string id in enemies)
        {
            Debug.Log("ATTACK: " + id);
        }
    }


}
