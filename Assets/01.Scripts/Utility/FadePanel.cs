using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadePanel : MonoBehaviour
{
    private static FadePanel instance;
    public static FadePanel Instance { get => instance; }

    [SerializeField] private Image Panel;
    [SerializeField] private Text txt;

    private float time = 0f;
    [SerializeField, Range(0.0f, 10.0f)] private float fadeTime = 0.2f;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    public void Fade() => StartCoroutine(FadeFlow());

    public void FadeIn() => StartCoroutine(FadeInProcess());

    public void FadeOut() => StartCoroutine(FadeOutProcess());

    private IEnumerator FadeInProcess()
    {
        Panel.gameObject.SetActive(true);

        time = 0f;

        Color alpha = Panel.color;

        while (alpha.a < 1f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(0, 1, time);
            Panel.color = alpha;
            yield return null;
        }
    }

    private IEnumerator FadeOutProcess()
    {
        Panel.gameObject.SetActive(true);
        txt.gameObject.SetActive(false);

        if (CampaignManager.Instance != null)
        {
            if (CampaignManager.Instance.StageType == StageTypes.Ballantines)
            {
                txt.gameObject.SetActive(true);
                txt.GetComponent<Animator>().SetTrigger("doHide");
            }
        }

        time = 0f;

        Color alpha = new Color(0f, 0f, 0f, 1f);

        while (alpha.a > 0f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(1, 0, time);
            Panel.color = alpha;
            yield return null;
        }

        Panel.gameObject.SetActive(false);
        yield return null;
    }

    private IEnumerator FadeFlow()
    {
        Panel.gameObject.SetActive(true);

        time = 0f;

        Color alpha = Panel.color;

        while(alpha.a < 1f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(0, 1, time);
            Panel.color = alpha;
            yield return null;
        }

        time = 0f;

        yield return new WaitForSeconds(1f);

        while (alpha.a > 0f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(1, 0, time);
            Panel.color = alpha;
            yield return null;
        }

        Panel.gameObject.SetActive(false);
        yield return null;
    }
}
