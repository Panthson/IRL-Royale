﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : MonoBehaviour
{
    public SpriteRenderer Circle;

    public SphereCollider Collider;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Range"))
        {
            Debug.Log("Player is colliding");
        }
    }


}
