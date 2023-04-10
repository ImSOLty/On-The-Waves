using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointForAgents : MonoBehaviour
{
    public int number;
    private CheckPointForAgentsControl control;

    private void Awake()
    {
        control = FindObjectOfType<CheckPointForAgentsControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag.StartsWith("Boat"))
        {
            //control.CheckCorrect(this, other);
        }
    }
}
