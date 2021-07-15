using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerControl: MonoBehaviour
{
    void FU_Represent()
    {
        handRepresent.Refresh();
        FistRepresent();
        BodyArrowRepresent();
    }

    void FistRepresent()
    {
        Color normal = normalColorRender.color;
        Color pressed = pressedColorRender.color;

        if (rightFistState.IsGrabPressed())
            rightFist.GetComponent<SpriteRenderer>().color = pressed;
        else
            rightFist.GetComponent<SpriteRenderer>().color = normal;
        if (leftFistState.IsGrabPressed())
            leftFist.GetComponent<SpriteRenderer>().color = pressed;
        else
            leftFist.GetComponent<SpriteRenderer>().color = normal;

        //EnvSign
        var lEnvSign = leftFist.transform.Find("EnvSign").gameObject;
        var rEnvSign = rightFist.transform.Find("EnvSign").gameObject;
        Y.DebugPanel.Log("lEnvSign", "Test", $"{lEnvSign}");
        Y.DebugPanel.Log("leftFistState == FistState.GrabEnv", "Test", $"{leftFistState == FistState.GrabEnv}");
        lEnvSign.SetActive(leftFistState == FistState.GrabEnv);
        rEnvSign.SetActive(rightFistState == FistState.GrabEnv);

        //SwordSign
        var lSwordSign = leftFist.transform.Find("SwordSign").gameObject;
        var rSwordSign = rightFist.transform.Find("SwordSign").gameObject;
        bool lGrabedStuffIsSword = false;
        {
            if (leftGrabedStuff != null)
            {
                Stuff.Stuff stuff = leftGrabedStuff.GetComponent<Stuff.Stuff>();
                if (stuff is Stuff.Sword)
                {
                    lGrabedStuffIsSword = true;
                }
            }
        }
        bool rGrabedStuffIsSword = false;
        {
            if (rightGrabedStuff != null)
            {
                Stuff.Stuff stuff = rightGrabedStuff.GetComponent<Stuff.Stuff>();
                if (stuff is Stuff.Sword)
                {
                    rGrabedStuffIsSword = true;
                }
            }
        }
        lSwordSign.SetActive(lGrabedStuffIsSword);
        rSwordSign.SetActive(rGrabedStuffIsSword);
    }

    void BodyArrowRepresent()
    {
        Vector2 defaultArrowDirection = Vector2.up;

        if (wholeState == WholeState.BeforeJump)
        {
            if (wholeVelocityBeforeJump.direction == Vector2.zero)
            {
                float maxSpeed = fistVelocity.left.GetSpeedMaxTwoFist();
                (Vector2 direction, WholeVelocityWhileJumping.StartJump_Direction_Type type) = wholeVelocityWhileJumping.Out_GetDirection(wholeVelocityBeforeJump);
                if (direction != Vector2.zero)
                {
                    float speed = wholeVelocityWhileJumping.Out_GetSpeed_EqualToBeforeJump(wholeVelocityBeforeJump, type);
                    float ratio = speed / maxSpeed;

                    arrowSign.SetActive(true);
                    SetArrowColor(GetGreenYellowRedGraduallyColor(ratio));
                    arrowSign.transform.localScale = new Vector3(ratio, ratio, 1);
                }
                else
                {
                    arrowSign.SetActive(false);
                }
                
            }
            else
            {
                Vector2 arrowDirection = wholeVelocityBeforeJump.direction;
                float angle = Vector2.SignedAngle(defaultArrowDirection, arrowDirection);
                arrowSign.transform.localRotation = Quaternion.Euler(0, 0, angle);

                //根据身体的移动速度，设置Arrow颜色
                {
                    //身体的最大移动速度，等于手的最大移动速度
                    float maxSpeed = fistVelocity.left.GetSpeedMaxTwoFist();
                    float ratio = wholeVelocityBeforeJump.speed / maxSpeed;

                    arrowSign.SetActive(true);
                    SetArrowColor(GetGreenYellowRedGraduallyColor(ratio));
                    arrowSign.transform.localScale = new Vector3(ratio, ratio, 1);
                }
            }
        }
        else
        {
            arrowSign.SetActive(false);
        }
    }

    void SetArrowColor(Color c)
    {
        SpriteRenderer[] arrowSprites = arrowSign.GetComponentsInChildren<SpriteRenderer>();
        foreach (var s in arrowSprites)
        {
            s.color = c;
        }
    }

    Color GetGreenYellowRedGraduallyColor(float x)
    {
        if (x <= 1 / 2f)
        {
            return new Color(g: 1, r: x / (1 / 2f), b: 0, a: 1);
        }
        else //  (1/2 , 1]
        {
            return new Color(g: (1 - x) / (1 / 2f), r: 1, b: 0, a: 1);
        }
    }
}
