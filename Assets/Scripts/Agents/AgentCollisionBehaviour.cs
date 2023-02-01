using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCollisionBehaviour : MonoBehaviour
{
    private BoatAgent _agent;
    private void Start()
    {
        _agent = transform.parent.GetComponentInChildren<BoatAgent>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Border"))
        {
            _agent.WallHit();
        }
    }
    // private void OnCollisionStay(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Border"))
    //     {
    //         _agent.WallSlide();
    //     }
    // }
}
