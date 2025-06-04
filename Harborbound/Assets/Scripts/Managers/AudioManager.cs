using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public enum SoundType
    {
        Bite,
        Shoot,
        Cast,
        CastSplash,
        Full_Inventory,
        Dad_Dialog,
        Pirate_Aggro,
        Sell,
        Buy,
        Death,
        Boat_Engine,
        Boat_Collide,
        Music_Island,
        Music_Island_Loop,
        Music_Ocean,
        Music_Ocean_Loop,
        Music_Battle,
        Music_Battle_Loop,
        Hurt_Player,
        Hurt_Pirate,
        // Add more sound types as needed
    }

    [System.Serializable]
    public class Sound
    {
        public SoundType Type;
        public AudioClip[] Clips; // Array of clips instead of single clip

        [Range(0f, 1f)]
        public float Volume = 1f;

        [HideInInspector]
        public AudioSource Source;

        // Get a random clip from the array
        public AudioClip GetRandomClip()
        {
            if (Clips == null || Clips.Length == 0) return null;
            return Clips[Random.Range(0, Clips.Length)];
        }
    }

    public static AudioManager Instance;
    public Sound[] AllSounds;

    private Dictionary<SoundType, Sound> _soundDictionary = new Dictionary<SoundType, Sound>();
    private Dictionary<SoundType, AudioSource> _persistentSounds = new Dictionary<SoundType, AudioSource>();
    private AudioSource _musicSource;
    private bool _isPlayingIntro = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //Set up sounds
        foreach (var s in AllSounds)
        {
            _soundDictionary[s.Type] = s;
        }
    }

    public void Play(SoundType type)
    {
        if (!_soundDictionary.TryGetValue(type, out Sound sound))
        {
            Debug.LogWarning($"Sound type {type} not found!");
            return;
        }

        AudioClip clipToPlay = sound.GetRandomClip();
        if (clipToPlay == null)
        {
            Debug.LogWarning($"No clips found for sound type {type}!");
            return;
        }

        //Creates a new sound object
        var soundObj = new GameObject($"Sound_{type}");
        var audioSrc = soundObj.AddComponent<AudioSource>();

        //Assigns your sound properties
        audioSrc.clip = clipToPlay;
        audioSrc.volume = sound.Volume;

        //Play the sound
        audioSrc.Play();

        //Destroy the object
        Destroy(soundObj, clipToPlay.length);
    }

    // Play a sound that persists until manually stopped
    public void PlayPersistent(SoundType type, bool loop = false)
    {
        // If this sound is already playing, don't start another
        if (_persistentSounds.ContainsKey(type) && _persistentSounds[type] != null && _persistentSounds[type].isPlaying)
        {
            return;
        }

        if (!_soundDictionary.TryGetValue(type, out Sound sound))
        {
            Debug.LogWarning($"Sound type {type} not found!");
            return;
        }

        AudioClip clipToPlay = sound.GetRandomClip();
        if (clipToPlay == null)
        {
            Debug.LogWarning($"No clips found for sound type {type}!");
            return;
        }

        // Stop any existing persistent sound of this type
        StopPlay(type);

        // Create persistent sound object
        var soundObj = new GameObject($"PersistentSound_{type}");
        var audioSrc = soundObj.AddComponent<AudioSource>();

        // Assign sound properties
        audioSrc.clip = clipToPlay;
        audioSrc.volume = sound.Volume;
        audioSrc.loop = loop;

        // Play the sound
        audioSrc.Play();

        // Store reference for later stopping
        _persistentSounds[type] = audioSrc;

        // If not looping, clean up when finished
        if (!loop)
        {
            StartCoroutine(CleanupPersistentSound(type, clipToPlay.length));
        }
    }

    // Stop a specific persistent sound
    public void StopPlay(SoundType type)
    {
        if (_persistentSounds.TryGetValue(type, out AudioSource audioSource) && audioSource != null)
        {
            audioSource.Stop();
            Destroy(audioSource.gameObject);
            _persistentSounds.Remove(type);
        }
    }

    // Check if a persistent sound is currently playing
    public bool IsPlaying(SoundType type)
    {
        return _persistentSounds.ContainsKey(type) &&
               _persistentSounds[type] != null &&
               _persistentSounds[type].isPlaying;
    }

    private IEnumerator CleanupPersistentSound(SoundType type, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_persistentSounds.ContainsKey(type))
        {
            var audioSource = _persistentSounds[type];
            if (audioSource != null)
            {
                Destroy(audioSource.gameObject);
            }
            _persistentSounds.Remove(type);
        }
    }

    //Call this method to change music tracks (plays intro first, then loops)
    public void ChangeMusic(SoundType introType)
    {
        Debug.Log($"[AudioManager] ChangeMusic called with: {introType}");

        // stop all coroutines
        StopAllCoroutines();

        // Stop any current music
        if (_musicSource != null)
        {
            Debug.Log($"[AudioManager] Stopping current music");
            _musicSource.Stop();
        }

        // Find the intro track
        if (!_soundDictionary.TryGetValue(introType, out Sound introTrack))
        {
            Debug.LogWarning($"Music track {introType} not found!");
            return;
        }

        AudioClip introClip = introTrack.GetRandomClip();
        if (introClip == null)
        {
            Debug.LogWarning($"No clips found for music track {introType}!");
            return;
        }

        Debug.Log($"[AudioManager] Playing intro clip: {introClip.name}");

        // Create music source if it doesn't exist
        if (_musicSource == null)
        {
            var container = new GameObject("SoundTrackObj");
            DontDestroyOnLoad(container);
            _musicSource = container.AddComponent<AudioSource>();
            Debug.Log($"[AudioManager] Created new music source");
        }

        // Play the intro with fade in
        _musicSource.clip = introClip;
        _musicSource.volume = 0f;
        _musicSource.loop = false;
        _musicSource.Play();
        _isPlayingIntro = true;

        Debug.Log($"[AudioManager] Started intro, length: {introClip.length}s");

        // Start coroutines for fade in and waiting for loop
        StartCoroutine(FadeIn(introTrack.Volume, 1f));
        StartCoroutine(WaitForIntroAndPlayLoop(introType));
    }

    private IEnumerator FadeIn(float targetVolume, float fadeTime)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / fadeTime);
            yield return null;
        }

        _musicSource.volume = targetVolume;
    }

    private IEnumerator WaitForIntroAndPlayLoop(SoundType introType)
    {
        Debug.Log($"[AudioManager] Waiting for intro to finish: {introType}");

        // Wait for the intro to finish
        yield return new WaitForSeconds(_musicSource.clip.length);

        Debug.Log($"[AudioManager] Intro finished, _isPlayingIntro: {_isPlayingIntro}");

        // If we're still playing this intro (user didn't change music)
        if (_isPlayingIntro)
        {
            // Determine the loop type based on intro type
            SoundType loopType = GetLoopType(introType);
            Debug.Log($"[AudioManager] Switching from {introType} to loop: {loopType}");

            if (_soundDictionary.TryGetValue(loopType, out Sound loopTrack))
            {
                AudioClip loopClip = loopTrack.GetRandomClip();
                if (loopClip != null)
                {
                    Debug.Log($"[AudioManager] Playing loop clip: {loopClip.name}");
                    _musicSource.clip = loopClip;
                    _musicSource.volume = loopTrack.Volume;
                    _musicSource.loop = true;
                    _musicSource.Play();
                    _isPlayingIntro = false;
                }
                else
                {
                    Debug.LogWarning($"No clips found for loop track {loopType}!");
                }
            }
            else
            {
                Debug.LogWarning($"Loop track {loopType} not found!");
            }
        }
        else
        {
            Debug.Log($"[AudioManager] Not playing intro anymore, skipping loop");
        }
    }

    private SoundType GetLoopType(SoundType introType)
    {
        SoundType loopType = introType switch
        {
            SoundType.Music_Island => SoundType.Music_Island_Loop,
            SoundType.Music_Ocean => SoundType.Music_Ocean_Loop,
            SoundType.Music_Battle => SoundType.Music_Battle_Loop,
            _ => introType
        };

        Debug.Log($"[AudioManager] GetLoopType: {introType} -> {loopType}");
        return loopType;
    }

    // Method to stop all music
    public void StopMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
            _isPlayingIntro = false;
        }
    }
}
