using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Movement : MonoBehaviour
{
    [SerializeField] public Rigidbody sphere;
    [SerializeField] public Transform model;
    [SerializeField] private Transform upmodel;
    [SerializeField] private Transform boatFoot;

    private Volume cameraPPVolume;
    private LensDistortion lensDistortion;

    [SerializeField] public float acceleration, steeringDefault, steeringDrift;

    [SerializeField] private float driftDecrease = 0.2f;

    public float currentRotate, currentSpeed;

    private float speed, rotate, steering, leftRight;
    private bool drift;

    public bool ml = false, isOwner, gameStarted;

    [SerializeField] private ParticleSystem engineParticlesHead;

    public void SetCamera()
    {
        CinemachineFreeLook freeLook = FindObjectOfType<CinemachineFreeLook>();
        freeLook.LookAt = sphere.transform;
        freeLook.Follow = transform;

        LensDistortion tmp;
        cameraPPVolume = FindObjectOfType<Volume>();
        if (!cameraPPVolume)
        {
            return;
        }

        if (cameraPPVolume.profile.TryGet(out tmp))
        {
            lensDistortion = tmp;
        }

        isOwner = true;
    }

    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(upmodel.position, Vector3.down, out hit, int.MaxValue, 1 << LayerMask.NameToLayer("Ground"));
        upmodel.rotation = Quaternion.Lerp(upmodel.rotation,
            Quaternion.FromToRotation(upmodel.up, hit.normal),
            Time.deltaTime * 2);

        bool grounded = Physics.CheckSphere(boatFoot.position, 1, 1 << LayerMask.NameToLayer("Ground"));
        bool space = !grounded ||
                     Input.GetAxisRaw("Jump") > 0;

        if (!ml && isOwner)
            SetInput(Input.GetAxisRaw("Vertical") > 0, Input.GetAxisRaw("Horizontal"), space);
    }

    public void SetInput(bool v, float h, bool j)
    {
        transform.position = sphere.transform.position - new Vector3(0, .5f, 0);
        if (v && gameStarted)
        {
            speed = acceleration;
            if (!ml && j)
            {
                speed -= acceleration * driftDecrease;
            }
        }

        drift = j;
        steering = drift ? steeringDrift : steeringDefault;

        leftRight = h;
        if (leftRight != 0)
        {
            rotate = steering * h * 0.003f * 3;
        }

        //Animations
        if (!ml)
        {
            ParticlesControl();
            PostProcessControl();
        }


        currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 20);
        speed = 0;
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 5);
        rotate = 0;
    }

    private void FixedUpdate()
    {
        sphere.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);
        if (speed == 0 && currentSpeed > 0)
        {
            sphere.AddForce(-sphere.velocity, ForceMode.Acceleration);
        }

        if (drift && sphere.velocity != Vector3.zero)
        {
            sphere.AddForce(-sphere.velocity * driftDecrease, ForceMode.Acceleration);
        }

        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles,
            new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.fixedDeltaTime * 5);
        model.localRotation = Quaternion.Lerp(model.localRotation,
            Quaternion.Euler(!ml ? -currentSpeed * 0.2f : -acceleration * 0.1f, transform.eulerAngles.y + currentRotate,
                -leftRight * 10f),
            Time.fixedDeltaTime * 5);
    }

    void ParticlesControl()
    {
        if (speed != 0 && engineParticlesHead.isStopped)
        {
            engineParticlesHead.Play();
        }
        else if (speed == 0 && !engineParticlesHead.isStopped)
        {
            engineParticlesHead.Stop();
        }
    }

    void PostProcessControl()
    {
        if (lensDistortion == null)
            return;
        float lensOnSpeed = -0.5f;
        float lensOnSlow = 0.0f;
        if (speed != 0)
        {
            lensDistortion.intensity.value = Mathf.Lerp(lensDistortion.intensity.value,
                lensOnSpeed,
                Time.deltaTime * 2);
        }
        else
        {
            lensDistortion.intensity.value = Mathf.Lerp(lensDistortion.intensity.value,
                lensOnSlow,
                Time.deltaTime * 2);
        }
    }

    public void StopAction()
    {
        sphere.velocity = Vector3.zero;
        speed = 0;
        rotate = 0;
        currentSpeed = 0;
        currentRotate = 0;
    }
}