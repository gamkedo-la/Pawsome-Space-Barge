using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [HideInInspector] public static SoundManager Instance;
    [SerializeField] bool ambientSound = true;
    [SerializeField] bool soundEffects = true;
    [SerializeField] AudioSource ambient, effects;


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
    }


    // void Start()
    // {
    //     //
    // }


    void Update()
    {
        UpdateAmbient(Noise.DarkLoops);
    }


    // add AudioSources
    private void InitializeAudioSources()
    {
        ambient = this.gameObject.AddComponent<AudioSource>();
        effects = this.gameObject.AddComponent<AudioSource>();
    }


    // turn ambient sounds on/off
    private void UpdateAmbient(Noise backgroundAmbiance)
    {
        if (ambientSound && !ambient.isPlaying)
        {
            ambient.clip = (AudioClip)Resources.Load(GetSound(backgroundAmbiance));
            ambient.loop = true;
            ambient.volume = 1;
            ambient.Play();
        }
        else if (!ambientSound)
        {
            ambient.Stop();
        }
    }


    // play OneShot sound
    public void PlaySound(Noise sound, float volume)
    {
        if (!effects.isPlaying && soundEffects)
        {
            effects.PlayOneShot((AudioClip)Resources.Load(GetSound(sound)), volume);
        }
    }


    // convert Noise enum into string
    private string GetSound(Noise sound)
    {
        return "Sounds/" + sound.ToString();
    }


    // enums for file names
    public enum Noise {
        DarkLoops,
        Ping
    }
}


