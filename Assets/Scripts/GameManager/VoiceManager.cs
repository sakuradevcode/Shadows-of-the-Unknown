using UnityEngine;
using UnityEngine.Audio;
using TMPro; 
using System.Collections;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance { get; private set; }

    [Header("Configuración de Audio")]
    public AudioSource voiceSource;
    public AudioMixer mainMixer;
    // Ahora tenemos dos parámetros expuestos
    public string musicDuckingParameter = "MusicVolume"; 
    public string sfxDuckingParameter = "SFXVolume";     

    [Header("Configuración de Subtítulos")]
    public TextMeshProUGUI subtitleTextUI; 

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
        if (subtitleTextUI != null) subtitleTextUI.text = "";

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

        voiceSource.clip = clip;
        if (subtitleTextUI != null) subtitleTextUI.text = subtitleLine;
        
        voiceSource.Play();
        
        float elapsedTime = 0f;
        float fadeTime = 0.5f;
        
        mainMixer.GetFloat(musicDuckingParameter, out float startVolMusic);
        mainMixer.GetFloat(sfxDuckingParameter, out float startVolSFX);
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime; // Normalizamos el tiempo entre 0 y 1
            
            mainMixer.SetFloat(musicDuckingParameter, Mathf.Lerp(startVolMusic, -15f, t));
            mainMixer.SetFloat(sfxDuckingParameter, Mathf.Lerp(startVolSFX, -15f, t));
            yield return null;
        }
        
        yield return new WaitForSeconds(clip.length);

        if (subtitleTextUI != null) subtitleTextUI.text = "";
        
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            
            mainMixer.SetFloat(musicDuckingParameter, Mathf.Lerp(-15f, startVolMusic, t));
            mainMixer.SetFloat(sfxDuckingParameter, Mathf.Lerp(-15f, startVolSFX, t));
            yield return null;
        }
    }
}