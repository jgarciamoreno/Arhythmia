using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : MonoBehaviour {

    [Header("Init")]
    public GameObject body;

    [Header("Input")]
    public float vertical;
    public float horizontal;
    private float moveAmount;
    private float desiredMoveAmount;
    public Vector3 movingDir;
    public Vector3 desiredDir;

    [Header("Stats")]
    public float moveSpeed = 7f;
    public float runSpeed = 14f;
    public float acceleration = 7.5f;
    public float minRotationSpeed = 2.5f;
    public float maxRotationSpeed = 20f;
    public float characterHeight = 0.3f;
    public float jumpForce = 5;

    [Header("States")]
    public bool Grounded;
    public bool Moving;
    public bool Running;
    public bool Sliding;
    public bool Jumping;
    public bool Falling;
    public bool QuickTurning;
    public bool Grappling;


    [HideInInspector]
    public Animator anim;
    private AnimationController animationController;
    [HideInInspector]
    public Rigidbody rBody;
    [HideInInspector]
    public LayerMask groundLayers;
    private CameraMovement cameraMovement;

    private RaycastHit hit;
    private float angle;
    private float rotation;

    private Vector3 forwardSpeed;
    private float currentSpeed;
    private float maxSpeed;
    private float stepSpeed = 2;
    private bool invertVertical;

    private Transform hookBase;
    private Transform comparisonPosition;
    private Vector3 comparisonDirection;
    private Vector3 latchedPos;
    private Vector3 latchedDir;
    private Action hookCallback;
    private float grappleBaseSpeed;
    private float grappleSpeed;

    public void Init()
    {
        SetupAnimtaor();
        rBody = GetComponent<Rigidbody>();
        rBody.angularDrag = 999;
        rBody.drag = 4;
        rBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        LayerMask l1 = 1 << 10;
        LayerMask l2 = 1 << 17;
        groundLayers = (l1 | l2);

        maxSpeed = moveSpeed;
    }

    private void SetupAnimtaor()
    {
        animationController = GetComponent<AnimationController>();
    }

    public void SetupCamera(CameraMovement cam)
    {
        cameraMovement = cam;
    }

    public void FixedTick(float delta)
    {
        Grounded = OnGround();
        rBody.drag = (moveAmount > 0 || Grounded == false) ? 0 : 4;

        Moving = moveAmount > 0.25f ? true : false;
        CalculateSpeed(delta);

        forwardSpeed = rBody.velocity;
        forwardSpeed.y = 0;

        if (Grappling)
        {
            rBody.velocity = latchedDir.normalized * grappleSpeed;
            if (Vector3.Distance(comparisonPosition.position, latchedPos) < 1f)
            {
                Grappling = false;
                if (hookCallback != null) hookCallback();
            }
        }

        if (Grounded && !Grappling)
        {
            Rotate(delta);

            int m = desiredMoveAmount >= moveAmount ? 1 : -1;
            m = desiredMoveAmount == 0 && moveAmount == desiredMoveAmount ? 0 : m;
            if (!QuickTurning)
            {
                br.x = Mathf.Abs(vertical) * currentSpeed;
                moveAmount += (stepSpeed * m) * delta;
            }
            else
            {
                br.x = vertical * currentSpeed;
            }

            moveAmount = Mathf.Clamp01(moveAmount);
            rBody.velocity = transform.forward * (currentSpeed * moveAmount);
            movingDir = transform.forward;
            movingDir.y = 0;

            br.z = -horizontal * currentSpeed;
            br.y = transform.rotation.eulerAngles.y;
            body.transform.rotation = Quaternion.Euler(br);

            animationController.Move(moveAmount);
        }
    }

    Vector3 br;
    private bool OnGround()
    {
        bool r = false;
        Vector3 origin = rBody.position + (Vector3.up * characterHeight);
        float distance = characterHeight + 0.1f;

        Jumping = rBody.velocity.y > 1f ? true : false;
        if(Physics.Raycast(origin, Vector3.down, out hit, distance, groundLayers) && !Jumping)
        {
            r = true;
            transform.position = hit.point;
        }

        return r;
    }

    private void CalculateSpeed(float delta)
    {
        if (Running)
        {
            if (currentSpeed < maxSpeed)
                currentSpeed += acceleration * delta;
            else
                currentSpeed = maxSpeed;
        }
        else
        {
            if (currentSpeed > maxSpeed)
                currentSpeed -= acceleration * delta;
            else
                currentSpeed = maxSpeed;
        }
    }

    private void Rotate(float delta)
    {
        angle = Vector3.Angle(movingDir, desiredDir);
        if (angle <= 120 || (!Running && currentSpeed < 10))
        {
            QuickTurning = false;
            Vector3 targetDir = desiredDir;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
            {
                targetDir = transform.forward;
            }

            rotation = maxRotationSpeed / Vector3.Magnitude(forwardSpeed);
            rotation = Mathf.Clamp(rotation, minRotationSpeed, maxRotationSpeed);

            Quaternion rot = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, rot, delta * moveAmount * rotation);
            transform.rotation = targetRotation;
        }
        else
        {
            if (forwardSpeed.magnitude > 0.5f)
                QuickTurning = true;

            stepSpeed = Running ? 2 : 3;
            moveAmount -= stepSpeed * delta;
            if (moveAmount <= 0)
            {
                Vector3 targetDir = desiredDir;
                targetDir.y = 0;
                if (targetDir == Vector3.zero)
                {
                    targetDir = transform.forward;
                }
                Quaternion rot = Quaternion.LookRotation(targetDir);
                transform.rotation = rot;

                if (QuickTurning && !invertVertical)
                {
                    cameraMovement.ResetCamera(transform.eulerAngles.y, 15f);
                    invertVertical = true;
                }
            }
        }

        if (!QuickTurning && !invertVertical)
        {
            float dir = horizontal;
            if (vertical < 0)
                dir += vertical * 2;
            cameraMovement.FollowRotation(dir * moveAmount);
        }
    }
    public void Move(Vector3 v, Vector3 h)
    {
        if (!invertVertical)
            desiredDir = (v + h).normalized;
        else
            desiredDir = movingDir + h.normalized;

        if (vertical >= 0)
            invertVertical = false;

        if (!QuickTurning)
        {
            float m = Mathf.Abs(vertical) + Mathf.Abs(horizontal);
            desiredMoveAmount = Mathf.Clamp01(m);
        }     
    }
    public void Run()
    {
        Running = !Running;
        stepSpeed = Running ? 1 : 3;
        maxSpeed = Running ? runSpeed : moveSpeed;
        minRotationSpeed = Running ? 4 : 9;
        invertVertical = !Running ? false : invertVertical;
    }
    public void Jump()
    {
        if (Grappling)
        {
            Grappling = false;
            hookCallback();
            return;
        }

        if (Grounded)
        {
            float qt = 1;
            if(QuickTurning)
            {
                transform.rotation = Quaternion.LookRotation(desiredDir);
                rBody.velocity = transform.forward * 0.2f;
                qt = 1.4f;
            }

            rBody.AddForce(Vector3.up * jumpForce * qt, ForceMode.VelocityChange);
        }
    }
    public void Grapple(Vector3 pos, bool isDodge, float distanceMultiplier, Action callback)
    {
        Grappling = true;
        comparisonPosition = isDodge ? transform : hookBase;
        latchedPos = pos;
        latchedDir = latchedPos - comparisonPosition.position;
        grappleSpeed = Vector3.Distance(latchedPos, transform.position) / (distanceMultiplier / 2 * grappleBaseSpeed);
        hookCallback = callback;
    }

    public void ApplyExternalForce(Vector3 force, bool stun)
    {
        rBody.AddForce(force, ForceMode.VelocityChange);
    }

    private void DetectAction()
    {
    }

    public void Ragdoll()
    {
        anim.enabled = !anim.enabled;
    }

    public void SetHookSpeed(float speed)
    {
        grappleBaseSpeed = speed;
    }
}
