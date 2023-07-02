using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.LSS;
using Photon.Pun;
using UnityEngine.SceneManagement;

public enum StageTypes
{
    Ballantines = 0,
    Halloween,
    Boss
}

public class CampaignManager : MonoBehaviourPun
{
    #region Singleton
    private static GameObject m_Container;
    private static GameObject Container { get; }

    private static CampaignManager m_Instance;
    public static CampaignManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Container = new GameObject();
                m_Container.name = "CampaignManager";
                m_Instance = m_Container.AddComponent(typeof(CampaignManager)) as CampaignManager;
            }

            return m_Instance;
        }
    }
    #endregion

    #region Public Member Variables
    public StageTypes StageType { get; private set; }
    public GameObject optionPanel;
    public GameObject minimap;
    public bool IsGameOver { get; private set; }
    public bool IsPlay { get; private set; }

    public float playTime { get; private set; }
    public int killCount { get; private set; }
    public float acquireExp { get; private set; }
    public int acquireCoin { get; private set; }
    #endregion

    #region Private Member Variables
    // Basic Scene Setting
    private DataManager dataManager;
    private CampaignUIManager uiMgr;
    [SerializeField] private LoadingScreenManager lsm;

    [Header("Level Environment Setting")]
    [SerializeField] private Material[] skyBoxs = null;
    [SerializeField] private Light levelLight = null;
    [SerializeField] private Color[] levelLightColor = null;

    [Header("Level Play Setting")]
    [SerializeField] private Transform[] playerPosArray = new Transform[2];
    [SerializeField] private GameObject[] ballPortal = null;
    [SerializeField] private GameObject hallDoor = null;
    [SerializeField] private KeyData[] keyDataArray = new KeyData[2];
    public KeyData[] KeyDataArray { get => keyDataArray; }
    [SerializeField] private GameObject bossMob = null;
    [SerializeField] private List<Enemy> ballEnemyList = new List<Enemy>();
    [SerializeField] private List<Enemy> hallEnemyList = new List<Enemy>();
    [SerializeField] private List<Enemy> bossEnemyList = new List<Enemy>();

    private PlayerCtrl player;
    #endregion

    private void Awake()
    {
        if (m_Instance == null)
            m_Instance = this;
        else
            Destroy(gameObject);

        if (SceneManager.GetActiveScene().name != "Campaign")
            Destroy(gameObject);
    }

    private void Start()
    {
        if (lsm == null)
            lsm = FindObjectOfType<LoadingScreenManager>();

        playTime = 0f;
        killCount = 0;
        acquireExp = 0f;
        acquireCoin = 0;

        dataManager = DataManager.Instance;
        uiMgr = CampaignUIManager.Instance;
        AudioManager.Instance.InitAudioSetting();

        ballPortal[0].gameObject.SetActive(true);
        ballPortal[1].gameObject.SetActive(false);

        uiMgr.SetKilledTxt(0);
        CreatePlayer();
        ChangeLevel(StageTypes.Ballantines);
        GameStart();
    }

    private void Update()
    {
        if (IsGameOver)
            return;

        OptionMenuCtrl();
        MinimapCtrl();
        DeveloperFunc();

        if (StageType != StageTypes.Boss)
            playTime += Time.deltaTime;
        else
            playTime -= Time.deltaTime;

        if (playTime <= 0f)
        {
            playTime = 0f;
            FindObjectOfType<PlayerHealth>().Die();
        }

        uiMgr.SetTimeTxt(playTime);
    }

    private void DeveloperFunc()
    {
        // 플레이어 체력 회복
        if (Input.GetKeyDown(KeyCode.Alpha0))
            player.GetComponent<PlayerHealth>().DevRestoreHealthUp();

        // 플레이어 스테미나 회복력 +5
        if (Input.GetKeyDown(KeyCode.Alpha9))
            player.GetComponent<PlayerStamina>().DevStaminaUp();

        // 플레이어 총알 +50
        if (Input.GetKeyDown(KeyCode.Alpha8))
            player.GetComponent<PlayerShooter>().DevAmmoUp();

        // 플레이어 총알 데미지 +10
        if (Input.GetKeyDown(KeyCode.Alpha7))
            player.GetComponent<PlayerShooter>().DevDamageUp();
    }

    #region MinimapCtrl & OptionMenuCtrl
    private void MinimapCtrl()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (minimap.activeSelf)
                minimap.SetActive(false);
            else
                minimap.SetActive(true);
        }
    }

    private void OptionMenuCtrl()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            optionPanel.GetComponentInParent<OptionCtrl>().SetOptionPanelActive();
        }
    }
    #endregion

    #region Level Settings
    public void ChangeLevel(StageTypes type)
    {
        StageType = type;

        if (type == StageTypes.Ballantines)
            AudioManager.Instance.PlayBGM("Ballantines");
        else if (type == StageTypes.Halloween)
            AudioManager.Instance.PlayBGM("Halloween");

        uiMgr.SetActiveFindImg(false);

        if (StageType == StageTypes.Boss)
        {
            DataManager.Instance.gameData.playTime += playTime;
            playTime = 300f;

            SetLevelMonsters();
            uiMgr.SetLevelTxt(StageType);
            uiMgr.SetEnemyTxt(bossEnemyList.Count);
            uiMgr.SetBossUI();
            uiMgr.SetTimeTxt(playTime);
            return;
        }

        FadePanel.Instance.FadeOut();

        RenderSettings.skybox = skyBoxs[(int)StageType];
        levelLight.color = levelLightColor[(int)StageType];

        player.PlayerSpawn(playerPosArray[(int)StageType]);

        SetLevelMonsters();
        uiMgr.SetLevelTxt(StageType);

        if (StageType == StageTypes.Ballantines)
            uiMgr.SetEnemyTxt(ballEnemyList.Count);
        else
            uiMgr.SetEnemyTxt(hallEnemyList.Count);

        uiMgr.SetKeyImg(keyDataArray[(int)StageType]);
    }

    public void SetLevelMonsters()
    {
        switch (StageType)
        {
            case StageTypes.Ballantines:
                foreach (var enemy in ballEnemyList)
                {
                    enemy.gameObject.SetActive(true);

                    // Set Enemy Health, Damage, Tracking Speed, Patrol Speed
                    enemy.Setup(Random.Range(10f, 30f), Random.Range(2f, 6f),
                        Random.Range(4f, 8f), Random.Range(2f, 4f));

                    enemy.OnDeath += () => ballEnemyList.Remove(enemy);
                    enemy.OnDeath += () => AddKillCount(1);
                    enemy.OnDeath += () => acquireExp += Random.Range(1f, 10f);
                    enemy.OnDeath += () => uiMgr.SetEnemyTxt(ballEnemyList.Count);
                    enemy.OnDeath += () => Destroy(enemy.gameObject, 3f);
                }
                break;

            case StageTypes.Halloween:
                foreach (var enemy in ballEnemyList)
                    enemy.gameObject.SetActive(false);

                foreach (var enemy in hallEnemyList)
                {
                    enemy.gameObject.SetActive(true);

                    // Set Enemy Health, Damage, Tracking Speed, Patrol Speed
                    enemy.Setup(Random.Range(20f, 50f), Random.Range(4f, 10f),
                        Random.Range(5f, 9f), Random.Range(3f, 5f));

                    enemy.OnDeath += () => hallEnemyList.Remove(enemy);
                    enemy.OnDeath += () => AddKillCount(1);
                    enemy.OnDeath += () => acquireExp += Random.Range(10f, 20f);
                    enemy.OnDeath += () => uiMgr.SetEnemyTxt(hallEnemyList.Count);
                    enemy.OnDeath += () => Destroy(enemy.gameObject, 3f);
                }
                break;

            case StageTypes.Boss:
                bossMob.SetActive(true);

                foreach (var enemy in hallEnemyList)
                    enemy.gameObject.SetActive(false);

                foreach (var enemy in bossEnemyList)
                {
                    enemy.gameObject.SetActive(true);

                    // Set Enemy Health, Damage, Tracking Speed, Patrol Speed
                    enemy.Setup(Random.Range(40f, 100f), Random.Range(10f, 20f),
                        Random.Range(8f, 10f), Random.Range(5f, 8f));

                    enemy.OnDeath += () => bossEnemyList.Remove(enemy);
                    enemy.OnDeath += () => AddKillCount(1);
                    enemy.OnDeath += () => acquireExp += Random.Range(20f, 40f);
                    enemy.OnDeath += () => uiMgr.SetEnemyTxt(bossEnemyList.Count);
                    enemy.OnDeath += () => Destroy(enemy.gameObject, 3f);
                }
                break;
        }
    }
    #endregion

    #region Game Controls (Start, Over, Clear)
    public void GameStart()
    {
        IsPlay = true;
        IsGameOver = false;
    }

    public void GameOver()
    {
        IsPlay = false;
        IsGameOver = true;
        CampaignUIManager.Instance.SetGameOverUI(true);
    }

    public void GameClear()
    {
        IsPlay = false;
        IsGameOver = true;

        dataManager.gameData.exp += acquireExp;
        dataManager.SetExp();
        dataManager.gameData.coin += acquireCoin;
        dataManager.gameData.killCount += killCount;

        dataManager.SaveUserData();
        dataManager.SaveGameData();

        player.GameClear();

        CampaignUIManager.Instance.SetGameClearUI(true);
    }
    #endregion

    public void CreatePlayer()
    {
        player = PhotonNetwork.Instantiate("Player", playerPosArray[(int)StageType].position,
            Quaternion.identity).GetComponent<PlayerCtrl>();
        player.actorID = player.photonView.ViewID;
        player.gameObject.name = PhotonNetwork.LocalPlayer.NickName;

        hallDoor.GetComponent<Door>().SetTarget(player.transform);
    }

    #region UI Events
    public void OnClickLobbyBtn()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        Destroy(dataManager.gameObject);
        Destroy(NetworkManager.Instance.gameObject);

        lsm.LoadScene("Main");
        LoadingScreen.Instance.virtualLoadingTimer =
            (Random.Range(1.5f, 5f));
    }

    public void OnClickQuitBtn()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        PhotonNetwork.Disconnect();
        Application.Quit();
    }
    #endregion

    public void AddExp(float value)
    {
        acquireExp += value;
    }

    public void AddCoin(int value)
    {
        acquireCoin += value;
    }

    public void AddKillCount(int value)
    {
        /*        if (IsGameOver)
                    return;
        */
        killCount += value;
        uiMgr.SetKilledTxt(killCount);
    }

    public void AddKey(StageTypes type)
    {
        if (keyDataArray[(int)type].keyNum < 2)
            ++keyDataArray[(int)type].keyNum;

        uiMgr.SetKeyImg(keyDataArray[(int)type]);

        if (type == StageTypes.Ballantines)
        {
            if (keyDataArray[(int)type].keyNum >= 2)
            {
                ballPortal[0].gameObject.SetActive(false);
                ballPortal[1].gameObject.SetActive(true);
            }
        }
    }
}