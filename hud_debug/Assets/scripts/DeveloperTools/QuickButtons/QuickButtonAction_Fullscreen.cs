using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickButtonAction_Fullscreen : QuickButtonAction
{
    public override void Start()
    {
        base.Start();
        commandName = "fullscreen";
    }

    public override void SetDisplayValue()
    {
        StartCoroutine(displayScreenMode());
    }
    
    IEnumerator displayScreenMode() //wait one frame before retrieving the value since it takes until the end of the frame to effectively change it
    {
        yield return null;
        displayValue.text = Screen.fullScreen ? "Fullscreen" : "Windowed";
    }
}
