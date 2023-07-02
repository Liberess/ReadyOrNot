using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public Image aimPointImg;
    public Image hitPointImg;

    public float smoothTime = 0.2f;

    private Camera screenCam;
    private RectTransform crosshairRectTr;

    private Vector2 currentHitPointVelocity;
    private Vector2 targetPos;

    private void Awake()
    {
        screenCam = Camera.main;
        crosshairRectTr = hitPointImg.GetComponent<RectTransform>();
    }

    public void SetActiveCrosshair(bool active)
    {
        aimPointImg.enabled = active;
        hitPointImg.enabled = active;
    }

    public void SetPos(Vector3 worldPos)
    {
        targetPos = screenCam.WorldToScreenPoint(worldPos);
    }

    private void Update()
    {
        if (!hitPointImg.enabled)
            return;

        crosshairRectTr.position = Vector2.SmoothDamp(crosshairRectTr.position, targetPos, ref currentHitPointVelocity, smoothTime);
    }
}