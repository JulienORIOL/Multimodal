using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 3f;
    
    [FormerlySerializedAs("FixedLeftJoystick")] [Header("Joystick Settings")]
    public FixedJoystick leftJoystick;
    public FixedJoystick rightJoystick;
    
    [Header("Gyroscope Settings")]
    public float lightGyroSensitivity = 2f;
    public float maxLightTiltAngle = 30f;
    public float lightSmoothSpeed = 5f;
    private Vector3 lightGyroOffset;

    
    [Header("Action Buttons")]
    public Button sprintButton;
    public Button jumpButton;
    public Button attackButton;
    public Button gestureButton1;
    public Button gestureButton2;
    
    [Header("Light Settings")]
    public Light spotLight;                    // Référence vers votre spotlight
    public float lightOffset = 1.5f;           // Distance au-dessus du personnage

    private Animator animator;
    private float currentSpeed = 0f;
    private bool isSprinting = false;
    private Gyroscope gyro;
    private Quaternion initialGyroRotation;
    private Transform cameraTransform;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Le composant Animator est manquant !");
        }
        if (spotLight == null)
        {
            Debug.LogError("Le composant Spotlight est manquant !");
        }
        // Récupérer la caméra principale
        cameraTransform = Camera.main.transform;

        // Initialisation du gyroscope
        EnableGyroscope();

        // Configuration des boutons
        SetupButtons();
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
        if (sprintButton != null)
        {
            EventTrigger trigger = sprintButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = sprintButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { isSprinting = true; });
            trigger.triggers.Add(pointerDown);

            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { isSprinting = false; });
            trigger.triggers.Add(pointerUp);
        }

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(() => {
                animator.SetBool("Jump_gggb", true);
                StartCoroutine(ResetJumpAnimation());
            });
        }

        if (attackButton != null)
        {
            attackButton.onClick.AddListener(() => {
                animator.SetInteger("WeaponType_int", 10);
                animator.SetInteger("Animation_int", 7);
                StartCoroutine(ResetAttackAnimation());
            });
        }

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
        // HandleGyroscopeRotation();
        UpdateAnimator();
        UpdateSpotlight();      // Ajoutez cet appel
    }

    void HandleMovement()
    {
        Vector3 direction = Vector3.forward * leftJoystick.Vertical + Vector3.right * leftJoystick.Horizontal;
    
        if (direction.magnitude >= 0.1f)
        {
           // Le déplacement suit la direction du joystick
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
            // Convertir les angles d'Euler actuels
            float currentX = cameraTransform.eulerAngles.x;
            if (currentX > 180f) currentX -= 360f;

            // Appliquer les rotations
            float newX = currentX - rightJoystick.Vertical * rotationSpeed * Time.deltaTime;
            float newY = cameraTransform.eulerAngles.y + rightJoystick.Horizontal * rotationSpeed * Time.deltaTime;

            // Appliquer la rotation
            cameraTransform.rotation = Quaternion.Euler(newX, newY, 0f);
        }
    }
    
    void UpdateSpotlight()
    {
        if (spotLight != null)
        {
            // Position de la lumière
            Vector3 lightPosition = transform.position + Vector3.up * lightOffset;
            spotLight.transform.position = lightPosition;
        
            // Rotation de base (suit la caméra)
            Quaternion baseRotation = Quaternion.Euler(
                cameraTransform.eulerAngles.x,
                cameraTransform.eulerAngles.y,
                0f
            );

            // Ajout de l'offset du gyroscope
            if (gyro != null && gyro.enabled)
            {
                Vector3 gyroInput = gyro.attitude.eulerAngles;
                Vector3 targetOffset = new Vector3(
                    Mathf.Clamp(gyroInput.x * lightGyroSensitivity, -maxLightTiltAngle, maxLightTiltAngle),
                    Mathf.Clamp(gyroInput.y * lightGyroSensitivity, -maxLightTiltAngle, maxLightTiltAngle),
                    0f
                );

                lightGyroOffset = Vector3.Lerp(lightGyroOffset, targetOffset, Time.deltaTime * lightSmoothSpeed);
                spotLight.transform.rotation = baseRotation * Quaternion.Euler(lightGyroOffset);
            }
            else
            {
                spotLight.transform.rotation = baseRotation;
            }
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed_f", currentSpeed);
        animator.SetBool("Static_b", currentSpeed == 0f);
    }

    System.Collections.IEnumerator ResetJumpAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Jump_gggb", false);
    }

    System.Collections.IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetInteger("WeaponType_int", 0);
        animator.SetInteger("Animation_int", 0);
    }
}