using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AmmoPack : MonoBehaviourPun, IItem
{
    private void Start()
    {
        transform.rotation = Quaternion.Euler(12.113f, 91f, 90f);
    }

    public void Use(GameObject target)
    {
        var playerShooter = target.GetComponent<PlayerShooter>();
        if (playerShooter == null)
            return;

        AudioManager.Instance.PlaySFX("GetItem");

        if (playerShooter != null && playerShooter.gun != null)
            playerShooter.gun.photonView.RPC("AddAmmo",
                RpcTarget.All, playerShooter.gun.GunData.InitAmmoRemain / 2);

        photonView.RPC("DestroyItem", RpcTarget.MasterClient, 0f);
    }

    [PunRPC]
    public void DestroyItem(float delay)
    {
        StartCoroutine(DetroyProcess(delay));
    }

    private IEnumerator DetroyProcess(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(gameObject);
    }
}