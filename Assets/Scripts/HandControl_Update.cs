using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HandControl : MonoBehaviour
{
    void Update()
    {
        HKey.UpdateRefresh();
        UpdateFistRepresent();
    }

    void UpdateFistRepresent()
    {
        //color
        Color normal = GameObject.Find("FistNormalColor").GetComponent<SpriteRenderer>().color;
        Color pressed = GameObject.Find("FistPressedColor").GetComponent<SpriteRenderer>().color;

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