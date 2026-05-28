using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FearManager : MonoBehaviour
{
    [Header("UI y Vida")]
    public Slider lifeBar;

    public float maxLife = 100f;
    public float lifeDecreaseRate = 5f;
    public float lifeIncreaseRate = 10f;

    [Header("Referencias")]
    public Light playerFlashlight;

    private bool isFlashlightOn = false;
    private float currentLife;

    // ahora guardamos luces reales
    private List<Light> safeLights = new List<Light>();

    private void Start()
    {
        currentLife = maxLife;

        if (lifeBar != null)
        {
            lifeBar.maxValue = maxLife;
            lifeBar.value = currentLife;
        }
    }

    private void Update()
    {
        UpdateFlashlightState();
        HandleLifeSystem();
        UpdateUI();
        CheckDeath();
    }

    private void UpdateFlashlightState()
    {
        if (playerFlashlight != null)
        {
            isFlashlightOn =
                playerFlashlight.enabled &&
                playerFlashlight.gameObject.activeInHierarchy;
        }
    }

    private void HandleLifeSystem()
    {
        bool hasActiveSafeLight = false;

        // revisar en tiempo real si alguna luz externa sigue encendida
        foreach (Light light in safeLights)
        {
            if (light != null &&
                light.enabled &&
                light.gameObject.activeInHierarchy)
            {
                hasActiveSafeLight = true;
                break;
            }
        }

        bool isSafeInLight =
            isFlashlightOn ||
            hasActiveSafeLight;

        if (isSafeInLight)
            currentLife += lifeIncreaseRate * Time.deltaTime;
        else
            currentLife -= lifeDecreaseRate * Time.deltaTime;

        currentLife = Mathf.Clamp(currentLife, 0f, maxLife);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateBreathing(currentLife, maxLife);
        }
    }

    private void UpdateUI()
    {
        if (lifeBar != null)
            lifeBar.value = currentLife;
    }

    private void CheckDeath()
    {
        if (currentLife <= 0f)
        {
            Debug.Log("¡Moriste!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SafeLight"))
            return;

        Light safeLight = other.GetComponentInChildren<Light>();

        if (safeLight != null && !safeLights.Contains(safeLight))
        {
            safeLights.Add(safeLight);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SafeLight"))
            return;

        Light safeLight = other.GetComponentInChildren<Light>();

        if (safeLight != null && safeLights.Contains(safeLight))
        {
            safeLights.Remove(safeLight);
        }
    }
}