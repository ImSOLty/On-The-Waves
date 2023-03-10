using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatEventArgs : EventArgs
{
    public BoatEventArgs(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}

public delegate void CustomEventHandler(object sender, BoatEventArgs args);

public class CheckPointForAgentsControl : MonoBehaviour
{
    public CustomEventHandler onCorrectCheckPoint, onIncorrectCheckPoint, onFinish;
    [SerializeField] private List<CheckpointForAgents> checkpoints = new List<CheckpointForAgents>();
    public GameObject startCheckpoint;
    [SerializeField] private LayerMask checkpointLayer;

    private float CumulativeReward = 0;
    private float MaxCumulativeReward = 0;
    private float endTotal = 0;
    private float minTime = 300;
    private float start = 0;
    private int finished = 0;

    private Dictionary<string, int> cur = new Dictionary<string, int>()
    {
        {"Boat 1", 1},
        {"Boat 2", 1},
        {"Boat 3", 1},
        {"Boat 4", 1},
        {"Boat 5", 1},
        {"Boat 6", 1},
        {"Boat 7", 1},
        {"Boat 8", 1},
    };

    private void Start()
    {
        // start = Time.unscaledTime;
        // onCorrectCheckPoint += onCorrectCheckPointDebug;
        // onIncorrectCheckPoint += onIncorrectCheckPointDebug;
        // onFinish += onFinished;
        // GetCheckPoints();
    }

    public void GetCheckPoints()
    {
        Vector3 curCh = startCheckpoint.GetComponent<Collider>().bounds.center;
        Vector3 way = -Vector3.forward;
        GameObject nextCheckpoint = null;
        RaycastHit hit;
        int i = 0;
        while (nextCheckpoint != startCheckpoint && i != 1000)
        {
            if (nextCheckpoint)
            {
                nextCheckpoint.layer = LayerMask.NameToLayer("Default");
            }

            Physics.Raycast(curCh, way, out hit, 30.0f, checkpointLayer.value);
            if (nextCheckpoint)
            {
                nextCheckpoint.layer = LayerMask.NameToLayer("Checkpoint");
            }

            Debug.DrawRay(curCh, way * 100, Color.red, 10000);
            nextCheckpoint = hit.collider.gameObject;

            curCh = hit.collider.bounds.center;
            way = -hit.normal;
            i++;
            CheckpointForAgents ch = nextCheckpoint.GetComponent<CheckpointForAgents>();
            ch.number = i;
            checkpoints.Add(ch);
        }
    }

    public CheckpointForAgents GetNextCheckpoint(string boat)
    {
        return checkpoints[cur[boat] - 1];
    }

    public void CheckCorrect(CheckpointForAgents c, Collider boatCollider)
    {
        string boat = boatCollider.tag;

        if (c.number == cur[boat])
        {
            if (c.number == checkpoints.Count)
            {
                onFinish.Invoke(this, new BoatEventArgs(boat));
            }
            else
            {
                //onCorrectCheckPoint.Invoke(this, new BoatEventArgs(boat));
                cur[boat]++;
            }
        }

        if (c.number > cur[boat] || c.number < cur[boat] - 1)
        {
            //onIncorrectCheckPoint.Invoke(this, new BoatEventArgs(boat));
        }
    }

    public void ResetProp(string boat)
    {
        cur[boat] = 1;
    }

    void onCorrectCheckPointDebug(object sender, BoatEventArgs e)
    {
        //Debug.Log( e.Name+ " Correct");
    }

    void onIncorrectCheckPointDebug(object sender, BoatEventArgs e)
    {
        //Debug.Log(e.Name + " Incorrect");
    }

    void onFinished(object sender, BoatEventArgs e)
    {
        //Debug.Log(e.Name + " Finished");
    }

    public void CommResult(float comm)
    {
        finished++;
        CumulativeReward += comm;
        endTotal += (Time.unscaledTime - start);
        
        if (MaxCumulativeReward < comm)
        {
            MaxCumulativeReward = comm;
        }
        if (minTime > (Time.unscaledTime - start))
        {
            minTime = (Time.unscaledTime - start);
        }
        
        if (finished == 8)
        {
            Debug.Log("Reward max: " + MaxCumulativeReward);
            Debug.Log("Time min: " + minTime);
            Debug.Log("Reward avg: " + CumulativeReward / 8);
            Debug.Log("Time avg: " + endTotal / 8);
        }
    }
}