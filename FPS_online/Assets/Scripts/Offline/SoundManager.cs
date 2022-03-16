using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

 
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public AudioMixer mixer;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    

    [Header("Dictionary Settings")]
    public List<SoundManagerConstants.Clips> clipName = new List<SoundManagerConstants.Clips>();
    public List<AudioClip> clipList = new List<AudioClip>();
    private Dictionary<SoundManagerConstants.Clips, AudioClip> clipLib = new Dictionary<SoundManagerConstants.Clips, AudioClip>();

    [Header("Pool Settings")]
    public GameObject prefabToPool;
    public int amountToPool;
    private List<GameObject> pooledPrefabs = new List<GameObject>();

    void Start()
    {  
        //Create Dictionary
        for (int i = 0; i < clipName.Count; i++)
            clipLib.Add(clipName[i], clipList[i]);

        CreateInstances(); 
    }


    public void PlaySound(SoundManagerConstants.Clips clip, SoundManagerConstants.AudioOutput group, Vector3 position, float volume = 1)
    {
        GameObject prefab = GetPoolObject();

        if (prefab == null) 
            return;

        prefab.transform.position = position;
        prefab.SetActive(true);

        AudioSource prefabAudioSource = prefab.GetComponent<AudioSource>();

        prefabAudioSource.clip = clipLib[clip];
        prefabAudioSource.volume = volume;
        prefabAudioSource.outputAudioMixerGroup = group == SoundManagerConstants.AudioOutput.SFX ? sfxGroup : musicGroup;
        prefabAudioSource.Play(); 

        StartCoroutine(BackToPool(prefab, clipLib[clip].length));
    }

    public AudioSource PlaySoundAndReturn(SoundManagerConstants.Clips clip, SoundManagerConstants.AudioOutput group, Vector3 position, float volume = 1)
    {
        GameObject prefab = GetPoolObject();

        if (prefab == null)
            return null;

        prefab.transform.position = position;
        prefab.SetActive(true);

        AudioSource prefabAudioSource = prefab.GetComponent<AudioSource>();

        prefabAudioSource.clip = clipLib[clip];
        prefabAudioSource.volume = volume;
        prefabAudioSource.outputAudioMixerGroup = group == SoundManagerConstants.AudioOutput.SFX ? sfxGroup : musicGroup;
        prefabAudioSource.Play();

        StartCoroutine(BackToPool(prefab, clipLib[clip].length));

        return prefabAudioSource;
    }

    public void PlaySound(AudioClip clip, SoundManagerConstants.AudioOutput group, Vector3 position, out float duration, float volume = 1)
    {
        GameObject prefab = GetPoolObject();

        if (prefab == null)
        {
            duration = 0;
            return;
        }

        prefab.transform.position = position;
        prefab.SetActive(true);

        AudioSource prefabAudioSource = prefab.GetComponent<AudioSource>();

        prefabAudioSource.clip = clip;
        prefabAudioSource.volume = volume;
        prefabAudioSource.outputAudioMixerGroup = group == SoundManagerConstants.AudioOutput.SFX ? sfxGroup : musicGroup;
        prefabAudioSource.Play();

        StartCoroutine(BackToPool(prefab, clip.length));

        duration = clip.length;
    }

    public void PlaySound(AudioClip clip, SoundManagerConstants.AudioOutput group, Vector3 position, float volume = 1)
    {
        GameObject prefab = GetPoolObject();

        if (prefab == null) return;

        prefab.transform.position = position;
        prefab.SetActive(true);

        AudioSource prefabAudioSource = prefab.GetComponent<AudioSource>();

        prefabAudioSource.clip = clip;
        prefabAudioSource.volume = volume;
        prefabAudioSource.outputAudioMixerGroup = group == SoundManagerConstants.AudioOutput.SFX ? sfxGroup : musicGroup;
        prefabAudioSource.Play();

        StartCoroutine(BackToPool(prefab, clip.length));
    }

    public void PlaySound(SoundManagerConstants.Clips clip, SoundManagerConstants.AudioOutput group, GameObject parent, float volume = 1)
    {
        GameObject prefab = GetPoolObject();

        if (prefab == null) return;

        #region Configure Prefab
        prefab.transform.position = parent.transform.position;
        prefab.transform.parent = parent.transform;
        prefab.SetActive(true);
        #endregion

        #region Configure AudioSource
        AudioSource prefabAudioSource = prefab.GetComponent<AudioSource>();

        prefabAudioSource.clip = clipLib[clip];
        prefabAudioSource.volume = volume;
        prefabAudioSource.outputAudioMixerGroup = group == SoundManagerConstants.AudioOutput.SFX ? sfxGroup : musicGroup;
        prefabAudioSource.Play();
        #endregion

        StartCoroutine(BackToPool(prefab, clipLib[clip].length, true));
    }

    public AudioSource PlaySoundAndReturn(SoundManagerConstants.Clips clip, SoundManagerConstants.AudioOutput group, GameObject parent, float volume = 1)
    {
        GameObject prefab = GetPoolObject();

        if (prefab == null) 
            return null;

        #region Configure Prefab
        prefab.transform.position = parent.transform.position;
        prefab.transform.parent = parent.transform;
        prefab.SetActive(true);
        #endregion

        #region Configure AudioSource
        AudioSource prefabAudioSource = prefab.GetComponent<AudioSource>();

        prefabAudioSource.clip = clipLib[clip];
        prefabAudioSource.volume = volume;
        prefabAudioSource.outputAudioMixerGroup = group == SoundManagerConstants.AudioOutput.SFX ? sfxGroup : musicGroup;
        prefabAudioSource.Play();
        #endregion

        StartCoroutine(BackToPool(prefab, clipLib[clip].length, true));

        return prefabAudioSource;
    }

    private void CreateInstances()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject go = Instantiate(prefabToPool, gameObject.transform);
            go.SetActive(false);
            pooledPrefabs.Add(go);
        }
    }

    private GameObject GetPoolObject()
    {
        for (int i = 0; i < pooledPrefabs.Count; i++)
        {
            if (pooledPrefabs[i].gameObject.transform.parent == gameObject.transform)
                if (!pooledPrefabs[i].gameObject.activeInHierarchy)
                    return pooledPrefabs[i];
        }

        return null;
    }

    public void DisablePool()
    {
        foreach (GameObject prefab in pooledPrefabs)
        {
            prefab.transform.parent = gameObject.transform;
            prefab.SetActive(false);
        }
    }

    IEnumerator BackToPool(GameObject prefab, float seconds, bool unparent = false)
    {
        yield return new WaitForSeconds(seconds);

        if (unparent)
        {
            prefab.transform.parent = gameObject.transform;
        }

        prefab.SetActive(false);
    }


    //Set Mixer Volumes
    public void SetSFXVolume(float sfxVol)
    {
        mixer.SetFloat("sfxVol", sfxVol);
    }

    public void SetMusicVolume(float musicVol)
    {
        mixer.SetFloat("musicVol", musicVol);
    }
}


public class SoundManagerConstants
{
    public enum AudioOutput
    {
        SFX,
        MUSIC
    }

    public enum Clips
    {
        RIFLE_SHOOT,
        HANDGUN_SHOOT,
        KNIFE_SWING,
        RIGHT_FOOTSTEP,
        RIGHT_FOOTSTEP_RUN,
        LEFT_FOOTSTEP,
        LEFT_FOOTSTEP_RUN,
        LANDING,
        DEATH,
        RELOAD_RIFLE,
        RELOAD_HANDGUN
    }
}