using UnityEngine;

// Esto añade automáticamente un Collider si se te olvida ponerlo
[RequireComponent(typeof(Collider))] 
public class UVReveal : MonoBehaviour
{
    [Header("Configuración de Luz")]
    [Tooltip("La linterna (Spotlight) del jugador")]
    public Light uvFlashlight; 
    [Tooltip("Distancia máxima a la que llega la luz UV")]
    public float revealDistance = 5f;
    [Tooltip("Grosor del rayo de luz (1f es un buen tamaño para linternas)")]
    public float lightRadius = 1f; 

    [Header("El Secreto")]
    [Tooltip("El objeto a ocultar/mostrar (Ej: El texto con el número)")]
    public GameObject hiddenObject;

    private Collider myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider>();
        // Lo volvemos Trigger por código para asegurarnos de que el jugador 
        // no choque físicamente con este objeto invisible al caminar.
        myCollider.isTrigger = true; 

        if (hiddenObject != null) hiddenObject.SetActive(false);
    }

    void Update()
    {
        bool isIlluminated = false;

        // 1. Verificamos si la linterna existe y está encendida
        if (uvFlashlight != null && uvFlashlight.enabled && uvFlashlight.gameObject.activeInHierarchy)
        {
            RaycastHit hit;
            
            // 2. Disparamos el SphereCast (Un cilindro invisible desde la linterna hacia adelante)
            if (Physics.SphereCast(uvFlashlight.transform.position, lightRadius, uvFlashlight.transform.forward, out hit, revealDistance))
            {
                // 3. Si la luz choca exactamente con ESTE objeto, encendemos el texto
                if (hit.collider == myCollider)
                {
                    isIlluminated = true;
                }
            }
        }

        // 4. Aplicamos el estado al texto (encendido o apagado)
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(isIlluminated);
        }
    }
}