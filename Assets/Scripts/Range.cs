using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : MonoBehaviour
{
    public SpriteRenderer Circle;
    public bool canAttack = false;
    public SphereCollider Collider;

    public HashSet<string> enemies;

    void Start()
    {
        enemies = new HashSet<string>();
    }

    void Update()
    {
        if (canAttack)
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
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string enemyID = other.gameObject.GetComponent<User>().id;
            enemies.Add(enemyID);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string enemyID = other.gameObject.GetComponent<User>().id;
            enemies.Remove(enemyID);
        }
    }

    private void AttackEnemies()
    {
        Debug.Log("Sending Attack Call to Enemies:");
        foreach (string enemyID in enemies)
        {
            Debug.Log(enemyID);
        }
        DatabaseManager.Instance.SendAttackCall(enemies);
    }


}
