using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalBtn : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void OnEnterBtn()
    {
        anim.SetTrigger("doEnter");
    }

    public void OnExitBtn()
    {
        anim.SetTrigger("doExit");
    }

    public void OnClickBtn()
    {

    }
}