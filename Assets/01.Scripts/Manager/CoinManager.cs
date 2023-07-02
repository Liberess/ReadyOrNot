using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Coin Object Pool
/// </summary>
public class CoinManager : MonoBehaviourPun
{
    private static CoinManager instance;
    public static CoinManager Instance { get => instance; }

    [SerializeField] private Coin[] coinPrefabs = new Coin[3];

    private List<Queue<Coin>> coinQueueList = new List<Queue<Coin>>();

    private Queue<Coin> redCoinQueue = new Queue<Coin>();
    private Queue<Coin> blueCoinQueue = new Queue<Coin>();
    private Queue<Coin> goldCoinQueue = new Queue<Coin>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if (PhotonNetwork.IsMasterClient)
            Initialize(20);
    }

    private void Initialize(int initCount)
    {
        for (int i = 0; i < initCount; i++)
            goldCoinQueue.Enqueue(CreateNewCoinObj(CoinTypes.Gold));

        for (int i = 0; i < initCount; i++)
            blueCoinQueue.Enqueue(CreateNewCoinObj(CoinTypes.Blue));

        for (int i = 0; i < initCount; i++)
            redCoinQueue.Enqueue(CreateNewCoinObj(CoinTypes.Red));

        coinQueueList.Add(goldCoinQueue);
        coinQueueList.Add(blueCoinQueue);
        coinQueueList.Add(redCoinQueue);
    }

    [PunRPC]
    private Coin CreateNewCoinObj(CoinTypes type)
    {
        var tempObj = coinPrefabs[(int)type];
        var newCoin = PhotonNetwork.Instantiate(tempObj.name,
            transform.position, Quaternion.identity).GetComponent<Coin>();
        newCoin.gameObject.SetActive(false);
        newCoin.transform.SetParent(transform);
        return newCoin;
    }

    [PunRPC]
    public static Coin GetCoinObj(CoinTypes type)
    {
        if (instance.goldCoinQueue.Count > 0)
        {
            var obj = Instance.goldCoinQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = Instance.CreateNewCoinObj(type);
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    [PunRPC]
    public static void ReturnCoinObj(Coin obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        //Instance.coinQueue.Enqueue(obj);
    }
}