using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // --- EL SINGLETON ---
    // Esto permite que cualquier script acceda al GameManager escribiendo: GameManager.Instance
    public static GameManager Instance { get; private set; }

    [Header("Instanciación (Spawns)")]
    public GameObject enemyPrefab;
    public Transform[] enemySpawnPoints;

    [Header("Sistema de Misiones")]
    public TextMeshProUGUI missionTextUI;
    
    [Header("Sistema de Audio Global")]
    public AudioSource ambientMusicSource;
    public AudioSource breathingSource;
    
    private void Awake()
    {
        // Configuramos el Singleton para asegurarnos de que solo haya un GameManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SpawnEntities();
        StartAtmosphere();
        UpdateMission("Encuentra la manera de salir de la cabaña.");
    }

    // --- 1. INSTANCIADOR ---
    void SpawnEntities()
    {
        // Instanciar a todos los enemigos en sus respectivos puntos
        if (enemyPrefab != null && enemySpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in enemySpawnPoints)
            {
                Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }

    // --- 2. AUDIO INICIAL ---
    void StartAtmosphere()
    {
        if (ambientMusicSource != null)
        {
            ambientMusicSource.loop = true;
            ambientMusicSource.Play();
        }

        if (breathingSource != null)
        {
            breathingSource.loop = true; // Respiración en bucle infinito
            breathingSource.pitch = 1.0f; // Velocidad normal
            breathingSource.Play();
        }
    }

    // --- 3. MANEJADOR DE MISIONES ---
    // Cualquier script puede llamar a: GameManager.Instance.UpdateMission("Nueva misión!");
    public void UpdateMission(string newMission)
    {
        if (missionTextUI != null)
        {
            missionTextUI.text = "Misión Actual: " + newMission;
        }
    }

    // --- 4. RESPIRACIÓN DINÁMICA ---
    // Esta función ahora es llamada automáticamente y en tiempo real por el FearManager
    public void UpdateBreathing(float currentLife, float maxLife)
    {
        if (breathingSource == null) return;

        float healthPercentage = currentLife / maxLife;

        // Si la vida baja del 40%, el jugador empieza a hiperventilar por el miedo
        if (healthPercentage <= 0.4f)
        {
            float targetPitch = Mathf.Lerp(1.7f, 1.0f, healthPercentage / 0.4f);
            breathingSource.pitch = targetPitch;
        }
        else
        {
            // Si está a salvo (más del 40%), respira normal
            breathingSource.pitch = 1.0f; 
        }
    }
}