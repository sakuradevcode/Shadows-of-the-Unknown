using UnityEngine;
using System.Collections;
using TMPro; // 1. Obligatorio para leer el TextMeshPro

[RequireComponent(typeof(Collider))]
public class UVReveal : MonoBehaviour
{
    public enum CodeType
    {
        FragmentCode,
        FullCode
    }

    [Header("Configuración de tipo de Puzzle")]
    public  CodeType codeType;
    
    [Header("Configuración de Luz")]
    public Light uvFlashlight;
    public float revealDistance = 5f;
    public float lightRadius = 1f;

    [Header("El Secreto")]
    public GameObject hiddenObject;
    
    [Header("Texto a Extraer")]
    public TextMeshPro textMeshPro; // 2. Arrastrá acá tu TextMeshPro desde el inspector

    private Collider myCollider;
    private bool isCurrentlyVisible = false;
    private bool alreadyFound = false; // 3. Candado para no enviar el número infinitas veces

    void Start()
    {
        myCollider = GetComponent<Collider>();
        myCollider.isTrigger = true;

        if (hiddenObject != null) hiddenObject.SetActive(false);
        
        // Si te olvidaste de arrastrarlo en el inspector, intenta buscarlo automáticamente
        if (textMeshPro == null) textMeshPro = GetComponentInChildren<TextMeshPro>(true);

        StartCoroutine(CheckUVRoutine());
    }
    
    IEnumerator CheckUVRoutine()
    {
        while (true)
        {
            bool shouldBeVisible = false;

            // --- VALIDACIÓN DE MISIÓN Y TIPO DE CÓDIGO ---
            // Si es FullCode, siempre se puede ver. Si es FragmentCode, depende de que la misión esté completa.
            bool canBeRevealed = (codeType == CodeType.FullCode) || MissionManager.Instance.IsMissionCompleted("bathroom_door");

            if (canBeRevealed)
            {
                // Solo si cumple la condición, calculamos los rayos de la linterna
                if (uvFlashlight != null && uvFlashlight.enabled && uvFlashlight.gameObject.activeInHierarchy)
                {
                    Vector3 origin = uvFlashlight.transform.position - (uvFlashlight.transform.forward * 0.2f);
                    
                    RaycastHit[] hits = Physics.SphereCastAll(origin, lightRadius, uvFlashlight.transform.forward, revealDistance + 0.2f);
                    
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider == myCollider)
                        {
                            shouldBeVisible = true;
                            break; // Rompemos el loop porque ya sabemos que hay que mostrarlo
                        }
                    }
                    
                    if (!shouldBeVisible)
                    {
                        Collider[] overlaps = Physics.OverlapSphere(uvFlashlight.transform.position, lightRadius / 2f);
                        foreach (Collider col in overlaps)
                        {
                            if (col == myCollider)
                            {
                                shouldBeVisible = true;
                                break;
                            }
                        }
                    }
                }
            } // Fin de la validación

            // --- LÓGICA DE REVELADO Y EXTRACCIÓN ---
            if (shouldBeVisible != isCurrentlyVisible)
            {
                isCurrentlyVisible = shouldBeVisible;
                if (hiddenObject != null) hiddenObject.SetActive(isCurrentlyVisible);
                
                // Si lo acabamos de hacer visible, y todavía no lo encontramos antes
                if (isCurrentlyVisible && !alreadyFound)
                {
                    if (codeType == CodeType.FragmentCode)
                    {
                        if (textMeshPro != null)
                        {
                            char number = textMeshPro.text.Trim()[0];
                            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(textMeshPro.color);

                            MissionManager.Instance.FindCodeFragment(number, hexColor);
                            alreadyFound = true;
                        }
                    }
                    else
                    {
                        MissionManager.Instance.CodeFound(); // Método para el código completo
                        alreadyFound = true; 
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}