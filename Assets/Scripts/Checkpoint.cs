using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private GameControl control;
    public int number;

    void Start()
    {
        control = FindObjectOfType<GameControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag.StartsWith("Boat"))
        {
            //control.UpdateFor(other.transform.tag, number);
        }
    }
}