using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AdaptiveMusicManager : MonoBehaviour
{
    public static AdaptiveMusicManager Instance;

    [Header("Snapshots del Mixer")]
    public AudioMixerSnapshot explorationSnapshot;
    public AudioMixerSnapshot dangerSnapshot;

    [Header("Tiempos de Transición")]
    public float timeToDanger = 1.0f;
    public float timeToExploration = 3.5f;

    private bool isLowHealth = false;
    
    // Acá guardamos QUÉ enemigos específicos nos están persiguiendo
    private HashSet<GameObject> chasingEnemies = new HashSet<GameObject>();
    
    // Candado para no mandarle spam de órdenes a Unity (Arregla el retraso)
    private bool isCurrentlyDanger = false; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (explorationSnapshot != null) explorationSnapshot.TransitionTo(0.1f);
    }

    // Ahora recibimos al GameObject del enemigo para sumarlo o restarlo de la lista
    public void SetEnemyChasing(GameObject enemy, bool isChasing)
    {
        if (isChasing)
            chasingEnemies.Add(enemy);
        else
            chasingEnemies.Remove(enemy);
            
        UpdateMusicState();
    }

    public void SetLowHealth(bool isLow)
    {
        isLowHealth = isLow;
        UpdateMusicState();
    }

    private void UpdateMusicState()
    {
        chasingEnemies.RemoveWhere(enemy => enemy == null || !enemy.activeInHierarchy);
        
        bool shouldBeDanger = (chasingEnemies.Count > 0) || isLowHealth;
        
        if (shouldBeDanger && !isCurrentlyDanger)
        {
            isCurrentlyDanger = true;
            if (dangerSnapshot != null) dangerSnapshot.TransitionTo(timeToDanger);
        }
        else if (!shouldBeDanger && isCurrentlyDanger)
        {
            isCurrentlyDanger = false;
            if (explorationSnapshot != null) explorationSnapshot.TransitionTo(timeToExploration);
        }
    }
}