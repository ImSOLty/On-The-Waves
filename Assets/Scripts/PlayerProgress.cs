using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProgress : MonoBehaviour
{
    public GameControl GameControl;
    public NetworkControl NetworkControl;
    public MapGenerator MapGenerator;
    private BoatAgent _agent;

    public ulong myId;
    private bool ml = false, myFinished = false;

    private int lap = 0;
    public int totalLaps = 0;
    private int checkpoint = 0;
    public int totalCheckpoints = 0;

    private int Multiplicator = 50;
    private int WinMultiplicator = 0, CoinMultiplicator;

    private float startTime;

    public void SetInitial(ulong id, NetworkControl nc, GameControl gameControl, bool isMl)
    {
        MapGenerator = FindObjectOfType<MapGenerator>();
        startTime = Time.unscaledTime;
        myId = id;
        ml = isMl;
        NetworkControl = nc;
        GameControl = gameControl;
        _agent = transform.parent.Find("Control").GetComponent<BoatAgent>();

        totalLaps = FindObjectOfType<MainMenu>().lapsAmount;
        totalCheckpoints = MapGenerator.checkpoints;
        WinMultiplicator = MapGenerator.multiplicator * totalLaps;
        CoinMultiplicator = MapGenerator.coinsAmount == 0 ? 0 : 5 / MapGenerator.coinsAmount;
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
                int chNumber = Int32.Parse(other.gameObject.name);
                if (checkpoint == chNumber)
                {
                    checkpoint++;
                    if (!ml) FindObjectOfType<AudioManager>().Play("Points");
                    Reward(2, "Checkpoint!");
                }
                else
                {
                    if (chNumber == -1 && checkpoint != 0)
                    {
                        checkpoint = 0;
                        lap++;
                        if (lap == totalLaps)
                        {
                            if (_agent)
                                _agent.enabled = false;
                            Reward(WinMultiplicator / (Time.unscaledTime - startTime), "Finished Track!",
                                finished: true);
                            break;
                        }

                        if (!ml) FindObjectOfType<AudioManager>().Play("Points");
                        Reward(3, "Finished Lap!");
                    }
                }

                break;
            case "ActualCorner":
                if (!ml) FindObjectOfType<AudioManager>().Play("Points");
                Vector3 checkpointForward = other.transform.forward;
                float directionDot = Vector3.Dot(transform.parent.Find("Control").forward, checkpointForward);
                Reward(directionDot, "Took A Corner!", "skill");
                break;
            case "Coin":
                if (!ml) FindObjectOfType<AudioManager>().Play("Points");
                NetworkControl.DestroyObjectServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId);
                Reward(CoinMultiplicator, "COIN!", "coins");
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
            FindObjectOfType<AudioManager>().Play("Crush");
            Reward(-0.5f, "Touched The Border!", "skill");
        }
    }

    void Reward(float amount, string reason, string type = "", bool finished = false)
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
        if (finished)
            myFinished = true;
    }
}