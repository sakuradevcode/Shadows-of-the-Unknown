using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // Añadido para detectar el clic del mouse

public class FlashlightBatterySystem : MonoBehaviour
{
    [Header("UI")]
    public Slider batteryBar;
    public TMP_Text batteryCountText;

    [Header("Battery Settings")]
    public float maxBatteryCharge = 100f;
    public float batteryDrainRate = 10f;
    public int totalBatteries = 3;

    [Header("Boost Settings (Foco)")] // --- NUEVAS VARIABLES ---
    public float boostDrainRate = 30f; // Se gasta mucho más rápido
    public float normalIntensity = 2f;
    public float boostIntensityMultiplier = 5f;
    public float normalSpotAngle = 60f;
    public float boostSpotAngle = 30f; // Cierra el ángulo de la luz
    public float boostRange = 15f;     // Distancia del rayo
    public Transform cameraHead;       // Desde dónde sale el rayo (tu cámara)
    public LayerMask enemyLayer;       // La capa del monstruo

    [Header("References")]
    public Light flashlight;
    public Light boostLight;
    public Animator armsAnimator;

    [Header("Animation")]
    [Tooltip("Nombre del trigger de recarga en el Animator")]
    public string reloadTriggerName = "reload";

    [Header("Configuración de Sonido (SFX)")]
    public AudioSource powerUpSource;
    
    [Header("Input System")]
    [Tooltip("Arrastrá acá tu Action para el Boost (Ej: Clic Izquierdo)")]
    public InputActionReference boostAction;
    
    private bool wasBoosting = false;

    private float currentBatteryCharge;
    private bool isFlashlightOn;
    private bool isReloading = false;

    private void Start()
    {
        currentBatteryCharge = maxBatteryCharge;

        if (batteryBar != null)
        {
            batteryBar.maxValue = maxBatteryCharge;
            batteryBar.value = currentBatteryCharge;
        }

        UpdateBatteryText();
        ResetFlashlightToNormal(); // Nos aseguramos de que empiece con luz normal
    }

    private void Update()
    {
        if (isReloading || Cursor.visible)
            return;

        UpdateFlashlightState();
        HandleBatteryDrain();
        UpdateUI();
    }

    private void UpdateFlashlightState()
    {
        if (flashlight != null)
        {
            isFlashlightOn =
                flashlight.enabled &&
                flashlight.gameObject.activeInHierarchy;
        }
    }

    private void HandleBatteryDrain()
    {
        if (!isFlashlightOn)
        {
            if (boostLight != null)
            {
                boostLight.enabled = false;
            }
            ResetFlashlightToNormal();
            return;
        }
        
        bool isBoosting = boostAction != null && boostAction.action.IsPressed();

        if (isBoosting)
        {
            
            if (!wasBoosting && powerUpSource != null)
            {
                powerUpSource.Play();
                wasBoosting = true;
            }
            
            if (boostLight != null)
            {
                boostLight.enabled = true;
            }
            
            // Drenar batería más rápido
            currentBatteryCharge -= boostDrainRate * Time.deltaTime;

            // Raycast para espantar al enemigo
            if (cameraHead != null)
            {
                if (Physics.Raycast(cameraHead.position, cameraHead.forward, out RaycastHit hit, boostRange, enemyLayer))
                {
                    EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
                    if (enemy != null)
                    {
                        enemy.Repel(); // Llama a la función de huida en el script del enemigo
                    }
                }
            }
        }
        else
        {
            // Estado normal
            wasBoosting = false;
            currentBatteryCharge -= batteryDrainRate * Time.deltaTime;
            if (boostLight != null)
            {
                boostLight.enabled = false;
            }
            ResetFlashlightToNormal();
        }
        // -----------------------------

        if (currentBatteryCharge <= 0f)
        {
            currentBatteryCharge = 0f;
            ResetFlashlightToNormal(); // Restaurar antes de apagar
            StartBatteryReload();
        }

        currentBatteryCharge = Mathf.Clamp(
            currentBatteryCharge,
            0f,
            maxBatteryCharge
        );
    }

    private void ResetFlashlightToNormal()
    {
        if (flashlight != null)
        {
            flashlight.intensity = normalIntensity;
            flashlight.spotAngle = normalSpotAngle;
        }
    }

    private void StartBatteryReload()
    {
        if (isReloading)
            return;

        // si no quedan baterías → apagar definitivamente
        if (totalBatteries <= 0)
        {
            if (flashlight != null)
                flashlight.enabled = false;

            Debug.Log("Sin baterías. Linterna apagada.");
            return;
        }

        isReloading = true;

        // apagar linterna inmediatamente
        if (flashlight != null) {
            flashlight.enabled = false;
        }

        if (boostLight != null)
        {
            boostLight.enabled = false;
        }

        // lanzar animación
        if (armsAnimator != null)
            armsAnimator.SetTrigger(reloadTriggerName);

        Debug.Log("Comenzando recarga...");
    }

    public void AnimationEvent_BatteryReloadFinished()
    {
        totalBatteries--;

        if (totalBatteries >= 0)
        {
            currentBatteryCharge = maxBatteryCharge;

            if (flashlight != null)
                flashlight.enabled = true;

            Debug.Log("Recarga terminada. Baterías restantes: " + totalBatteries);
        }

        UpdateBatteryText();
        UpdateUI();

        isReloading = false;
    }

    private void UpdateUI()
    {
        if (batteryBar != null)
            batteryBar.value = currentBatteryCharge;
    }

    private void UpdateBatteryText()
    {
        if (batteryCountText != null)
            batteryCountText.text = totalBatteries.ToString();
    }

    public void AddBattery(int amount)
    {
        totalBatteries += amount;
        UpdateBatteryText();

        Debug.Log("Baterías actuales: " + totalBatteries);
    }
    
    private void OnEnable()
    {
        if (boostAction != null)
        {
            boostAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (boostAction != null)
        {
            boostAction.action.Disable();
        }
    }
}