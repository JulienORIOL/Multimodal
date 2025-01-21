// InteractionLogger.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class InteractionLogger : MonoBehaviour
{
    private static InteractionLogger instance;
    public static InteractionLogger Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InteractionLogger>();
                if (instance == null)
                {
                    GameObject go = new GameObject("InteractionLogger");
                    instance = go.AddComponent<InteractionLogger>();
                }
            }
            return instance;
        }
    }

    private List<LogEntry> interactionLogs = new List<LogEntry>();
    [SerializeField] private GameObject logPanel;
    private GameObject activeLogPanel;  // Gardé pour compatibilité
    private TextMeshProUGUI logText;
    private bool isPanelVisible = false;

    [System.Serializable]
    public class LogEntry
    {
        public string objectName;
        public string interactionType;
        public string details;
        public DateTime timestamp;

        public LogEntry(string obj, string type, string det)
        {
            objectName = obj;
            interactionType = type;
            details = det;
            timestamp = DateTime.Now;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LogInteraction(string objectName, string interactionType, string details = "")
    {
        LogEntry entry = new LogEntry(objectName, interactionType, details);
        interactionLogs.Add(entry);
        Debug.Log($"New log added: {objectName} - {interactionType} - {details}");  // Ajoutez cette ligne

        if (isPanelVisible && logText != null)
        {
            UpdateLogDisplay();
        }
    }

    public void ToggleLogPanel()
    {
        isPanelVisible = !isPanelVisible;

        if (isPanelVisible)
        {
            if (activeLogPanel == null)
            {
                CreateLogPanel();
            }
            activeLogPanel.SetActive(true);
            UpdateLogDisplay();
        }
        else if (activeLogPanel != null)
        {
            activeLogPanel.SetActive(false);
        }
    }

    private void CreateLogPanel()
    {
        activeLogPanel = logPanel;  // Utiliser le panel existant
        logText = activeLogPanel.GetComponentInChildren<TextMeshProUGUI>();

        // Positionner le panel devant la caméra
        logPanel.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        logPanel.transform.rotation = Quaternion.LookRotation(logPanel.transform.position - Camera.main.transform.position);

        // Add drag functionality
        if (activeLogPanel.GetComponent<ARDragHandler>() == null)
        {
            activeLogPanel.AddComponent<ARDragHandler>();
        }
    }

    private void UpdateLogDisplay()
    {
        if (logText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<size=20><color=#2196F3><b>Interaction Logs</b></color></size>\n");

        var sortedLogs = interactionLogs.OrderByDescending(log => log.timestamp).Take(20);

        foreach (var log in sortedLogs)
        {
            sb.AppendLine($"<color=#4CAF50>[{log.timestamp:HH:mm:ss}]</color>");
            sb.AppendLine($"<b>{log.objectName}</b> - {log.interactionType}");
            if (!string.IsNullOrEmpty(log.details))
            {
                sb.AppendLine($"<color=#808080>{log.details}</color>");
            }
            sb.AppendLine();
        }

        logText.text = sb.ToString();
    }

    public Dictionary<string, int> GetMostInteractedObjects()
    {
        return interactionLogs
            .GroupBy(log => log.objectName)
            .ToDictionary(
                group => group.Key,
                group => group.Count()
            )
            .OrderByDescending(pair => pair.Value)
            .Take(10)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public Dictionary<string, int> GetMostCommonInteractions()
    {
        return interactionLogs
            .GroupBy(log => log.interactionType)
            .ToDictionary(
                group => group.Key,
                group => group.Count()
            )
            .OrderByDescending(pair => pair.Value)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}