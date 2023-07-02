using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCam : MonoBehaviour
{
    private Transform target;
    [SerializeField] private Vector3 offset;

    public void SetCam()
    {
        target = FindObjectOfType<PlayerCtrl>().transform;
        offset = transform.position - target.transform.position;
    }

    private void LateUpdate()
    {
        if(target != null)
            transform.position = target.transform.position + offset;
    }
}