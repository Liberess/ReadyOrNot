using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatUpSlot : MonoBehaviour
{
    private DataManager dataManager;

    [SerializeField] private StatType statType;
    private int ID;

    [SerializeField] private Text statLvTxt;

    [SerializeField] private Text preLvTxt;
    [SerializeField] private Text preStatTxt;
    [SerializeField] private Text nextLvTxt;
    [SerializeField] private Text nextStatTxt;

    [SerializeField] private Text costTxt;
    [SerializeField] private Button upBtn;

    private void Awake()
    {
        dataManager = DataManager.Instance;

        ID = (int)statType;
    }

    private void Start()
    {
        OnUpdateUI();
    }

    public void OnUpdateUI()
    {
        statLvTxt.text = "Lv." + dataManager.userData.statUpLevels[ID];

        if (dataManager.userData.statUpLevels[ID] >= 10)
            costTxt.gameObject.SetActive(false);

        costTxt.text = (int)(dataManager.userData.statUpLevels[ID] * 100 * 1.5f) + " Coin";

        SetPreStatUI();
        SetNextStatUI();
    }

    private void SetPreStatUI()
    {
        preLvTxt.text = "Lv." + dataManager.userData.statUpLevels[ID];
        SetStatTxt(0, preStatTxt);
    }

    private void SetStatTxt(int type, Text txt)
    {
        switch (statType)
        {
            case StatType.Health:
                txt.text = (int)(((dataManager.userData.statUpLevels[ID] + type) *
                    0.5f) * 100) + " HP";
                break;
            case StatType.Speed:
                txt.text = (int)(((dataManager.userData.statUpLevels[ID] + type) *
                    0.5f) + 3) + " SPEED";
                break;
            case StatType.Stamina:
                txt.text = (int)(((dataManager.userData.statUpLevels[ID] + type) *
                    0.5f) * 50) + " STAMINA";
                break;
            default:
                break;
        }
    }

    private void SetNextStatUI()
    {
        if (dataManager.userData.statUpLevels[ID] + 1 > 10)
        {
            nextLvTxt.text = "Lv.Max";
            SetStatTxt(0, nextStatTxt);
            upBtn.gameObject.SetActive(false);
            return;
        }

        nextLvTxt.text = "Lv." + (dataManager.userData.statUpLevels[ID] + 1);
        SetStatTxt(1, nextStatTxt);
    }

    public void OnClickStatUpBtn()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        if (dataManager.userData.statUpLevels[ID] >= 10)
            return;

        int value = Mathf.RoundToInt(dataManager.userData.statUpLevels[ID] * 100 * 1.5f);

        if (dataManager.gameData.coin < value)
            return;

        dataManager.gameData.coin -= value;
        ++dataManager.userData.statUpLevels[ID];

        string cost = string.Format("{0:#,###}", value);
        costTxt.text = cost + " Coin";

        OnUpdateUI();
        ShopManager.Instance.UpdateUI();
    }
}