using System;
using UnityEngine;
using UnityEngine.UI;

public class CharaterManager : MonoBehaviour
{
    private static CharaterManager mInstance;
    public static CharaterManager Instance { get => mInstance; }

    private event Action OnUpdateUI;

    [Header("Player Panel")]
    [SerializeField] private Text playerLvTxt;
    [SerializeField] private Text playerNameTxt;
    [SerializeField] private Slider expSlider;
    [SerializeField] private Text expTxt;
    [SerializeField] private Text hpLvTxt;
    [SerializeField] private Text speedLvTxt;
    [SerializeField] private Text staminaLvTxt;

    [Header("Weapon Panel")]
    [SerializeField] private GameObject[] weaponSlots = new GameObject[9];

    [Header("Player Model")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject[] weaponModels = new GameObject[9];

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;

        for (int i = 0; i < weaponSlots.Length; i++)
            OnUpdateUI += weaponSlots[i].GetComponent<MyWeaponSlot>().OnUpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        OnUpdateUI();

        playerLvTxt.text = "Level " + DataManager.Instance.gameData.level;
        playerNameTxt.text = MainUIManager.Instance.nameTxt.text;

        hpLvTxt.text = "Lv." + DataManager.Instance.userData.statUpLevels[0];
        speedLvTxt.text = "Lv." + DataManager.Instance.userData.statUpLevels[1];
        staminaLvTxt.text = "Lv." + DataManager.Instance.userData.statUpLevels[2];

        expSlider.maxValue = DataManager.Instance.gameData.level * 100;
        expSlider.value = DataManager.Instance.gameData.exp;

        expTxt.text = string.Format("{0:0.#}", expSlider.value) + " / "
            + string.Format("{0:0.#}", (float)expSlider.maxValue) + " XP";

        SetSlotActive();
        SetWeaponModel();
    }

    public void SetSlotActive()
    {
        for(int i = 0; i < weaponSlots.Length; i++)
        {
            if (DataManager.Instance.userData.haveGuns[i])
                weaponSlots[i].SetActive(true);
            else
                weaponSlots[i].SetActive(false);
        }
    }

    public void SetWeaponModel()
    {
        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (DataManager.Instance.userData.equipGunNum == i)
            {
                weaponModels[i].gameObject.SetActive(true);
                playerPrefab.GetComponent<Animator>().runtimeAnimatorController =
                    DataManager.Instance.userData.animCtrls[i];
            }
            else
            {
                weaponModels[i].gameObject.SetActive(false);
            }
        }
    }

    public void ActiveWeaponSlot(int ID)
    {
        weaponSlots[ID].gameObject.SetActive(true);
    }
}