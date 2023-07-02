using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EnemyType
{
    Slime = 0,
    TurtleShell,
    Chest,
    Beholder
}

public class Enemy : LivingEntity
{
    private enum State
    {
        Patrol = 0,     //순찰
        Tracking,       //추적
        AttackBegin, //공격 준비
        Attacking      //공격
    }

    [SerializeField] private State state;
    [SerializeField] private EnemyType enemyType;

    private PhotonView pv;
    public PhotonView PV { get => pv; }

    [Header("Target Setting")]
    //[HideInInspector] public LivingEntity targetEntity;
    public LivingEntity targetEntity;
    public List<LivingEntity> targetEntityList = new List<LivingEntity>();
    [SerializeField] private LayerMask whatIsTarget;

    [Header("View Setting")]
    [SerializeField] private float fieldOfView = 50f;
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private Transform eyeTransform;

    [Header("Move Setting")]
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField, Range(0.01f, 2f)] private float turnSmoothTime = 0.1f; // 방향 회전 지연 시간
    private float turnSmoothVelocity;

    [Header("Attack Setting")]
    [SerializeField] private Transform attackRoot;
    [SerializeField] private float damage = 30f;
    [SerializeField] private float attackRadius = 2f;
    private float attackDistance;

    [Header("Reward Setting")]
    [SerializeField] private Coin[] coinPrefabs = new Coin[3];
    [SerializeField] private float exp = 5f;

    private RaycastHit[] hits = new RaycastHit[10];
    private List<LivingEntity> lastAttackedTargets = new List<LivingEntity>();
    private bool hasTarget => targetEntity != null && !targetEntity.dead;

    private Vector3 prePos;
    private Vector3 nowPos;
    private float gap;

    private Animator anim;
    private NavMeshAgent navAgent;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (attackRoot != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(attackRoot.position, attackRadius);
        }

        if (eyeTransform != null)
        {
            var leftEyeRotation = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up);
            var leftRayDirection = leftEyeRotation * transform.forward;
            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, fieldOfView, viewDistance);
        }
    }
