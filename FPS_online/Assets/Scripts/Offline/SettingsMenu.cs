using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private Resolution[] resolutions;
    private string[] qualities;

    [Header("Settings Components")]
    public Slider sensitivitySlider;
    public Slider masterVolSlider;
    public Slider sfxVolSlider;
    public Slider MusicVolSlider;
    public Toggle fpsToggle;
    public Toggle latencyToggle;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public AudioMixer mainMixer; 

    [Header("UI Display")]
    public GameObject fpsDisplay;
    public GameObject pingDisplay;

    private void Awake()
    {
        //Saved values
        if (PlayerPrefs.HasKey("Settings->General->FPS"))
            fpsToggle.isOn = PlayerPrefs.GetInt("Settings->General->FPS") == 1 ? true : false;
        
        if (PlayerPrefs.HasKey("Settings->General->Ping"))
            latencyToggle.isOn = PlayerPrefs.GetInt("Settings->General->Ping") == 1 ? true : false;
        
        if (PlayerPrefs.HasKey("Settings->General->Sensitivity"))
            sensitivitySlider.value = PlayerPrefs.GetFloat("Settings->General->Sensitivity");
        else
            sensitivitySlider.value = sensitivitySlider.maxValue * 0.4f;
         
        if (PlayerPrefs.HasKey("Settings->Graphics->Quality"))
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Settings->Graphics->Quality"));
       
        if (PlayerPrefs.HasKey("Settings->Audio->Master"))
            masterVolSlider.value = PlayerPrefs.GetFloat("Settings->Audio->Master");
        else
            masterVolSlider.value = masterVolSlider.maxValue; 
         
        if (PlayerPrefs.HasKey("Settings->Audio->SFX"))
            sfxVolSlider.value = PlayerPrefs.GetFloat("Settings->Audio->SFX");
        else
            sfxVolSlider.value = sfxVolSlider.maxValue;

        if (PlayerPrefs.HasKey("Settings->Audio->Music"))
            MusicVolSlider.value = PlayerPrefs.GetFloat("Settings->Audio->Music");
        else
            MusicVolSlider.value = MusicVolSlider.maxValue;
    }

    private void Start()
    { 
        //Quality
        qualities = QualitySettings.names;
        List<string> qualityOptions = new List<string>();
        int currentQuality = 0;
        for (int i = 0; i < qualities.Length; i++)
        {
            qualityOptions.Add(qualities[i]);

            if (QualitySettings.GetQualityLevel() == i) 
                currentQuality = i; 
        }
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(qualityOptions);
        qualityDropdown.value = currentQuality;
        qualityDropdown.RefreshShownValue();

        //Resolution
#if UNITY_EDITOR
        resolutions = Screen.resolutions;
#else
        resolutions = Screen.resolutions.Where(resolution => resolution.refreshRate == 60).ToArray();
#endif
        List<string> resOptions = new List<string>();
        int currentRes = 0;
        for (int i = 0; i < resolutions.Length; i++)
        { 
            string option = resolutions[i].width + "x" + resolutions[i].height;
            resOptions.Add(option);

            if(resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                currentRes = i;
            }
        }
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resOptions);
        resolutionDropdown.value = currentRes;
        resolutionDropdown.RefreshShownValue();

        //Set counters
        fpsDisplay.SetActive(fpsToggle.isOn);
        pingDisplay.SetActive(latencyToggle.isOn);
        
        //Set sensitivity
        if (PhotonNetwork.IsConnected)
        {
            var players = FindObjectsOfType<NETPlayerController>();
            if (players != null && players.Length > 0)
            {
                foreach (var item in players)
                {
                    if (item.PV.IsMine)
                        item.sensitivityMultiplier = sensitivitySlider.value;
                }
            }
        }
        else
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
                player.sensitivityMultiplier = sensitivitySlider.value;
        }

        //Set volumes
        mainMixer.SetFloat("masterVol", masterVolSlider.value);
        mainMixer.SetFloat("sfxVol", sfxVolSlider.value);
        mainMixer.SetFloat("musicVol", MusicVolSlider.value);
    }


    public void ShowFPS(bool _isActive)
    {
        fpsDisplay.SetActive(_isActive);

        PlayerPrefs.SetInt("Settings->General->FPS", _isActive ? 1 : 0);
    }

    public void ShowPing(bool _isActive)
    {
        pingDisplay.SetActive(_isActive);

        PlayerPrefs.SetInt("Settings->General->Ping", _isActive ? 1 : 0);
    }

    public void SetSensitivity(float _value)
    {
        if (PhotonNetwork.IsConnected)
        {
            var players = FindObjectsOfType<NETPlayerController>();
            if (players != null && players.Length > 0)
            {
                foreach (var item in players)
                {
                    if (item.PV.IsMine)
                        item.sensitivityMultiplier = _value;
                }
            }
        }
        else
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
                player.sensitivityMultiplier = _value;
        }      

        PlayerPrefs.SetFloat("Settings->General->Sensitivity", _value);
    }

    public void SetResolution(int _resIndex)
    {
        Resolution res = resolutions[_resIndex];

        Screen.SetResolution(res.width, res.height, Screen.fullScreen); 
    }

    public void SetFullscreen(bool _isFullScreen)
    {
        Screen.fullScreen = _isFullScreen;
    }

    public void SetQuality(int _qualityIndex)
    {
        QualitySettings.SetQualityLevel(_qualityIndex);

        PlayerPrefs.SetInt("Settings->Graphics->Quality", _qualityIndex);
    }

    public void SetVolumeMaster(float _vol)
    {
        mainMixer.SetFloat("masterVol", _vol);

        PlayerPrefs.SetFloat("Settings->Audio->Master", _vol); 
    }

    public void SetVolumeSFX(float _vol)
    {
        mainMixer.SetFloat("sfxVol", _vol);

        PlayerPrefs.SetFloat("Settings->Audio->SFX", _vol); 
    }

    public void SetVolumeMusic(float _vol)
    {
        mainMixer.SetFloat("musicVol", _vol);
        
        PlayerPrefs.SetFloat("Settings->Audio->Music", _vol); 
    }
}
