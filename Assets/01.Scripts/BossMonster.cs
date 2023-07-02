using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class BossMonster : LivingEntity
{
    private LivingEntity targetEntity;

    [Header("Move Setting")]
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float patrolSpeed = 10f;

    [Header("Reward Setting")]
    [SerializeField] private Coin[] coinPrefabs = new Coin[3];
    [SerializeField] private float exp = 5f;

    private AudioSource audioSrc;
    [Header("Audio Setting")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;

    private HpBarEft hpBarEft;

    public float distance;

    private Animator anim;
    private NavMeshAgent navAgent;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        navAgent = GetComponent<NavMeshAgent>();

        hpBarEft = GetComponent<HpBarEft>();

        navAgent.isStopped = false;
        navAgent.speed = patrolSpeed;

        if(originHealth == 0f)
            originHealth = 1000f;

        Health = originHealth;
    }

    private void Start()
    {
        targetEntity = FindObjectOfType<PlayerCtrl>().
            gameObject.GetComponent<LivingEntity>();

        if (targetEntity != null)
            StartCoroutine(UpdatePath());

        hpBarEft.SetUp(originHealth);
    }

    private void Update()
    {
        if (dead)
            return;

        if (navAgent.isStopped)
            anim.SetBool("isMove", false);
        else
            anim.SetBool("isMove", true);

        anim.SetFloat("Speed", navAgent.desiredVelocity.magnitude);
    }

    private IEnumerator UpdatePath()
    {
        while (!dead)
        {
            distance = Vector3.Distance(transform.position, targetEntity.transform.position);

            if(distance <= 10f)
            {
                navAgent.speed = runSpeed;
                var dirc = (transform.position - targetEntity.transform.position).normalized;
                navAgent.SetDestination(dirc);
            }
            else
            {
                if (navAgent.remainingDistance <= 1f)
                {
                    navAgent.speed = patrolSpeed;
                    var patrolTargetPos = Utility.GetRandPointOnNavMesh(
                        transform.position, 20f, NavMesh.AllAreas);
                    navAgent.SetDestination(patrolTargetPos);
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    [PunRPC]
    public override void ApplyDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.ApplyDamage(damage, hitPoint, hitNormal);

        hpBarEft.currentHp = Health;
        hpBarEft.StartCoroutine(hpBarEft.Hit());

        EffectManager.Instance.PlayHitEffect(hitPoint, hitNormal, transform, EffectType.Flesh);

        if (hitClip != null)
            audioSrc.PlayOneShot(hitClip);
    }

    public override void Die()
    {
        base.Die();

        CampaignManager.Instance.AddExp(exp);

                                    // G     B   Red Coin
        float[] percents = { 10f, 20f, 70f };

        for (int i = 0; i < Random.Range(1, 15); i++)
        {
            if (GetCoin(percents) == false) //만약 코인이 아예 나오지 않았으면 다시
                --i;
        }

        GetComponent<Rigidbody>().AddForce(Vector3.up * 0.5f);
        //GetComponent<Collider>().enabled = false;

        navAgent.enabled = false;
        anim.applyRootMotion = true;
        anim.SetTrigger("doDie");

        if (deathClip != null)
            audioSrc.PlayOneShot(deathClip);

        CampaignManager.Instance.GameClear();
        GetComponent<BossMonster>().enabled = false;
    }

    private bool GetCoin(float[] per) //각각의 확률에 맞춰 코인 생성
    {
        if (ChanceMaker.GetThisChanceResult_Percentage(per[(int)CoinTypes.Gold]))
        {
            var coin = PhotonNetwork.Instantiate(coinPrefabs[(int)CoinTypes.Gold].name,
                transform.position, Quaternion.identity).GetComponent<Coin>();
            coin.transform.position = transform.position + (Vector3.up * 10f);
            coin.transform.localScale = new Vector3(5f, 5f, 5f);
            coin.transform.rotation = Quaternion.identity;
            coin.Burst();
            return true;
        }
        else if (ChanceMaker.GetThisChanceResult_Percentage(per[(int)CoinTypes.Blue]))
        {
            var coin = PhotonNetwork.Instantiate(coinPrefabs[(int)CoinTypes.Blue].name,
                transform.position, Quaternion.identity).GetComponent<Coin>();
            coin.transform.position = transform.position + (Vector3.up * 10f);
            coin.transform.localScale = new Vector3(5f, 5f, 5f);
            coin.transform.rotation = Quaternion.identity;
            coin.Burst();
            return true;
        }
        else if (ChanceMaker.GetThisChanceResult_Percentage(per[(int)CoinTypes.Red]))
        {
            var coin = PhotonNetwork.Instantiate(coinPrefabs[(int)CoinTypes.Red].name,
                transform.position, Quaternion.identity).GetComponent<Coin>();
            coin.transform.position = transform.position + (Vector3.up * 10f);
            coin.transform.localScale = new Vector3(5f, 5f, 5f);
            coin.transform.rotation = Quaternion.identity;
            coin.Burst();
            return true;
        }

        return false;
    }
}