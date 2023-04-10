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
    private CheckPointForAgentsControl _checkPointControl;
    [SerializeField] private Transform colliderT;
    [SerializeField] private Transform startForward;
    [SerializeField] private Vector3 startPosition;
    
    [SerializeField] private bool learning=true;

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
        //Time.timeScale = 8;
        _checkPointControl = FindObjectOfType<CheckPointForAgentsControl>();
        if (!learning)
            return;
        _checkPointControl.onCorrectCheckPoint += onCarCorrectCheckpoint;
        _checkPointControl.onIncorrectCheckPoint += onCarWrongCheckpoint;
        _checkPointControl.onFinish += onFinished;
    }

    private void onFinished(object sender, BoatEventArgs e)
    {
        if (!e.Name.Equals(colliderT.tag))
            return;
        float reward = Time.unscaledTime - start;
        AddReward(100/reward);
        Debug.Log(100/reward);
        _checkPointControl.CommResult(GetCumulativeReward());
        EndEpisode();
    }

    private void onCarWrongCheckpoint(object sender, BoatEventArgs e)
    {
        if (!e.Name.Equals(colliderT.tag))
            return;
        AddReward(-1f);
        //_checkPointControl.CommResult(GetCumulativeReward());
        EndEpisode();
    }

    private void onCarCorrectCheckpoint(object sender, BoatEventArgs e)
    {
        if (!e.Name.Equals(colliderT.tag))
            return;
        Vector3 checkpointForward = _checkPointControl.GetNextCheckpoint(colliderT.tag).transform.forward;
        float directionDot = Vector3.Dot(transform.forward, checkpointForward);
        AddReward(directionDot);
        // AddReward(1f);
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
        colliderT.localPosition = startPosition;
        transform.forward = startForward.forward;
        _checkPointControl.ResetProp(colliderT.tag);
        
        start = Time.unscaledTime;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = transform.forward;
        if (learning)
        {
            if (_checkPointControl.GetNextCheckpoint(colliderT.tag))
                checkpointForward = _checkPointControl.GetNextCheckpoint(colliderT.tag).transform.forward;
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
        AddReward(-5f);
        //_checkPointControl.CommResult(GetCumulativeReward());
        EndEpisode();
    }

    public void WallSlide()
    {
        if (!learning)
            return;
        AddReward(-0.1f);
    }
}