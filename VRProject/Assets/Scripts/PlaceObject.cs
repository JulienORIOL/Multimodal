using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectPlacer : MonoBehaviour
{
    // Bouton de placement
    public Button placementButton;
    
    // Menu de sélection d'objets
    public GameObject selectionMenu;
    
    // Liste des objets placables
    public GameObject[] placableObjects;
    
    // Boutons de sélection d'objets
    public Button[] objectSelectionButtons;

    // Objet actuellement sélectionné
    private GameObject selectedObject;
    private GameObject previewObject;

    
    // États du placement
    private enum PlacementState
    {
        Idle,
        WaitingForPlacement
    }
    private PlacementState currentState = PlacementState.Idle;

    void Start()
    {
        // Configurer le bouton de placement
        placementButton.onClick.AddListener(StartPlacementMode);
        
        // Configurer les boutons de sélection d'objets
        for (int i = 0; i < objectSelectionButtons.Length; i++)
        {
            int index = i;
            objectSelectionButtons[i].onClick.AddListener(() => SelectObject(index));
            
            // Nommer le bouton avec le nom du prefab (version sécurisée)
            // if (i < placableObjects.Length)
            // {
            //     TextMeshProUGUI tmpText = objectSelectionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            //     if (tmpText != null)
            //     {
            //         tmpText.text = placableObjects[i].name;
            //     }
            //     else
            //     {
            //         Text text = objectSelectionButtons[i].GetComponentInChildren<Text>();
            //         if (text != null)
            //         {
            //             text.text = placableObjects[i].name;
            //         }
            //     }
            // }
        }
        
        // Cacher le menu initialement
        selectionMenu.SetActive(false);
    }

    void Update()
    {
        // Vérifier si nous sommes en mode placement ET un objet est sélectionné
        // if (currentState == PlacementState.WaitingForPlacement && selectedObject != null)
        // {

        //     // Détecter le clic souris
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         Debug.Log("Cliquez dans la scène pour placer l'objet : " + selectedObject.name);
        //         PlaceSelectedObject();
        //     }
        // }

        if (currentState == PlacementState.WaitingForPlacement && previewObject != null)
        {
            // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // RaycastHit hit;
            // if (Physics.Raycast(ray, out hit))
            // {
            //     previewObject.transform.position = hit.point;
            // }

            // // Placer l'objet en cliquant
            // if (Input.GetMouseButtonUp(0)) // Lorsqu'on relâche le clic gauche
            // {
            //     PlaceSelectedObject();
            // }
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Relâchez le clic pour placer l'objet : " + selectedObject.name);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Vérifier si le rayon touche un plan ou le sol
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log("Mouse position : " + hit.point);
                }
            }
            else if (Input.GetMouseButtonUp(0)) // Lorsqu'on relâche le clic gauche
            {
                PlaceSelectedObject();
            }
        }
    }

    void StartPlacementMode()
    {
        if (currentState == PlacementState.WaitingForPlacement)
        {
            ResetPlacement();
            return;
        }
        else
        {
            currentState = PlacementState.WaitingForPlacement;
            selectionMenu.SetActive(true);
        }
        
        // Réinitialiser la sélection
        selectedObject = null;
    }

    public void SelectObject(int objectIndex)
    {
        // Vérifier l'index
        if (objectIndex < 0 || objectIndex >= placableObjects.Length)
        {
            Debug.LogError("Index d'objet invalide : " + objectIndex);
            return;
        }

        // Sélectionner l'objet à partir de la liste
        selectedObject = placableObjects[objectIndex];
        Debug.Log("Objet sélectionné : " + selectedObject.name);

        if (previewObject != null) Destroy(previewObject);
        previewObject = Instantiate(selectedObject);
        // previewObject.GetComponent<Collider>().enabled = false; // Désactiver les collisions
        SetPreviewMaterial(previewObject); // Définir un matériau transparent
        
        // Fermer le menu de sélection
        selectionMenu.SetActive(false);
    }

    private void SetPreviewMaterial(GameObject obj)
    {
        Material previewMaterial = new Material(Shader.Find("Standard"));
        previewMaterial.color = new Color(1, 1, 1, 0.5f); // Semi-transparent
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = previewMaterial;
        }
    }

    void PlaceSelectedObject()
    {
        // Créer un rayon depuis la souris
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Vérifier si le rayon touche un plan ou le sol
        if (Physics.Raycast(ray, out hit))
        {
            // Placer l'objet sélectionné à l'endroit du clic
            if (selectedObject != null)
            {
                Instantiate(selectedObject, hit.point, Quaternion.identity);
                Debug.Log("Objet placé à la position : " + hit.point);
            }

            // Réinitialiser l'état
            ResetPlacement();
        }
    }

    public void CancelPlacement()
    {
        // Annuler le placement
        ResetPlacement();
    }

    void ResetPlacement()
    {
        currentState = PlacementState.Idle;
        selectionMenu.SetActive(false);
        selectedObject = null;
    }
}