#endif

    private void Awake()
    {
        anim = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        pv = GetComponent<PhotonView>();

        var attackPivot = attackRoot.position;
        attackPivot.y = transform.position.y;

        attackDistance = Vector3.Distance(transform.position, attackPivot) + attackRadius;

        navAgent.isStopped = false;
        navAgent.stoppingDistance = attackDistance;
        navAgent.speed = patrolSpeed;
    }

    private void Start()
    {
        // 호스트가 아니라면, AI의 추적 루틴 실행 X
        if (!PhotonNetwork.IsMasterClient)
            return;

        prePos = transform.position;

        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
        StartCoroutine(CheckStopProcess());
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (dead)
            return;

        nowPos = transform.position;

        if (state == State.Tracking)
        {
            var distance = Vector3.Distance(targetEntity.transform.position, transform.position);
            if (distance <= attackDistance)
                BeginAttack();
        }

        if (navAgent.isStopped)
            anim.SetBool("isMove", false);
        else
            anim.SetBool("isMove", true);

        anim.SetFloat("Speed", navAgent.desiredVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (dead)
            return;

        if (state == State.AttackBegin || state == State.Attacking)
        {
            var lookRotation = Quaternion.LookRotation(targetEntity.transform.position - transform.position);
            var targetAngleY = lookRotation.eulerAngles.y;

            targetAngleY = Mathf.SmoothDampAngle
                (transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity, turnSmoothTime);
            transform.eulerAngles = Vector3.up * targetAngleY;
        }

        if (state == State.Attacking)
        {
            var direction = transform.forward;
            var deltaDistance = navAgent.velocity.magnitude * Time.deltaTime;

            //움직이는 궤적에 있는 콜라이더 감지
            var size = Physics.SphereCastNonAlloc
                (attackRoot.position, attackRadius, direction, hits, deltaDistance, whatIsTarget);

            for (int i = 0; i < size; i++)
            {
                var attackTargetEntity = hits[i].collider.GetComponent<LivingEntity>();

                if (attackTargetEntity != null && !lastAttackedTargets.Contains(attackTargetEntity))
                {
                    var message = new DamageMessage();
                    message.dmgAmount = damage;
                    message.damager = gameObject;

                    if (hits[i].distance <= 0f)
                        message.hitPoint = attackRoot.position;
                    else
                        message.hitPoint = hits[i].point;

                    message.hitNormal = hits[i].normal;

                    attackTargetEntity.ApplyDamage(message.dmgAmount, message.hitPoint, message.hitNormal);
                    lastAttackedTargets.Add(attackTargetEntity);
                    break;
                }
            }
        }
    }

    [PunRPC]
    public void Setup(float health, float damage, float runSpeed, float patrolSpeed)
    {
        originHealth = health;
        this.Health = health;
        this.damage = damage;
        this.runSpeed = runSpeed;
        this.patrolSpeed = patrolSpeed;

        // 탐색 단계 애니메이션
        navAgent.speed = patrolSpeed;
    }

    [PunRPC]
    public void SetExp(float _exp) => exp = _exp;

    private IEnumerator UpdatePath()
    {
        while (!dead)
        {
            if (hasTarget)
            {
                if (state == State.Patrol)
                {
                    state = State.Tracking;
                    navAgent.speed = runSpeed;
                }
                navAgent.SetDestination(targetEntity.transform.position);
            }
            else
            {
                prePos = transform.position;

                if (targetEntity != null)
                    targetEntity = null;

                if (state != State.Patrol)
                {
                    state = State.Patrol;
                    navAgent.speed = patrolSpeed;
                }

                if (navAgent.remainingDistance <= 1f)
                {
                    var patrolTargetPos = Utility.GetRandPointOnNavMesh(
                        transform.position, 20f, NavMesh.AllAreas);
                    navAgent.SetDestination(patrolTargetPos);
                }

                var colliders = Physics.OverlapSphere(
                    eyeTransform.position, viewDistance, whatIsTarget);

                foreach (var collider in colliders)
                {
                    if (!IsTargetOnSight(collider.transform))
                        continue;

                    var livingEntity = collider.GetComponent<LivingEntity>();

                    if (livingEntity != null && !livingEntity.dead)
                    {
                        targetEntity = livingEntity;
                        targetEntityList.Add(livingEntity);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator CheckStopProcess()
    {
        yield return new WaitForSeconds(1f);

        nowPos = transform.position;
        gap = Vector3.Distance(prePos, nowPos);
        if (Mathf.Abs(gap) <= 0.1f)
        {
            var patrolTargetPos = Utility.GetRandPointOnNavMesh(
                transform.position, 20f, NavMesh.AllAreas);
            navAgent.SetDestination(patrolTargetPos);
        }

        StartCoroutine(CheckStopProcess());
    }

    [PunRPC]
    public override void ApplyDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.ApplyDamage(damage, hitPoint, hitNormal);

        if (targetEntity == null) //아직 추격할 대상이 없는데, 공격을 당했다면
            targetEntity = FindObjectOfType<PlayerCtrl>().GetComponent<LivingEntity>();
            //targetEntity = dmgMsg.damager.GetComponent<LivingEntity>();

        EffectManager.Instance.PlayHitEffect(hitPoint, hitNormal, transform, EffectType.Flesh);

        AudioManager.Instance.PlaySFX("MonsterHit");
    }

    private bool IsTargetOnSight(Transform target)
    {
        // 눈의 위치에서 타겟을 향한 방향
        var direction = target.position - eyeTransform.position;

        // 높이(수직) 각도 차이는 신경쓰지 않기 위한 것!!!
        direction.y = eyeTransform.forward.y;

        if (Vector3.Angle(direction, eyeTransform.forward) > fieldOfView * 0.5f)
            return false;

        //이거 때문에 찾지를 못했었음...;;
        //direction = target.position - eyeTransform.position;

        RaycastHit hit;

        if (Physics.Raycast(eyeTransform.position, direction, out hit, viewDistance, whatIsTarget))
        {
            if (hit.transform == target)
                return true;
        }

        return false;
    }

    #region Attack
    public void BeginAttack()
    {
        state = State.AttackBegin;
        navAgent.isStopped = true;
        anim.SetTrigger("doAttack");
    }

    public void EnableAttack()
    {
        state = State.Attacking;
        lastAttackedTargets.Clear();
    }

    public void DisableAttack()
    {
        if (hasTarget)
            state = State.Tracking;
        else
            state = State.Patrol;

        navAgent.isStopped = false;
    }
    #endregion

    public override void Die()
    {
        base.Die();

        if (PV.IsMine && CampaignManager.Instance == null)
            DataManager.Instance.gameData.exp += exp;

        if (NetworkManager.Instance.PlayType != GameTypes.Campaign) //Campaign 모드가 아님
        {
            float[] percents = { 0f };

            switch (enemyType)
            {                                                                             // G    B  Red Coin
                case EnemyType.Slime: percents = new float[] { 94f, 5f, 1f }; break;
                case EnemyType.TurtleShell: percents = new float[] { 50f, 40f, 10f }; break;
                case EnemyType.Chest: percents = new float[] { 20f, 60f, 20f }; break;
                case EnemyType.Beholder: percents = new float[] { 10f, 40f, 50f }; break;
                default: percents = new float[] { 94f, 5f, 1f }; break;
            }

            for (int i = 0; i < Random.Range(1, 6); i++)
            {
                if (GetCoin(percents) == false) //만약 코인이 아예 나오지 않았으면 다시
                    --i;
            }
        }
        else
        {
            float[] percents = { 0f };

            switch (CampaignManager.Instance.StageType)
            {
                case StageTypes.Ballantines: percents = new float[] { 94f, 5f, 1f }; break;
                case StageTypes.Halloween: percents = new float[] { 50f, 40f, 10f }; break;
                case StageTypes.Boss: percents = new float[] { 30f, 50f, 20f }; break;
                default: percents = new float[] { 94f, 5f, 1f }; break;
            }

            for (int i = 0; i < Random.Range(1, 6); i++)
            {
                if (GetCoin(percents) == false) //만약 코인이 아예 나오지 않았으면 다시
                    --i;
            }
        }

        GetComponent<Rigidbody>().AddForce(Vector3.up * 0.5f);
        //GetComponent<Collider>().enabled = false;

        navAgent.enabled = false;
        anim.applyRootMotion = true;
        anim.SetTrigger("doDie");

        AudioManager.Instance.PlaySFX("MonsterDie");

        //Drop Key
        var dropKey = GetComponent<DropKey>();

        if (dropKey != null)
            dropKey.CreateKey();

        //GetComponent<Enemy>().enabled = false;
        gameObject.layer = 15;
    }

    private bool GetCoin(float[] per) //각각의 확률에 맞춰 코인 생성
    {
        if (ChanceMaker.GetThisChanceResult_Percentage(per[(int)CoinTypes.Red]))
        {
            //var coin = CoinManager.GetCoinObj((int)CoinTypes.Red);
            var coin = PhotonNetwork.Instantiate(coinPrefabs[(int)CoinTypes.Red].name,
                transform.position, Quaternion.identity).GetComponent<Coin>();
            coin.transform.position = transform.position + (Vector3.up * 3f);
            coin.transform.rotation = Quaternion.identity;
            coin.Burst();
            return true;
        }
        else if (ChanceMaker.GetThisChanceResult_Percentage(per[(int)CoinTypes.Blue]))
        {
            //var coin = CoinManager.GetCoinObj((int)CoinTypes.Blue);
            var coin = PhotonNetwork.Instantiate(coinPrefabs[(int)CoinTypes.Blue].name,
    transform.position, Quaternion.identity).GetComponent<Coin>();
            coin.transform.position = transform.position + (Vector3.up * 3f);
            coin.transform.rotation = Quaternion.identity;
            coin.Burst();
            return true;
        }
        else if (ChanceMaker.GetThisChanceResult_Percentage(per[(int)CoinTypes.Gold]))
        {
            //var coin = CoinManager.GetCoinObj((int)CoinTypes.Gold);
            var coin = PhotonNetwork.Instantiate(coinPrefabs[(int)CoinTypes.Gold].name,
    transform.position, Quaternion.identity).GetComponent<Coin>();
            coin.transform.position = transform.position + (Vector3.up * 3f);
            coin.transform.rotation = Quaternion.identity;
            coin.Burst();
            return true;
        }

        return false;
    }
}