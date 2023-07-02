using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerSlot : MonoBehaviourPun
{
    private PhotonView pv;
    public PhotonView PV { get => pv; }

    public string playerName;
    public int modelNum;
    public int index;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] weaponModels = new GameObject[9];

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        NetworkManager.Instance.PlayerList.Add(this);

        transform.SetParent(NetworkManager.Instance.SlotGrid.transform);
        transform.localPosition = new Vector3(0, -0, -50);
        transform.localScale = new Vector3(1, 1, 1);
    }

    private void OnDestroy()
    {
        //NetworkManager.Instance.PlayerList.Remove(this);
    }

    [PunRPC]
    public void SetPlayerName(string value)
    {
        name = value;
        playerName = value;
        transform.Find("NameTxt").GetComponent<Text>().text = value;
        NetworkManager.Instance.SetPlayerTag("Name", value);
    }

    [PunRPC]
    public void SetPlayerAnim(int value)
    {
        player.GetComponent<Animator>().runtimeAnimatorController =
    DataManager.Instance.userData.animCtrls[value];

        NetworkManager.Instance.SetPlayerTag("Anim", value);
        modelNum = value;
    }

    [PunRPC]
    public void SetWeaponModel(int value)
    {
        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (value == i)
            {
                weaponModels[i].gameObject.SetActive(true);
                SetPlayerAnim(i);
            }
            else
            {
                weaponModels[i].gameObject.SetActive(false);
            }
        }

        NetworkManager.Instance.SetPlayerTag("Gun", value);
        modelNum = value;
    }
}