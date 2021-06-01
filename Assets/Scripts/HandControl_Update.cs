using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HandControl : MonoBehaviour
{
    [Header("Update Settings")]
    [SerializeField]
    SpriteRenderer normalColorRender;
    [SerializeField]
    SpriteRenderer pressedColorRender;
    /*  
        目前，color和HandControlFade部分有交互
        所以，在运行时会变化，所以，放在Update里，而不是一开始初始化
    */
    Color normal;
    Color pressed;

    void Update()
    {
        HKey.UpdateRefresh();
        UpdateFistRepresent();
    }

    void UpdateFistRepresent()
    {
        //color
        normal = normalColorRender.color;
        pressed = pressedColorRender.color;

        if (rightFistState.IsGrabPressed())
            rightFist.GetComponent<SpriteRenderer>().color = pressed;
        else
            rightFist.GetComponent<SpriteRenderer>().color = normal;
        if (leftFistState.IsGrabPressed())
            leftFist.GetComponent<SpriteRenderer>().color = pressed;
        else
            leftFist.GetComponent<SpriteRenderer>().color = normal;

        //grab sign
        var rGrabSign = rightFist.transform.Find("GrabSign").gameObject;
        var lGrabSign = leftFist.transform.Find("GrabSign").gameObject;
        rGrabSign.SetActive(rightFistState.IsGrabingThings());
        lGrabSign.SetActive(leftFistState.IsGrabingThings());
    }
}