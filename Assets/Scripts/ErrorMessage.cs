using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour
{
    private Animator errorAnimator;
    private Text text;

    private void Start()
    {
        text = GetComponent<Text>();
        errorAnimator = GetComponent<Animator>();
    }

    public void Error(string info)
    {
        text.text = info;
        errorAnimator.SetTrigger("ShowUp");
    }
}