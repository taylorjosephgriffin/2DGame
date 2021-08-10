using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [HideInInspector]
    public static AudioManager instance;
    [System.Serializable]
    public class Mixer {
        public AudioMixer mixer;
        public bool isActive;
        public float volume;
        public string exposedParameterName;
    }
    public List<Mixer> mixers = new List<Mixer>();
    public delegate void OnVolumeChangeCallback(Slider slider);
    public AudioSettingsUI audioSettingsUI;
    PlayerControls controls;

    bool audioSettingsOpen = false;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Start is called before the first frame updatde
    void Start()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        controls.Gameplay.AudioSettings.performed += ctx => triggerAudioSettingsStateChange();
        foreach (Mixer mixer in mixers) {
            mixer.mixer.SetFloat(mixer.exposedParameterName, Mathf.Log10(mixer.volume) * 20);
        }
    }

    // Update is called once per frame

    public void ChangeSoundEffectMixerVolume(Slider slider) {
        float currentVolume;
        mixers[0].mixer.GetFloat("SoundEffectMainVolume", out currentVolume);
        mixers[0].mixer.SetFloat("SoundEffectMainVolume", Mathf.Log10(slider.value) * 20);
    }

    public void ChangeMusicMixerVolume(Slider slider) {
        float currentVolume;
        mixers[1].mixer.GetFloat("MusicMainVolume", out currentVolume);
        mixers[1].mixer.SetFloat("MusicMainVolume", Mathf.Log10(slider.value) * 20);
    }

    void triggerAudioSettingsStateChange()
    {
        if (!audioSettingsOpen && UIManager.currentUIState == UIManager.UIStates.IN_GAME)
        {
            audioSettingsOpen = true;
            audioSettingsUI.gameObject.SetActive(true);
            PauseManager.PauseGame();
            UIManager.currentUIState = UIManager.UIStates.AUDIO_SETTINGS;
        }
        else if (audioSettingsOpen  && UIManager.currentUIState == UIManager.UIStates.AUDIO_SETTINGS)
        {
            audioSettingsOpen = false;
            audioSettingsUI.gameObject.SetActive(false);
            PauseManager.PlayGame();
            UIManager.currentUIState = UIManager.UIStates.IN_GAME;
        }
    }
}
