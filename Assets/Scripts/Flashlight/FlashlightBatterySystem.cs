using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightBatterySystem : MonoBehaviour
{
    [Header("UI")]
    public Slider batteryBar;
    public TMP_Text batteryCountText;

    [Header("Battery Settings")]
    public float maxBatteryCharge = 100f;
    public float batteryDrainRate = 10f;
    public int totalBatteries = 3;

    [Header("References")]
    public Light flashlight;
    public Animator armsAnimator;

    [Header("Animation")]
    [Tooltip("Nombre del trigger de recarga en el Animator")]
    public string reloadTriggerName = "reload";

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
    }

    private void Update()
    {
        if (isReloading)
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
            return;

        currentBatteryCharge -= batteryDrainRate * Time.deltaTime;

        if (currentBatteryCharge <= 0f)
        {
            currentBatteryCharge = 0f;
            StartBatteryReload();
        }

        currentBatteryCharge = Mathf.Clamp(
            currentBatteryCharge,
            0f,
            maxBatteryCharge
        );
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
        if (flashlight != null)
            flashlight.enabled = false;

        // lanzar animación
        if (armsAnimator != null)
            armsAnimator.SetTrigger(reloadTriggerName);

        Debug.Log("Comenzando recarga...");
    }

    /*
        =====================================
        ANIMATION EVENT
        =====================================

        Este método debe llamarse desde el final
        del clip de animación de reload.
    */

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
}