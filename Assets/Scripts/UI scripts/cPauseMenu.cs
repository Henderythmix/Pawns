using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class cPauseMenu : MonoBehaviour
{
    public KeyCode pauseGameButton;
    public GameObject pauseMenu;
    [Header("Audio Mixers")]
    public AudioMixer gameVolume;

    public Slider masterSlider;
    public Slider soundSlider;
    public Slider musicSlider;

    private void Update()
    {
        if (Input.GetKeyDown(pauseGameButton) )
        {
            if (!pauseMenu.activeInHierarchy)
                PauseMenu();
            else
                UnPause();
        }
    }
    public void PauseMenu()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void UnPause()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void SetMasterVolume()
    {
        gameVolume.SetFloat("masterVolume", masterSlider.value);
    }
    public void SetSoundVolume()
    {
        gameVolume.SetFloat("soundVolume", soundSlider.value);
    }

    public void SetMusicVolume()
    {
        gameVolume.SetFloat("musicVolume", musicSlider.value);
    }
}
