using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviour
{
    public int ID;
    public float damage;
    public Vector3 hitPoint;
    public Vector3 hitNormal;

    public float speed = 20f;
    public float hitOffset = 0f;
    private bool isDestroy = false;
    private Vector3 startPos;
    public float fireDistance;
    public bool UseFirePointRotation;
    /*    public Vector3 rotationOffset = new Vector3(0, 0, 0);*/
    public GameObject hit;
    public GameObject flash;
    public GameObject[] Detached;

    private BulletEffect hitInstance;
    private BulletEffect flashInstance;

    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        /*        if (!PhotonNetwork.IsMasterClient || !photonView.IsMine)
                    return;*/

        speed = 50f;
        isDestroy = false;

        startPos = transform.position;

        if (flash != null)
        {
            flashInstance = Instantiate(flash,
                transform.position, transform.rotation).GetComponent<BulletEffect>();
            flashInstance.transform.forward = gameObject.transform.forward;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();

            if (flashPs != null)
            {
                flashInstance.DestroyProcess(flashPs.main.duration);
            }
            else
            {
                var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                flashInstance.DestroyProcess(flashPsParts.main.duration);
            }
        }
    }

    private void Update()
    {
        /*        if (!PhotonNetwork.IsMasterClient)
                    return;*/

        // 이미 파괴 동작을 수행 중이라면 실행X
        if (isDestroy)
            return;

        var distance = Vector3.Distance(startPos, transform.position);
        if (distance >= fireDistance)
        {
            isDestroy = true;
            DestroyBullet(0f);

            if (hitInstance != null)
                hitInstance.DestroyProcess(0f);

            if (flashInstance != null)
                flashInstance.DestroyProcess(0f);
        }
    }

    private void FixedUpdate()
    {
        rigid.velocity = transform.forward * speed;
    }

    [PunRPC]
    public void Setup(int _ID, float _damage, Vector3 _hitPoint, Vector3 _hitNormal, float distance)
    {
        ID = _ID;
        damage = _damage;
        hitPoint = _hitPoint;
        hitNormal = _hitNormal;
        fireDistance = distance;
    }

    [PunRPC]
    private void DestroyBullet(float delay)
    {
        StartCoroutine(DestroyBulletProcess(delay));
    }

    private IEnumerator DestroyBulletProcess(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);

        /*        if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.Destroy(photonView);*/
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            var target = collision.gameObject.GetComponent<PlayerCtrl>();

            if (target != null && target.actorID != ID)
            {
                speed = 0;
                rigid.constraints = RigidbodyConstraints.FreezeAll;

                if (target.photonView.IsMine)
                    AudioManager.Instance.PlaySFX("PlayerHit");

                target.GetComponent<LivingEntity>().photonView.RPC
                    ("ApplyDamage", RpcTarget.All, damage, hitPoint, hitNormal);
                DestroyBullet(0f);
            }

            return;
        }
        else if (collision.gameObject.tag == "Monster")
        {
            var target = collision.gameObject.GetComponent<LivingEntity>();

            if (target != null)
            {
                speed = 0;
                rigid.constraints = RigidbodyConstraints.FreezeAll;
                AudioManager.Instance.PlaySFX("MonsterHit");
                target.photonView.RPC("ApplyDamage", RpcTarget.All, damage, hitPoint, hitNormal);
            }

            DestroyBullet(0f);

            return;
        }
        else if (hit != null)
        {
            speed = 0;
            rigid.constraints = RigidbodyConstraints.FreezeAll;

            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point + contact.normal * hitOffset;

            hitInstance = Instantiate(hit, pos, rot).
                GetComponent<BulletEffect>();

            if (UseFirePointRotation)
            {
                hitInstance.transform.rotation =
                    gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0);
            }
            else
            {
                hitInstance.transform.LookAt(contact.point + contact.normal);
            }

            var hitPs = hitInstance.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                hitInstance.DestroyProcess(hitPs.main.duration);
            }
            else
            {
                var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                hitInstance.DestroyProcess(hitPsParts.main.duration);
            }
        }

        foreach (var detachedPrefab in Detached)
        {
            if (detachedPrefab != null)
                detachedPrefab.transform.parent = null;
        }

        DestroyBullet(0f);
    }
}