using UnityEngine;
using UnityEngine.UI;

public class JoystickLightController : MonoBehaviour 
{
    [Header("Light Control")]
    public Light spotLight;                    
    public VariableJoystick lightJoystick;     
    public float minIntensity = 0f;            
    public float maxIntensity = 2f;            
    public float intensityChangeSpeed = 1f;    
    public float deadzone = 0.1f;

    [Header("Color Buttons")]
    public Button redButton;
    public Button greenButton;
    public Button blueButton;
    
    private Image redButtonImage;
    private Image greenButtonImage;
    private Image blueButtonImage;

    private bool redEnabled = true;
    private bool greenEnabled = true;
    private bool blueEnabled = true;
    private Vector3 colorModification = Vector3.zero;

    void Start()
    {
        redButtonImage = redButton.gameObject.GetComponent<Image>() ?? redButton.gameObject.AddComponent<Image>();
        greenButtonImage = greenButton.gameObject.GetComponent<Image>() ?? greenButton.gameObject.AddComponent<Image>();
        blueButtonImage = blueButton.gameObject.GetComponent<Image>() ?? blueButton.gameObject.AddComponent<Image>();

        SetupColorButtons();
        UpdateLightColor();
    }

    void SetupColorButtons()
    {

        redButton.onClick.AddListener(() => ToggleColor(ref redEnabled, redButtonImage, Color.red));
        greenButton.onClick.AddListener(() => ToggleColor(ref greenEnabled, greenButtonImage, Color.green));
        blueButton.onClick.AddListener(() => ToggleColor(ref blueEnabled, blueButtonImage, Color.blue));

        UpdateButtonColors();
    }

    void ToggleColor(ref bool colorEnabled, Image buttonImage, Color baseColor)
    {
        colorEnabled = !colorEnabled;
        buttonImage.color = colorEnabled ? baseColor : new Color(0.3f, 0.3f, 0.3f);
        UpdateLightColor();
    }

    void UpdateButtonColors()
    {
        redButtonImage.color = redEnabled ? Color.red : new Color(0.3f, 0.3f, 0.3f);
        greenButtonImage.color = greenEnabled ? Color.green : new Color(0.3f, 0.3f, 0.3f);
        blueButtonImage.color = blueEnabled ? Color.blue : new Color(0.3f, 0.3f, 0.3f);
    }

    void UpdateLightColor()
    {
        Color newColor = new Color(
            redEnabled ? 1f : 0f,
            greenEnabled ? 1f : 0f,
            blueEnabled ? 1f : 0f
        );
        
        if (newColor == Color.black) newColor = Color.white;
        spotLight.color = newColor;
    }

    private void Update()
    {
        if (Mathf.Abs(lightJoystick.Vertical) > deadzone)
        {
            float intensityChange = lightJoystick.Vertical * intensityChangeSpeed * Time.deltaTime;
            spotLight.intensity = Mathf.Clamp(spotLight.intensity + intensityChange, minIntensity, maxIntensity);
        }
    }
}