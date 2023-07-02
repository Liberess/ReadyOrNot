using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Missile : MonoBehaviourPun
{
    [SerializeField] private GameObject hitEft;

    public float damage;
    public Vector3 hitPoint;
    public Vector3 hitNormal;

    public float speed = 20f;
    private bool isDestroy = false;
    private Vector3 startPos;
    public float fireDistance;
    [SerializeField] private float explosionRange = 5f;

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
        rigid.AddForce(transform.forward * speed * 220f);
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
            photonView.RPC("Explosion", RpcTarget.All);
        }
    }

    private void FixedUpdate()
    {
        //rigid.velocity = transform.forward * speed;
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
    private void Explosion()
    {
        speed = 0;
        Collider[] cols = Physics.OverlapSphere(transform.position, explosionRange);

        if (cols != null)
        {
            foreach (var col in cols)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();

                if(rb != null)
                    rb.AddExplosionForce(100.0f, transform.position, 10.0f, 300.0f);

                var target = col.gameObject.GetComponent<LivingEntity>();

                if (target != null)
                    target.photonView.RPC("ApplyDamage", RpcTarget.All, damage, hitPoint, hitNormal);
            }
        }

        if (hitEft != null)
        {
            GameObject hit = Instantiate(hitEft, transform.position, Quaternion.identity);
            Destroy(hit, 3f);
            //gameObject.SetActive(false);
            photonView.RPC("DestroyMissileRPC", RpcTarget.All, 0f);
            //Destroy(gameObject, 3f);
        }
    }

    [PunRPC]
    private void DestroyMissileRPC(float delay)
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
        if (!PhotonNetwork.IsMasterClient)
            return;

        line.enabled = false;

        GetComponent<CapsuleCollider>().enabled = false;
        photonView.RPC("Explosion", RpcTarget.All);

        if (other.CompareTag("Breakable"))
            other.GetComponent<Breakable>().Hit(damage);
    }
}