using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject psFishes;
    public GameObject psBubbles;
    void Start()
    {
        Application.targetFrameRate = 300;
        //Generate();
    }

    void Generate()
    {
        GameObject particleSystems = new GameObject();
        for (int i = -5; i < 5; i++)
        {
            for (int j = -5; j < 5; j++)
            {
                GameObject tmp1 = Instantiate(psFishes, new Vector3(i * 25, -6, j * 25), Quaternion.Euler(new Vector3(0, Random.Range(0,360),0)));
                GameObject tmp2 = Instantiate(psBubbles, new Vector3(i * 25-12.5f, -6, j * 25-12.5f), Quaternion.Euler(new Vector3(0, Random.Range(0,360),0)));
                tmp1.transform.parent = particleSystems.transform;
                tmp2.transform.parent = particleSystems.transform;
            }
        }
    }
}
