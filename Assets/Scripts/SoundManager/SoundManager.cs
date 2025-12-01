using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
                CreateSingleton();
            return _instance;
        }
    }
    private static SoundManager _instance;

    private string currentMusicID = "";

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("Mixer Parameters")]
    public string masterVolumeParam = "MasterVolume";
    public string musicVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "SFXVolume";
    public string uiVolumeParam = "UIVolume";
    public string ambienceVolumeParam = "AmbienceVolume";
    public string continuousVolumeParam = "ContinuousVolume";

    [Header("Mixer Groups")]
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup uiGroup;
    public AudioMixerGroup ambienceGroup;
    public AudioMixerGroup continuousGroup;

    [Header("Audio Library")]
    public AudioLibrary library;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource uiSource;
    private AudioSource ambienceSource;

    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private bool isUsingA = true;

    private Dictionary<string, AudioSource> continuousSources = new Dictionary<string, AudioSource>();

    // Tried multiple fades at once → caused issues, so I track only the active one
    private Coroutine currentFadeRoutine = null;

    private void Awake()
    {
        // Basic singleton setup (keeps one manager alive across scenes)
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
        LoadSavedVolumes();

        // Connecting automatic music change on scene load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    void SetupSources()
    {
        // Splitting into two music sources → allows clean crossfades
        musicSourceA = CreateSource("Music_A", musicGroup);
        musicSourceB = CreateSource("Music_B", musicGroup);

        musicSourceA.loop = true;
        musicSourceB.loop = true;

        musicSourceA.volume = 0f;
        musicSourceB.volume = 0f;

        musicSource = musicSourceA;

        // SFX sources
        sfxSource = CreateSource("SFXSource", sfxGroup);
        uiSource = CreateSource("UISource", uiGroup);
        ambienceSource = CreateSource("AmbienceSource", ambienceGroup);
    }


    AudioSource CreateSource(string name, AudioMixerGroup group)
    {
        // Just spawning child objects to keep everything organized
        GameObject obj = new GameObject(name);
        obj.transform.parent = transform;

        AudioSource src = obj.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.outputAudioMixerGroup = group;

        return src;
    }


    public void PlaySFX(string id)
    {
        PlayOneShot(id, sfxSource);
    }

    public AudioSource GetSFXSource()
    {
        return sfxSource;
    }

    public void PlayUI(string id)
    {
        PlayOneShot(id, uiSource);
    }

    public void PlayAmbience(string id, bool loop = true)
    {
        PlayLoop(id, ambienceSource, loop);
    }

    public void PlayMusic(string id, bool loop = true)
    {
        // Avoid restarting same track
        if (currentMusicID == id && musicSource.isPlaying)
            return;

        AudioEvent evt = library.Get(id);
        if (evt == null) return;

        currentMusicID = id;

        musicSource.clip = evt.clip;
        musicSource.volume = evt.volume;
        musicSource.pitch = evt.GetPitch();
        musicSource.loop = loop;
        musicSource.Play();
    }


    void PlayOneShot(string id, AudioSource source)
    {
        AudioEvent evt = library.Get(id);
        if (evt == null) 
        {
            Debug.LogWarning($"[SoundManager] AudioEvent id '{id}' not found in AudioLibrary");
            return;
        }

        source.pitch = evt.GetPitch();
        source.PlayOneShot(evt.clip, evt.volume);
    }


    void PlayLoop(string id, AudioSource source, bool loop)
    {
        AudioEvent evt = library.Get(id);
        if (evt == null) return;

        source.clip = evt.clip;
        source.pitch = evt.GetPitch();
        source.volume = evt.volume;
        source.loop = loop;
        source.Play();
    }


    public void SetVolume(string mixerParam, float value)
    {
        // Converting 0–1 linear slider to dB
        value = Mathf.Clamp(value, 0.0001f, 1f);
        mixer.SetFloat(mixerParam, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(mixerParam, value);
    }

    public float GetVolume(string mixerParam, float defaultValue = 0.75f)
    {
        return PlayerPrefs.GetFloat(mixerParam, defaultValue);
    }


    void LoadSavedVolumes()
    {
        SetVolume(masterVolumeParam, GetVolume(masterVolumeParam));
        SetVolume(musicVolumeParam, GetVolume(musicVolumeParam));
        SetVolume(sfxVolumeParam, GetVolume(sfxVolumeParam));
        SetVolume(uiVolumeParam, GetVolume(uiVolumeParam));
        SetVolume(ambienceVolumeParam, GetVolume(ambienceVolumeParam));
    }


    public void PlayContinuous(string id, float volume = 1f)
    {
        // If already playing — just adjust volume
        if (continuousSources.ContainsKey(id))
        {
            continuousSources[id].volume = volume;
            return;
        }

        // Spawning a new continuous source
        AudioSource src = CreateSource("Continuous_" + id, continuousGroup);
        src.loop = true;

        AudioEvent evt = library.Get(id);
        if (evt == null) return;

        src.clip = evt.clip;
        src.volume = volume;
        src.pitch = evt.GetPitch();
        src.Play();

        continuousSources[id] = src;
    }


    public void StopContinuous(string id)
    {
        if (!continuousSources.ContainsKey(id)) return;

        AudioSource src = continuousSources[id];
        src.Stop();
        Destroy(src.gameObject);

        continuousSources.Remove(id);
    }


    // Crossfading system — refined after several failed attempts :)
    public void FadeMusic(string id, float fadeTime = 1f)
    {
        // Prevent stacking multiple fades → only allow one active
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
        }

        AudioEvent evt = library.Get(id);
        if (evt == null) return;

        // Switch A/B channels manually for smoother transitions
        AudioSource newSource = isUsingA ? musicSourceB : musicSourceA;
        AudioSource oldSource = isUsingA ? musicSourceA : musicSourceB;
        isUsingA = !isUsingA;

        newSource.clip = evt.clip;
        newSource.volume = 0f;
        newSource.pitch = evt.GetPitch();
        newSource.loop = true;
        newSource.Play();

        musicSource = newSource;
        currentMusicID = id;

        currentFadeRoutine = StartCoroutine(FadeRoutine(oldSource, newSource, fadeTime));
    }


    private IEnumerator FadeRoutine(AudioSource oldSrc, AudioSource newSrc, float time)
    {
        float t = 0f;

        float oldStart = oldSrc != null ? oldSrc.volume : 0f;
        float newStart = newSrc != null ? newSrc.volume : 0f;

        // Simple linear blend after some experimentation
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / time);

            if (oldSrc != null)
                oldSrc.volume = Mathf.Lerp(oldStart, 0f, k);

            if (newSrc != null)
                newSrc.volume = Mathf.Lerp(newStart, 1f, k);

            yield return null;
        }

        if (oldSrc != null)
        {
            oldSrc.volume = 0f;
            oldSrc.Stop();
        }

        if (newSrc != null)
            newSrc.volume = 1f;

        currentFadeRoutine = null;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Just auto-playing default music for specific scenes
        if (scene.name == "MainMenu" || scene.name == "Level1")
            SoundManager.Instance.FadeMusic("bgm_cannon", 1f);
    }


    private static void CreateSingleton()
    {
        // If one exists in scene, re-use it
        SoundManager existing = Object.FindFirstObjectByType<SoundManager>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        // Otherwise try to auto-spawn from Resources
        SoundManager prefab = Resources.Load<SoundManager>("SoundManager");
        if (prefab != null)
        {
            _instance = Instantiate(prefab);
            _instance.gameObject.name = "SoundManager (AutoCreated)";
            return;
        }

        // As last fallback, create empty version
        Debug.LogError("SoundManager prefab not found in Resources folder!");
    }

}
