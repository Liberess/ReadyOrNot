using System;
using UnityEngine;
using Photon.Pun;

public class LivingEntity : MonoBehaviourPun, IDamageable
{
    public int livingID;

    [Header("Health")]
    public float originHealth = 100f;
    [SerializeField] private float health;
    public float Health
    {
        get => health;

        protected set
        {
            if (health + value >= originHealth)
                health = originHealth;
            else if (health - value <= 0)
                health = 0f;
        }
    }
    public bool dead { get; protected set; }

    public event Action OnDeath;

    private const float minTimeBetDamaged = 0.1f; // 공격 허용할 딜레이
    private float lastDamagedTime;

    protected bool IsInvulerable
    {
        get
        {
            if (Time.time >= lastDamagedTime + minTimeBetDamaged)
                return false;

            return true;
        }
    }

    protected virtual void OnEnable()
    {
        dead = false;
        health = originHealth;
    }

    [PunRPC]
    public void SetLivingID(int id)
    {
        if (photonView.IsMine)
            livingID = id;
    }

    [PunRPC]
    public virtual void ApplyDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsInvulerable || dead)
            return;

        lastDamagedTime = Time.time;
        health -= damage;

        /*        if(PhotonNetwork.IsMasterClient)
                {
                    if (IsInvulerable || dead)
                        return;

                    lastDamagedTime = Time.time;
                    health -= damage;

                    // 호스트 -> 클라이언트
                    photonView.RPC("ApplyUpdate", RpcTarget.Others, health, dead);

                    // 다른 클라이언트들도 ApplyDamage 실행
                    photonView.RPC("ApplyDamage", RpcTarget.Others,
                        damage, hitPoint, hitNormal);
                }*/

        if (health <= 0 && !dead)
        {
            health = 0f;
            Die();
        }
    }

    [PunRPC]
    private void ApplyUpdate(float newHealth, bool newDead)
    {
        health = newHealth;
        dead = newDead;
    }

    [PunRPC]
    public virtual void RestoreHealth(float value)
    {
        if (dead)
            return;

        if (health + value >= originHealth)
            health = originHealth;
        else
            health += value;

        /*        if(PhotonNetwork.IsMasterClient)
                {
                    if (health + value >= originHealth)
                        health = originHealth;
                    else
                        health += value;

                    photonView.RPC("ApplyUpdate", RpcTarget.Others, health, dead);
                    photonView.RPC("RestoreHealth", RpcTarget.Others, value);
                }*/
    }

    public virtual void Die()
    {
        if (OnDeath != null)
            OnDeath();

        dead = true;
    }
}