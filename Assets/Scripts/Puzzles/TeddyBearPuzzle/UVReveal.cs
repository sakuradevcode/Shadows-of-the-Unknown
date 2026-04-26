using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class UVReveal : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Light uvFlashlight;
    public float revealDistance = 5f;
    public float lightRadius = 1f;

    [Header("El Secreto")]
    public GameObject hiddenObject;

    private Collider myCollider;
    private bool isCurrentlyVisible = false;

    void Start()
    {
        myCollider = GetComponent<Collider>();
        myCollider.isTrigger = true;

        if (hiddenObject != null) hiddenObject.SetActive(false);
        StartCoroutine(CheckUVRoutine());
    }
    
    IEnumerator CheckUVRoutine()
    {
        while (true)
        {
            bool shouldBeVisible = false;

            if (uvFlashlight != null && uvFlashlight.enabled && uvFlashlight.gameObject.activeInHierarchy)
            {
                Vector3 origin = uvFlashlight.transform.position - (uvFlashlight.transform.forward * 0.2f);
                
                RaycastHit[] hits = Physics.SphereCastAll(origin, lightRadius, uvFlashlight.transform.forward, revealDistance + 0.2f);
                
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider == myCollider)
                    {
                        shouldBeVisible = true;
                        break;
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

            if (shouldBeVisible != isCurrentlyVisible)
            {
                isCurrentlyVisible = shouldBeVisible;
                if (hiddenObject != null) hiddenObject.SetActive(isCurrentlyVisible);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}