using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Breakable : MonoBehaviourPun
{
    [SerializeField] private GameObject fractured = null;
    [SerializeField, Range(0.1f, 5.0f)] private float breakForce = 1f;
    [SerializeField] private int hp = 3;
    [SerializeField] private GameObject[] items = null;

    public void Break()
    {
        GameObject frac = Instantiate(fractured, transform.position, transform.rotation);

        foreach (Rigidbody rigid in frac.GetComponentsInChildren<Rigidbody>())
        {
            //rigid.gameObject.AddComponent<BreakableObject>();
            Vector3 force = (rigid.transform.position - transform.position).normalized * breakForce;
            rigid.AddForce(force * 2f);
            //StartCoroutine(EnableObject(rigid, 2f));
        }

        if(items.Length > 0)
        {
            Vector3 pos = new Vector3(transform.position.x, 1f, transform.position.z);
            PhotonNetwork.Instantiate(items[Random.Range(0, 2)].name,
                pos, Quaternion.identity);
        }

        //Drop Key
        var dropKey = GetComponent<DropKey>();

        if (dropKey != null)
            dropKey.CreateKey();

        Destroy(frac, 4f);
        gameObject.SetActive(false);
        Destroy(gameObject, 0.5f);
    }

    private IEnumerator EnableObject(Rigidbody target, float delay)
    {
        yield return new WaitForSeconds(delay);

        target.useGravity = false;
        target.GetComponent<MeshCollider>().enabled = false;

        Destroy(target.gameObject, 5f);
    }

    public void Hit(float dmg)
    {
        hp -= Mathf.RoundToInt(dmg);

        if (hp <= 0)
            Break();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            --hp;

            if (hp <= 0)
                Break();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            --hp;

            if (hp <= 0)
                Break();
        }
    }
}

/*class BreakableObject : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (gameObject.layer == 8)
            return;

        if (collision.gameObject.tag == "Ground")
            gameObject.layer = 8;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (gameObject.layer == 8)
            return;

        if (collision.gameObject.tag == "Ground")
            gameObject.layer = 8;
    }
}*/