using UnityEngine;
using UnityEngine.Audio;
using TMPro; // Necesario para la UI de texto
using System.Collections;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance { get; private set; }

    [Header("Configuración de Audio")]
    public AudioSource voiceSource;
    public AudioMixer mainMixer;
    public string duckingParameter = "MusicVolume";

    [Header("Configuración de Subtítulos")]
    public TextMeshProUGUI subtitleTextUI; // Arrastrá acá tu texto de UI

    [Header("Frase 1: Inicio")]
    public AudioClip phrase1_Start;
    [TextArea] public string text1 = "Estoy atrapado... Tiene que haber una salida. No puedo quedarme aquí.";

    [Header("Frase 2: Enemigos")]
    public AudioClip phrase2_Enemies;
    [TextArea] public string text2 = "Esos ruidos... hay algo ahí fuera. Mejor me mantengo alejado.";

    [Header("Frase 3: El Libro")]
    public AudioClip phrase3_Book;
    [TextArea] public string text3 = "¡La luz! ... Si enfoco el haz, creo que puedo conducirlas hacia atrás.";

    [Header("Frase 4: El Camión")]
    public AudioClip phrase4_Truck;
    [TextArea] public string text4 = "Ese camión... Si logro llegar hasta allí, tal vez pueda escapar de esta pesadilla.";

    private bool hasSeenTruck = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ocultar el texto de subtítulos al inicio
        if (subtitleTextUI != null) subtitleTextUI.text = "";

        // Disparamos las automáticas
        StartCoroutine(PlayVoiceRoutine(phrase1_Start, text1, 1f));
        StartCoroutine(PlayVoiceRoutine(phrase2_Enemies, text2, 10f));
    }

    public void PlayBookPhrase()
    {
        StartCoroutine(PlayVoiceRoutine(phrase3_Book, text3, 0f));
    }

    public void PlayTruckPhrase()
    {
        if (!hasSeenTruck)
        {
            hasSeenTruck = true;
            StartCoroutine(PlayVoiceRoutine(phrase4_Truck, text4, 0f));
        }
    }

    private IEnumerator PlayVoiceRoutine(AudioClip clip, string subtitleLine, float delay)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        
        if (voiceSource.isPlaying) yield return new WaitUntil(() => !voiceSource.isPlaying);

        // Preparamos audio y subtítulo
        voiceSource.clip = clip;
        if (subtitleTextUI != null) subtitleTextUI.text = subtitleLine;
        
        voiceSource.Play();

        // Ducking In
        float elapsedTime = 0f;
        float fadeTime = 0.5f;
        mainMixer.GetFloat(duckingParameter, out float startVol);
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            mainMixer.SetFloat(duckingParameter, Mathf.Lerp(startVol, -15f, elapsedTime / fadeTime));
            yield return null;
        }

        // Espera lo que dure la frase
        yield return new WaitForSeconds(clip.length);

        // Limpia el subtítulo de la pantalla
        if (subtitleTextUI != null) subtitleTextUI.text = "";

        // Ducking Out
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            mainMixer.SetFloat(duckingParameter, Mathf.Lerp(-15f, 0f, elapsedTime / fadeTime));
            yield return null;
        }
    }
}