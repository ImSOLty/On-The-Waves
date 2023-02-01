using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ManageCameras : MonoBehaviour
{
    public CinemachineFreeLook frontCFL;
    public Camera  cameraFront, cameraUp;
    public Text text;
    private List<(Transform, Transform)> boats = new List<(Transform, Transform)>();
    int current = 0;
    private int curview = 0;
    void Start()
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject boat = GameObject.Find("ML (" + i + ")");
            boats.Add((boat.transform.GetChild(1),boat.transform.GetChild(2)));
            Debug.Log(boats.Count);
        }
    }

    public void SwitchPlayer()
    {
        current++;
        if (current == 8)
        {
            current = 0;
        }

        frontCFL.Follow = boats[current].Item1;
        frontCFL.LookAt = boats[current].Item2;
        text.text = "Player " + (current+1);
    }
    public void SwitchView()
    {
        curview++;
        if (curview == 2)
        {
            curview = 0;
        }

        switch (curview)
        {
            case 0:
                cameraUp.enabled = false; cameraFront.enabled = true;
                break;
            case 1:
                cameraFront.enabled = false; cameraUp.enabled = true;
                break;
        }
    }
}
