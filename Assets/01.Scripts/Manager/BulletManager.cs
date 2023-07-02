using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Bullet Object Pool
/// </summary>
public class BulletManager : MonoBehaviourPun
{
    private static BulletManager instance;
    public static BulletManager Instance { get => instance; }
    [SerializeField] private GameObject bulletPrefab;
    Queue<Bullet> bulletQueue = new Queue<Bullet>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if(PhotonNetwork.IsMasterClient)
            Initialize(100);
    }

    [PunRPC]
    private void Initialize(int initCount)
    {
        for (int i = 0; i < initCount; i++)
            bulletQueue.Enqueue(CreateNewBulletObj());
    }

    [PunRPC]
    private Bullet CreateNewBulletObj()
    {
        var newBullet = PhotonNetwork.Instantiate(
            bulletPrefab.name, transform.position, Quaternion.identity).GetComponent<Bullet>();
        newBullet.gameObject.SetActive(false);
        newBullet.transform.SetParent(transform);
        return newBullet;
    }

    [PunRPC]
    public static Bullet GetBulletObj()
    {
        if (instance.bulletQueue.Count > 0)
        {
            var obj = Instance.bulletQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = Instance.CreateNewBulletObj();
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    [PunRPC]
    public static void ReturnBulletObj(Bullet obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        Instance.bulletQueue.Enqueue(obj);
    }
}