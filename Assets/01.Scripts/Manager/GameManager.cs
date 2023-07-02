using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Michsky.LSS;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private LoadingScreenManager lsm;
    private DataManager dataManager;

    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyManager enemyManager;

    public List<PlayerCtrl> playerList = new List<PlayerCtrl>();
    public string sceneName { get; private set; }

    public GameObject optionPanel;
    [SerializeField] private int killCount;

    [SerializeField] private bool isGameOver;
    public bool IsGameOver { get => isGameOver; }
    [SerializeField] private bool isPlay;
    public bool IsPlay { get => isPlay; }

    public float time;
    public int wave;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        isPlay = false;
        isGameOver = true;

        sceneName = SceneManager.GetActiveScene().name;

        playerList.Clear();

        if (lsm == null)
            lsm = FindObjectOfType<LoadingScreenManager>();

        dataManager = DataManager.Instance;

        if (photonView.IsMine)
            AudioManager.Instance.InitAudioSetting();

        AudioManager.Instance.PlayBGM("Survival");

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("GameStart", RpcTarget.AllBuffered);

            if (NetworkManager.Instance.PlayType == GameTypes.SoloSurvival)
                enemyManager.gameObject.SetActive(true);
            else
                enemySpawner.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isPlay && SceneManager.GetActiveScene().buildIndex > 1)
            return;

        time += Time.deltaTime;
        UIManager.Instance.SetTimeTxt(time);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionPanel.activeSelf)
            {
                optionPanel.SetActive(false);
                bl_UCrosshair.Instance.OnAim(true);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                optionPanel.SetActive(true);
                bl_UCrosshair.Instance.OnAim(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void CreatePlayer()
    {
        var player = PhotonNetwork.Instantiate("Player", Vector3.zero,
            Quaternion.identity).GetComponent<PlayerCtrl>();
        player.actorID = player.photonView.ViewID;
        player.gameObject.name = PhotonNetwork.LocalPlayer.NickName;

        if (PhotonNetwork.IsMasterClient)
            playerList.Add(player);

        if (SceneManager.GetActiveScene().name != "Campaign")
            player.Respawn();
    }

    [PunRPC]
    public void PlayerRespawnRPC(int id)
    {
        if (photonView.IsMine)
            UIManager.Instance.SetGameOverUI(true);

        StartCoroutine(PlayerRespawnProcess(id, 5f));
    }

    private IEnumerator PlayerRespawnProcess(int id, float delay)
    {
        yield return new WaitForSeconds(delay);

        if(PhotonNetwork.IsMasterClient)
            playerList[id].Respawn();

        UIManager.Instance.SetGameOverUI(false);
    }

    [PunRPC]
    public void GameStart()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            return;

        isPlay = true;
        isGameOver = false;

        UIManager.Instance.SetGameOverUI(false);

        CreatePlayer();
    }

    public void OnClickOptionPanelExit()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        optionPanel.SetActive(false);
        bl_UCrosshair.Instance.OnAim(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnClickLobbyBtn()
    {
        if(photonView.IsMine)
        {
            AudioManager.Instance.PlaySFX("UIClick");

            dataManager.SaveUserData();
            dataManager.SaveGameData();
        }

        Destroy(DataManager.Instance.gameObject);
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

    public void AddCoin(int value)
    {
        if (!isGameOver)
        {
            DataManager.Instance.gameData.coin += value;
        }
    }

    public void AddKillCount(int value)
    {
        if (!isGameOver)
        {
            killCount += value;
            DataManager.Instance.gameData.killCount += value;
            UIManager.Instance.SetKilledTxt(killCount);
        }
    }

    public void GameOver()
    {
        isPlay = false;
        isGameOver = true;
        UIManager.Instance.SetGameOverUI(true);
    }

    private void OnApplicationQuit()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

        }
        else
        {

        }
    }
}