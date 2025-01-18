using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectPlacer : MonoBehaviour
{
    public Button placementButton;
    public GameObject selectionMenu;
    public GameObject replacementMenu;  // Nouveau menu pour le remplacement
    public GameObject[] placableObjects;
    public Button[] objectSelectionButtons;
    public Button deleteButton;  // Bouton pour supprimer l'objet
    
    private GameObject selectedObject;
    private GameObject previewObject;
    private GameObject objectToReplace;  // Référence à l'objet à remplacer
    
    private enum PlacementState
    {
        Idle,
        WaitingForPlacement,
        WaitingForReplacement
    }
    private PlacementState currentState = PlacementState.Idle;

    void Start()
    {
        placementButton.onClick.AddListener(StartPlacementMode);
        
        // Configuration des boutons de sélection
        for (int i = 0; i < objectSelectionButtons.Length; i++)
        {
            int index = i;
            objectSelectionButtons[i].onClick.AddListener(() => SelectObject(index));
            
            if (i < placableObjects.Length)
            {
                TextMeshProUGUI tmpText = objectSelectionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = placableObjects[i].name;
                }
                else
                {
                    Text text = objectSelectionButtons[i].GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text = placableObjects[i].name;
                    }
                }
            }
        }

        // Configuration du bouton de suppression
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(DeleteSelectedObject);
        }
        
        selectionMenu.SetActive(false);
        replacementMenu.SetActive(false);
        deleteButton.gameObject.SetActive(false);

    }

    void Update()
    {
        if (currentState == PlacementState.WaitingForPlacement && previewObject != null)
        {
            HandlePlacementPreview();
        }
        else if (currentState == PlacementState.Idle)
        {
            HandleObjectSelection();
        }
    }

    private void HandlePlacementPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
            
        if (Physics.Raycast(ray, out hit))
        {
            previewObject.transform.position = hit.point;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlaceSelectedObject();
        }
    }

    private void HandleObjectSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Vérifier si nous avons cliqué sur un objet placé
                GameObject clickedObject = hit.collider.gameObject;
                foreach (GameObject placableObject in placableObjects)
                {
                    if (clickedObject.name.Contains(placableObject.name))
                    {
                        ShowReplacementMenu(clickedObject);
                        break;
                    }
                }
            }
        }
    }

    private void ShowReplacementMenu(GameObject clickedObject)
    {
        objectToReplace = clickedObject;
        currentState = PlacementState.WaitingForReplacement;
        replacementMenu.SetActive(true);
        deleteButton.gameObject.SetActive(true);
    }

    void StartPlacementMode()
    {
        if (currentState == PlacementState.WaitingForPlacement)
        {
            CancelPlacement();
            return;
        }
        
        currentState = PlacementState.WaitingForPlacement;
        selectionMenu.SetActive(true);
        selectedObject = null;
    }

    public void SelectObject(int objectIndex)
    {
        if (objectIndex < 0 || objectIndex >= placableObjects.Length)
        {
            Debug.LogError("Index d'objet invalide : " + objectIndex);
            return;
        }

        selectedObject = placableObjects[objectIndex];
        Debug.Log("Objet sélectionné : " + selectedObject.name);

        if (previewObject != null) 
        {
            Destroy(previewObject);
        }

        if (currentState == PlacementState.WaitingForReplacement)
        {
            ReplaceObject();
        }
        else
        {
            selectionMenu.SetActive(false);
            previewObject = Instantiate(selectedObject);
            previewObject.GetComponent<Collider>().enabled = false;
            SetPreviewMaterial(previewObject);
        }
    }

    private void ReplaceObject()
    {
        if (objectToReplace != null && selectedObject != null)
        {
            Vector3 position = objectToReplace.transform.position;
            Quaternion rotation = objectToReplace.transform.rotation;
            Destroy(objectToReplace);
            Instantiate(selectedObject, position, rotation);
        }

        CancelReplacement();
    }

    private void DeleteSelectedObject()
    {
        if (objectToReplace != null)
        {
            Destroy(objectToReplace);
            CancelReplacement();
        }
    }

    private void CancelReplacement()
    {
        replacementMenu.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        currentState = PlacementState.Idle;
        objectToReplace = null;
    }

    private void SetPreviewMaterial(GameObject obj)
    {
        Material previewMaterial = new Material(Shader.Find("Standard"));
        previewMaterial.color = new Color(1, 1, 1, 0.5f);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = previewMaterial;
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }
    }

    void PlaceSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (selectedObject != null)
            {
                Instantiate(selectedObject, hit.point, Quaternion.identity);
                Debug.Log("Objet placé à la position : " + hit.point);
            }
            CancelPlacement();
        }
    }

    public void CancelPlacement()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        currentState = PlacementState.Idle;
        selectionMenu.SetActive(false);
        selectedObject = null;
    }
}