using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProgress : MonoBehaviour
{
    public GameControl GameControl;
    public NetworkControl NetworkControl;
    private BoatAgent _agent;
    
    public ulong myId;
    private bool ml = false, myFinished = false;

    private int lap = 0;
    public int totalLaps = 0;
    private int checkpoint = 0;
    public int totalCheckpoints = 0;

    private int Multiplicator = 50;
    private int WinMultiplicator = 0;

    private float startTime;

    public void SetInitial(ulong id, NetworkControl nc, GameControl gameControl, bool isMl)
    {
        startTime = Time.unscaledTime;
        myId = id;
        ml = isMl;
        NetworkControl = nc;
        GameControl = gameControl;
        _agent = transform.parent.Find("Control").GetComponent<BoatAgent>();
        
        totalLaps = FindObjectOfType<MainMenu>().lapsAmount;
        totalCheckpoints = FindObjectOfType<MapGenerator>().checkpoints;
        WinMultiplicator = FindObjectOfType<MapGenerator>().multiplicator*totalLaps;
        if (!ml)
            GameControl.UpdateForMe(lap, checkpoint, totalLaps, totalCheckpoints);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (myFinished)
        {
            return;
        }
        switch (other.gameObject.tag)
        {
            case "ActualCheckpoint":
                Checkpoint ch = other.gameObject.GetComponent<Checkpoint>();
                if (checkpoint == ch.number)
                {
                    checkpoint++;
                    Reward(2, "Checkpoint!");
                }
                else
                {
                    if (ch.number == -1 && checkpoint != 0)
                    {
                        checkpoint = 0;
                        lap++;
                        if (lap == totalLaps)
                        {
                            if(_agent)
                                _agent.enabled = false;
                            Reward(WinMultiplicator/(Time.unscaledTime-startTime), "Finished Track!", finished:true);
                            break;
                        }

                        Reward(3, "Finished Lap!");
                    }
                }

                break;
            case "ActualCorner":
                Vector3 checkpointForward = other.transform.forward;
                float directionDot = Vector3.Dot(transform.parent.Find("Control").forward, checkpointForward);
                Reward(directionDot, "Took A Corner!", "skill");
                break;
            default:
                return;
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        if (myFinished)
        {
            return;
        }
        if (other.gameObject.CompareTag("Border"))
        {
            Reward(-0.5f, "Touched The Border!", "skill");
        }
    }

    void Reward(float amount, string reason, string type="", bool finished = false)
    {
        int actualReward = Mathf.RoundToInt(amount * Multiplicator);
        if (!ml)
        {
            GameObject notification = Instantiate(GameControl.notificationPrefab, GameObject.Find("Canvas").transform);
            Text[] texts = notification.GetComponentsInChildren<Text>();
            texts[0].text = amount < 0 ? actualReward.ToString() : "+" + actualReward;
            texts[0].color = amount < 0 ? GameControl.notificationBad : GameControl.notificationGood;
            texts[1].text = reason;
            texts[1].color = amount < 0 ? GameControl.notificationBad : GameControl.notificationGood;
        }

        NetworkControl.UpdateControlServerRpc(myId, actualReward, finished, type);
        if (!ml)
            GameControl.UpdateForMe(lap, checkpoint, totalLaps, totalCheckpoints);
        if(finished)
            myFinished = true;
    }
}