using System.Collections;
using UnityEngine;
using Photon.Pun;

public class HealthPack : MonoBehaviourPun, IItem
{
    private void Start()
    {
        transform.rotation = Quaternion.Euler(-19.364f, -24.384f, 0.526f);
    }

    public void Use(GameObject target)
    {
        var livingEntity = target.GetComponent<LivingEntity>();
        if (livingEntity == null)
            return;

        AudioManager.Instance.PlaySFX("GetItem");

        var health = livingEntity.originHealth / 2f;

        if (livingEntity != null && livingEntity.dead == false)
            livingEntity.RestoreHealth(health);

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