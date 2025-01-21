using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Linq;

public class ImageTracker : MonoBehaviour
{
    [System.Serializable]
    public struct PrefabImagePair
    {
        public string imageName;
        public GameObject prefab;
    }

    [SerializeField] private PrefabImagePair[] prefabImagePairs;
    [SerializeField] private GameObject infoPanelPrefab;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float rotationSmoothSpeed = 5f;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> prefabsDictionary = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> roomVisibility = new Dictionary<string, bool>();
    private Dictionary<string, Vector3> targetPositions = new Dictionary<string, Vector3>();
    private Dictionary<string, Quaternion> targetRotations = new Dictionary<string, Quaternion>();

    private string currentTimeFilter = "";
    private string currentSpecFilter = "";
    private string currentTransportFilter = "";

    private void Start()
    {
        foreach (var pair in prefabImagePairs)
        {
            prefabsDictionary.Add(pair.imageName, pair.prefab);
            roomVisibility[pair.imageName] = true;
        }
    }

    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void Update()
    {
        // Smooth movement update
        foreach (var entry in spawnedObjects)
        {
            string imageName = entry.Key;
            GameObject obj = entry.Value;

            if (targetPositions.ContainsKey(imageName) && targetRotations.ContainsKey(imageName))
            {
                // Position smoothing
                obj.transform.position = Vector3.Lerp(
                    obj.transform.position,
                    targetPositions[imageName],
                    Time.deltaTime * smoothSpeed
                );

                // Rotation smoothing
                obj.transform.rotation = Quaternion.Lerp(
                    obj.transform.rotation,
                    targetRotations[imageName],
                    Time.deltaTime * rotationSmoothSpeed
                );
            }
        }
    }

    public void UpdateFilters(string timeFilter, string specFilter, string transportFilter)
    {
        currentTimeFilter = timeFilter;
        currentSpecFilter = specFilter;
        currentTransportFilter = transportFilter;

        // Mettre à jour la visibilité de toutes les salles
        foreach (string roomName in prefabsDictionary.Keys)
        {
            bool shouldBeVisible = CheckRoomMatchesFilters(roomName);
            roomVisibility[roomName] = shouldBeVisible;

            // Mettre à jour la visibilité des objets déjà instanciés
            if (spawnedObjects.TryGetValue(roomName, out GameObject spawnedObject))
            {
                if (spawnedObject != null)
                {
                    spawnedObject.SetActive(shouldBeVisible);
                }
            }
        }
    }

    private bool CheckRoomMatchesFilters(string roomName)
    {
        if (string.IsNullOrEmpty(currentTimeFilter) && 
            string.IsNullOrEmpty(currentSpecFilter) && 
            string.IsNullOrEmpty(currentTransportFilter))
        {
            return true; // Si aucun filtre n'est actif, la salle est visible
        }

        RoomData roomData = null;
        if (spawnedObjects.TryGetValue(roomName, out GameObject spawnedObject))
        {
            roomData = spawnedObject.GetComponent<RoomData>();
        }

        if (roomData == null) return false;

        // Vérifier le filtre de temps
        if (!string.IsNullOrEmpty(currentTimeFilter))
        {
            if (!roomData.studentsByHour.ContainsKey(currentTimeFilter) || 
                roomData.studentsByHour[currentTimeFilter] == 0)
            {
                return false;
            }
        }

        // Vérifier la spécialisation
        if (!string.IsNullOrEmpty(currentSpecFilter))
        {
            if (!roomData.GetStudentsBySpecialization().ContainsKey(currentSpecFilter) ||
                roomData.GetStudentsBySpecialization()[currentSpecFilter] == 0)
            {
                return false;
            }
        }

        // Vérifier le transport
        if (!string.IsNullOrEmpty(currentTransportFilter))
        {
            if (!roomData.GetStudentsByTransport().ContainsKey(currentTransportFilter) ||
                roomData.GetStudentsByTransport()[currentTransportFilter] == 0)
            {
                return false;
            }
        }

        return true;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            HandleAddedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            HandleUpdatedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            HandleRemovedImage(trackedImage);
        }
    }

    private void HandleAddedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        var validPair = prefabImagePairs.FirstOrDefault(p => p.imageName == imageName && p.prefab != null);

        if (validPair.prefab == null)
        {
            Debug.Log($"No valid prefab found for image: {imageName}");
            return;
        }

        if (spawnedObjects.ContainsKey(imageName))
        {
            Destroy(spawnedObjects[imageName]);
            spawnedObjects.Remove(imageName);
        }

        GameObject spawnedObject = InstantiateAndSetupPrefab(validPair.prefab, trackedImage, imageName);
        spawnedObjects.Add(imageName, spawnedObject);
        targetPositions[imageName] = trackedImage.transform.position;
        targetRotations[imageName] = trackedImage.transform.rotation;
    }

    private GameObject InstantiateAndSetupPrefab(GameObject prefab, ARTrackedImage trackedImage, string imageName)
    {
        GameObject spawnedObject = Instantiate(prefab, trackedImage.transform.position, trackedImage.transform.rotation);
        RoomData roomData = spawnedObject.GetComponent<RoomData>();

        if (roomData != null)
        {
            roomData.roomName = imageName;
            roomData.roomInfo = DataManager.Instance.GetRoomInfo(imageName);
            GameObject infoPanel = Instantiate(infoPanelPrefab, spawnedObject.transform);
            
            RectTransform rectTransform = infoPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localPosition = new Vector3(0, 1f, 0);
            }

            Canvas canvas = infoPanel.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.worldCamera = Camera.main;
            }

            roomData.infoPanel = infoPanel;
            infoPanel.SetActive(false);

            // Appliquer la visibilité en fonction des filtres
            spawnedObject.SetActive(roomVisibility[imageName]);
            spawnedObject.transform.parent = trackedImage.transform;
        }

        return spawnedObject;
    }

    private void HandleUpdatedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedObjects.TryGetValue(imageName, out GameObject obj))
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                obj.SetActive(true);
                // Mettre à jour les positions cibles plutôt que directement la position
                targetPositions[imageName] = trackedImage.transform.position;
                targetRotations[imageName] = trackedImage.transform.rotation;
            }
            else
            {
                obj.SetActive(false);
            }
        }
    }

    private void HandleRemovedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        if (spawnedObjects.TryGetValue(imageName, out GameObject obj))
        {
            targetPositions.Remove(imageName);
            targetRotations.Remove(imageName);
            Destroy(obj);
            spawnedObjects.Remove(imageName);
        }
    }
}