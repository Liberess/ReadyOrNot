using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum EffectType
{
    Common = 0,
    Flesh,
    Shell
}

[System.Serializable]
public class Effect
{
    public string name;
    public EffectType type;
    public ParticleSystem effectPrefab;
}

public class EffectManager : MonoBehaviourPun
{
    private static EffectManager m_Instance;
    public static EffectManager Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = FindObjectOfType<EffectManager>();
            return m_Instance;
        }
    }

    [SerializeField] private List<Effect> particleList = new List<Effect>();

    public void PlayHitEffect(Vector3 pos, Vector3 normal, Transform parent = null, EffectType effectType = EffectType.Common)
    {
        var effect = Instantiate(particleList[(int)effectType].effectPrefab, pos, Quaternion.LookRotation(normal));

        if (parent != null)
            effect.transform.SetParent(parent);

        effect.Play();

        //photonView.RPC("DestroyEffect", RpcTarget.MasterClient, 2f);
        Destroy(effect, 2f);
    }

    [PunRPC]
    private void DestroyEffect(float delay)
    {
        StartCoroutine(DestroyEffectProcess(delay));
    }

    private IEnumerator DestroyEffectProcess(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);

        /*        if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.Destroy(photonView);*/
    }
}