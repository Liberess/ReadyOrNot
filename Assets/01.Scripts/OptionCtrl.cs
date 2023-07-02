using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.SceneManagement;

public class OptionCtrl : MonoBehaviour
{
    private DataManager dataManager;

    [Header("Set OptionPanel")]
    [SerializeField] private GameObject optionPanel;

    [Header("Set Mouse Sensitivity")]
    [SerializeField] private Slider mouseSlider;
    [SerializeField] private Text mouseNumTxt;
    [SerializeField] private CinemachineFreeLook tpsCam;

    [Header("Set Crosshair")]
    [SerializeField] private Text crosshairTxt;
    [SerializeField] private GameObject[] crosshairPrefabs;
    [SerializeField] private GameObject preCrosshair;    // 변경 전의 크로스헤어
    [SerializeField] private GameObject nowCrosshair;  // 현재의 크로스헤어

    private void Start()
    {
        dataManager = DataManager.Instance;

        InitMouseSetting();
        InitCrosshairSetting();
        SetCrosshair();
    }

    public void InitMouseSetting()
    {
        mouseSlider.maxValue = 20f;
        mouseSlider.value = dataManager.userData.mouseSensitivity;
        mouseNumTxt.text = Mathf.RoundToInt(mouseSlider.value).ToString();

        if (SceneManager.GetActiveScene().buildIndex != 0)
            tpsCam = GameObject.Find("TPS Cam").GetComponent<CinemachineFreeLook>();
    }

    public void InitCrosshairSetting()
    {
        foreach (var target in crosshairPrefabs)
            target.SetActive(false);

        var num = dataManager.userData.crosshairNum;
        nowCrosshair = crosshairPrefabs[num];

        if (num == 0)
            preCrosshair = nowCrosshair;
        else
            preCrosshair = crosshairPrefabs[num - 1];

    }

    public void OnChangeBGM()
    {
        AudioManager.Instance.BGMSave();
    }

    public void OnChangeSFX()
    {
        AudioManager.Instance.SFXSave();
    }

    public void OnChangeMouseSens() //Sensitivity (감도)
    {
        mouseNumTxt.text = Mathf.RoundToInt(mouseSlider.value).ToString();
        dataManager.userData.mouseSensitivity = mouseSlider.value;
        SetMouseSens();
    }

    public void SetOptionPanelActive()
    {
        if (optionPanel.activeSelf)
        {
            optionPanel.SetActive(false);
            bl_UCrosshair.Instance.OnAim(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            optionPanel.SetActive(true);
            bl_UCrosshair.Instance.OnAim(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnClickBackBtn()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        transform.GetChild(0).gameObject.SetActive(false);

        if (SceneManager.GetActiveScene().buildIndex == 0)
            return;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SetMouseSens()
    {
        if (tpsCam == null)
            return;

        tpsCam.m_XAxis.m_MaxSpeed = 100f + dataManager.userData.mouseSensitivity * 10f;
        tpsCam.m_YAxis.m_MaxSpeed = 0.5f + dataManager.userData.mouseSensitivity * 0.1f;
    }

    public void OnClickPreviousBtn()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        if (dataManager.userData.crosshairNum <= 0)
            return;

        preCrosshair = crosshairPrefabs[dataManager.userData.crosshairNum];
        --dataManager.userData.crosshairNum;
        nowCrosshair = crosshairPrefabs[dataManager.userData.crosshairNum];
        SetCrosshair();
    }

    public void OnClickNextBtn()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        if (dataManager.userData.crosshairNum >= crosshairPrefabs.Length - 1)
            return;

        preCrosshair = crosshairPrefabs[dataManager.userData.crosshairNum];
        ++dataManager.userData.crosshairNum;
        nowCrosshair = crosshairPrefabs[dataManager.userData.crosshairNum];
        SetCrosshair();
    }

    private void SetCrosshair()
    {
        crosshairTxt.text = "Crosshair " + dataManager.userData.crosshairNum;
        bl_UCrosshair.Instance.Change(dataManager.userData.crosshairNum);

        preCrosshair.SetActive(false);
        nowCrosshair.SetActive(true);
    }
}