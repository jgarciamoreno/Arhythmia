using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraMovement : MonoBehaviour {

    [SerializeField]
    protected PlayerTarget playerToTarget;
    protected enum PlayerTarget { Cam_P1, Cam_P2, Cam_P3, Cam_P4 }

    [Header("Input")]
    public float horizontal;
    public float vertical;
    private bool canHandleCamera = true;
    #region Camera Movement Vars
    [Header("Movement Vars")]
    public float turnSpeed = 1.5f;
    private float lockSpeed = 15f;
    public float turnsmoothing = .05f;
    private float tiltMax = 75f;
    private float tiltMin = 45f;

    private float lookAngle;
    private float tiltAngle;

    private float smoothX = 0;
    private float smoothY = 0;
    private float smoothXvelocity = 0;
    private float smoothYvelocity = 0;

    public float lockedRotationThreshold = 0.05f;
    public float HcamResetSpeed = 6;
    public float VcamResetSpeed = 6;
    #endregion
    #region Collision Vars
    [Header("Distance From Char")]
    public float distanceFromTarget = 3f;
    public float adjustmentDistance = -5f;

    public Vector3 targetPosOffset = Vector3.zero;
    private Vector3 targetPos = Vector3.zero;
    [HideInInspector]
    public Vector3 destination = Vector3.zero;
    [HideInInspector]
    public Vector3 adjustedDestination = Vector3.zero;
    private Vector3 camVel = Vector3.zero;
    #endregion
    #region References
    public Transform lookAtTarget;
    public Transform orbitTarget;

    private Character character;
    private Camera cam;
    private Transform pivot;

    private Transform HUDHolder;
    private Image enemyHP;
    private Image enemyCurrentHP;
    private Slider playerHP;
    private Slider playerShield;

    private RawImage hitImage;
    private Color hitColor;
    private RawImage hitDir;
    private float hitDirOffset;

    private RawImage passive;
    private RectTransform skillHolder;
    private Image skillOne;
    private Image skillTwo;
    private Image skillThree;

    private GameObject[] accuracyIndicators = new GameObject[5];
    private GameObject currentAcurracyActive;
    private Text combo;
    private float accuracyTime;
    //private int comboCounter = 0;

    //private bool accuracyStarted = false;
    #endregion
    public CollisionHandler collision = new CollisionHandler();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        SetTarget(GameObject.FindGameObjectWithTag(playerToTarget.ToString()).transform);
        
        cam = GetComponentInChildren<Camera>();
        pivot = cam.transform.parent;
        collision.Initialize(cam);
    }

    private void OnEnable()
    {
        Enemy.OnDamageTaken += UpdateEnemyBar;
    }
    private void OnDisable()
    {
        Enemy.OnDamageTaken -= UpdateEnemyBar;
    }

    public void OnPlayerLock(Enemy targetEnemy, bool active)
    {
        if (!active)
        {
            targetPosOffset = new Vector3(0, 1.3f, 0);

            if (currentAcurracyActive != null)
            {
                currentAcurracyActive.SetActive(false);
                currentAcurracyActive = null;
            }
        }
        else
        {
            targetPosOffset = new Vector3(0.5f, 1.6f, 0);
            //parentHUD.transform.position = cam.WorldToScreenPoint(targetEnemy.lockPosition.position);
            //enemyHP.transform.position = Vector2.zero;
            if (targetEnemy)
                UpdateEnemyBar(character.lockedEnemy.currentHealthPoints, character.lockedEnemy.maxHealthPoints);
        }

        //enemyHP.gameObject.SetActive(active);
        HUDHolder.gameObject.SetActive(active);
    }
    public void OnPlayerTakeDamage()
    {
        if (character.currentShiledPoints > 0)
            playerShield.value = character.currentShiledPoints / character.maxShieldPoints;
        else
            playerHP.value = character.currentHealthPoints / character.maxHealthPoints;

        StartCoroutine(Hit());
    }
    public void OnPlayerHeal()
    {
        playerHP.value = character.currentHealthPoints / character.maxHealthPoints;
    }

    private IEnumerator Hit()
    {
        float time = 0.5f;
        while (time > 0)
        {
            hitColor.a = time;
            hitImage.color = hitColor;
            time -= Time.deltaTime;
            yield return null;
        }

        yield break;
    }

    public void UpdatePlayerBar()
    {
        playerShield.value = character.currentShiledPoints / character.maxShieldPoints;
        playerHP.value = character.currentHealthPoints / character.maxHealthPoints;
    }
    public void UpdateEnemyBar(float currentHealth, float maxHealth)
    {
        //enemyCurrentHP.fillAmount = currentHealth / maxHealth;
    }
    public void UpdateEnemyBar(Enemy e, float currentHealth, float maxHealth)
    {
        if (e == character.lockedEnemy)
            enemyCurrentHP.fillAmount = currentHealth / maxHealth;
    }

    protected void Start()
    {
        if (lookAtTarget != null)
        {
            //lookAtTarget.GetComponentInParent<Locomotion>().SetupCamera(cam.transform, lookAtTarget);
            character = lookAtTarget.GetComponentInParent<Character>();
            character.SetupCamera(this);

            lookAngle += character.transform.eulerAngles.y - transform.eulerAngles.y;
        }

        collision.UpdateCameraClipPoints(cam.transform.position + targetPosOffset, cam.transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, cam.transform.rotation, ref collision.desiredCameraClipPoints);
    }

    private void FixedUpdate()
    {
        FollowTarget();

        if (!character.isLockedOnEnemy)
            HandleRotationMovement();
        else
        {
            HandleLockedRotation(lockSpeed);
            HandleTargetHUD();
        }

        collision.UpdateCameraClipPoints(cam.transform.position, cam.transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, cam.transform.rotation, ref collision.desiredCameraClipPoints);

        collision.CheckColliding(targetPos);
        adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(targetPos);
    }

    private void FollowTarget()
    {
        targetPos = orbitTarget.position + Vector3.up * targetPosOffset.y + Vector3.forward * targetPosOffset.z + transform.TransformDirection(Vector3.right * targetPosOffset.x);
        destination = Quaternion.Euler(tiltAngle, lookAngle, 0) * -Vector3.forward * distanceFromTarget;
        destination += targetPos;

        if (collision.colliding)
        {
            adjustedDestination = Quaternion.Euler(tiltAngle, lookAngle, 0) * -Vector3.forward * adjustmentDistance;
            adjustedDestination += targetPos;

            transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, ref camVel, 0.2f);
        }
        else
        {
            if (canHandleCamera)
                transform.position = destination;//transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, 0.1f);
            else
                transform.position = Vector3.Slerp(transform.position, destination, lockSpeed * Time.deltaTime);
        }
    }
    public void FollowRotation(float rot)
    {
        lookAngle += rot / 2;
    }

    private void HandleRotationMovement()
    {
        if (!canHandleCamera)
            return;

        if (turnsmoothing > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, horizontal, ref smoothXvelocity, turnsmoothing);
            smoothY = Mathf.SmoothDamp(smoothY, -vertical, ref smoothYvelocity, turnsmoothing);
        }
        else
        {
            smoothX = horizontal;
            smoothY = -vertical;
        }

        lookAngle += smoothX * turnSpeed;
        transform.rotation = Quaternion.Euler(0, lookAngle, 0);
        lookAngle = transform.eulerAngles.y;

        tiltAngle -= smoothY * turnSpeed;
        tiltAngle = Mathf.Clamp(tiltAngle, -tiltMin, tiltMax);

        pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
    }
    private void HandleLockedRotation(float speed)
    {
        Vector3 lookPos = character.lockedEnemy.lockPosition.position;
        Vector3 lookDir = lookPos - transform.position;
        lookDir.y = 0;

        Quaternion rot = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.time * speed);
        lookAngle = transform.eulerAngles.y;
        pivot.localRotation = Quaternion.Slerp(pivot.localRotation, Quaternion.identity, Time.time * speed);
        tiltAngle = pivot.eulerAngles.x;
    }

    private void HandleTargetHUD()
    {
        HandleHitDirection();
    }
    private void HandleHitDirection()
    {
    }

    public void ResetCamera(float dir)
    {
        StartCoroutine(CameraReset(dir, HcamResetSpeed));
    }
    public void ResetCamera(float dir, float speed)
    {
        StartCoroutine(CameraReset(dir, speed));
    }
    private IEnumerator CameraReset(float dir, float speed)
    {
        float d = Mathf.Abs(dir - lookAngle);
        float t = 0;
        float time = speed == HcamResetSpeed ? 0.5f : 1f;
        while ((Mathf.Abs(d) > 0.2f || Mathf.Abs(tiltAngle) > 0.2f) && t < time)
        {
            t += Time.deltaTime;
            canHandleCamera = false;
            d = dir - lookAngle;
            if (d > 180) d -= 360; else if (d < -180) d += 360;
            lookAngle += d / speed;
            tiltAngle += -tiltAngle / speed;
            transform.rotation = Quaternion.Euler(0, lookAngle, 0);
            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
            yield return new WaitForFixedUpdate();
        }

        canHandleCamera = true;
    }

    public void SetTarget(Transform target)
    {
        lookAtTarget = target;
        orbitTarget = target.parent.GetComponent<Transform>();
    }
    public void SetHUDObjects(Transform p, int pn)
    {
        HUDHolder = p;

        playerHP = HUDHolder.Find("HP").GetComponent<Slider>();
        playerShield = HUDHolder.Find("Shield").GetComponent<Slider>();
        hitImage = HUDHolder.Find("hitImage").GetComponent<RawImage>();
        hitColor = hitImage.color;//Get color of image
        hitColor.a = 0;//Kill alpha
        hitImage.color = hitColor;//Reinsert new color without alpha

        skillHolder = HUDHolder.Find("SkillzP" + pn).GetChild(0).GetComponent<RectTransform>();
        skillHolder.localPosition = new Vector3(-Screen.width / 6, 50, 0);
        passive = skillHolder.Find("Passive").GetComponent<RawImage>();
        skillOne = skillHolder.Find("SkillOne").GetComponentInChildren<Image>();
        skillTwo = skillHolder.Find("SkillTwo").GetComponentInChildren<Image>();
        skillThree = skillHolder.Find("SkillThree").GetComponentInChildren<Image>();
    }

    public void MoveCamera(float time)
    {
        canHandleCamera = false;
    }
    private IEnumerator Countdown(float time)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            yield return null;
        }

        canHandleCamera = true;
    }

    public Camera GetCamera()
    {
        return cam;
    }

    public void SetSkillOnCooldown(int skill, float fillValue)
    {
        switch (skill)
        {
            case 0:
                passive.color = fillValue == 1 ? Color.green : Color.white;
                break;
            case 1:
                skillOne.fillAmount = fillValue;
                break;
            case 2:
                skillTwo.fillAmount = fillValue;
                break;
            case 3:
                skillThree.fillAmount = fillValue;
                break;
        }
    }
}


