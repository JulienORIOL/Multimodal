using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 3f;
    
    [Header("Joystick Settings")]
    [FormerlySerializedAs("FixedLeftJoystick")]
    public FixedJoystick leftJoystick;
    public FixedJoystick rightJoystick;
    
    [Header("Gyroscope Settings")]
    public float lightGyroSensitivity = 2f;
    public float maxLightTiltAngle = 80f;
    public float lightSmoothSpeed = 5f;
    private Vector3 lightGyroOffset;

    [Header("Shake Detection Settings")]
    public float shakeDetectionThreshold = 2.0f;
    public float minShakeInterval = 0.5f;
    private float sqrShakeDetectionThreshold;
    private float timeSinceLastShake;
    private Vector3 accelerometerPrevious;
    private Queue<Vector3> accelerometerReadings;
    private const int ReadingsCount = 5;
    
    [Header("Action Buttons")]
    public Button sprintButton;
    public Button jumpButton;
    public Button attackButton;
    public Button gestureButton1;
    public Button gestureButton2;
    
    [Header("Shader Settings")]
    public Material visionMaterial; // Matériau utilisant le shader
    public float smoothEdgeMin = -0.8f; // Valeur de bord minimal
    public float smoothEdgeMax = -0.1f; // Valeur de bord maximal
    public float smoothTransitionSpeed = 2f; // Vitesse de transition
    
    [Header("Light Settings")]
    public Light spotLight;
    public float lightOffset = 1.5f;

    private Animator animator;
    private float currentSpeed = 0f;
    private bool isSprinting = false;
    private Gyroscope gyro;
    private Quaternion initialGyroRotation;
    private Transform cameraTransform;

    void Start()
    {
        // Initialisation des composants
        InitializeComponents();
        
        // Initialisation du gyroscope et de l'accéléromètre
        EnableGyroscope();
        InitializeShakeDetection();
        
        // Configuration des boutons UI
        SetupButtons();
    }

    void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Le composant Animator est manquant!");
        }
        
        if (spotLight == null)
        {
            Debug.LogError("Le composant Spotlight est manquant!");
        }
        
        cameraTransform = Camera.main.transform;
    }

    void InitializeShakeDetection()
    {
        sqrShakeDetectionThreshold = Mathf.Pow(shakeDetectionThreshold, 2);
        accelerometerReadings = new Queue<Vector3>();
        Input.gyro.enabled = true;
    }
    
    void EnableGyroscope()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            initialGyroRotation = Quaternion.Euler(180f, -transform.eulerAngles.y, 0f);
        }
        else
        {
            Debug.LogWarning("Le gyroscope n'est pas supporté sur cet appareil.");
        }
    }

    void SetupButtons()
    {
        SetupSprintButton();
        SetupJumpButton();
        SetupAttackButton();
        SetupGestureButtons();
    }

    void SetupSprintButton()
    {
        if (sprintButton != null)
        {
            EventTrigger trigger = sprintButton.gameObject.GetComponent<EventTrigger>() 
                ?? sprintButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            pointerDown.callback.AddListener((data) => { isSprinting = true; });
            trigger.triggers.Add(pointerDown);

            EventTrigger.Entry pointerUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            pointerUp.callback.AddListener((data) => { isSprinting = false; });
            trigger.triggers.Add(pointerUp);
        }
    }

    void SetupJumpButton()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(() => {
                animator.SetBool("Jump_gggb", true);
                StartCoroutine(ResetJumpAnimation());
            });
        }
    }

    void SetupAttackButton()
    {
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(() => {
                animator.SetInteger("WeaponType_int", 10);
                animator.SetInteger("Animation_int", 7);
                StartCoroutine(ResetAttackAnimation());
            });
        }
    }

    void SetupGestureButtons()
    {
        if (gestureButton1 != null)
        {
            gestureButton1.onClick.AddListener(() => animator.SetInteger("Animation_int", 4));
        }
        
        if (gestureButton2 != null)
        {
            gestureButton2.onClick.AddListener(() => animator.SetInteger("Animation_int", 5));
        }
    }

    void Update()
    {
        HandleMovement();
        HandleJoystickRotation();
        UpdateAnimator();
        UpdateSpotlight();
        DetectShake();
        UpdateShaderSmoothness();
    }

    void HandleMovement()
    {
        Vector3 direction = Vector3.forward * leftJoystick.Vertical + Vector3.right * leftJoystick.Horizontal;
    
        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * direction;
            currentSpeed = isSprinting ? runSpeed : walkSpeed;
            currentSpeed *= direction.magnitude;
            transform.position += moveDir.normalized * currentSpeed * Time.deltaTime;
        }
        else
        {
            currentSpeed = 0f;
        }
    }
    
    void HandleJoystickRotation()
    {
        if (rightJoystick.Direction.magnitude >= 0.1f)
        {
            float currentX = transform.eulerAngles.x;
            if (currentX > 180f) currentX -= 360f;

            float newX = currentX - rightJoystick.Vertical * rotationSpeed * Time.deltaTime;
            float newY = transform.eulerAngles.y + rightJoystick.Horizontal * rotationSpeed * Time.deltaTime;

            transform.rotation = Quaternion.Euler(newX, newY, 0f);
        }
    }

    void DetectShake()
    {
        Vector3 acceleration = Input.acceleration;
        
        accelerometerReadings.Enqueue(acceleration);
        if (accelerometerReadings.Count > ReadingsCount)
        {
            accelerometerReadings.Dequeue();
        }
        
        if (accelerometerPrevious != Vector3.zero)
        {
            Vector3 deltaAcceleration = acceleration - accelerometerPrevious;
            float shakeMagnitude = deltaAcceleration.sqrMagnitude;
            
            if (shakeMagnitude > sqrShakeDetectionThreshold && 
                Time.unscaledTime >= timeSinceLastShake + minShakeInterval)
            {
                DetermineShakeAnimation(deltaAcceleration);
                timeSinceLastShake = Time.unscaledTime;
            }
        }
        
        accelerometerPrevious = acceleration;
    }

    void DetermineShakeAnimation(Vector3 deltaAcceleration)
    {
        float absX = Mathf.Abs(deltaAcceleration.x);
        float absY = Mathf.Abs(deltaAcceleration.y);
        float absZ = Mathf.Abs(deltaAcceleration.z);
        
        if (absX > absY && absX > absZ)
        {
            // Secousse latérale - Attaque
            animator.SetInteger("WeaponType_int", 10);
            animator.SetInteger("Animation_int", 7);
            StartCoroutine(ResetAttackAnimation());
        }
        else if (absY > absX && absY > absZ)
        {
            if (deltaAcceleration.y > 0)
            {
                // Secousse vers le haut - Saut
                animator.SetBool("Jump_gggb", true);
                StartCoroutine(ResetJumpAnimation());
            }
            else
            {
                // Secousse vers le bas - Geste 1
                animator.SetInteger("Animation_int", 4);
                StartCoroutine(ResetGeneralAnimation());
            }
        }
        else
        {
            // Secousse avant/arrière - Geste 2
            animator.SetInteger("Animation_int", 5);
            StartCoroutine(ResetGeneralAnimation());
        }
    }
    
    void UpdateSpotlight()
    {
        if (spotLight != null)
        {
            Vector3 lightPosition = transform.position + Vector3.up * lightOffset;
            spotLight.transform.position = lightPosition;

            if (gyro != null && gyro.enabled)
            {
                Quaternion landscapeLeftRotation = Quaternion.Euler(90f, 0f, 0f);
                Quaternion rawGyroRotation = Input.gyro.attitude;
            
                Quaternion correctedGyroRotation = Quaternion.Euler(
                    -rawGyroRotation.eulerAngles.x,
                    -rawGyroRotation.eulerAngles.z,
                    rawGyroRotation.eulerAngles.y
                );

                Quaternion finalRotation = landscapeLeftRotation * correctedGyroRotation;

                Vector3 eulerAngles = finalRotation.eulerAngles;
                eulerAngles.x = Mathf.Clamp(eulerAngles.x, -maxLightTiltAngle, maxLightTiltAngle);
                eulerAngles.z = Mathf.Clamp(eulerAngles.z, -maxLightTiltAngle, maxLightTiltAngle);

                spotLight.transform.rotation = Quaternion.Slerp(
                    spotLight.transform.rotation,
                    Quaternion.Euler(eulerAngles),
                    Time.deltaTime * lightSmoothSpeed
                );
            }
            else
            {
                spotLight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed_f", currentSpeed);
        animator.SetBool("Static_b", currentSpeed == 0f);
    }

    IEnumerator ResetJumpAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Jump_gggb", false);
    }

    IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetInteger("WeaponType_int", 0);
        animator.SetInteger("Animation_int", 0);
    }

    IEnumerator ResetGeneralAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetInteger("Animation_int", 0);
    }
    
    void UpdateShaderSmoothness()
    {
        if (visionMaterial != null)
        {
            // Déterminer la valeur cible en fonction de la vitesse
            float targetSmoothness = currentSpeed > 0f ? smoothEdgeMax : smoothEdgeMin;
            
            // Obtenir la valeur actuelle de `_Smoothness`
            float currentSmoothness = visionMaterial.GetFloat("_Smoothness");

            // Interpoler vers la nouvelle valeur
            float newSmoothness = Mathf.Lerp(currentSmoothness, targetSmoothness, Time.deltaTime * smoothTransitionSpeed);

            // Mettre à jour la propriété du shader
            visionMaterial.SetFloat("_Smoothness", newSmoothness);
        }
    }
}