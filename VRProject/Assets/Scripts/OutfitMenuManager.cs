using UnityEngine;
using UnityEngine.UI;

public class OutfitMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject outfitPanel;        // R�f�rence vers votre panel
    public Button toggleMenuButton;       // R�f�rence vers votre bouton
    public Button outfitButtonPrefab;

    [Header("Character References")]
    public SkinnedMeshRenderer characterMesh;
    public Material[] outfitMaterials;
    public Sprite[] outfitPreviews;
    public AudioClip changeOutfitSound;
    private AudioSource audioSource;

    private bool isMenuVisible = false;   // �tat du menu

    void Start()
    {
        // S'assurer que le panel est cach� au d�marrage
        outfitPanel.SetActive(false);

        // Ajouter le listener pour le bouton
        toggleMenuButton.onClick.AddListener(ToggleMenu);
        CreateOutfitButtons();

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("AudioSource manquant sur l'objet !");
        }
    }

    void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;
        outfitPanel.SetActive(isMenuVisible);
        outfitButtonPrefab.gameObject.SetActive(false);
    }

    void CreateOutfitButtons()
    {
        // Cr�er un bouton pour chaque material
        for (int i = 0; i < outfitMaterials.Length; i++)
        {
            Button newButton = Instantiate(outfitButtonPrefab, outfitPanel.transform);
            int index = i; // N�cessaire pour la capture dans le lambda
            newButton.onClick.AddListener(() => ChangeOutfit(index));
            newButton.gameObject.SetActive(true);

            Image buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null && i < outfitPreviews.Length)
            {
                buttonImage.sprite = outfitPreviews[i];
            }

            // Optionnel : changer le texte du bouton
            TMPro.TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Tenue {i + 1}";
            }
        }
    }

    void ChangeOutfit(int materialIndex)
    {
        if (materialIndex >= 0 && materialIndex < outfitMaterials.Length)
        {
            Material[] materials = characterMesh.materials;
            materials[0] = outfitMaterials[materialIndex];  // Change le premier material
            characterMesh.materials = materials;
            
            if (changeOutfitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(changeOutfitSound);
            }
            else
            {
                Debug.LogWarning("Aucun son ou AudioSource configuré pour le changement d'outfit !");
            }
        }
    }
}