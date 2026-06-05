using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Missions UI")]
    public TextMeshProUGUI missionsText; // Arrastrá acá tu TextMeshPro de la UI

    // Internal class to save data for each mission
    private class Mission
    {
        public string id;
        public string description;
        public bool isCompleted;
        public string codeProgress; // Extra text to show the numbers
    }

    private List<Mission> activeMissions = new List<Mission>();
    
    // Array to manage the 5 digits of the code
    private List<string> foundDigits = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Start with the first mission
        AddMission("clues", "- Buscar pistas");
        
        StartCoroutine(AddMonstersMissionDelayed());
    }
    
    private System.Collections.IEnumerator AddMonstersMissionDelayed()
    {
        // Le decimos a Unity que espere exactamente 5 segundos
        yield return new WaitForSeconds(10f);

        // Una vez que pasan los 5 segundos, ejecuta esto:
        AddMission("monsters", "- Encuentra una forma de ahuyentar a esos monstruos (opcional)");
    }
    
    // --- METHODS TO UPDATE MISSIONS FROM OTHER SCRIPTS ---

    // Call this when the player interacts with the bathroom door
    public void TryOpenBathroom()
    {
        if (!MissionExists("bathroom_door"))
        {
            AddMission("bathroom_door", "- Encuentra la manera de abrir la puerta del baño");
        }
    }

    // Call this when the player interacts with the main house door
    public void TryOpenHouse()
    {
        if (!MissionExists("house_door"))
        {
            AddMission("house_door", "- Encuentra una manera de salir de la casa");
        }
    }

    // Call this when successfully opening the bathroom
    public void OpenBathroom()
    {
        CompleteMission("bathroom_door");
        AddMission("code", "- Sigue las pistas para encontrar el código");
        UpdateUIText(); // To show the initial underscores
    }

    // Ahora recibe el número y el color (en inglés o en hexadecimal, ej: "red", "blue", "#FFA500")
    public void FindCodeFragment(char number, string color)
    {
        // Armamos el texto con la etiqueta de color para TextMeshPro
        string coloredNumber = $"<color={color}>{number}</color>";

        // Lo agregamos a la bolsa de números encontrados (validamos que no esté ya agregado por si toca 2 veces)
        if (!foundDigits.Contains(coloredNumber))
        {
            foundDigits.Add(coloredNumber);
        }

        // Actualizamos la descripción de la misión del código
        Mission m = GetMission("code"); // O "codigo" dependiendo de cómo lo hayas dejado
        if (m != null && !m.isCompleted)
        {
            // Unimos todos los números encontrados separados por un guion o espacio
            m.codeProgress = $" [ Encontrados: {string.Join(" - ", foundDigits)} ]";
            UpdateUIText();
        }
    }

    // Call this when successfully exiting the house
    public void OpenHouse()
    {
        CompleteMission("code");
        CompleteMission("house_door");

        AddMission("escape", "- Encuentra una forma de escapar, ve al granero");
    }

    public void CodeFound()
    {
        CompleteMission("clues");
        if (!MissionExists("bathroom_door"))
        {
            AddMission("bathroom_door", "- Encuentra la manera de abrir la puerta del baño");
        }
    }
    
    public void TruckFound()
    {
        CompleteMission("escape");
        AddMission("truck_escape", "- Escapa en el camión");
    }

    // Call this when reading the book
    public void ReadBook()
    {
        CompleteMission("monsters");
    }
    
    public bool IsMissionCompleted(string id)
    {
        Mission m = GetMission(id); // O ObtenerMision(id) si lo dejaste en español
        return m != null && m.isCompleted; // (o m.completada)
    }

    // --- INTERNAL LOGIC OF THE SYSTEM ---

    private void AddMission(string missionId, string missionText)
    {
        if (!MissionExists(missionId))
        {
            Mission newMission = new Mission
            {
                id = missionId,
                description = missionText,
                isCompleted = false,
                codeProgress = "" // Empty by default
            };
            activeMissions.Add(newMission);
            UpdateUIText();
        }
    }

    private void CompleteMission(string missionId)
    {
        Mission m = GetMission(missionId);
        if (m != null && !m.isCompleted)
        {
            m.isCompleted = true;
            UpdateUIText();
        }
    }

    private void UpdateUIText()
    {
        if (missionsText == null) return;

        string finalText = "";

        foreach (Mission m in activeMissions)
        {
            if (m.isCompleted)
            {
                // If it is completed, paint it green and cross it out with <s>
                finalText += $"<color=green><s>{m.description}{m.codeProgress}</s></color>\n";
            }
            else
            {
                // If not, show it normally in white
                finalText += $"{m.description}{m.codeProgress}\n";
            }
        }

        missionsText.text = finalText;
    }

    private bool MissionExists(string id)
    {
        return activeMissions.Exists(m => m.id == id);
    }

    private Mission GetMission(string id)
    {
        return activeMissions.Find(m => m.id == id);
    }
}