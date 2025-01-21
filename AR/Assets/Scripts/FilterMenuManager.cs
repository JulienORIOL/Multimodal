using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

public class FilterMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown timeDropdown;
    [SerializeField] private TMP_Dropdown specializationDropdown;
    [SerializeField] private TMP_Dropdown transportDropdown;

    private List<StudentData> allStudents = new List<StudentData>(); 
    private ImageTracker imageTracker;

    void Start()
    {
        imageTracker = FindObjectOfType<ImageTracker>();
        InitializeDropdowns();
        StartCoroutine(LoadStudentData());
    }

    private void InitializeDropdowns()
    {
        ClearAndPopulateDropdown(timeDropdown, new[] { "All", "13h", "14h", "15h", "16h", "17h" });
        ClearDropdowns();

        timeDropdown.onValueChanged.AddListener(delegate {
            AudioManager.Instance.PlayClickSound();
            ApplyFilters();
        });

        specializationDropdown.onValueChanged.AddListener(delegate {
            AudioManager.Instance.PlayClickSound();
            ApplyFilters();
        });

        transportDropdown.onValueChanged.AddListener(delegate {
            AudioManager.Instance.PlayClickSound();
            ApplyFilters();
        });
    }

    private void ClearAndPopulateDropdown(TMP_Dropdown dropdown, string[] options)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options.Select(o => new TMP_Dropdown.OptionData(o)).ToList());
    }

    private void ClearDropdowns()
    {
        specializationDropdown.ClearOptions();
        transportDropdown.ClearOptions();

        specializationDropdown.AddOptions(new List<TMP_Dropdown.OptionData> 
        { 
            new TMP_Dropdown.OptionData("All") 
        });
        transportDropdown.AddOptions(new List<TMP_Dropdown.OptionData> 
        { 
            new TMP_Dropdown.OptionData("All") 
        });
    }

    private IEnumerator LoadStudentData()
    {
        allStudents.Clear();
        string csvContent = null;

        // Chemin pour Android et iOS
        string filePath = Path.Combine(Application.streamingAssetsPath, "Prof", "students.csv");

        if (filePath.Contains("://") || filePath.Contains("jar"))
        {
#if UNITY_ANDROID
            using (UnityWebRequest www = UnityWebRequest.Get(filePath))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    csvContent = www.downloadHandler.text;
                    Debug.Log("Successfully loaded CSV for FilterMenu from Android StreamingAssets");
                }
                else
                {
                    Debug.LogError($"Failed to load CSV for FilterMenu from Android StreamingAssets: {www.error}");
                }
            }
#else
            // Pour iOS, lecture directe du fichier
            csvContent = File.ReadAllText(filePath);
#endif
        }
        else
        {
            // Pour l'éditeur Unity et les builds standalone
            if (File.Exists(filePath))
            {
                csvContent = File.ReadAllText(filePath);
                Debug.Log("Successfully loaded CSV for FilterMenu from file system");
            }
        }

        // Fallback sur les ressources si nécessaire
        if (string.IsNullOrEmpty(csvContent))
        {
            TextAsset textAsset = Resources.Load<TextAsset>("Prof/students");
            if (textAsset != null)
            {
                csvContent = textAsset.text;
                Debug.Log("Successfully loaded CSV for FilterMenu from Resources");
            }
        }

        if (string.IsNullOrEmpty(csvContent))
        {
            Debug.LogError("Could not load students.csv from any location for FilterMenu!");
            yield break;
        }

        ProcessCSVContent(csvContent);
    }


    private void ProcessCSVContent(string csvContent)
    {
        // Nettoyer le contenu CSV et gérer les virgules dans les cellules
        csvContent = csvContent.Replace("\r", "");
        string[] lines = csvContent.Split('\n');

        // Variables pour les dropdowns
        HashSet<string> specializations = new HashSet<string>();
        HashSet<string> transports = new HashSet<string>();

        // Commencer à partir de la ligne 1 pour ignorer l'en-tête
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Utiliser une méthode plus robuste de séparation des colonnes
            string[] columns = SplitCSVLine(line);
            
            if (columns.Length < 13) 
            {
                Debug.LogWarning($"Ligne {i} invalide : {line}");
                continue;
            }

            string studentName = columns[0].Trim();
            string transport = columns[9].Trim();
            string specialization = columns[12].Trim();

            // Ajouter aux ensembles pour les dropdowns
            specializations.Add(specialization);
            transports.Add(transport);

            // Ajouter des étudiants pour chaque créneau horaire
            for (int hour = 13; hour <= 17; hour++)
            {
                string roomName = columns[hour - 11].Trim();
                if (!string.IsNullOrEmpty(roomName))
                {
                    allStudents.Add(new StudentData
                    {
                        name = studentName,
                        specialization = specialization,
                        transport = transport,
                        schedule = $"{hour}h",
                        room = roomName
                    });
                }
            }
        }

        // Mettre à jour les dropdowns de spécialisation et transport
        UpdateSpecializationDropdown(specializations);
        UpdateTransportDropdown(transports);

        Debug.Log($"Loaded {allStudents.Count} student entries");
        ApplyFilters();
    }

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
                result.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(line[i]);
            }
        }
        result.Add(currentField.ToString().Trim());

        return result.ToArray();
    }

    private void UpdateSpecializationDropdown(HashSet<string> specializations)
    {
        specializationDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData> 
        { 
            new TMP_Dropdown.OptionData("All") 
        };
        options.AddRange(specializations.OrderBy(s => s).Select(s => new TMP_Dropdown.OptionData(s)));
        specializationDropdown.AddOptions(options);
    }

    private void UpdateTransportDropdown(HashSet<string> transports)
    {
        transportDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData> 
        { 
            new TMP_Dropdown.OptionData("All") 
        };
        options.AddRange(transports.OrderBy(t => t).Select(t => new TMP_Dropdown.OptionData(t)));
        transportDropdown.AddOptions(options);
    }

    private void ApplyFilters()
    {
        string timeFilter = timeDropdown.value == 0 ? "" : timeDropdown.options[timeDropdown.value].text;
        string specializationFilter = specializationDropdown.value == 0 ? "" : specializationDropdown.options[specializationDropdown.value].text;
        string transportFilter = transportDropdown.value == 0 ? "" : transportDropdown.options[transportDropdown.value].text;


        // Log l'application des filtres
        if (timeFilter != "" || specializationFilter != "" || transportFilter != "")
        {
            InteractionLogger.Instance.LogInteraction(
                "FilterMenu",
                "Filters Changed",
                $"Time: {timeFilter}, Spec: {specializationFilter}, Transport: {transportFilter}"
            );
        }
        // Filtrer les étudiants
        var filteredStudents = allStudents.Where(s =>
            (string.IsNullOrEmpty(timeFilter) || s.schedule == timeFilter) &&
            (string.IsNullOrEmpty(specializationFilter) || s.specialization == specializationFilter) &&
            (string.IsNullOrEmpty(transportFilter) || s.transport == transportFilter)
        ).ToList();

        // Mettre à jour l'ImageTracker
        if (imageTracker != null)
        {
            imageTracker.UpdateFilters(timeFilter, specializationFilter, transportFilter);
        }

        // Mettre à jour l'affichage des salles
        foreach (var room in FindObjectsOfType<RoomData>())
        {
            var roomStudents = filteredStudents.Where(s => s.room == room.roomName).ToList();
            room.UpdateStudentsList(roomStudents, timeFilter, specializationFilter, transportFilter);
        }
    }
}