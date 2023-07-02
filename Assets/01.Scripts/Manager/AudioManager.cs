using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer masterMixer;

    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Text bgmNumTxt;
    [SerializeField] private Text sfxNumTxt;

    public AudioSource bgmPlayer = null;
    public AudioSource[] sfxPlayer = null;

    [SerializeField] Sound[] bgm = null;
    [SerializeField] Sound[] sfx = null;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitAudioSetting();
    }

    public void SetAudioObjects()
    {
        if (optionPanel == null)
            optionPanel = GameObject.Find("OptionCanvas").transform.GetChild(0).gameObject;

        var grid = optionPanel.transform.Find("PanelGrid").gameObject;

        if (bgmSlider == null || sfxSlider == null)
        {
            bgmSlider = grid.transform.GetChild(0).transform.Find("BgmSlider").GetComponent<Slider>();
            sfxSlider = grid.transform.GetChild(1).transform.Find("SfxSlider").GetComponent<Slider>();
        }

        if (bgmNumTxt == null || sfxNumTxt == null)
        {
            bgmNumTxt = grid.transform.GetChild(0).transform.Find("BgmNumTxt").GetComponent<Text>();
            sfxNumTxt = grid.transform.GetChild(1).transform.Find("SfxNumTxt").GetComponent<Text>();
        }
    }

    public void InitAudioSetting()
    {
        if (bgmSlider == null || sfxSlider == null
            || bgmNumTxt == null || sfxNumTxt == null)
            SetAudioObjects();

        bgmSlider.maxValue = 100f;
        sfxSlider.maxValue = 100f;

        bgmSlider.value = DataManager.Instance.gameData.bgm;
        sfxSlider.value = DataManager.Instance.gameData.sfx;

        bgmNumTxt.text = Mathf.RoundToInt(bgmSlider.value).ToString();
        sfxNumTxt.text = Mathf.RoundToInt(sfxSlider.value).ToString();

        masterMixer.SetFloat("BGM", bgmSlider.value / 100f);
        masterMixer.SetFloat("SFX", sfxSlider.value / 100f);
    }

    public void BGMSave()
    {
        bgmNumTxt.text = Mathf.RoundToInt(bgmSlider.value).ToString();
        bgmPlayer.volume = bgmSlider.value / 100f;
        DataManager.Instance.gameData.bgm = bgmSlider.value;
    }

    public void SFXSave()
    {
        for (int i = 0; i < sfxPlayer.Length; i++)
            sfxPlayer[i].volume = sfxSlider.value / 100f;

        sfxNumTxt.text = Mathf.RoundToInt(sfxSlider.value).ToString();
        DataManager.Instance.gameData.sfx = sfxSlider.value;
    }

    public void PlayBGM(string p_bgmName)
    {
        if (bgmPlayer.clip != null && bgmPlayer.clip.name == p_bgmName)
            return;

        for (int i = 0; i < bgm.Length; i++)
        {
            if (p_bgmName == bgm[i].name)
            {
                bgmPlayer.clip = bgm[i].clip;
                bgmPlayer.Play();
            }
        }
    }

    public void PlaySFX(string p_sfxName)
    {
        for (int i = 0; i < sfx.Length; i++)
        {
            if (p_sfxName == sfx[i].name)
            {
                for (int x = 0; x < sfxPlayer.Length; x++)
                {
                    if (!sfxPlayer[x].isPlaying)
                    {
                        sfxPlayer[x].clip = sfx[i].clip;
                        sfxPlayer[x].Play();
                        return;
                    }
                }
                return;
            }
        }
    }
}