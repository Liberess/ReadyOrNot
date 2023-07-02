using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject gameoverUI;

    [SerializeField] private Text waveTxt;
    [SerializeField] private Text timeTxt;
    [SerializeField] private Text killTxt;
    [SerializeField] private Text enemyTxt;
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

        timeTxt.text = string.Format("{0:D2}:{1:D2}", minute, second);
    }

    public void SetWaveTxt(int wave) => waveTxt.text = wave + " WAVE";

    public void SetKilledTxt(int killed) => killTxt.text = killed + " KILLED";

    public void SetEnemyTxt(int enemy) => enemyTxt.text = enemy + " ENEMY";

    public void SetHealthSlider(float value) => healthSlider.value = value;

    public void SetMaxHealth(float value) => healthSlider.maxValue = value;

    public void SetStaminaSlider(float value) => staminaSlider.value = value;
    
    public void SetMaxStamina(float value) => staminaSlider.maxValue = value;

    public void SetGameOverUI(bool active) => gameoverUI.SetActive(active);
}