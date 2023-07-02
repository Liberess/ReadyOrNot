using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PVPUIManager : MonoBehaviour
{
    public static PVPUIManager Instance { get; private set; }

    [SerializeField] private GameObject gameoverUI;

    [SerializeField] private Text countTxt;
    [SerializeField] private Text[] killTxts;
    [SerializeField] private Image[] killImgs;
    [SerializeField] private Text ammoTxt;
    [SerializeField] private Image stateImg;
    [SerializeField] private Sprite[] stateSprites;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetStateImg(PlayerState state) => stateImg.sprite = stateSprites[(int)state];

    public void SetAmmoTxt(int magAmmo, int remainAmmo)
        => ammoTxt.text = magAmmo + " / " + remainAmmo;

    public void SetTimeTxt(float totalSeconds)
    {
        int minute = (int)totalSeconds / 60;
        int second = (int)totalSeconds % 60;
        minute = minute % 60;

        countTxt.text = string.Format("{0:D2}:{1:D2}", minute, second);
    }

    public void SetKillUI(int red, int blue)
    {
        int[] values = { red, blue };

        for(int i = 0; i < killTxts.Length; i++)
        {
            killTxts[i].text = values[i].ToString();

            if (values[i] > 0)
                killImgs[i].fillAmount = values[i] / 20f;
            else
                killImgs[i].fillAmount = 0.01f;
        }
    }

    public void SetHealthSlider(float value) => healthSlider.value = value;

    public void SetMaxHealth(float value) => healthSlider.maxValue = value;

    public void SetStaminaSlider(float value) => staminaSlider.value = value;
    
    public void SetMaxStamina(float value) => staminaSlider.maxValue = value;

    public void SetGameOverUI(bool active) => gameoverUI.SetActive(active);
}