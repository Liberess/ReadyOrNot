using Photon.Pun;
using UnityEngine;
using Cinemachine;

public class TPSCamera : MonoBehaviourPun
{
    private void Awake()
    {
        if (!photonView.IsMine)
            return;

        CinemachineFreeLook freeLookCam =
GameObject.Find("TPS Cam").GetComponent<CinemachineFreeLook>();

        freeLookCam.Follow = transform;
        freeLookCam.LookAt = transform;
    }

    private void Start()
    {

    }
}