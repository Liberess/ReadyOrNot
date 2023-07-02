using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Transform target;
    private bool isOpen = false;
    private Animator anim;

    private float distance;

    private void Awake() => anim = GetComponent<Animator>();

    private void Start() => isOpen = false;

    public void SetTarget(Transform _target) => target = _target;

    private void Update()
    {
        if (isOpen)
            return;

        if (CampaignManager.Instance.KeyDataArray[(int)StageTypes.Halloween].keyNum < 2)
            return;

        distance = Vector3.Distance(transform.position, target.position);
        if (distance <= 10f)
        {
            isOpen = true;
            GetComponent<Animator>().SetTrigger("doOpen");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CampaignManager.Instance.KeyDataArray[(int)StageTypes.Halloween].keyNum < 2)
            return;

        if(other.CompareTag("Player"))
        {
            anim.SetTrigger("doClose");
            CampaignManager.Instance.ChangeLevel(StageTypes.Boss);
            anim.enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
}