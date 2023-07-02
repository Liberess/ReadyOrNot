using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletEffect : MonoBehaviourPun
{
    [PunRPC]
    public void DestroyProcess(float delay)
    {
        StartCoroutine(DestroyObj(delay));
    }

    private IEnumerator DestroyObj(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
        /*        if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(photonView);
                    //Destroy(gameObject);
                }
                else
                {
        *//*            Debug.Log("클라아니라 걍 삭제");
                    Destroy(gameObject);*//*
                }*/
    }
}