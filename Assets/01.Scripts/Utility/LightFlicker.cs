using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightFlicker : MonoBehaviour
{
    private Light lightLamp;
    [SerializeField, Range(0f, 50f)] private float lightIntensity = 0.7f;

    private float time = 0f;
    [SerializeField, Range(0f, 5f)] private float fadeTime = 0.2f;

    private void Awake()
    {
        lightLamp = GetComponent<Light>();
    }

    private void Start()
    {
        StartCoroutine(FadeFlow());
    }

    IEnumerator FadeFlow()
    {
        time = 0f;

        while (lightLamp.intensity < lightIntensity)
        {
            time += Time.deltaTime / fadeTime;
            lightLamp.intensity = Mathf.Lerp(0, lightIntensity, time);
            yield return null;
        }

        time = 0f;

        yield return new WaitForSeconds(1f);

        while (lightLamp.intensity > 0f)
        {
            time += Time.deltaTime / fadeTime;
            lightLamp.intensity = Mathf.Lerp(lightIntensity, 0, time);
            yield return null;
        }

        StartCoroutine(FadeFlow());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}