using UnityEngine;
using UnityEngine.Events; // Necesario para los eventos en el Inspector

[RequireComponent(typeof(Collider))]
public class GenericTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El Tag que debe tener el objeto para activar esto (Ej: Player)")]
    public string targetTag = "Player";
    
    [Tooltip("¿El trigger desaparece para siempre después de usarse una vez?")]
    public bool destroyAfterActivation = true;

    [Header("Acciones a ejecutar")]
    [Tooltip("Lo que va a pasar cuando el jugador cruce este espacio")]
    public UnityEvent onTriggerEnterEvent;

    private void Start()
    {
        // Por seguridad, lo volvemos un Trigger
        GetComponent<Collider>().isTrigger = true;
        
        // Hacemos el cubo invisible automáticamente al darle Play
        if (TryGetComponent<MeshRenderer>(out MeshRenderer render))
        {
            render.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto que entra tiene el Tag correcto (Ej: "Player")
        if (other.CompareTag(targetTag))
        {
            // Disparamos todas las acciones configuradas en el Inspector
            onTriggerEnterEvent.Invoke();
            
            // Si está configurado para un solo uso, lo destruimos
            if (destroyAfterActivation)
            {
                Destroy(gameObject); 
            }
        }
    }
}