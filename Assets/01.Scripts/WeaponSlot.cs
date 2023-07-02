using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    private DataManager dataManager;

    [SerializeField] private GunData gunData;
    public GunData GunData { get => gunData; }

    [SerializeField] private Text gunLevelTxt;
    [SerializeField] private Text gunNameTxt;
    [SerializeField] private Text gunBuyCostTxt;
    [SerializeField] private Text gunUpCostTxt;

    [SerializeField] private Button buyBtn;
    [SerializeField] private Button upBtn;

    private int ID;

    private void Awake()
    {
        dataManager = DataManager.Instance;

        ID = (int)gunData.GunType;

        SetNameTxt();
        SetGunModel();
        OnUpdateUI();
    }

    public void OnUpdateUI()
    {
        SetLevelTxt();
        SetBuyCostTxt();
        SetUpCostTxt();
        SetBuyBtn();
        SetUpBtn();
    }

    private void SetGunModel()
    {
        GameObject model = Instantiate(
            gunData.GunUIPrefab, Vector3.zero, Quaternion.Euler(0, 90, 0));
        model.transform.SetParent(transform);
        model.transform.localScale = new Vector3(108, 108, 108);
        model.transform.localPosition = new Vector3(-50, 0, -5);
    }

    private void SetLevelTxt()
    {
        gunLevelTxt.text = "Lv." + dataManager.userData.gunLevels[ID];
    }

    private void SetNameTxt()
    {
        gunNameTxt.text = gunData.GunName;
    }

    private void SetBuyCostTxt()
    {
        if (dataManager.userData.haveGuns[ID])
            return;

        string cost = string.Format("{0:#,###}", gunData.GunBuyCost);
        gunBuyCostTxt.text = cost + " Coin";

        MainUIManager.Instance.SetCoinTxt();
    }

    private void SetUpCostTxt()
    {
        int cost = dataManager.userData.gunLevels[ID]
            * gunData.GunUpCost;
        string str = string.Format("{0:#,###}", cost);
        gunUpCostTxt.text = str + " Coin";
    }

    private void SetBuyBtn()
    {
        if(dataManager.userData.haveGuns[ID])
        {
            gunBuyCostTxt.text = "Sold Out";
            buyBtn.gameObject.SetActive(false);
            return;
        }

        if(dataManager.gameData.level >= (ID + 1) * 2)
        {
            buyBtn.interactable = true;
            buyBtn.gameObject.transform.Find("LockPanel").gameObject.SetActive(false);
        }
        else
        {
            buyBtn.interactable = false;
            buyBtn.gameObject.transform.Find("LockPanel").gameObject.SetActive(true);
        }
    }

    private void SetUpBtn()
    {
        if (dataManager.userData.gunLevels[ID] > 9)
        {
            gunUpCostTxt.text = "Max Upgrade";
            upBtn.gameObject.SetActive(false);
            return;
        }

        if (dataManager.userData.haveGuns[ID])
        {
            upBtn.interactable = true;
            upBtn.gameObject.transform.Find("LockPanel").gameObject.SetActive(false);
        }
        else
        {
            upBtn.interactable = false;
            upBtn.gameObject.transform.Find("LockPanel").gameObject.SetActive(true);
        }
    }

    public void OnClickBuyBtn()
    {
        if(dataManager.gameData.coin >= gunData.GunBuyCost)
        {
            AudioManager.Instance.PlaySFX("UIClick");
            dataManager.gameData.coin -= gunData.GunBuyCost;
            dataManager.userData.haveGuns[ID] = true;
            OnUpdateUI();
            ShopManager.Instance.SetCoinTxt();
            CharaterManager.Instance.ActiveWeaponSlot(ID);
            CharaterManager.Instance.UpdateUI();
        }
    }

    public void OnClickUpBtn()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        if (dataManager.gameData.coin >=
            gunData.GunUpCost * dataManager.userData.gunLevels[ID])
        {
            dataManager.gameData.coin -=
                gunData.GunUpCost * dataManager.userData.gunLevels[ID];
            dataManager.userData.haveGuns[ID] = true;
            ++dataManager.userData.gunLevels[ID];
            OnUpdateUI();
            ShopManager.Instance.SetCoinTxt();
        }
    }
}