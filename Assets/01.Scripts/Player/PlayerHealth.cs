using UnityEngine;
using Photon.Pun;

public class PlayerHealth : LivingEntity
{
    private Animator anim;

    [Header("Audio Setting")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter; // 플레이어 슈터 컴포넌트

    private void Awake()
    {
        anim = GetComponent<Animator>();

        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    private void Start()
    {
        originHealth = DataManager.Instance.userData.statUpLevels[(int)StatType.Health] * 0.5f * 100f;
        Health = originHealth;

        if (photonView.IsMine)
        {
            livingID = photonView.ViewID;
            photonView.RPC("SetLivingID", RpcTarget.All, livingID);
            UpdateAllHealthUI();
        }

        // 플레이어 조작을 받는 컴포넌트들 활성화
        playerMovement.enabled = true;
        playerShooter.enabled = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        originHealth = DataManager.Instance.userData.statUpLevels[(int)StatType.Health] * 0.5f * 100f;
        Health = originHealth;

        if(photonView.IsMine)
            UpdateAllHealthUI();
    }

    public void DevRestoreHealthUp()
    {
        Health += 1000f;
        UpdateHealthUI();
    }

    public override void RestoreHealth(float value)
    {
        base.RestoreHealth(value);

        if (photonView.IsMine)
            UpdateHealthUI();
    }

    private void UpdateAllHealthUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetMaxHealth(originHealth);
            UIManager.Instance.SetHealthSlider(Health);
        }
        else if (PVPUIManager.Instance != null)
        {
            PVPUIManager.Instance.SetMaxHealth(originHealth);
            PVPUIManager.Instance.SetHealthSlider(Health);
        }
        else if (CampaignUIManager.Instance != null)
        {
            CampaignUIManager.Instance.SetMaxHealth(originHealth);
            CampaignUIManager.Instance.SetHealthSlider(Health);
        }
    }

    private void UpdateHealthUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.SetHealthSlider(Health);
        else if (PVPUIManager.Instance != null)
            PVPUIManager.Instance.SetHealthSlider(Health);
        else if (CampaignUIManager.Instance != null)
            CampaignUIManager.Instance.SetHealthSlider(Health);
    }

    [PunRPC]
    public override void ApplyDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!playerMovement.canHit)
            return;

        if (dead)
            return;

        base.ApplyDamage(damage, hitPoint, hitNormal);

        AudioManager.Instance.PlaySFX("PlayerHit");

        //EffectManager.Instance.PlayHitEffect(hitPoint, hitNormal, transform, EffectType.Flesh);
        EffectManager.Instance.PlayHitEffect(transform.position, transform.position, transform, EffectType.Flesh);

        if (photonView.IsMine)
            UpdateHealthUI();
    }

    public override void Die()
    {
        base.Die();

        AudioManager.Instance.PlaySFX("MonsterDie");

        anim.SetTrigger("Die");

        if (photonView.IsMine)
            UpdateHealthUI();

        // 플레이어 조작을 받는 컴포넌트들 비활성화
        playerMovement.enabled = false;
        playerShooter.enabled = false;
        GetComponent<PlayerHealth>().enabled = false;

        if(NetworkManager.Instance.PlayType == GameTypes.Match)
        {
            PVPManager.Instance.photonView.RPC(
                "PlayerRespawnRPC", RpcTarget.All, photonView.ViewID);
        }
    }
}