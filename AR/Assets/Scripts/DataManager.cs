using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DataManager");
                    instance = go.AddComponent<DataManager>();
                }
            }
            return instance;
        }
    }

    private Dictionary<string, RoomInfo> roomDatabase = new Dictionary<string, RoomInfo>();
    private Dictionary<string, List<string>> studentSchedules = new Dictionary<string, List<string>>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStudentData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadStudentData()
    {
        string csvContent = TryLoadCSV();

        if (string.IsNullOrEmpty(csvContent))
        {
            Debug.LogError("Failed to load student data from all sources");
            return;
        }

        ProcessCSVContent(csvContent);
    }

    private string TryLoadCSV()
    {
        // Chemins potentiels pour le fichier CSV
        string[] potentialPaths = {
            Path.Combine(Application.streamingAssetsPath, "Prof", "students.csv"),
            Path.Combine(Application.dataPath, "StreamingAssets", "Prof", "students.csv"),
            Path.Combine(Application.persistentDataPath, "students.csv")
        };

        // Essayer de charger le fichier localement
        foreach (var path in potentialPaths)
        {
            Debug.Log($"Trying to load CSV from: {path}");
            if (File.Exists(path))
            {
                try 
                {
                    return File.ReadAllText(path);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading CSV from {path}: {e.Message}");
                }
            }
        }

        // Fallback sur les ressources
        TextAsset textAsset = Resources.Load<TextAsset>("Prof/students");
        if (textAsset != null)
        {
            Debug.Log("Successfully loaded CSV from Resources");
            return textAsset.text;
        }

        return null;
    }

    private void ProcessCSVContent(string csvContent)
    {
        try 
        {
            // Nettoyer le contenu CSV et gérer les virgules dans les cellules
            string[] lines = SplitCSVLines(csvContent);

            // Skip header
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = SplitCSVLine(lines[i]);
                
                if (columns.Length < 13) 
                {
                    Debug.LogWarning($"Invalid line {i}: {lines[i]}");
                    continue;
                }

                string studentName = columns[0].Trim();
                string transport = columns[9].Trim();
                string specialization = columns[12].Trim();

                // Vérifier chaque créneau horaire (13h à 17h)
                for (int h = 0; h < 5; h++)
                {
                    string room = columns[h + 2].Trim();
                    if (!string.IsNullOrEmpty(room))
                    {
                        string hourKey = $"{13 + h}h";

                        if (!roomDatabase.ContainsKey(room))
                        {
                            roomDatabase[room] = new RoomInfo();
                        }

                        if (!roomDatabase[room].studentsByHour.ContainsKey(hourKey))
                        {
                            roomDatabase[room].studentsByHour[hourKey] = new List<StudentInfo>();
                        }

                        // Ajouter l'étudiant
                        StudentInfo studentInfo = new StudentInfo
                        {
                            name = studentName,
                            transport = transport,
                            specialization = specialization
                        };

                        roomDatabase[room].studentsByHour[hourKey].Add(studentInfo);

                        // Mettre à jour les statistiques
                        UpdateStats(roomDatabase[room].transportStats, transport);
                        UpdateStats(roomDatabase[room].specializationStats, specialization);
                    }
                }
            }
            
            Debug.Log($"Loaded data for {roomDatabase.Count} rooms");
            
            // Afficher quelques informations de débogage
            foreach (var room in roomDatabase.Keys)
            {
                Debug.Log($"Room {room}:");
                foreach (var hour in roomDatabase[room].studentsByHour.Keys)
                {
                    Debug.Log($"  {hour}: {roomDatabase[room].studentsByHour[hour].Count} students");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing student data: {e.Message}");
        }
    }

    // Méthode robuste pour diviser le CSV en lignes
    private string[] SplitCSVLines(string csvContent)
    {
        return csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    // Méthode robuste pour diviser une ligne CSV
    private string[] SplitCSVLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var currentField = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
                inQuotes = !inQuotes;
            else if (line[i] == ',' && !inQuotes)
            {
                result.Add(currentField.ToString().Trim('"', ' '));
                currentField.Clear();
            }
            else
            {
                currentField.Append(line[i]);
            }
        }
        
        // Ajouter le dernier champ
        result.Add(currentField.ToString().Trim('"', ' '));

        return result.ToArray();
    }

    private void UpdateStats(Dictionary<string, int> stats, string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        
        if (!stats.ContainsKey(key))
            stats[key] = 0;
        stats[key]++;
    }

    public RoomInfo GetRoomInfo(string roomName)
    {
        if (roomDatabase.TryGetValue(roomName, out RoomInfo info))
        {
            return info;
        }
        return null;
    }

    public List<string> GetStudentSchedule(string studentName)
    {
        if (studentSchedules.TryGetValue(studentName, out List<string> schedule))
        {
            return schedule;
        }
        return new List<string>();
    }
}

[System.Serializable]
public class RoomInfo
{
    public Dictionary<string, List<StudentInfo>> studentsByHour = new Dictionary<string, List<StudentInfo>>();

    // Pour les statistiques
    public Dictionary<string, int> transportStats = new Dictionary<string, int>();
    public Dictionary<string, int> specializationStats = new Dictionary<string, int>();
    public int capacity = 30; // On garde une capacité par défaut
}

[System.Serializable]
public class StudentInfo
{
    public string name;
    public string specialization;
    public string transport;
}