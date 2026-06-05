using System;
using UnityEngine;
using System.Collections;

public class TruckObserver : MonoBehaviour
{
    [Header("Configuración de Visión")]
    public float visionDistance = 30f;
    public string truckTag = "Truck";
    
    [Header("Optimización")]
    [Tooltip("Cuántos rayos lanza por segundo (Ej: 5 = un rayo cada 0.2 segundos)")]
    public float checksPerSecond = 5f;

    [Header("Indicador de Destino (Guía)")]
    [Tooltip("El objeto de la flecha que guiará al jugador")]
    public GameObject pointerArrow; 
    [Tooltip("El Transform del camión, para saber hacia dónde debe apuntar la flecha")]
    public Transform truckTransform;

    [Header("Depuración")]
    public bool isSearchActive = false;
    [SerializeField] private bool hasSeenTruck = false;
    
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;

        // Por seguridad, nos aseguramos de que la flecha arranque apagada
        if (pointerArrow != null)
        {
            pointerArrow.SetActive(false);
        }
    }

    void Update()
    {
        // ---> NUEVO: Rotamos la flecha de forma fluida todos los frames
        if (isSearchActive && !hasSeenTruck && pointerArrow != null && truckTransform != null)
        {
            // La función LookAt hace que la flecha apunte mágicamente hacia el objetivo
            pointerArrow.transform.LookAt(truckTransform.position);
        }
    }

    public void StartLookingForTruck()
    {
        Debug.Log("StartLookingForTruck: " + isSearchActive + " - " + hasSeenTruck);
        if (!isSearchActive && !hasSeenTruck)
        {
            isSearchActive = true;
            
            // ---> NUEVO: Encendemos la flecha visual
            if (pointerArrow != null)
            {
                pointerArrow.SetActive(true);
            }

            StartCoroutine(SearchRoutine());
        }
    }

    private IEnumerator SearchRoutine()
    {
        float waitTime = 1f / checksPerSecond;
        WaitForSeconds wait = new WaitForSeconds(waitTime);

        while (isSearchActive && !hasSeenTruck && playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, visionDistance))
            {
                if (hit.collider.CompareTag(truckTag))
                {
                    hasSeenTruck = true;
                    isSearchActive = false;

                    MissionManager.Instance.TruckFound();
                    
                    // ---> NUEVO: Apagamos la flecha porque ya vimos el camión
                    if (pointerArrow != null)
                    {
                        pointerArrow.SetActive(false);
                    }

                    if (VoiceManager.Instance != null)
                    {
                        VoiceManager.Instance.PlayTruckPhrase();
                    }
                }
            }
            
            yield return wait;
        }
    }
}