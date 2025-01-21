using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.IO;
using System;
using TMPro;
using System.Linq;

public class ImageTracker : MonoBehaviour
{
    [System.Serializable]
    public struct PrefabImagePair
    {
        public string imageName;
        public GameObject prefab;
    }

    [SerializeField]
    private PrefabImagePair[] prefabImagePairs;
    [SerializeField]
    private GameObject infoPanelPrefab;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    private void Start()
    {
        Debug.Log("Available prefabs:");
        foreach (var pair in prefabImagePairs)
        {
            Debug.Log($"Image: {pair.imageName}, Prefab: {(pair.prefab != null ? pair.prefab.name : "null")}");
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

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"Detected image: {imageName}");

            // Vérifier si l'image est valide ET si son prefab existe
            var validPair = prefabImagePairs.FirstOrDefault(p => p.imageName == imageName && p.prefab != null);
            if (validPair.prefab == null)
            {
                Debug.Log($"No valid prefab found for image: {imageName}");
                continue;
            }

            // Supprimer l'ancien objet s'il existe
            if (spawnedObjects.ContainsKey(imageName))
            {
                Destroy(spawnedObjects[imageName]);
                spawnedObjects.Remove(imageName);
            }

            // Création de l'objet
            GameObject spawnedObject = Instantiate(validPair.prefab, trackedImage.transform.position, trackedImage.transform.rotation);
            RoomData roomData = spawnedObject.GetComponent<RoomData>();

            if (roomData != null)
            {
                roomData.roomName = imageName;
                roomData.roomInfo = DataManager.Instance.GetRoomInfo(imageName);

                GameObject infoPanel = Instantiate(infoPanelPrefab, spawnedObject.transform);
                roomData.infoPanel = infoPanel;
            }

            spawnedObject.transform.parent = trackedImage.transform;
            spawnedObjects.Add(imageName, spawnedObject);
        }

        // Mise à jour des objets existants
        foreach (var trackedImage in eventArgs.updated)
        {
            string imageName = trackedImage.referenceImage.name;
            if (spawnedObjects.TryGetValue(imageName, out GameObject spawnedObject))
            {
                spawnedObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);

                // Mettre à jour la position uniquement si l'objet est suivi
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    spawnedObject.transform.position = trackedImage.transform.position;
                    spawnedObject.transform.rotation = trackedImage.transform.rotation;
                }
            }
        }

        // Suppression des objets non suivis
        foreach (var trackedImage in eventArgs.removed)
        {
            string imageName = trackedImage.referenceImage.name;
            if (spawnedObjects.TryGetValue(imageName, out GameObject spawnedObject))
            {
                Destroy(spawnedObject);
                spawnedObjects.Remove(imageName);
            }
        }
    }
}
