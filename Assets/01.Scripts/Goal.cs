using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<Animator>().SetTrigger("doActive");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CampaignManager.Instance.ChangeLevel(StageTypes.Halloween);
            Destroy(gameObject, 3f);
        }
    }
}