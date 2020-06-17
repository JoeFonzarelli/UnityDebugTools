using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickButtonAction_Resolution : QuickButtonAction
{
    public override void Start()
    {
        commandName = "resolution";
        base.Start();
    }

    public override void SetDisplayValue()
    {
        StartCoroutine(displayResolution());
    }

    IEnumerator displayResolution() //wait one frame before retrieving the value since it takes until the end of the frame to effectively change it
    {
        yield return null;
        displayValue.text = UnityEngine.Screen.width + "x" + UnityEngine.Screen.height;
    }
}
