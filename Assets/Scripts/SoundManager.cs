using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple sound manager.
/// It can play a noise and background loop, not much else.
/// Use Noise and Music enums for calling sound files.
/// </summary>
public class SoundManager : MonoBehaviour
{
    [HideInInspector] public static SoundManager Instance;
    [SerializeField] private bool ambientSound = true;
    [SerializeField] private bool soundEffects = true;
    [SerializeField] private AudioSource ambient, effects;


    private void Awake()
    {
        // setup singleton
        if (Instance != null && Instance != this) {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        InitializeAudioSources();

        SetAmbientSound(Music.DarkLoops);
    }


    /// <summary> Add AudioSources. </summary>
    private void InitializeAudioSources()
    {
        ambient = this.gameObject.AddComponent<AudioSource>();
        effects = this.gameObject.AddComponent<AudioSource>();
    }


    /// <summary> Sets ambient AudioClip. </summary>
    /// <param name="track">Music track to loop.</param>
    private void SetAmbientSound(Music track)
    {
       ambient.clip = (AudioClip)Resources.Load(GetSound(track));
    }


    // void Start()
    // {
    //     //
    // }


    private void Update()
    {
        UpdateAmbient();
    }


    /// <summary> Turn ambient sounds on or off as bool toggled. </summary>
    private void UpdateAmbient()
    {
        if (ambientSound && !ambient.isPlaying)
        {
            ambient.loop = true;
            ambient.volume = 1;
            ambient.Play();
        }
        else if (!ambientSound)
        {
            ambient.Stop();
        }
    }


    /// <summary> Play OneShot Sound. </summary>
    /// <param name="sound">Which sound to play.</param>
    /// <param name="volume">How loud? [0-1]</param>
    public void PlaySound(Noise sound, float volume)
    {
        if (!effects.isPlaying && soundEffects)
        {
            effects.PlayOneShot((AudioClip)Resources.Load(GetSound(sound)), volume);
        }
    }


    /// <summary> Convert Noise enum into file path string. </summary>
    /// <typeparam name="T">Music or Noise enum.</typeparam>
    /// <param name="sound">Which sound to load.</param>
    /// <returns></returns>
    private string GetSound<T>(T sound)
    {
        return "Sounds/" + sound.ToString();
    }


    // some sort of better storage for audio files is needed
    // I want an inspector modifiable Dictionary<string, AudioClip>
    // but I don't think such a thing exists.

    
    /// <summary> Game noise tracks. </summary>
    public enum Noise
    {
        Ping,
    }


    /// <summary> Game music tracks. </summary>
    public enum Music
    {
        DarkLoops,
    }
}


