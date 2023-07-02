using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    private float shakeAmount = 0f;
    private float shakeTime = 0f;

    private bool isShake = false;

    private void Update()
    {
        if (isShake)
        {
            if (shakeTime > 0)
            {
                shakeTime -= Time.deltaTime;
                transform.position = (Random.insideUnitSphere * shakeAmount) + transform.position;
            }
            else
            {
                isShake = false;
                shakeTime = 0f;
            }
        }
    }

    public void VibrateForTime(float time, float power)
    {
        isShake = true;
        shakeTime = time;
        shakeAmount = power;
    }
}