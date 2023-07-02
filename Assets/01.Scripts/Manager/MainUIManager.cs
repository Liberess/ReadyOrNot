using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MainUIManager : MonoBehaviour
{
    private static MainUIManager mInstance;
    public static MainUIManager Instance
    {
        get
        {
            if (mInstance == null)
                mInstance = FindObjectOfType<MainUIManager>();

            return mInstance;
        }
    }
    private DataManager dataMgr;

    public Text nameTxt;
    public Text coinTxt;
    public Text levelTxt;
    public Text expTxt;
    public Slider expSlider;

    [SerializeField] private GameObject mainPlayer;
    [SerializeField] private GameObject[] weaponModels = new GameObject[9];
    [SerializeField] private RuntimeAnimatorController[] animCtrls =
        new RuntimeAnimatorController[9];

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;
        else if (mInstance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        dataMgr = DataManager.Instance;

        OnUpdateUI();
    }

    public void OnUpdateUI()
    {
        SetNameTxt();
        SetCoinTxt();
        SetExpSlider();
        SetLevelTxt();
        SetWeaponModel();
    }

    public void SetNameTxt()
    {
        nameTxt.text = PhotonNetwork.LocalPlayer.NickName;
    }

    public void SetCoinTxt()
    {
        string coin;

        if (dataMgr.gameData.coin > 0)
            coin = string.Format("{0:#,###}", dataMgr.gameData.coin);
        else
            coin = "0";

        coinTxt.text = "<color=#12E3DF>" + coin + "</color>";
    }

    public void SetLevelTxt()
    {
        levelTxt.text = "Level " + dataMgr.gameData.level;
    }

    public void SetExpSlider()
    {
        dataMgr.SetExp();

        expSlider.maxValue = dataMgr.gameData.level * 100;
        expSlider.value = dataMgr.gameData.exp;

        expTxt.text = string.Format("{0:0.#}", expSlider.value) + " / "
            + string.Format("{0:0.#}", (float)expSlider.maxValue) + " XP";
    }

    public void OnClickSetActive(GameObject obj)
    {
        if (obj.activeSelf)
            obj.SetActive(false);
        else
            obj.SetActive(true);
    }

    public void SetShopUI()
    {
        ShopManager.Instance.UpdateUI();
    }

    public void SetWeaponModel()
    {
        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (dataMgr.userData.equipGunNum == i)
            {
                weaponModels[i].gameObject.SetActive(true);
                mainPlayer.GetComponent<Animator>().runtimeAnimatorController = animCtrls[i];
            }
            else
            {
                weaponModels[i].gameObject.SetActive(false);
            }
        }
    }
}