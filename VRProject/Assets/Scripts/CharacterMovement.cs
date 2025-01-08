using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float walkSpeed = 3;
    public float runSpeed = 6f;
    public float rotationSpeed = 90f;

    private Animator animator;
    private float currentSpeed = 0f; // Vitesse actuelle du personnage

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Le composant Animator est manquant !");
        }

        rotationSpeed = 90f;
    }

    void Update()
    {
        HandleMovement();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.W))
        {
            currentSpeed = isSprinting ? runSpeed : walkSpeed;
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            currentSpeed = -walkSpeed;
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }
        else
        {
            currentSpeed = 0f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0f, -rotationSpeed * Time.deltaTime, 0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }

        if (Input.GetKey(KeyCode.Space)) {
            animator.SetBool("Jump_b", true);
        } else {
            animator.SetBool("Jump_b", false);
        }

        if (Input.GetKey(KeyCode.G)) {
            animator.SetInteger("WeaponType_int", 10);
        } else {
            animator.SetInteger("WeaponType_int", 0);
        }

        if (Input.GetKey(KeyCode.H)) {
            animator.SetInteger("Animation_int", 4);
        } else if (Input.GetKey(KeyCode.K)) {
            animator.SetInteger("Animation_int", 5);
        } else if (Input.GetKey(KeyCode.J)) {
            animator.SetInteger("Animation_int", 7);
        } else if (Input.GetKey(KeyCode.L)) {
            animator.SetInteger("Animation_int", 9);
        } else {
            animator.SetInteger("Animation_int", 0);
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed_f", currentSpeed);
        animator.SetBool("Static_b", currentSpeed == 0f);
    }
}
