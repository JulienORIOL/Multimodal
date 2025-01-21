// DataManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

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
        try
        {
            string csvPath = Path.Combine(Application.streamingAssetsPath, "Prof", "students.csv");
            Debug.Log($"Trying to read CSV at: {csvPath}");

            var fileContents = new WWW(csvPath);
            while (!fileContents.isDone) { }

            string[] lines = fileContents.text.Split('\n');

            // Skip header
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');
                if (columns.Length >= 13)
                {
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
                            roomDatabase[room].studentsByHour[hourKey].Add(new StudentInfo
                            {
                                name = studentName,
                                transport = transport,
                                specialization = specialization
                            });

                            // Mettre à jour les statistiques
                            UpdateStats(roomDatabase[room].transportStats, transport);
                            UpdateStats(roomDatabase[room].specializationStats, specialization);
                        }
                    }
                }
            }
            Debug.Log($"Loaded data for {roomDatabase.Count} rooms");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading student data: {e.Message}");
        }
    }

    private void UpdateStats(Dictionary<string, int> stats, string key)
    {
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