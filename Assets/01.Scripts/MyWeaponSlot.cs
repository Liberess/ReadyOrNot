using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyWeaponSlot : MonoBehaviour
{
    [SerializeField] private GunData gunData;
    public GunData GunData { get => gunData; }

    [SerializeField] private Text gunLevelTxt;
    [SerializeField] private Text gunNameTxt;

    [SerializeField] private Button equipBtn;

    [SerializeField] private int ID;

    private void Start()
    {
        ID = (int)gunData.GunType;

        SetNameTxt();
        SetGunModel();
        OnUpdateUI();
    }

    public void OnUpdateUI()
    {
        SetLevelTxt();
        SetEquipBtn();
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
        gunLevelTxt.text = "Lv." + DataManager.Instance.userData.gunLevels[ID];
    }

    private void SetNameTxt()
    {
        gunNameTxt.text = gunData.GunName;
    }

    private void SetEquipBtn()
    {
        if (DataManager.Instance.userData.equipGunNum == ID)
            equipBtn.interactable = false;
        else
            equipBtn.interactable = true;
    }

    public void OnClickEquipBtn()
    {
        AudioManager.Instance.PlaySFX("UIClick");
        DataManager.Instance.userData.equipGunNum = ID;
        DataManager.Instance.userData.equipGunData = gunData;
        CharaterManager.Instance.UpdateUI();
        MainUIManager.Instance.SetWeaponModel();
    }
}