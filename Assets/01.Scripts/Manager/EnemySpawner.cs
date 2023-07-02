using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private List<Enemy> enemyList = new List<Enemy>(); // 생성된 적들을 담는 리스트
    public Enemy[] enemyPrefabs = new Enemy[4]; // 생성할 적 AI

    public float damageMax = 40f; // 최대 공격력
    public float damageMin = 10f; // 최소 공격력

    public float healthMax = 200f; // 최대 체력
    public float healthMin = 50f; // 최소 체력

    public float speedMax = 3f; // 최대 속도
    public float speedMin = 1f; // 최소 속도

    private int enemyRangeNum = 0;
    private int enemyCount = 0; // 남은 적의 수
    private int wave; // 현재 웨이브

    private float startTime;

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        // 호스트만 적을 직접 생성할 수 있음
        // 다른 클라이언트들은 호스트가 생성한 적을 동기화를 통해 받아옴
        if (PhotonNetwork.IsMasterClient)
        {
            // 게임 오버 상태일때는 생성하지 않음
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            // 적을 모두 물리친 경우 다음 스폰 실행
            if (enemyList.Count <= 0 && /*GameManager.Instance.IsPlay && */Time.time - startTime >= 5f)
                SpawnWave();
        }

        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 표시
    private void UpdateUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 호스트는 직접 갱신한 적 리스트를 통해 남은 적의 수를 표시함
            UIManager.Instance.SetWaveTxt(wave);
            UIManager.Instance.SetEnemyTxt(enemyList.Count);
        }
        else
        {
            // 클라이언트는 적 리스트를 갱신할 수 없으므로, 호스트가 보내준 enemyCount를 통해 적의 수를 표시함
            UIManager.Instance.SetWaveTxt(wave);
            UIManager.Instance.SetEnemyTxt(enemyCount);
        }
    }

    // 현재 웨이브에 맞춰 적을 생성
    private void SpawnWave()
    {
        // 웨이브 1 증가
        ++wave;

        // 현재 웨이브 * 1.5에 반올림 한 개수 만큼 적을 생성
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);

        // spawnCount 만큼 적을 생성
        for (int i = 0; i < spawnCount; i++)
        {
            // 적의 세기를 0%에서 100% 사이에서 랜덤 결정
            float enemyIntensity = Random.Range(0f, 1f);
            // 적 생성 처리 실행
            CreateEnemy(enemyIntensity);
        }
    }

    // 적을 생성하고 생성한 적에게 추적할 대상을 할당
    private void CreateEnemy(float intensity)
    {
        // intensity를 기반으로 적의 능력치 결정
        float health = Mathf.Lerp(healthMin, healthMax, intensity);
        float damage = Mathf.Lerp(damageMin, damageMax, intensity);
        float speed = Mathf.Lerp(speedMin, speedMax, intensity);

        // 생성할 위치를 랜덤으로 결정
        var spawnPoint = Utility.GetRandPointOnNavMesh(
            transform.position, Random.Range(10f, 30f), NavMesh.AllAreas);

        if (wave % 15 == 0)
            ++enemyRangeNum;

        Debug.Log("몬스터 생성");
        // 적 프리팹으로부터 적을 생성, 네트워크 상의 모든 클라이언트들에게 생성됨
        var enemy = PhotonNetwork.Instantiate(
            enemyPrefabs[Random.Range(0, enemyRangeNum)].gameObject.name,
            spawnPoint,
            Quaternion.identity).GetComponent<Enemy>();

        if (wave % 5 == 0)
        {
            health = enemy.originHealth * wave * 0.5f;
        }

        // 생성한 적의 능력치와 추적 대상 설정
        enemy.photonView.RPC("Setup", RpcTarget.All, health, damage, speed, speed * 0.3f);

        // 생성된 적을 리스트에 추가
        enemyList.Add(enemy);

        // 적의 onDeath 이벤트에 익명 메서드 등록
        // 사망한 적을 리스트에서 제거
        enemy.OnDeath += () => enemyList.Remove(enemy);
        // 적 사망시 점수 상승
        enemy.OnDeath += () => GameManager.Instance.AddKillCount(1);
        // 사망한 적을 10 초 뒤에 파괴
        enemy.OnDeath += () => StartCoroutine(DestroyAfter(enemy.gameObject, 5f));
    }

    // 포톤의 Network.Destroy()는 지연 파괴를 지원하지 않으므로 지연 파괴를 직접 구현함
    IEnumerator DestroyAfter(GameObject target, float delay)
    {
        // delay 만큼 쉬고
        yield return new WaitForSeconds(delay);

        // target이 아직 파괴되지 않았다면
        if (target != null)
        {
            // target을 모든 네트워크 상에서 파괴
            PhotonNetwork.Destroy(target);
        }
    }

    // 주기적으로 자동 실행되는, 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 로컬 오브젝트라면 쓰기 부분이 실행됨
        if (stream.IsWriting)
        {
            // 적의 남은 수를 네트워크를 통해 보내기
            stream.SendNext(enemyList.Count);
            // 현재 웨이브를 네트워크를 통해 보내기
            stream.SendNext(wave);
        }
        else
        {
            // 리모트 오브젝트라면 읽기 부분이 실행됨
            // 적의 남은 수를 네트워크를 통해 받기
            enemyCount = (int)stream.ReceiveNext();
            // 현재 웨이브를 네트워크를 통해 받기 
            wave = (int)stream.ReceiveNext();
        }
    }
}