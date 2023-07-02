using System.Collections.Generic;
using UnityEngine;

public enum GunTypes
{
    Pistol = 0,
    SMG,
    AssaultRifle,
    HuntingRifle,
    CrossBow,
    Shotgun,
    SniperRifle,
    Bazooka,
    MiniGun
}

[CreateAssetMenu(fileName = "Gun Data", menuName = "Scriptable Object/Gun Data", order = int.MaxValue)]
public class GunData : ScriptableObject
{
    [Header("Gun Setting")]
    [SerializeField] private string gunName;
    public string GunName { get => gunName; }
    [SerializeField] private GunTypes gunType;
    public GunTypes GunType { get => gunType; }
    [SerializeField] private int gunBuyCost;
    public int GunBuyCost { get => gunBuyCost; }
    [SerializeField] private int gunUpCost;
    public int GunUpCost { get => gunUpCost; set => gunUpCost = value; }
    [SerializeField] private GameObject gunPrefab;
    public GameObject GunPrefab { get => gunPrefab; }
    [SerializeField] private GameObject gunUIPrefab;
    public GameObject GunUIPrefab { get => gunUIPrefab; }

    [Header("Position Setting")]
    public int empty = 0;

    [Header("Bullet Stat Setting")]
    [SerializeField] private float damage = 25f;
    public float Damage { get => damage; set => damage = value; }
    [SerializeField] private float fireDistance = 100f;                                                 // 사거리
    public float FireDistance { get => fireDistance; set => fireDistance = value; }
    [SerializeField] private int bulletNum = 1;
    public int BulletNum { get => bulletNum; }                                                          // 발사할 총알 개수

    [Header("Gun Stat Setting")]
    [SerializeField] private int initAmmoRemain;
    public int InitAmmoRemain { get => initAmmoRemain; }                                     // 처음 총알 수
    [SerializeField] private int ammoRemain = 100;                                                   // 남은 탄약 수
    public int AmmoRemain { get => ammoRemain; set => ammoRemain = value; }
    [SerializeField] private int magAmmo;                                                                  // 현재 탄창 안의 탄약 수
    public int MagAmmo { get => magAmmo; set => magAmmo = value; }
    [SerializeField] private int magCapacity = 30;                                                     // 탄창 용량
    public int MagCapacity { get => magCapacity; set => magCapacity = value; }
    [SerializeField] private float timeBetweenFire = 0.12f;                                       // 총알 발사 간격
    public float TimeBetweenFire { get => timeBetweenFire; }
    [SerializeField] private float reloadTime = 1.8f;                                                  // 재장전 소요 시간
    public float ReloadTime { get => reloadTime; set => reloadTime = value; }
    [SerializeField, Range(0f, 10f)] private float maxSpread = 3f;                            // 탄착군 최대 범위
    public float MaxSpread { get => maxSpread; }
    [SerializeField, Range(1f, 10f)] private float stability = 1f;                                 // 안정성(높을수록 반동 낮음)
    public float Stability { get => stability; set => stability = value; }
    [SerializeField, Range(0.01f, 3f)] private float restoreFromRecoilSpeed = 2f;   // 반동 회복 속도
    public float RestoreFromRecoilSpeed { get => restoreFromRecoilSpeed; }

    [Header("SFX Setting")]
    [SerializeField] private AudioClip shotClip;
    public AudioClip ShotClip { get => shotClip; }
    [SerializeField] private AudioClip reloadClip;
    public AudioClip ReloadClip { get => reloadClip; }
}