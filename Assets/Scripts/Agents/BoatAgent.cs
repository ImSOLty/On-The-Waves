using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoatAgent : Agent
{
    [SerializeField] private Transform colliderT;
    [SerializeField] private Transform startForward;

    [SerializeField] private bool learning=true;
    [SerializeField] private CheckPointForAgentsControl _checkPointControl;
    private int Multiplicator = 50;

    private float start = 0;

    //  private List<float> poss = new List<float>()
    //  {
    //      0, 2.5f, 5f, 7.5f, 10f, 12.5f, 15f, 17.5f
    //  };
    //
    // private float x = 0;

    public Movement movement;

    private void Start()

    {
        if (!learning)
            return;
        _checkPointControl.onCorrectCheckPoint += onCarCorrectCheckpoint;
        _checkPointControl.onIncorrectCheckPoint += onCarWrongCheckpoint;
        _checkPointControl.onFinish += onFinished;
    }

    private void onFinished(object sender, BoatEventArgs e)
    {
        if (!learning)
            return;
        if (!e.Name.Equals(colliderT.parent.name))
            return;
        float reward = Time.unscaledTime - start;
        AddReward(Multiplicator);
        EndEpisode();
    }

    private void onCarWrongCheckpoint(object sender, BoatEventArgs e)
    {
        if (!learning)
            return;
        if (!e.Name.Equals(colliderT.parent.name))
            return;
        AddReward(-0.4f*Multiplicator);
        EndEpisode();
    }

    private void onCarCorrectCheckpoint(object sender, BoatEventArgs e)
    {
        if (!learning)
            return;
        if (!e.Name.Equals(colliderT.parent.name))
            return;
        Vector3 checkpointForward = _checkPointControl.GetNextCheckpoint(colliderT.parent.name).transform.forward;
        float directionDot = Vector3.Dot(transform.forward, checkpointForward);
        AddReward(directionDot*Multiplicator/2);

        float reach = Time.unscaledTime;
        AddReward(Multiplicator/((reach-start)*20));
        start = reach;
    }

    public override void OnEpisodeBegin()
    {
        if (!learning)
            return;
        movement.StopAction();
        // colliderT.localPosition = new Vector3(poss[Mathf.RoundToInt(x)], startPosition.y, startPosition.z);
        // x += 0.5f;
        // if (Mathf.RoundToInt(x) == 8)
        // {
        //     x = 0;
        // }
        colliderT.localPosition = new Vector3(Random.Range(0,17.5f),1.5f,0);
        transform.forward = startForward.forward;
        _checkPointControl.ResetProp(colliderT.parent.name);
        //_checkPointControl.UpdateCoins();
        start = Time.unscaledTime;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = transform.forward;
        if (learning)
        {
            if (_checkPointControl.GetNextCheckpoint(colliderT.parent.name))
                checkpointForward = _checkPointControl.GetNextCheckpoint(colliderT.parent.name).transform.forward;
        }

        float directionDot = Vector3.Dot(transform.forward, checkpointForward);
        //Debug.Log(directionDot);
        
        sensor.AddObservation(directionDot);

        // Checkpoint c = _checkPointControl.GetNextCheckpoint(colliderT.tag);
        // sensor.AddObservation(Vector3.Distance(transform.position,c.GetComponent<Collider>().bounds.center));
        sensor.AddObservation(movement.currentRotate);
        sensor.AddObservation(movement.currentSpeed);
    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        bool forwardAmount = false;
        float turnAmount = 0f;
        bool breakAmount = false;

        switch (vectorAction.DiscreteActions[0])
        {
            case 0:
                forwardAmount = false;
                break;
            case 1:
                forwardAmount = true;
                break;
        }

        switch (vectorAction.DiscreteActions[1])
        {
            case 0:
                turnAmount = 0f;
                break;
            case 1:
                turnAmount = 1f;
                break;
            case 2:
                turnAmount = -1f;
                break;
        }

        switch (vectorAction.DiscreteActions[2])
        {
            case 0:
                breakAmount = false;
                break;
            case 1:
                breakAmount = true;
                break;
        }
        
        movement.SetInput(forwardAmount, turnAmount, breakAmount);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (!learning)
            return;
        int forwardAmount = 0;
        int turnAmount = 0;
        int breakAmount = 0;

        if (Input.GetAxisRaw("Vertical") > 0) forwardAmount = 1;
        if (Input.GetAxisRaw("Vertical") < 0) forwardAmount = 2;
        if (Input.GetAxisRaw("Horizontal") > 0) turnAmount = 1;
        if (Input.GetAxisRaw("Horizontal") < 0) turnAmount = 2;
        if (Input.GetAxisRaw("Jump") > 0) breakAmount = 1;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = forwardAmount;
        discreteActions[1] = turnAmount;
        discreteActions[2] = breakAmount;
    }

    public void WallHit()
    {
        if (!learning)
            return;
        AddReward(-0.5f*Multiplicator);
        EndEpisode();
    }
    
    // public void CoinCollect()
    // {
    //     if (!learning)
    //         return;
    //     AddReward(10f*Multiplicator);
    // }

    public void WallSlide()
    {
        if (!learning)
            return;
        AddReward(-0.1f*Multiplicator);
    }
}