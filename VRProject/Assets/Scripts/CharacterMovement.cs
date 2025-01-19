using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 90f;
    
    [Header("Joystick Settings")]
    public FloatingJoystick variableJoystick;
    
    [Header("Gyroscope Settings")]
    public float gyroRotationSpeed = 2f;
    public bool invertGyroX = false;
    public bool invertGyroY = false;
    
    [Header("Action Buttons")]
    public Button sprintButton;
    public Button jumpButton;
    public Button attackButton;
    public Button gestureButton1;
    public Button gestureButton2;

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
            initialGyroRotation = Quaternion.Euler(90f, 0f, 0f);
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
        HandleGyroscopeRotation();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        // Obtenir la direction du joystick
        Vector3 direction = Vector3.forward * variableJoystick.Vertical + Vector3.right * variableJoystick.Horizontal;
        
        // Si le joystick est utilisé
        if (direction.magnitude >= 0.1f)
        {
            // Calculer l'angle de rotation en fonction de la direction du joystick
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            
            // Appliquer la rotation au personnage
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Calculer la direction du mouvement en tenant compte de la rotation de la caméra
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Calculer la vitesse en fonction du sprint et de l'intensité du joystick
            currentSpeed = isSprinting ? runSpeed : walkSpeed;
            currentSpeed *= direction.magnitude; // Ajuster la vitesse en fonction de l'intensité du joystick

            // Déplacer le personnage
            transform.position += moveDir.normalized * currentSpeed * Time.deltaTime;
        }
        else
        {
            currentSpeed = 0f;
        }
    }

    void HandleGyroscopeRotation()
    {
        if (gyro != null && gyro.enabled)
        {
            // Conversion des données du gyroscope en rotation de la caméra
            Quaternion gyroRotation = gyro.attitude;
            Quaternion rotationOffset = Quaternion.Euler(
                invertGyroY ? -90 : 90, 
                invertGyroX ? -gyro.attitude.eulerAngles.x : gyro.attitude.eulerAngles.x, 
                0
            );
            cameraTransform.rotation = initialGyroRotation * gyroRotation * rotationOffset;
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