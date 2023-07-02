using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyData
{
    public string keyName;
    public int keyNum;
}

public class Key : MonoBehaviour
{
    private StageTypes myType;
    private Rigidbody rigid;

    private void Awake() => rigid = GetComponent<Rigidbody>();

    private void Start()
    {
        myType = CampaignManager.Instance.StageType;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        transform.position = new Vector3(transform.position.x,
            transform.position.y + 1f, transform.position.z);

        Burst();
    }

    public void Burst()
    {
        Vector3 vec = new Vector3(Random.Range(-180f, 180f),
            Random.Range(-180f, 180f), Random.Range(-180f, 180f));

        rigid.AddForce(vec, ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            rigid.useGravity = false;
            rigid.velocity = Vector3.zero;
        }

        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySFX("GetItem");
            CampaignManager.Instance.AddKey(myType);
            Destroy(gameObject);
        }
    }
}