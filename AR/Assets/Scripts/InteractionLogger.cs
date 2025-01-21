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

    private float lastLogTime = 0f;
    private const float LOG_COOLDOWN = 0.5f; // 500ms entre chaque log

    public void LogInteraction(string objectName, string interactionType, string details = "")
    {
        // Éviter le spam de logs
        if (Time.time - lastLogTime < LOG_COOLDOWN) return;

        lastLogTime = Time.time;
        LogEntry entry = new LogEntry(objectName, interactionType, details);
        interactionLogs.Add(entry);

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

        // Configurer le ScrollRect si ce n'est pas déjà fait
        ScrollRect scrollRect = activeLogPanel.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = activeLogPanel.AddComponent<ScrollRect>();

            // Créer ou configurer le Viewport si nécessaire
            Transform viewportTransform = activeLogPanel.transform.Find("Viewport");
            if (viewportTransform == null)
            {
                GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
                viewport.transform.SetParent(activeLogPanel.transform, false);

                // Configurer le RectTransform du viewport
                RectTransform viewportRect = viewport.GetComponent<RectTransform>();
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.sizeDelta = Vector2.zero;
                viewportRect.anchoredPosition = Vector2.zero;

                // Ajouter les composants nécessaires au viewport
                Image viewportImage = viewport.AddComponent<Image>();
                viewportImage.color = new Color(0, 0, 0, 0.1f); // Légèrement visible pour le debug
                viewport.AddComponent<Mask>();

                viewportTransform = viewport.transform;
            }

            scrollRect.viewport = viewportTransform as RectTransform;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

        // Trouver ou créer le LogsText
        logText = activeLogPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (logText == null)
        {
            GameObject textObj = new GameObject("LogsText", typeof(RectTransform));
            textObj.transform.SetParent(scrollRect.viewport.transform, false);
            logText = textObj.AddComponent<TextMeshProUGUI>();

            // Configurer le RectTransform du texte
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 1);
            textRect.sizeDelta = new Vector2(0, 0);

            // Configurer le TextMeshPro
            logText.alignment = TextAlignmentOptions.TopLeft;
            logText.fontSize = 14;
            logText.enableWordWrapping = true;
            logText.margin = new Vector4(10, 10, 10, 10);
            logText.color = Color.white;
        }

        // Assigner le contenu au ScrollRect
        scrollRect.content = logText.GetComponent<RectTransform>();

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