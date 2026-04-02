using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FearManager : MonoBehaviour
{
    [Header("UI y Miedo")]
    public Slider fearBar; // La barra visual en la pantalla
    public float maxFear = 100f;
    [Tooltip("Cuánto miedo suma por segundo en la oscuridad")]
    public float fearIncreaseRate = 5f; 
    [Tooltip("Cuánto miedo resta por segundo en la luz")]
    public float fearDecreaseRate = 10f; 
    
    private float currentFear = 0f;

    [Header("Linterna")]
    public Light flashlight; // Asigna el Spotlight (hijo de la cámara)
    public InputActionReference toggleFlashlightAction; // Tecla para encender/apagar (Ej: F)
    
    private bool isFlashlightOn = true;

    // Usamos un contador en lugar de un booleano por si el jugador se para 
    // en un lugar donde se cruzan dos luces externas a la vez.
    private int externalLightsCount = 0; 

    void Start()
    {
        currentFear = 0f;
        
        if (fearBar != null)
        {
            fearBar.maxValue = maxFear;
            fearBar.value = currentFear;
        }
        
        // Sincronizar el estado inicial de la linterna
        if (flashlight != null)
        {
            isFlashlightOn = flashlight.enabled;
        }
    }

    void Update()
    {
        // 1. Control de la Linterna
        if (toggleFlashlightAction != null && toggleFlashlightAction.action.WasPressedThisFrame())
        {
            isFlashlightOn = !isFlashlightOn;
            if (flashlight != null)
            {
                flashlight.enabled = isFlashlightOn;
            }
        }

        // 2. Lógica de Miedo (¿Estamos a salvo?)
        bool isSafeInLight = isFlashlightOn || externalLightsCount > 0;

        if (isSafeInLight)
        {
            currentFear -= fearDecreaseRate * Time.deltaTime; // Relajarse
        }
        else
        {
            currentFear += fearIncreaseRate * Time.deltaTime; // Asustarse
        }

        // Mantener el valor entre 0 y el máximo
        currentFear = Mathf.Clamp(currentFear, 0f, maxFear);

        // 3. Actualizar la UI
        if (fearBar != null)
        {
            fearBar.value = currentFear;
        }

        // 4. ¿Qué pasa si el miedo llega al máximo?
        if (currentFear >= maxFear)
        {
            // Aquí puedes agregar un Game Over, un sonido de grito, o reiniciar el nivel
            Debug.Log("¡Te moriste de miedo!");
        }
    }

    // --- DETECCIÓN DE ZONAS DE LUZ EXTERNAS ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeLight"))
        {
            externalLightsCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeLight"))
        {
            externalLightsCount--;
        }
    }

    // --- ACTIVAR INPUTS ---
    private void OnEnable()
    {
        if (toggleFlashlightAction != null) toggleFlashlightAction.action.Enable();
    }

    private void OnDisable()
    {
        if (toggleFlashlightAction != null) toggleFlashlightAction.action.Disable();
    }
}