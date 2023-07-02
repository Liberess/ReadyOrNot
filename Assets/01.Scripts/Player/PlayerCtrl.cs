using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class PlayerCtrl : MonoBehaviourPun, IPunObservable
{
    public int actorID;
    public string actorName;
    public PhotonView PV { get; private set; }

    [SerializeField] private GunData playerGunData;
    public GunData PlayerGunData { get => playerGunData; }
    private int gunID;
    [SerializeField] private float reloadAnimTime;

    private PlayerHealth playerHealth;
    private PlayerShooter playerShooter;
    private PlayerMovement playerMovement;

    private Animator anim;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
        playerShooter = GetComponent<PlayerShooter>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        if (!PV.IsMine)
            return;

        if (NetworkManager.Instance.PlayType == GameTypes.Campaign)
        {
            var minimap = FindObjectOfType<MinimapCam>();
            minimap.SetCam();
        }

        playerHealth.OnDeath += HandleDeath;

        actorID = photonView.ViewID;
        gameObject.name = PhotonNetwork.LocalPlayer.NickName;
        actorName = PhotonNetwork.LocalPlayer.NickName;
        playerHealth.livingID = actorID;
        playerHealth.photonView.RPC("SetLivingID", RpcTarget.All, actorID);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        playerGunData = DataManager.Instance.userData.equipGunData;
        gunID = (int)playerGunData.GunType;
        anim.runtimeAnimatorController = DataManager.Instance.userData.animCtrls[gunID];

        SetGunReloadAnim();
    }

    private void SetGunReloadAnim()
    {
        switch (playerGunData.GunType)
        {
            case GunTypes.Pistol: anim.SetFloat("ReloadTime", 1.5f); reloadAnimTime = 1.5f;  break;
            case GunTypes.SMG: anim.SetFloat("ReloadTime", 1.2f); reloadAnimTime = 1.2f; break;
            case GunTypes.AssaultRifle: anim.SetFloat("ReloadTime", 0.7f); reloadAnimTime = 0.7f; break;
            case GunTypes.HuntingRifle: anim.SetFloat("ReloadTime", 0.52f); reloadAnimTime = 0.52f; break;
            case GunTypes.CrossBow: anim.SetFloat("ReloadTime", 0.38f); reloadAnimTime = 0.38f; break;
            case GunTypes.Shotgun: anim.SetFloat("ReloadTime", 0.47f); reloadAnimTime = 0.47f; break;
            case GunTypes.SniperRifle: anim.SetFloat("ReloadTime", 0.5f); reloadAnimTime = 0.5f; break;
            case GunTypes.Bazooka: anim.SetFloat("ReloadTime", 0.3f); reloadAnimTime = 0.3f; break;
            case GunTypes.MiniGun: anim.SetFloat("ReloadTime", 0.3f); reloadAnimTime = 0.3f; break;
            default: break;
        }
    }

    private void HandleDeath()
    {
        playerHealth.enabled = false;
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        GameTypes playType = NetworkManager.Instance.PlayType;

        switch (playType)
        {
            case GameTypes.Campaign:
                CampaignManager.Instance.GameOver();
                break;
            case GameTypes.SoloSurvival:
                GameManager.Instance.GameOver();
                break;
            case GameTypes.MultiSurvival:
                GameManager.Instance.GameOver();
                break;
            case GameTypes.Match:
                PVPUIManager.Instance.SetGameOverUI(true);

                if (PhotonNetwork.IsMasterClient)
                {
                    PVPManager.Instance.AddKillCount((int)TeamTypes.Blue, 1);
                }
                else
                {
                    //PVPManager.Instance.AddKillCount((int)TeamTypes.Red, 1);
                    PVPManager.Instance.photonView.RPC("AddKillCount", RpcTarget.MasterClient,
                        (int)TeamTypes.Red, 1);
                }

                PVPManager.Instance.photonView.RPC(
                    "PlayerRespawnRPC", RpcTarget.MasterClient, photonView.ViewID);
                break;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void GameClear()
    {
        playerHealth.enabled = false;
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        gameObject.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlayerSpawn(Transform target)
    {
        gameObject.SetActive(false);

        transform.position = target.position;
        transform.rotation = target.rotation;

        playerHealth.enabled = true;
        playerMovement.enabled = true;
        playerShooter.enabled = true;

        gameObject.SetActive(true);

        playerShooter.gun.ammoRemain =
            playerShooter.gun.GunData.InitAmmoRemain;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    [PunRPC]
    public void Respawn() //PVP전용
    {
        if(NetworkManager.Instance.PlayType == GameTypes.Match)
            PVPUIManager.Instance.SetGameOverUI(false);

        gameObject.SetActive(false);

        transform.position = Utility.GetRandPointOnNavMesh(
            transform.position, 30f, NavMesh.AllAreas);

        playerHealth.enabled = true;
        playerMovement.enabled = true;
        playerShooter.enabled = true;

        gameObject.SetActive(true);

        if (PV.IsMine)
            PV.RPC("SetReloadAnim", RpcTarget.All, reloadAnimTime);

        playerShooter.gun.ammoRemain =
            playerShooter.gun.GunData.InitAmmoRemain;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    [PunRPC]
    public void SetReloadAnim(float time)
    {
        anim.SetFloat("ReloadTime", time);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PV.IsMine)
            return;

        if (playerHealth.dead)
            return;

        var item = other.GetComponent<IItem>();

        if (item != null)
        {
            item.Use(gameObject);

            if (other.GetComponent<HealthPack>() && UIManager.Instance != null)
                UIManager.Instance.SetHealthSlider(playerHealth.Health);
            else if (other.GetComponent<HealthPack>() && CampaignUIManager.Instance != null)
                CampaignUIManager.Instance.SetHealthSlider(playerHealth.Health);
            else if (other.GetComponent<HealthPack>() && PVPUIManager.Instance != null)
                PVPUIManager.Instance.SetHealthSlider(playerHealth.Health);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(gunID);
            stream.SendNext(actorID);
            stream.SendNext(actorName);
            stream.SendNext(reloadAnimTime);
        }
        else
        {
            gunID = (int)stream.ReceiveNext();
            actorID = (int)stream.ReceiveNext();
            actorName = (string)stream.ReceiveNext();
            reloadAnimTime = (float)stream.ReceiveNext();
            anim.SetFloat("ReloadTime", reloadAnimTime);
        }
    }
}