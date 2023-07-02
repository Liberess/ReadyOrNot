using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum CoinTypes
{
    Gold = 0,
    Blue,
    Red
}

public class Coin : MonoBehaviourPun, IItem
{
    [SerializeField] private GameObject[] players = null;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float minDistance = 5f;
    private float distance;
    [SerializeField] private int coin;
    [SerializeField] private GameObject coinTxtPrefab;

    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        moveSpeed = 10f;

        players = GameObject.FindGameObjectsWithTag("Player");
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        for(int i = 0; i < players.Length; i++)
        {
            distance = Vector3.Distance(transform.position, players[i].transform.position);
            if (distance <= minDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    players[i].transform.position + (Vector3.up * 1.5f), moveSpeed * Time.deltaTime);
            }
        }
    }

    [PunRPC]
    public void Burst()
    {
        Vector3 vec = new Vector3(Random.Range(-180f, 180f),
            Random.Range(-180f, 180f), Random.Range(-180f, 180f));

        rigid.AddForce(vec * 0.5f, ForceMode.Acceleration);
    }

    [PunRPC]
    public void Use(GameObject target)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.AddCoin(coin);
        else if(CampaignManager.Instance != null)
            CampaignManager.Instance.AddCoin(coin);

        AudioManager.Instance.PlaySFX("GetCoin");

        var coinTxt = Instantiate(coinTxtPrefab,
            transform.position, Quaternion.identity).GetComponent<CoinTxt>();
        coinTxt.coin = coin;
        StartCoroutine(DestroyCoin(0f));
    }

    private IEnumerator DestroyCoin(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(photonView);
        //CoinManager.ReturnCoinObj(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if(other.CompareTag("Ground"))
        {
            rigid.useGravity = false;
            rigid.velocity = Vector3.zero;
        }
    }
}