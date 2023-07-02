using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamCtrl : MonoBehaviour
{
    [SerializeField, Range(60f, 103f)]
    private float m_viewAngle = 60f;
    public float viewAngle
    {
        get => m_viewAngle;

        set => SetViewAngle(value);
    }

    private CinemachineFreeLook freeCam;

    private void Start()
    {
        freeCam = GetComponent<CinemachineFreeLook>();

        if (freeCam.Follow == null)
            freeCam.Follow = FindObjectOfType<PlayerCtrl>().transform;

        if(freeCam.LookAt == null)
            freeCam.Follow = FindObjectOfType<PlayerCtrl>().transform;
    }

    private void SetViewAngle(float value)
    {
        if (value < 60f)
            m_viewAngle = 60f;
        else if (value > 103f)
            m_viewAngle = 103f;
        else
            m_viewAngle = value;

        freeCam.m_Lens.FieldOfView = m_viewAngle;
    }
}