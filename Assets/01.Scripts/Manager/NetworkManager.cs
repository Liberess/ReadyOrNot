using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Hashtable을 사용하기 위한 코드
using System.Linq;
using Michsky.LSS;

public enum GameTypes
{
    Campaign = 0,
    SoloSurvival,
    MultiSurvival,
    Match
}

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region 싱글톤
    private static GameObject mContainer;
    public static GameObject Container { get => mContainer; }

    private static NetworkManager mInstance;
    public static NetworkManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mContainer = new GameObject();
                mContainer.name = "NetworkManager";
                mInstance = mContainer.AddComponent(typeof(NetworkManager)) as NetworkManager;
                DontDestroyOnLoad(mContainer);
            }

            return mInstance;
        }
    }
    #endregion

    #region 멤버변수
    [Header("[ DisconnectPanel ]")]
    public GameObject DisconnectPanel;
    public InputField NickNameInput;

    [Header("[ LobbyPanel ]")]
    public GameObject LobbyPanel;
    public Button CampaignBtn;
    public Button SoloSurvivalBtn;
    public Button TeamSurvivalBtn;
    public Button MatchBtn;

    [Header("[ RoomPanel ]")]
    public List<PlayerSlot> PlayerList = new List<PlayerSlot>();
    public PlayerSlot MyPlayerSlot;
    public GameObject RoomPanel;
    public GameObject SlotGrid;
    public Text RoomInfoTxt;
    public GameObject ChatPanel;
    public Text[] ChatTxts;
    public InputField ChatInput;
    public Button StartBtn;

    [Header("Minimum number of people to start the game")]
    [SerializeField, Range(1, 7)]
    private int minPlayerCount = 1;

    [Header("ETC")]
    [SerializeField] private string gameVersion = "0.0.3";
    [SerializeField] private LoadingScreenManager lsm;
    public PhotonView PV;
    [SerializeField] private GameTypes playType;
    public GameTypes PlayType { get => playType; }
    public List<PlayerCtrl> players = new List<PlayerCtrl>();
    public List<int> playerGunNum = new List<int>();
    #endregion

    #region 서버연결 (Awake, Start)
    private void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (mInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.SetResolution(1920, 1080, true);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Start()
    {
        SetVariables();

        if (PhotonNetwork.IsConnected)
        {
            DisconnectPanel.SetActive(false);
            LobbyPanel.SetActive(true);
            RoomPanel.SetActive(false);
        }
        else // 로그인 하기 전이면 Dinsconnet Panel 활성화
        {
            PhotonNetwork.GameVersion = gameVersion;

            DisconnectPanel.SetActive(true);
            LobbyPanel.SetActive(false);
            RoomPanel.SetActive(false);

            if (lsm == null)
                lsm = FindObjectOfType<LoadingScreenManager>();
        }
    }

    private void SetVariables()
    {
        // Disconnect Panel Setting
        DisconnectPanel = GameObject.Find("NetworkCanvas").transform.GetChild(0).gameObject;
        NickNameInput = DisconnectPanel.transform.Find("Grid").
            transform.Find("NameInputField").GetComponent<InputField>();

        // Lobby Panel Setting
        LobbyPanel = GameObject.Find("LobbyCanvas").transform.GetChild(0).gameObject;
        CampaignBtn = LobbyPanel.transform.Find("ContentsGrid").transform.
            GetChild(0).GetComponent<Button>();
        SoloSurvivalBtn = LobbyPanel.transform.Find("ContentsGrid").transform.
            Find("SurvivalBtn").GetChild(1).GetComponent<Button>();
        TeamSurvivalBtn = LobbyPanel.transform.Find("ContentsGrid").transform.
            Find("SurvivalBtn").GetChild(2).GetComponent<Button>();
        MatchBtn = LobbyPanel.transform.Find("ContentsGrid").transform.
            GetChild(2).GetComponent<Button>();

        // Room Panel Setting
        RoomPanel = GameObject.Find("RoomCanvas").transform.GetChild(0).gameObject;
        SlotGrid = RoomPanel.transform.Find("SlotGrid").gameObject;
        RoomInfoTxt = RoomPanel.transform.Find("RoomInfoTxt").GetComponent<Text>();
        ChatPanel = RoomPanel.transform.Find("ChatPanel").gameObject;

        GameObject txts = ChatPanel.transform.Find("ChatScrollView").transform.
            Find("Viewport").transform.Find("Content").gameObject;
        for (int i = 0; i < txts.transform.childCount; i++)
            ChatTxts[i] = txts.transform.GetChild(i).GetComponent<Text>();

        ChatInput = ChatPanel.transform.Find("ChatInput").GetComponent<InputField>();
        StartBtn = RoomPanel.transform.Find("StartBtn").GetComponent<Button>();

        PV = GetComponent<PhotonView>();
    }

    public void Connect()
    {
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        DataManager.Instance.userData.userName = PhotonNetwork.LocalPlayer.NickName;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        AudioManager.Instance.PlayBGM("Main");
        DisconnectPanel.SetActive(false);
        RoomPanel.SetActive(false);
        LobbyPanel.SetActive(true);

        MainUIManager.Instance.OnUpdateUI();

        PlayerList.Clear();
    }

    public void Disconnect()
    {
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
        DisconnectPanel.SetActive(true);
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);
    }
    #endregion

    #region 방
    public void CreateRoom()
    {
        CampaignBtn.interactable = false;
        MatchBtn.interactable = false;

        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);

        PhotonNetwork.CreateRoom("Room" + Random.Range(0, 100), new RoomOptions { MaxPlayers = 4 });
    }

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveLobby()
    {
        RoomPanel.SetActive(true);
        LobbyPanel.SetActive(false);
    }

    public void LeaveRoom()
    {
        if (MyPlayerSlot != null)
            PhotonNetwork.Destroy(MyPlayerSlot.PV);

        RoomPanel.SetActive(false);
        LobbyPanel.SetActive(true);

        CampaignBtn.interactable = true;
        SoloSurvivalBtn.interactable = true;
        TeamSurvivalBtn.interactable = true;
        MatchBtn.interactable = true;

        StartBtn.gameObject.SetActive(false);

        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        if (lsm == null)
            lsm = FindObjectOfType<LoadingScreenManager>();

        if (playType == GameTypes.MultiSurvival || playType == GameTypes.Match)
        {
            StartBtn.gameObject.SetActive(false);
            RoomPanel.SetActive(true);
            LobbyPanel.SetActive(false);

            MyPlayerSlot = PhotonNetwork.Instantiate("RoomSlot",
                Vector3.zero, Quaternion.identity).GetComponent<PlayerSlot>();

            MyPlayerSlot.PV.RPC("SetPlayerName", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
            MyPlayerSlot.PV.RPC("SetWeaponModel", RpcTarget.All, DataManager.Instance.userData.equipGunNum);
            MyPlayerSlot.PV.RPC("SetPlayerAnim", RpcTarget.All, DataManager.Instance.userData.equipGunNum);

            UpdatePlayerSlot();

            RoomRenewal();

            ChatInput.text = "";
            for (int i = 0; i < ChatTxts.Length; i++)
                ChatTxts[i].text = "";
        }
        else // Campaign 모드 또는 솔로 모드
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;

            if (playType == GameTypes.Campaign)
            {
                lsm.LoadScene("Campaign");
                FadePanel.Instance.FadeIn();
                //PhotonNetwork.LoadLevel("Campaign");
            }
            else
            {
                lsm.LoadScene("Survival");
                //PhotonNetwork.LoadLevel("Survival");
            }

            LoadingScreen.Instance.virtualLoadingTimer = (Random.Range(1.5f, 5f));
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    private void UpdatePlayerSlot()
    {
        for (int i = 0; i < PlayerList.Count; i++)
        {
            if (PlayerList[i].photonView == null || PlayerList[i].gameObject.activeSelf == false)
            {
                PlayerList.RemoveAt(i);
                continue;
            }

            PlayerList[i].PV.RPC("SetPlayerName", RpcTarget.AllBuffered, PlayerList[i].playerName);
            PlayerList[i].PV.RPC("SetPlayerAnim", RpcTarget.AllBuffered, PlayerList[i].modelNum);
            PlayerList[i].PV.RPC("SetWeaponModel", RpcTarget.AllBuffered, PlayerList[i].modelNum);
        }
    }

    public void SetPlayerTag(string key, object value)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { key, value } });
    }

    public object GetPlayerTag(string key)
    {
        return PhotonNetwork.LocalPlayer.CustomProperties[key];
    }

    private void RoomRenewal()
    {
        if (PhotonNetwork.IsMasterClient)
            UpdatePlayerSlot();

        RoomInfoTxt.text = PhotonNetwork.CurrentRoom.Name + " / " +
            PhotonNetwork.CurrentRoom.PlayerCount + "명 / " +
            "최대 " + PhotonNetwork.CurrentRoom.MaxPlayers + "명";

        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= minPlayerCount)
            {
                Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount + "플레이어가 모여서, 게임을 시작이 가능합니다!");
                StartBtn.gameObject.SetActive(true);
                StartBtn.interactable = true;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            else
            {
                StartBtn.gameObject.SetActive(false);
                StartBtn.interactable = false;
                PhotonNetwork.CurrentRoom.IsVisible = true;
                PhotonNetwork.CurrentRoom.IsOpen = true;
            }
        }
    }

    public void OnClickStartBtn(GameType gameType)
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        switch (gameType.Type)
        {
            case GameTypes.Campaign:
                playType = GameTypes.Campaign;
                PhotonNetwork.CreateRoom("Room" + Random.Range(0, 100), new RoomOptions { MaxPlayers = 1 });
                break;
            case GameTypes.SoloSurvival:
                playType = GameTypes.SoloSurvival;
                PhotonNetwork.CreateRoom("Room" + Random.Range(0, 100), new RoomOptions { MaxPlayers = 1 });
                break;
            case GameTypes.MultiSurvival:
                playType = GameTypes.MultiSurvival;
                JoinRandomRoom();
                break;
            case GameTypes.Match:
                playType = GameTypes.Match;
                JoinRandomRoom();
                break;
            default:
                break;
        }
    }

    public void SoloGameToLobby()
    {
        Destroy(gameObject);
        PhotonNetwork.IsMessageQueueRunning = true;
        PhotonNetwork.LoadLevel("Main");
    }

    public void OnClickGameStart()
    {
        PV.RPC("GameStartRPC", RpcTarget.AllViaServer); //그전에는 All
    }

    [PunRPC]
    public void GameStartRPC()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.AutomaticallySyncScene = true;

        if (playType == GameTypes.MultiSurvival)
            lsm.LoadScene("Survival");
        else
            lsm.LoadScene("Match");

        LoadingScreen.Instance.virtualLoadingTimer = 2f;
    }
    #endregion

    #region 채팅
    public void ChatPanelSet()
    {
        if (ChatPanel.activeSelf)
            ChatPanel.SetActive(false);
        else
            ChatPanel.SetActive(true);
    }

    public void Send()
    {
        if (ChatInput.text != "")
        {
            PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
            ChatInput.text = "";
        }
        else
        {
            ChatInput.text = "";
        }
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    private void ChatRPC(string msg)
    {
        bool isInput = false;

        for (int i = 0; i < ChatTxts.Length; i++)
        {
            if (ChatTxts[i].text == "")
            {
                isInput = true;
                ChatTxts[i].text = msg;
                break;
            }
        }

        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatTxts.Length; i++) ChatTxts[i - 1].text = ChatTxts[i].text;
            ChatTxts[ChatTxts.Length - 1].text = msg;
        }
    }
    #endregion

    public void GameQuit()
    {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }
}