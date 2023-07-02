using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Michsky.LSS;

public enum TeamTypes
{
    Red = 0,
    Blue
}

public class PVPManager : MonoBehaviourPun, IPunObservable
{
    public static PVPManager Instance { get; private set; }
    private DataManager dataManager;
    [SerializeField] private LoadingScreenManager lsm;

    public List<PlayerCtrl> playerList = new List<PlayerCtrl>();

    public GameObject optionPanel;
    public GameObject resultPanel;

    public int maxKill { get; private set; }
    [SerializeField] private float countTime;
    [SerializeField] private int redKillCount;
    [SerializeField] private int blueKillCount;
    [SerializeField] private bool isGameOver;
    public bool IsGameOver { get => isGameOver; }
    public bool IsPlay { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        IsPlay = false;
        isGameOver = true;

        maxKill = 20;
        countTime = 600f;

        playerList.Clear();

        dataManager = DataManager.Instance;

        if (lsm == null)
            lsm = FindObjectOfType<LoadingScreenManager>();

        redKillCount = 0;
        blueKillCount = 0;

        if (photonView.IsMine)
            AudioManager.Instance.InitAudioSetting();

        AudioManager.Instance.PlayBGM("Survival");

        /*        if (PhotonNetwork.IsMasterClient)
                    photonView.RPC("GameStart", RpcTarget.AllViaServer);*/

        GameStart();

        PVPUIManager.Instance.SetKillUI(0, 0);
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (!IsPlay && SceneManager.GetActiveScene().buildIndex > 1)
            return;

        countTime -= Time.deltaTime;
        PVPUIManager.Instance.SetTimeTxt(countTime);

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

        PVPUIManager.Instance.SetKillUI(redKillCount, blueKillCount);

        if(countTime <= 0f || (redKillCount >= 20 || blueKillCount >= 20))
        {
            photonView.RPC("EndGame", RpcTarget.AllViaServer);
        }
    }

    public void CreatePlayer()
    {
        Debug.Log("CreatePlayer");
        var player = PhotonNetwork.Instantiate("PvpPlayer", Vector3.zero,
            Quaternion.identity).GetComponent<PlayerCtrl>();
        player.photonView.RPC("Respawn", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void PlayerRespawnRPC(int id)
    {
        PVPUIManager.Instance.SetKillUI(redKillCount, blueKillCount);
        StartCoroutine(PlayerRespawnProcess(id, 5f));
    }

    private IEnumerator PlayerRespawnProcess(int id, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.IsMasterClient)
        {
            var players = FindObjectsOfType<PlayerCtrl>();
            foreach (var player in players)
            {
                if (player.photonView.ViewID == id)
                    player.photonView.RPC("Respawn", RpcTarget.AllViaServer);
            }
        }

        PVPUIManager.Instance.SetGameOverUI(false);
    }

    [PunRPC]
    public void AddKillCount(int type, int value)
    {
        if (!photonView.IsMine)
            return;

        if (!isGameOver)
        {
            if (type == 0)
                redKillCount += value;
            else
                blueKillCount += value;

            DataManager.Instance.gameData.killCount += value;
        }
    }

    [PunRPC]
    public void GameStart()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            return;

        IsPlay = true;
        isGameOver = false;

        resultPanel.SetActive(false);
        PVPUIManager.Instance.SetGameOverUI(false);

        CreatePlayer();
    }

    public void OnClickOptionPanelExit()
    {
        AudioManager.Instance.PlaySFX("UIClick");

        if (!photonView.IsMine)
            return;

        optionPanel.SetActive(false);
        bl_UCrosshair.Instance.OnAim(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnClickLobbyBtn()
    {
        if (photonView.IsMine)
        {
            AudioManager.Instance.PlaySFX("UIClick");

            foreach (var myPlayer in playerList)
            {
                if (myPlayer.photonView.IsMine)
                    PhotonNetwork.Destroy(myPlayer.photonView);
            }

            dataManager.SaveUserData();
            dataManager.SaveGameData();

            Destroy(DataManager.Instance.gameObject);
            Destroy(NetworkManager.Instance.gameObject);

            lsm.LoadScene("Main");
            LoadingScreen.Instance.virtualLoadingTimer =
                (Random.Range(1.5f, 5f));
        }
    }

    public void OnClickQuitBtn()
    {
        if (photonView.IsMine)
        {
            AudioManager.Instance.PlaySFX("UIClick");

            foreach (var myPlayer in playerList)
            {
                if (myPlayer.photonView.IsMine)
                    PhotonNetwork.Destroy(myPlayer.photonView);
            }
        }

        PhotonNetwork.Disconnect();
        Application.Quit();
    }

    [PunRPC]
    public void EndGame()
    {
        IsPlay = false;
        isGameOver = true;

        resultPanel.SetActive(true);

        Text txt = resultPanel.transform.Find("ResultTxt").GetComponent<Text>();

        if(redKillCount > blueKillCount)
        {
            txt.text = "RED WIN";
            txt.color = Color.red;
        }
        else
        {
            txt.text = "BLUE WIN";
            txt.color = Color.blue;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
            stream.SendNext(maxKill);
            stream.SendNext(redKillCount);
            stream.SendNext(blueKillCount);
            stream.SendNext(countTime);
        }
        else
        {
            maxKill = (int)stream.ReceiveNext();
            redKillCount = (int)stream.ReceiveNext();
            blueKillCount = (int)stream.ReceiveNext();
            countTime = (float)stream.ReceiveNext();
            PVPUIManager.Instance.SetTimeTxt(countTime);
            PVPUIManager.Instance.SetKillUI(redKillCount, blueKillCount);
        }
    }
}