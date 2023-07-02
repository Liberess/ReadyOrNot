using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    private static ShopManager mInstance;
    public static ShopManager Instance { get => mInstance; }

    private event Action OnUpdateUI;

    [SerializeField] private GunData[] gunDatas = new GunData[9];
    public GameObject[] weaponSlots = new GameObject[9];
    public GameObject[] statUpSlots = new GameObject[3];

    [SerializeField] private Text coinTxt;

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;
        else
            Destroy(this);
        
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            gunDatas[i] = weaponSlots[i].GetComponent<WeaponSlot>().GunData;
            OnUpdateUI += weaponSlots[i].GetComponent<WeaponSlot>().OnUpdateUI;
        }

        for (int i = 0; i < statUpSlots.Length; i++)
            OnUpdateUI += statUpSlots[i].GetComponent<StatUpSlot>().OnUpdateUI;
    }

    private void Start()
    {
        SetCoinTxt();
    }

    public void UpdateUI()
    {
        SetCoinTxt();
        MainUIManager.Instance.SetCoinTxt();
        OnUpdateUI();
    }

    public void SetCoinTxt()
    {
        string coin;

        if (DataManager.Instance.gameData.coin > 0)
            coin = string.Format("{0:#,###}", DataManager.Instance.gameData.coin);
        else
            coin = "0";

        coinTxt.text = coin;
    }
}