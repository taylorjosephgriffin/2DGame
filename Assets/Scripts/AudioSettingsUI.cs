using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    public AudioManager audioManager;
    public Text soundEffectSliderTexts;
    public Text musicSliderTexts;

    public Slider soundEffectSlider;
    public Slider musicSlider;

    Slider currentSlider;

    // Update is called once per frame
    void Start()
    {
        soundEffectSliderTexts.text = (audioManager.mixers[0].volume * 100).ToString();
        musicSliderTexts.text =  (audioManager.mixers[1].volume * 100).ToString();
        soundEffectSlider.value = audioManager.mixers[0].volume;
        musicSlider.value = audioManager.mixers[1].volume;
    }

    public void UpdateVolumeText(Text sliderText) {
        sliderText.GetComponent<Text>().text = ((int)(currentSlider.value * 100)).ToString();
    }

    public void SetCurrentSlider(Slider slider) {
        currentSlider = slider;
    }
}
