using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignUIManager : MonoBehaviour
{
    public static CampaignUIManager Instance { get; private set; }

    [Header("Game Result Panel")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameClearUI;
    [SerializeField] private Text[] resultTxts;

    [Header("In-Game UI")]
    [SerializeField] private Text levelTxt;
    [SerializeField] private Text nameTxt;
    [SerializeField] private Text timeTxt;
    [SerializeField] private Text killTxt;
    [SerializeField] private Text enemyTxt;
    [SerializeField] private Text ammoTxt;
    [SerializeField] private Image stateImg;
    [SerializeField] private Sprite[] stateSprites;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image[] keyImgs;
    [SerializeField] private GameObject findImg;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < keyImgs.Length; i++)
            keyImgs[i].color = new Color(0f, 0f, 0f, 0.5f);
    }

    public void SetStateImg(PlayerState state) => stateImg.sprite = stateSprites[(int)state];

    public void SetAmmoTxt(int magAmmo, int remainAmmo) => ammoTxt.text = magAmmo + " / " + remainAmmo;

    public void SetTimeTxt(float totalSeconds)
    {
        int minute = (int)totalSeconds / 60;
        int second = (int)totalSeconds % 60;
        minute = minute % 60;

        timeTxt.text = string.Format("{0:D2}:{1:D2}", minute, second);
    }

    public void SetBossUI()
    {
        for (int i = 0; i < keyImgs.Length; i++)
            keyImgs[i].gameObject.SetActive(false);

        findImg.SetActive(false);

        timeTxt.color = Color.red;
        timeTxt.fontSize = 80;
    }

    public void SetLevelTxt(StageTypes type)
    {
        levelTxt.text = "LEVEL " + ((int)type + 1);

        switch(type)
        {
            case StageTypes.Ballantines: nameTxt.text = "Ballantines"; break;
            case StageTypes.Halloween: nameTxt.text = "Halloween"; break;
            case StageTypes.Boss: nameTxt.text = "Boss"; break;
            default: nameTxt.text = "NULL"; break;
        }
    }

    public void SetActiveFindImg(bool active) => findImg.SetActive(active);

    public void SetKeyImg(KeyData data)
    {
        if(data.keyNum <= 0)
        {
            for (int i = 0; i < keyImgs.Length; i++)
                keyImgs[i].color = new Color(0f, 0f, 0f, 0.1f);
        }

        for(int i = 0; i < data.keyNum; i++)
            keyImgs[i].color = new Color(1f, 1f, 1f, 0.9f);

        if (data.keyNum >= 2)
            findImg.SetActive(true);
    }

    public void SetKilledTxt(int killed) => killTxt.text = killed + " KILLED";

    public void SetEnemyTxt(int enemy) => enemyTxt.text = enemy + " ENEMY";

    public void SetHealthSlider(float value) => healthSlider.value = value;

    public void SetMaxHealth(float value) => healthSlider.maxValue = value;

    public void SetStaminaSlider(float value) => staminaSlider.value = value;

    public void SetMaxStamina(float value) => staminaSlider.maxValue = value;

    public void SetGameOverUI(bool active) => gameOverUI.SetActive(active);

    public void SetGameClearUI(bool active)
    {
        gameClearUI.SetActive(active);

        float totalSeconds = CampaignManager.Instance.playTime;
        int minute = (int)totalSeconds / 60;
        int second = (int)totalSeconds % 60;
        minute = minute % 60;

        resultTxts[0].text = string.Format("{0:D2}:{1:D2}", minute, second); //Play Time

        string killCountStr = string.Format("{0:#,###}", CampaignManager.Instance.killCount);
        resultTxts[1].text = killCountStr; //Kill Count

        var exp = Mathf.Round(CampaignManager.Instance.acquireExp * 10f) * 0.1f;
        string expStr = string.Format("{0:#,###}", exp);
        resultTxts[2].text = "+" + "<color=#12E3DF>" + expStr + "</color>"; //Get Exp

        string coinStr = string.Format("{0:#,###}", CampaignManager.Instance.acquireCoin);
        resultTxts[3].text = "+" + "<color=#12E3DF>" + coinStr + "</color>"; //Get Coin
    }
}