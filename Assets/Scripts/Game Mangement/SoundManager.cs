using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple sound manager.
/// It can play a noise and background loop, not much else.
/// Add audio files in inspector window.
/// </summary>
public class SoundManager : MonoBehaviour
{
    [HideInInspector] public static SoundManager Instance;
    [SerializeField] private bool ambientSound = true;
    [SerializeField][Range(0,1)] private float ambientVolume = 1;
    [SerializeField] private string defaultAmbientSound;
    [SerializeField] private float crossFadeSpeed = 0.2f;
    [SerializeField] private bool soundEffects = true;
    private AudioSource ambient, effects, shipThrusters;

    [Tooltip("Dictionary of game sounds.")]
    [SerializeField] private List<AudioClipStruct<string, AudioClip>> audioClips
        = new List<AudioClipStruct<string, AudioClip>>();

    // actual dictionary used for audio lookup
    private Dictionary<string, AudioClip> audioLookup;

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

        // turn List<> from inspector into usable Dictionary<>
        audioLookup = new Dictionary<string, AudioClip>();
        foreach (var entry in audioClips)
        {
            audioLookup[entry.name] = entry.file;
        }

        InitializeAudioSources();

        SetAmbientSound(defaultAmbientSound);
    }


    /// <summary> Add AudioSources. </summary>
    private void InitializeAudioSources()
    {
        ambient = this.gameObject.AddComponent<AudioSource>();
        effects = this.gameObject.AddComponent<AudioSource>();
        shipThrusters = this.gameObject.AddComponent<AudioSource>();
    }


    private void Update()
    {
        UpdateAmbient();
    }


    /// <summary> Turn ambient sounds on or off as bool toggled. </summary>
    private void UpdateAmbient()
    {
        if (ambientSound)
        {
            ambient.volume = ambientVolume;
            if (!ambient.isPlaying)
            {
                ambient.loop = true;
                ambient.Play();
            }
        }
        else
        {
            ambient.Stop();
        }
    }


    /// <summary> Sets ambient AudioClip. </summary>
    /// <param name="track">Music track to loop.</param>
    public void SetAmbientSound(string track)
    {
        var nextClip = audioLookup[string.IsNullOrEmpty(track) ? defaultAmbientSound : track];
        if (nextClip == ambient.clip) return;
        
        if (ambient.isPlaying)
        {
            StartCoroutine(SwitchAmbientTracks(nextClip));
        }
        else
        {
            ambient.clip = nextClip;
        }
    }

    private IEnumerator SwitchAmbientTracks(AudioClip nextClip)
    {
        var originalVolume = ambientVolume;
        while (ambientVolume > 0)
        {
            ambientVolume = Mathf.Max(0f, ambientVolume - crossFadeSpeed * Time.deltaTime);
            yield return null;
        }

        ambient.Stop();
        ambient.clip = nextClip;
        
        while (ambientVolume < originalVolume)
        {
            ambientVolume = Mathf.Min(originalVolume, ambientVolume + crossFadeSpeed * Time.deltaTime);
            yield return null;
        }
    }


    /// <summary> Play OneShot Sound. </summary>
    /// <param name="sound">Which sound to play.</param>
    /// <param name="volume">How loud? [0-1]</param>
    public void PlaySound(string sound, float volume)
    {
        if (!effects.isPlaying && soundEffects)
        {
            effects.PlayOneShot(audioLookup[sound], volume);
        }
    }

    /// <summary> Play A Sound ON TOP OF OTHERS. </summary>
    /// <param name="sound">Which sound to play.</param>
    /// <param name="volume">How loud? [0-1]</param>
    public void PlayShipThrusters(string sound, float volume)
    {
        if (!shipThrusters.isPlaying && soundEffects)
        {
            shipThrusters.loop = true;
            shipThrusters.PlayOneShot(audioLookup[sound], volume);
        }
    }
    
    
    /// <summary> Play A Sound ON TOP OF OTHERS. </summary>
    /// <param name="sound">Which sound to play.</param>
    /// <param name="volume">How loud? [0-1]</param>
    public void AdjustThrusterDirection(float pan)
    {
        shipThrusters.panStereo = pan;
        if (pan is < -.1f or > .1f )
            shipThrusters.pitch = 1;
        else
            shipThrusters.pitch = 1.07f;
    }
    
    /// <summary> STOP PLAYING A Sound ON 2ND LAYER. </summary>
    /// <param name="sound">Which sound to stop playing.</param>
    public void StopEffects()
    {
        effects.Stop();
        effects.loop = false;
    }    
    
    /// <summary> STOP PLAYING A Sound ON 2ND LAYER. </summary>
    /// <param name="sound">Which sound to stop playing.</param>
    public void StopThrusters()
    {
        shipThrusters.Stop();
        shipThrusters.loop = false;
    }
}


/// <summary>
/// Part of serializable Dictionary for AudioClips.
/// <see href="https://buildingblocksgamedesign.com/serialize-a-dictionary-in-unity/">
/// </summary>
/// <typeparam name="String"></typeparam>
/// <typeparam name="AudioClip"></typeparam>
[Serializable]
public struct AudioClipStruct<String, AudioClip>
{
    // contsructor
    public AudioClipStruct(string clipName, AudioClip clipFile) => (name, file) = (clipName, clipFile);

    [Tooltip("Name of the audio file for in-game lookup.")]
    [field: SerializeField] public string name { get; private set; }

    [Tooltip("Audio file.")]
    [field: SerializeField] public AudioClip file { get; private set; }

    // deconstructor
    public void Destructor(out string clipName, out AudioClip clipFile) => (clipName, clipFile) = (name, file);
}
