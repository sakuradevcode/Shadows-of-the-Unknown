using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

public class FearManager : MonoBehaviour
{
    [Header("UI y Vida")]
    public Slider lifeBar;
    public float maxLife = 100f;
    public float lifeDecreaseRate = 5f;
    public float lifeIncreaseRate = 10f;

    [Header("Efecto de Miedo (Pantalla Roja)")]
    public Image redScreenVignette; 

    [Header("Audio (Respiración)")]
    public AudioSource breathingAudio; // Arrastrá acá el AudioSource con el jadeo
    public float normalPitch = 1f;     // Velocidad normal
    public float fastPitch = 2.5f;     // Velocidad máxima cuando está por morir

    [Header("Referencias")]
    public Light playerFlashlight;

    private bool isFlashlightOn = false;
    private float currentLife;
    private List<Light> safeLights = new List<Light>();

    private void Start()
    {
        currentLife = maxLife;

        if (lifeBar != null)
        {
            lifeBar.maxValue = maxLife;
            lifeBar.value = currentLife;
        }

        if (redScreenVignette != null)
        {
            Color c = redScreenVignette.color;
            c.a = 0f;
            redScreenVignette.color = c;
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
            isFlashlightOn = playerFlashlight.enabled && playerFlashlight.gameObject.activeInHierarchy;
        }
    }

    private void HandleLifeSystem()
    {
        bool hasActiveSafeLight = false;

        foreach (Light light in safeLights)
        {
            if (light != null && light.enabled && light.gameObject.activeInHierarchy)
            {
                hasActiveSafeLight = true;
                break;
            }
        }

        bool isSafeInLight = isFlashlightOn || hasActiveSafeLight;

        if (isSafeInLight)
            currentLife += lifeIncreaseRate * Time.deltaTime;
        else
            currentLife -= lifeDecreaseRate * Time.deltaTime;

        currentLife = Mathf.Clamp(currentLife, 0f, maxLife);
        
        // Si seguís usando esto en el GameManager, lo dejamos para que siga mandando datos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateBreathing(currentLife, maxLife);
        }

        UpdateRedScreenEffect();
        UpdateBreathingSpeed(); // Llamamos a la nueva función
    }

    private void UpdateRedScreenEffect()
    {
        if (redScreenVignette != null)
        {
            float thresholdLife = maxLife * 0.75f; 

            if (currentLife <= thresholdLife)
            {
                float fearIntensity = 1f - (currentLife / thresholdLife); 
                float alpha = fearIntensity * 0.8f;

                Color c = redScreenVignette.color;
                c.a = alpha;
                redScreenVignette.color = c;
            }
            else
            {
                Color c = redScreenVignette.color;
                c.a = 0f;
                redScreenVignette.color = c;
            }
        }
    }

    // --- NUEVA FUNCIÓN PARA EL AUDIO ---
    private void UpdateBreathingSpeed()
    {
        if (breathingAudio != null)
        {
            float thresholdLife = maxLife * 0.75f;

            if (currentLife <= thresholdLife)
            {
                // Calcula el nivel de pánico de 0 a 1
                float fearIntensity = 1f - (currentLife / thresholdLife);

                // Acelera levemente (acordate de poner fastPitch en 1.2 o 1.3 en el Inspector)
                breathingAudio.pitch = Mathf.Lerp(normalPitch, fastPitch, fearIntensity);
                
                // ¡EL TRUCO! Sube el volumen a medida que entra en pánico
                // (Cambia el 0.3f y el 1f si querés que empiece más fuerte o termine más suave)
                breathingAudio.volume = Mathf.Lerp(0.3f, 1f, fearIntensity); 
                
                if (!breathingAudio.isPlaying) breathingAudio.Play();
            }
            else
            {
                // Si está tranquilo, respira normal y el volumen se queda bajito de fondo
                breathingAudio.pitch = normalPitch;
                breathingAudio.volume = 0.3f; 
            }
        }
    }

    private void UpdateUI()
    {
        if (lifeBar != null)
            lifeBar.value = currentLife;
    }
    
    // --- NUEVA FUNCIÓN PARA RECIBIR DAÑO ---
    public void TakeDamage(float damageAmount)
    {
        currentLife -= damageAmount;
        
        currentLife = Mathf.Clamp(currentLife, 0f, maxLife);
        
        UpdateRedScreenEffect();
        
        if (currentLife <= 0f)
        {
            DieByMonster();
        }
    }

    private void CheckDeath()
    {
        if (currentLife <= 0f)
        {
            PlayerPrefs.SetString("ReasonDeath", "EL PÁNICO TE CONSUMIÓ EN LA OSCURIDAD");
            SceneManager.LoadScene("GameOver");
        }
    }

    public void DieByMonster()
    {
        PlayerPrefs.SetString("ReasonDeath", "TE ATRAPARON LAS CRIATURAS");
        SceneManager.LoadScene("GameOver");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SafeLight")) return;
        Light safeLight = other.GetComponentInChildren<Light>();
        if (safeLight != null && !safeLights.Contains(safeLight)) safeLights.Add(safeLight);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SafeLight")) return;
        Light safeLight = other.GetComponentInChildren<Light>();
        if (safeLight != null && safeLights.Contains(safeLight)) safeLights.Remove(safeLight);
    }
}