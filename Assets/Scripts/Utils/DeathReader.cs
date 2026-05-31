using UnityEngine;
using TMPro; // Para usar textos profesionales

public class DeathReader : MonoBehaviour
{
    public TextMeshProUGUI textReason; // Arrastrá acá tu texto

    void Start()
    {
        if (textReason != null)
        {
            // Lee el post-it. Si por algún error no lo encuentra, dice "HAS MUERTO"
            textReason.text = PlayerPrefs.GetString("ReasonDeath", "HAS MUERTO");
        }
    }
}