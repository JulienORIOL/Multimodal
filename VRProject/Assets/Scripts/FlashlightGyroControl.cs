using UnityEngine;

public class FlashlightGyroControl : MonoBehaviour
{
    [Header("Gyroscope Settings")]
    public Light spotlight;
    public float gyroSensitivity = 2f;
    public float maxTiltAngle = 30f;
    public float smoothSpeed = 5f;
   
    private Vector3 initialRotation;
    private Quaternion targetRotation;
    private bool gyroEnabled;

    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            gyroEnabled = true;
            initialRotation = spotlight.transform.eulerAngles;
        }
    }

    void LateUpdate() // Utilisez LateUpdate pour appliquer après CharacterMovement
    {
        if (!gyroEnabled) return;

        Vector3 gyroInput = Input.gyro.attitude.eulerAngles;
       
        float tiltX = Mathf.Clamp(gyroInput.x * gyroSensitivity, -maxTiltAngle, maxTiltAngle);
        float tiltY = Mathf.Clamp(gyroInput.y * gyroSensitivity, -maxTiltAngle, maxTiltAngle);

        targetRotation = Quaternion.Euler(
            initialRotation.x + tiltX,
            initialRotation.y + tiltY,
            0f
        );

        spotlight.transform.rotation = Quaternion.Slerp(
            spotlight.transform.rotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }
}