[System.Serializable]
public class CollisionHandler
{
    public LayerMask collisionLayer;

    [HideInInspector]
    public bool colliding = false;
    [HideInInspector]
    public Vector3[] adjustedCameraClipPoints;
    [HideInInspector]
    public Vector3[] desiredCameraClipPoints;

    private Camera camera;

    public void Initialize(Camera cam)
    {
        camera = cam;
        adjustedCameraClipPoints = new Vector3[5];
        desiredCameraClipPoints = new Vector3[5];
    }

    public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
    {
        if (!camera)
            return;

        //clear intoArray
        intoArray = new Vector3[5];

        float z = camera.nearClipPlane;
        float x = Mathf.Tan(camera.fieldOfView / 3.41f) * z;
        float y = x / camera.aspect;

        //top left
        intoArray[0] = (atRotation * new Vector3(-x, y, z) + cameraPosition);//added and rotated the point relative to camera
                                                                             //top right
        intoArray[1] = (atRotation * new Vector3(x, y, z) + cameraPosition);//added and rotated the point relative to camera
                                                                            //bottom left
        intoArray[2] = (atRotation * new Vector3(-x, -y, z) + cameraPosition);//added and rotated the point relative to camera
                                                                              //bottom right
        intoArray[3] = (atRotation * new Vector3(x, -y, z) + cameraPosition);//added and rotated the point relative to camera
                                                                             //camera's position
        intoArray[4] = cameraPosition - camera.transform.forward;
    }

    private bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
    {
        for (int i = 0; i < clipPoints.Length; i++)
        {
            Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
            float distance = Vector3.Distance(clipPoints[i], fromPosition);

            if (Physics.Raycast(ray, distance, collisionLayer))
                return true;
        }

        return false;
    }

    public float GetAdjustedDistanceWithRayFrom(Vector3 from)
    {
        float distance = -1;

        for (int i = 0; i < desiredCameraClipPoints.Length; i++)
        {
            Ray ray = new Ray(from, desiredCameraClipPoints[i] - from);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (distance == -1)
                    distance = hit.distance;
                else
                {
                    if (hit.distance < distance)
                        distance = hit.distance;
                }
            }
        }

        if (distance == -1)
            return 0;
        else
            return distance;
    }

    public void CheckColliding(Vector3 targetPosition)
    {
        if (CollisionDetectedAtClipPoints(desiredCameraClipPoints, targetPosition))
            colliding = true;
        else
            colliding = false;
    }
}