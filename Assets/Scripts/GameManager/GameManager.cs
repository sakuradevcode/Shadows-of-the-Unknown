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
    
    [Header("Sistema de Audio Global")]
    public AudioSource ambientMusicSource;
    
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
    }
}