using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CanvasLookAtCamera : MonoBehaviour
{
    private Camera _camera;

    void Update()
    {
        if (_camera == null)
        {
            try
            {
                _camera = FindObjectOfType<CinemachineBrain>().GetComponent<Camera>();
            }
            catch (Exception)
            {
                // ignored
            }

            return;
        }

        var rotation = _camera.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
    }
}