using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Gun : MonoBehaviourPun, IPunObservable
{
    public enum GunState
    {
        Ready = 0,
        Empty,
        Reloading
    }

    public enum ViewState
    {
        TPS = 0,
        FPS
    }

    public static string[] gunNames =
    {
        "Pistol", "SMG", "AssaultRifle", "HuntingRifle", "CrossBow",
        "Shotgun", "SniperRifle", "Bazooka", "MiniGun"
    };

    public GunState gunState { get; private set; }

    private int actorID;
    private PlayerCtrl playerCtrl;
    private PlayerShooter gunHolder;

    [Header("Position Setting")]
    public Transform[] firePos = new Transform[9];

    [Header("Bullet Stat Setting")]
    public float damage = 25f;                                                                                    // 총알 데미지
    public float fireDistance = 100f;                                                                           // 총알 범위

    [Header("Gun Stat Setting")]
    [SerializeField] private int id;
    public int ID { get => id; }
    public ViewState viewState;
    [SerializeField] private GunData gunData;
    public GunData GunData { get => gunData; }
    public int ammoRemain = 100;                                                                             // 남은 탄약 수
    public int magAmmo;                                                                                            // 현재 탄창 안의 탄약 수
    [SerializeField] private int magCapacity = 30;                                                     // 탄창 용량
    [SerializeField] private float timeBetweenFire = 0.12f;                                       // 총알 발사 간격
    [SerializeField] private float reloadTime = 1.8f;                                                  // 재장전 소요 시간
    [SerializeField, Range(0f, 10f)] private float maxSpread = 3f;                            // 탄착군 최대 범위
    [SerializeField, Range(1f, 10f)] private float stability = 1f;                                 // 안정성(높을수록 반동 낮음)
    [SerializeField, Range(0.01f, 3f)] private float restoreFromRecoilSpeed = 2f;   // 반동 회복 속도
    private float currentSpread;                                                                                 // 현재 탄 퍼짐 정도
    private float currentSpreadVelocity;                                                                    // 탄 퍼짐 변화량

    private float lastFireTime;                                                                                   // 마지막으로 발사한 시간
    private LayerMask excludeTarget;                                                                      // 총알을 맞으면 안되는 대상

    [Header("Effect Setting")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private GameObject bulletPrefab;

    private void OnEnable()
    {
        InitGunStat();
        gunState = GunState.Ready;
    }

    private void OnDisable() => StopAllCoroutines();

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        currentSpread = Mathf.Clamp(currentSpread, 0f, maxSpread);
        currentSpread = Mathf.SmoothDamp(
            currentSpread, 0f, ref currentSpreadVelocity, 1f / restoreFromRecoilSpeed);
    }

    public void Setup(PlayerShooter gunHolder)
    {
        this.gunHolder = gunHolder;
        excludeTarget = gunHolder.excludeTarget;
        playerCtrl = gunHolder.GetComponent<PlayerCtrl>();
        actorID = playerCtrl.actorID;
        gunData = playerCtrl.PlayerGunData;

        InitGunStat();
    }

    private void InitGunStat()
    {
        gunData = DataManager.Instance.userData.equipGunData;

        id = (int)gunData.GunType;

        damage = gunData.Damage;
        fireDistance = gunData.FireDistance;

        ammoRemain = gunData.InitAmmoRemain;
        magAmmo = gunData.MagCapacity;
        magCapacity = gunData.MagCapacity;
        timeBetweenFire = gunData.TimeBetweenFire;
        reloadTime = gunData.ReloadTime;
        maxSpread = gunData.MaxSpread;
        stability = gunData.Stability;
        restoreFromRecoilSpeed = gunData.RestoreFromRecoilSpeed;

        lastFireTime = 0f;
        currentSpread = 0f;
        currentSpreadVelocity = 0f;
    }

    [PunRPC]
    public void AddAmmo(int value) => ammoRemain += value;

    public bool Fire(Vector3 aimTarget)
    {
        if (gunState == GunState.Ready && Time.time >= lastFireTime + timeBetweenFire)
        {
            var fireDir = aimTarget - firePos[ID].position;

            var xError = Utility.GetRandNormalDistribution(0f, currentSpread);
            var yError = Utility.GetRandNormalDistribution(0f, currentSpread);

            fireDir = Quaternion.AngleAxis(yError, Vector3.up) * fireDir;
            fireDir = Quaternion.AngleAxis(xError, Vector3.right) * fireDir;

            currentSpread += 1f / stability;

            lastFireTime = Time.time;
            Shot(firePos[ID].position, fireDir);

            return true;
        }

        return false;
    }

    private void Shot(Vector3 startPoint, Vector3 direction)
    {
        EffectManager.Instance.PlayHitEffect(transform.position,
            transform.position, transform, EffectType.Shell);

        //AudioManager.Instance.PlaySFX("Shot");

        photonView.RPC("ShotProcessRPC", RpcTarget.All, startPoint, direction);

        --magAmmo;
        if (magAmmo <= 0)
            gunState = GunState.Empty;
    }

    [PunRPC]
    private void ShotProcessRPC(Vector3 startPoint, Vector3 direction)
    {
        AudioManager.Instance.PlaySFX("Shot");

        var hitPos = startPoint + direction * fireDistance;

        firePos[ID].LookAt(hitPos);

        if (gunData.BulletNum <= 1)
        {
            if (gunData.GunType == GunTypes.CrossBow)
            {
                var arrow = Instantiate(arrowPrefab,
                    firePos[ID].transform.position, firePos[ID].transform.rotation).GetComponent<Arrow>();
                arrow.Setup(damage, direction, direction, fireDistance);
            }
            else if (gunData.GunType == GunTypes.Bazooka)
            {
                var missile = Instantiate(missilePrefab,
                    firePos[ID].transform.position, firePos[ID].transform.rotation).GetComponent<Missile>();
                missile.Setup(damage, direction, direction, fireDistance);
            }
            else
            {
                var bullet = Instantiate(bulletPrefab, firePos[ID].transform.position,
                    firePos[ID].transform.rotation).GetComponent<Bullet>();
                bullet.Setup(actorID, damage, direction, direction, fireDistance);
            }
        }
        else // 샷건
        {
            for (int i = 0; i < gunData.BulletNum; i++)
            {
                var xError = Utility.GetRandNormalDistribution(0.1f, currentSpread * 3f);
                var yError = Utility.GetRandNormalDistribution(0.1f, currentSpread * 3f);
                var zError = Utility.GetRandNormalDistribution(0.1f, currentSpread * 3f);

                var fireDir = direction;

                fireDir = Quaternion.AngleAxis(xError, Vector3.right) * fireDir;
                fireDir = Quaternion.AngleAxis(yError, Vector3.up) * fireDir;
                fireDir = Quaternion.AngleAxis(zError, Vector3.forward) * fireDir;

                firePos[ID].LookAt(fireDir);

                var bullet = Instantiate(bulletPrefab, firePos[ID].transform.position,
                    firePos[ID].transform.rotation).GetComponent<Bullet>();
                bullet.Setup(actorID, damage, direction, direction, fireDistance);
            }
        }
    }

    public bool Reload()
    {
        if (gunState == GunState.Reloading || ammoRemain <= 0
            || magAmmo >= magCapacity)
            return false;

        StartCoroutine(ReloadRoutine());
        return true;
    }

    private IEnumerator ReloadRoutine()
    {
        gunState = GunState.Reloading;
        AudioManager.Instance.PlaySFX("Reload");

        yield return new WaitForSeconds(reloadTime);

        var ammoToFill = Mathf.Clamp(magCapacity - magAmmo,
            0, ammoRemain); // 채울 수 있는 탄약 수

        magAmmo += ammoToFill;
        ammoRemain -= ammoToFill;

        gunState = GunState.Ready;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(id);
            stream.SendNext(reloadTime);
            stream.SendNext(ammoRemain);
            stream.SendNext(magAmmo);
            stream.SendNext(gunState);
        }
        else
        {
            id = (int)stream.ReceiveNext();
            reloadTime = (float)stream.ReceiveNext();
            ammoRemain = (int)stream.ReceiveNext();
            magAmmo = (int)stream.ReceiveNext();
            gunState = (GunState)stream.ReceiveNext();
        }
    }
}