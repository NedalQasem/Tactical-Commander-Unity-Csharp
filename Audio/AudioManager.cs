using UnityEngine;
using System.Collections.Generic;

public enum SoundType
{
    // UI
    ButtonClick,
    Hover,
    Victory,
    Defeat,

    // Units
    UnitAttack,
    UnitHit,
    UnitDie,
    UnitSpawn,

    // Buildings
    BuildingPlaced,
    BuildingDestroyed,

    // Misc
    GoldGain
}

[System.Serializable]
public class SoundClip
{
    public SoundType type;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Settings")]
    public AudioClip defaultBGM; // 🎵 Simply drag your music file here!
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Update()
    {
        // 🎚️ Real-time Volume Adjustment
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    [Header("Sound Library")]
    public List<SoundClip> soundLibrary;

    private Dictionary<SoundType, SoundClip> soundDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 🛠️ Auto-Setup AudioSources
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            InitializeLibrary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-Play Background Music
        if (defaultBGM != null)
        {
            PlayMusic(defaultBGM);
        }
    }

    void InitializeLibrary()
    {
        soundDictionary = new Dictionary<SoundType, SoundClip>();
        foreach (var sound in soundLibrary)
        {
            if (!soundDictionary.ContainsKey(sound.type))
            {
                soundDictionary.Add(sound.type, sound);
            }
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource == null) return;
        
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Play Global 2D Sound
    public void PlaySFX(SoundType type)
    {
        if (sfxSource == null) return;

        if (soundDictionary.TryGetValue(type, out SoundClip sound))
        {
            if (sound.clip != null)
            {
                sfxSource.pitch = sound.pitch; 
                sfxSource.PlayOneShot(sound.clip, sound.volume);
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Sound {type} playing failed (NotFound).");
        }
    }

    // Play 3D Sound at Position (Combat, Explosions)
    public void PlaySFXAt(SoundType type, Vector3 position)
    {
        if (soundDictionary.TryGetValue(type, out SoundClip sound))
        {
            if (sound.clip != null)
            {
                //  Custom 3D Sound Creation
                GameObject audioObj = new GameObject($"SFX_{type}");
                audioObj.transform.position = position;
                
                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = sound.clip;
                source.volume = sound.volume;
                source.pitch = sound.pitch;
                
                
                source.spatialBlend = 1f; // Fully 3D
                source.minDistance = 8f;  // Hear fully within 8 meters (Adjusted for camera height)
                source.maxDistance = 50f; // Fade out at 50 meters
                source.rolloffMode = AudioRolloffMode.Linear; 
                
                source.Play();
                
                // Cleanup
                Destroy(audioObj, sound.clip.length / source.pitch + 0.1f);
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = volume;
    }
}
