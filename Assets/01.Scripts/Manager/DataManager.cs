using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DataManager : MonoBehaviourPun
{
    private string GameDataFileName = "/GameData.json";
    private string UserDataFileName = "/UserData.json";

    #region Singleton
    private static GameObject mContainer;
    private static GameObject Container { get; }

    private static DataManager mInstance;
    public static DataManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mContainer = new GameObject();
                mContainer.name = "DataManager";
                mInstance = mContainer.AddComponent(typeof(DataManager)) as DataManager;
            }

            return mInstance;
        }
    }

    [SerializeField] private GameData mGameData;
    public GameData gameData
    {
        get
        {
            if (mGameData == null)
            {
                LoadGameData();
                SaveGameData();
            }

            return mGameData;
        }
    }

    [SerializeField] private UserData mUserData;
    public UserData userData
    {
        get
        {
            if (mUserData == null)
            {
                LoadUserData();
                SaveUserData();
            }

            return mUserData;
        }
    }
    #endregion

    private PhotonView pv;
    public PhotonView PV { get => pv; }

    private void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (mInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        LoadGameData();
        LoadUserData();

        if (pv == null)
            pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        SaveGameData();
        SaveUserData();
    }

    private void InitGameData()
    {
        mGameData.sfx = 50f;
        mGameData.bgm = 50f;

        mGameData.coin = 0;
        mGameData.exp = 0.0f;
        mGameData.level = 1;

        mGameData.killCount = 0;
        mGameData.deathCount = 0;
    }

    private void InitUserData()
    {
        mUserData.userName = "Unknown";
        mUserData.mouseSensitivity = 10f;
        mUserData.crosshairNum = 0;

        for (int i = 0; i < mUserData.gunLevels.Length; i++)
            mUserData.gunLevels[i] = 1;

        mUserData.haveGuns[(int)GunTypes.Pistol] = true;
        for (int i = 1; i < mUserData.haveGuns.Length; i++)
            mUserData.haveGuns[i] = false;

        for (int i = 0; i < mUserData.statUpLevels.Length; i++)
            mUserData.statUpLevels[i] = 1;

        for (int i = 0; i < mUserData.gunDatas.Length; i++)
        {
            mUserData.gunDatas[i] =
                Resources.Load("ScriptableObject/" + Gun.gunNames[i]) as GunData;
        }

        mUserData.equipGunNum = 0; // Pistol
        mUserData.equipGunData = mUserData.gunDatas[0];

        for (int i = 0; i < mUserData.animCtrls.Length; i++)
        {
            mUserData.animCtrls[i] =
                Resources.Load("Player_" + Gun.gunNames[i]) as RuntimeAnimatorController;
        }
    }

    public void LoadGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        if (File.Exists(filePath))
        {
            string code = File.ReadAllText(filePath);
            byte[] bytes = System.Convert.FromBase64String(code);
            string FromJsonData = System.Text.Encoding.UTF8.GetString(bytes);
            mGameData = JsonUtility.FromJson<GameData>(FromJsonData);

            if (mGameData.level == 0)
                InitGameData();
        }
        else
        {
            mGameData = new GameData();
            File.Create(Application.persistentDataPath + GameDataFileName);

            InitGameData();
        }
    }

    public void LoadUserData()
    {
        string filePath = Application.persistentDataPath + UserDataFileName;

        if (File.Exists(filePath))
        {
            string code = File.ReadAllText(filePath);
            byte[] bytes = System.Convert.FromBase64String(code);
            string FromJsonData = System.Text.Encoding.UTF8.GetString(bytes);
            mUserData = JsonUtility.FromJson<UserData>(FromJsonData);

            if (mUserData.equipGunData == null)
                InitUserData();

            mUserData.equipGunData = mUserData.gunDatas[mUserData.equipGunNum];
        }
        else
        {
            mUserData = new UserData();
            File.Create(Application.persistentDataPath + UserDataFileName);

            InitUserData();
        }
    }

    public void SaveGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        string ToJsonData = JsonUtility.ToJson(mGameData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJsonData);
        string code = System.Convert.ToBase64String(bytes);
        File.WriteAllText(filePath, code);
    }

    public void SaveUserData()
    {
        string filePath = Application.persistentDataPath + UserDataFileName;

        string ToJsonData = JsonUtility.ToJson(mUserData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJsonData);
        string code = System.Convert.ToBase64String(bytes);
        File.WriteAllText(filePath, code);
    }

    public void SetExp()
    {
        while (true)
        {
            var value = gameData.level * 100; // 필요 경험치

            if (value <= gameData.exp)
            {
                gameData.exp -= value;
                ++gameData.level;
                continue;
            }

            break;
        }
    }

    private void OnApplicationPause(bool pause)
    {
        SaveGameData();
        SaveUserData();
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
        SaveUserData();
    }

    private void OnDestroy()
    {
/*        if (pv.IsMine)
        {
            SaveGameData();
            SaveUserData();
        }*/
    }
}