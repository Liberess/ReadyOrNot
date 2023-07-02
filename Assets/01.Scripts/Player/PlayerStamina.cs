using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerStamina : MonoBehaviourPun
{
    private PhotonView pv;
    public PhotonView PV { get => pv; }

    [SerializeField] private float originStamina = 100f;
    [SerializeField] private float restoreStaminaSpeed = 5f;

    public bool useStamina { get; private set; }
    public float stamina { get; private set; }

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void OnEnable() => Setup();

    private void Start() => Setup();

    private void Update()
    {
        if (!pv.IsMine)
            return;

        if (stamina < originStamina)
            stamina += Time.deltaTime * restoreStaminaSpeed;

        UpdateStaminaUI();
    }

    private void Setup()
    {
        useStamina = true;
        originStamina = (DataManager.Instance.userData.
            statUpLevels[(int)StatType.Stamina] * 0.5f) * 50f;
        restoreStaminaSpeed = 6 - (DataManager.Instance.
            userData.statUpLevels[(int)StatType.Stamina] * 0.5f);
        stamina = originStamina;

        if (UIManager.Instance != null)
            UIManager.Instance.SetMaxStamina(originStamina);
        else if (PVPUIManager.Instance != null)
            PVPUIManager.Instance.SetMaxStamina(originStamina);
        else if (CampaignUIManager.Instance != null)
            CampaignUIManager.Instance.SetMaxStamina(originStamina);

        UpdateStaminaUI();
    }

    public void UseStamina(float value)
    {
        stamina -= value;

        if (stamina <= 1f)
        {
            useStamina = false;
            StartCoroutine(Cool());
        }
    }

    private IEnumerator Cool()
    {
        yield return new WaitForSeconds(1f);
        useStamina = true;
    }

    private void UpdateStaminaUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.SetStaminaSlider(stamina);
        else if (PVPUIManager.Instance != null)
            PVPUIManager.Instance.SetStaminaSlider(stamina);
        else if (CampaignUIManager.Instance != null)
            CampaignUIManager.Instance.SetStaminaSlider(stamina);
    }

    public void DevStaminaUp() => restoreStaminaSpeed += 5f;
}