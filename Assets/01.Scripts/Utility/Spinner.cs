using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Spinner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusTxt;
    private float delayTime = 5f;

    private void OnEnable()
    {
        delayTime = 5f;
    }

    private void Update()
    {
        delayTime -= Time.deltaTime;
        int value = Mathf.RoundToInt(delayTime);
        if (value <= 0.0f)
            statusTxt.text = "0";
        else
            statusTxt.text = value.ToString();
    }
}