using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyManager : MonoBehaviourPun, IPunObservable
{
    private static EnemyManager instance;
    public static EnemyManager Instance { get => instance; }

    [Header("Set Enemy Prefab")]
    Queue<Enemy> enemyQueue = new Queue<Enemy>();
    [SerializeField] private List<Enemy> enemiyList = new List<Enemy>();
    [SerializeField] private Enemy[] enemyPrefabs = new Enemy[4];
    public Transform[] spawnPoints;
    private int enemyRangeNum = 0;
    private int enemyCount = 0;

    [Header("Set Enemy Stat")]
    public float healthMax = 100f;
    public float healthMin = 10f;
    public float dmgMax = 20f;
    public float dmgMin = 2f;
    public float speedMax = 6f;
    public float speedMin = 2f;

    [SerializeField] private int wave = 0;
    [SerializeField] private float time = 0f;
    public float nextWaveTime = 180f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if (PhotonNetwork.IsMasterClient)
            Initialize(60);
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        wave = 0;
        time = nextWaveTime;

        healthMax = 50f;
        healthMin = 10f;
        dmgMax = 10f;
        dmgMin = 2f;
        speedMax = 6f;
        speedMin = 2f;

        UpdateUI();

        Invoke("Spawn", 10f);
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPlay)
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            if (time > 0f)
            {
                time -= Time.deltaTime;
            }
            else
            {
                Spawn();
                time = nextWaveTime;
            }
        }

        UIManager.Instance.SetTimeTxt(time);
    }

    [PunRPC]
    private void Initialize(int initCount)
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            for (int j = 0; j < initCount / enemyPrefabs.Length; j++)
                enemyQueue.Enqueue(CreateNewObj(i));
        }
    }

    private Enemy CreateNewObj(int type)
    {
        var tempObj = enemyPrefabs[type];
        var newObj = PhotonNetwork.Instantiate(tempObj.name,
            transform.position, Quaternion.identity).GetComponent<Enemy>();
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public static Enemy GetObj(int type)
    {
        if (instance.enemyQueue.Count > 0)
        {
            var obj = Instance.enemyQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = Instance.CreateNewObj(type);
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    public static void ReturnObj(Enemy obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        Instance.enemyQueue.Enqueue(obj);
    }

    private void Spawn()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver
            && GameManager.Instance.IsPlay)
        {
            ++wave;
            time = nextWaveTime;

            int spawnCount = wave + 1;

            UIManager.Instance.SetEnemyTxt(enemyCount);

            for (int i = 0; i < spawnCount; i++)
            {
                var enemyIntensity = Random.Range(0f, 1f);
                CreateEnemy(enemyIntensity);
            }

            UpdateUI();
        }
    }

    private void CheckEnemyList()
    {
        if (enemiyList.Count <= 0)
            Spawn();
    }

    private void UpdateUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            UIManager.Instance.SetWaveTxt(wave);
            UIManager.Instance.SetEnemyTxt(enemiyList.Count);
        }
        else
        {
            UIManager.Instance.SetWaveTxt(wave);
            UIManager.Instance.SetEnemyTxt(enemyCount);
        }
    }

    private void CreateEnemy(float intensity)
    {
        var health = Mathf.Lerp(healthMin, healthMax, intensity);
        var damage = Mathf.Lerp(dmgMin, dmgMax, intensity);
        var speed = Mathf.Lerp(speedMin, speedMax, intensity);

        var spawnPoint = Utility.GetRandPointOnNavMesh(Vector3.zero,
            Random.Range(0f, 45f), NavMesh.AllAreas);

        if (wave % 15 == 0 && enemyRangeNum < enemyPrefabs.Length)
            ++enemyRangeNum;

        Enemy enemy = GetObj(Random.Range(0, enemyRangeNum));
        enemy.transform.position = spawnPoint;

        if (wave % 5 == 0)
        {
            health += wave * 1.2f;
            damage += wave * 1.2f;
            speed += wave * 1.1f;
        }

        enemy.PV.RPC("Setup", RpcTarget.All, health, damage, speed, speed * 0.3f);
        enemiyList.Add(enemy);

        enemy.OnDeath += () => enemiyList.Remove(enemy);
        enemy.OnDeath += () => CheckEnemyList();
        enemy.OnDeath += () => GameManager.Instance.AddKillCount(1);
        enemy.OnDeath += () => StartCoroutine(DestroyObj(enemy, 3f));
        enemy.OnDeath += () => --enemyCount;
    }

    private IEnumerator DestroyObj(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if(PhotonNetwork.IsMasterClient)
            ReturnObj(target);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(time);
            stream.SendNext(wave);
            stream.SendNext(nextWaveTime);
            stream.SendNext(enemiyList.Count);
        }
        else
        {
            time = (float)stream.ReceiveNext();
            wave = (int)stream.ReceiveNext();
            nextWaveTime = (float)stream.ReceiveNext();
            enemyCount = (int)stream.ReceiveNext();
        }
    }
}