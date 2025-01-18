using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.IO;
using System;

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
    private GameObject infoPanelPrefab; // Le prefab du panel UI

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> prefabsDictionary = new Dictionary<string, GameObject>();
    private Dictionary<string, RoomData> roomDataDict = new Dictionary<string, RoomData>();

    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        foreach (var pair in prefabImagePairs)
        {
            prefabsDictionary.Add(pair.imageName, pair.prefab);
        }

        LoadRoomData();
    }

    private void LoadRoomData()
    {
        try
        {
            // Modifier le chemin pour pointer vers le bon dossier
            string csvPath = Application.dataPath + "/Prof/students.csv";
            string csvData = File.ReadAllText(csvPath);
            Debug.Log($"Successfully read CSV file from: {csvPath}");

            var lines = csvData.Split('\n');
            Debug.Log($"Number of lines in CSV: {lines.Length}");

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                var columns = lines[i].Split(',');
                if (columns.Length >= 7)
                {
                    string studentName = columns[0];
                    string[] hours = { columns[2], columns[3], columns[4], columns[5], columns[6] };

                    // Pour chaque heure, ajouter l'étudiant à la salle correspondante
                    for (int h = 0; h < hours.Length; h++)
                    {
                        string roomName = hours[h].Trim();
                        if (!string.IsNullOrEmpty(roomName))
                        {
                            if (!roomDataDict.ContainsKey(roomName))
                            {
                                roomDataDict.Add(roomName, new RoomData());
                                roomDataDict[roomName].roomName = roomName;
                                Debug.Log($"Created new room data for: {roomName}");
                            }

                            if (!roomDataDict[roomName].studentsPresent.Contains(studentName))
                            {
                                roomDataDict[roomName].studentsPresent.Add(studentName);
                            }

                            string hourKey = $"{13 + h}h";
                            if (!roomDataDict[roomName].studentsByHour.ContainsKey(hourKey))
                                roomDataDict[roomName].studentsByHour[hourKey] = 0;
                            roomDataDict[roomName].studentsByHour[hourKey]++;
                        }
                    }
                }
            }

            // Log pour vérifier que les données sont bien chargées
            foreach (var room in roomDataDict)
            {
                Debug.Log($"Room {room.Key} has {room.Value.studentsPresent.Count} students");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading CSV: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
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
            Debug.Log($"Trying to spawn object for {imageName}");

            if (!spawnedObjects.ContainsKey(imageName) && prefabsDictionary.TryGetValue(imageName, out GameObject prefab))
            {
                // Instancier le prefab de la room
                GameObject spawnedObject = Instantiate(prefab,
                    trackedImage.transform.position,
                    trackedImage.transform.rotation);

                // Configurer le RoomData component
                RoomData roomData = spawnedObject.GetComponent<RoomData>();
                if (roomData != null)
                {
                    Debug.Log($"RoomData component found for {imageName}");

                    // Instancier l'InfoPanel
                    if (infoPanelPrefab != null)
                    {
                        GameObject infoPanel = Instantiate(infoPanelPrefab, spawnedObject.transform);

                        // Configurer la position du panel
                        RectTransform rectTransform = infoPanel.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.localPosition = new Vector3(0, 1f, 0); // Ajustez la hauteur selon vos besoins
                                                                                 // Ne pas définir la rotation ici car elle sera gérée par le script Billboard
                        }

                        // Configurer le Canvas
                        Canvas canvas = infoPanel.GetComponent<Canvas>();
                        if (canvas != null)
                        {
                            canvas.worldCamera = Camera.main;
                        }

                        roomData.infoPanel = infoPanel;
                        infoPanel.SetActive(false);
                    }
                    else
                    {
                        Debug.LogError("InfoPanel prefab is not assigned in ImageTracker!");
                    }
                }
                else
                {
                    Debug.LogError($"No RoomData component found on prefab for {imageName}");
                }

                spawnedObject.transform.parent = trackedImage.transform;
                spawnedObjects.Add(imageName, spawnedObject);
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            string imageName = trackedImage.referenceImage.name;
            if (spawnedObjects.TryGetValue(imageName, out GameObject spawnedObject))
            {
                spawnedObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            }
        }

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