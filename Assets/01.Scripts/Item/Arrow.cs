using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Arrow : MonoBehaviourPun
{
    public float damage;
    public Vector3 hitPoint;
    public Vector3 hitNormal;

    public float speed = 15f;
    private bool isDestroy = false;
    private Vector3 startPos;
    public float fireDistance;

    private Rigidbody rigid;
    private LineRenderer line;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        isDestroy = false;
        rigid.useGravity = true;
        line.enabled = true;

        if (!PhotonNetwork.IsMasterClient)
            return;

        startPos = transform.position;
    }

    private void Update()
    {
        // 이미 파괴 동작을 수행 중이라면 실행X
        if (isDestroy)
            return;

        var distance = Vector3.Distance(startPos, transform.position);
        if (distance >= fireDistance)
        {
            isDestroy = true;
            line.enabled = false;
            GetComponent<CapsuleCollider>().isTrigger = false;
            photonView.RPC("DestroyArrowRPC", RpcTarget.All, 5f);
        }
    }

    private void FixedUpdate()
    {
        if (isDestroy)
            return;

        rigid.velocity = transform.forward * speed + Vector3.down * Time.deltaTime * 0.5f;
    }

    [PunRPC]
    public void Setup(float _damage, Vector3 _hitPoint, Vector3 _hitNormal, float distance)
    {
        damage = _damage;
        hitPoint = _hitPoint;
        hitNormal = _hitNormal;
        fireDistance = distance;
    }

    [PunRPC]
    private void DestroyArrowRPC(float delay)
    {
        StartCoroutine(DestroyProcess(delay));
    }

    private IEnumerator DestroyProcess(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gameObject != null && PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDestroy)
            return;

        speed = 0;
        rigid.isKinematic = true;
        line.enabled = false;

        if (other.tag == "Monster" || other.tag == "Player")
        {
            var target = other.GetComponent<LivingEntity>();

            if (target != null)
            {
                hitPoint = transform.position;
                hitNormal = transform.position;

                target.photonView.RPC("ApplyDamage",
                    RpcTarget.All, damage, hitPoint, hitNormal);
            }

            transform.SetParent(other.transform);
        }

        GetComponent<CapsuleCollider>().enabled = false;

        if (other.CompareTag("Breakable"))
        {
            other.GetComponent<Breakable>().Hit(damage);
            photonView.RPC("DestroyArrowRPC", RpcTarget.All, 0f);
        }

        //Destroy(gameObject, 5f);
        photonView.RPC("DestroyArrowRPC", RpcTarget.All, 5f);
    }
}