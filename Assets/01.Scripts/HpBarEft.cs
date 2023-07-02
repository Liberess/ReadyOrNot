using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBarEft : MonoBehaviour
{ 
    [SerializeField] private Text hpTxt;
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider backHpBar;

    public float maxHp;
    public float currentHp;

    private bool hit = false;

    public void SetUp(float _maxHp)
    {
        hit = false;
        maxHp = _maxHp;
        currentHp = maxHp;

        hpBar.maxValue = 1f;
        backHpBar.maxValue = 1f;
        hpBar.value = maxHp;
        backHpBar.value = maxHp;
    }

    private void Update()
    {
        hpBar.value = Mathf.Lerp(hpBar.value, currentHp / maxHp, Time.deltaTime * 5f);

        if (hit)
        {
            backHpBar.value = Mathf.Lerp(backHpBar.value, hpBar.value, Time.deltaTime * 10f);

            if (hpBar.value >= backHpBar.value - 0.01f)
            {
                hit = false;
                backHpBar.value = hpBar.value;
            }
        }

        if(hpTxt != null)
            hpTxt.text = Mathf.RoundToInt(currentHp) + "/" + Mathf.RoundToInt(maxHp);
    }

    public IEnumerator Hit()
    {
        yield return new WaitForSeconds(0.5f);
        hit = true;
    }
}