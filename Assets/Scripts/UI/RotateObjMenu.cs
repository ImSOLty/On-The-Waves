using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjMenu : MonoBehaviour
{
    [SerializeField] private float speed;
    void Update()
    {
        transform.Rotate(new Vector3(0,speed,0)*Time.deltaTime);
    }
}
