using UnityEngine;
using Ludo.Extensions;

namespace Ludo.TwoHandsWar
{
    public class THWRepresent
    {
        //Setting
        string normalColorStr = "E77500";
        string pressedColorStr = "F5F542";

        //Field
        THWController controller;
        GameObject represent;
        GameObject rFist;
        GameObject lFist;
        GameObject arrow;

        //Fast Access
        Color normalColor;
        Color pressedColor;
        SpriteRenderer rRender;
        SpriteRenderer lRender;
        GameObject lEnvSign;
        GameObject rEnvSign;
        GameObject lSwordSign;
        GameObject rSwordSign;
        SpriteRenderer[] arrowRenderers;

        public THWRepresent(THWController controller, GameObject represent)
        {
            this.controller = controller;
            this.represent = represent;
            rFist = represent.transform.Find("RHFist").gameObject;
            lFist = represent.transform.Find("LHFist").gameObject;
            arrow = represent.transform.Find("Body").LudoFind("ArrowSign", includeInactive: true).gameObject;

            ColorUtility.TryParseHtmlString(normalColorStr, out normalColor);
            ColorUtility.TryParseHtmlString(pressedColorStr, out pressedColor);
            rRender = rFist.GetComponent<SpriteRenderer>();
            lRender = lFist.GetComponent<SpriteRenderer>();
            lEnvSign = lFist.transform.Find("EnvSign").gameObject;
            rEnvSign = rFist.transform.Find("EnvSign").gameObject;
            lSwordSign = lFist.transform.Find("SwordSign").gameObject;
            rSwordSign = rFist.transform.Find("SwordSign").gameObject;
            arrowRenderers = arrow.GetComponentsInChildren<SpriteRenderer>();
        }

        public void FixedUpdate()
        {
            FistRepresent();
            BodyArrowRepresent();
        }

        void FistRepresent()
        {
            //Fist Color
            if (controller.rightFistState.IsGrabPressed())
                rRender.color = pressedColor;
            else
                rRender.color = normalColor;
            if (controller.leftFistState.IsGrabPressed())
                lRender.color = pressedColor;
            else
                lRender.color = normalColor;

            //EnvSign
            lEnvSign.SetActive(controller.leftFistState == FistState.GrabEnv);
            rEnvSign.SetActive(controller.rightFistState == FistState.GrabEnv);

            //SwordSign
            bool lGrabedStuffIsSword = false;
            {
                if (controller.leftGrabedStuff != null)
                {
                    Stuff.Stuff stuff = controller.leftGrabedStuff.GetComponent<Stuff.Stuff>();
                    if (stuff is Stuff.Sword)
                    {
                        lGrabedStuffIsSword = true;
                    }
                }
            }
            bool rGrabedStuffIsSword = false;
            {
                if (controller.rightGrabedStuff != null)
                {
                    Stuff.Stuff stuff = controller.rightGrabedStuff.GetComponent<Stuff.Stuff>();
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

            if (controller.wholeState == WholeState.BeforeJump)
            {
                if (controller.wholeVelocityBeforeJump.direction == Vector2.zero)
                {
                    float maxSpeed = controller.fistVelocity.left.GetSpeedMaxTwoFist();
                    (Vector2 direction, WholeVelocityWhileJumping.StartJump_Direction_Type type) = controller.wholeVelocityWhileJumping.Out_GetDirection(controller.wholeVelocityBeforeJump);
                    if (direction != Vector2.zero)
                    {
                        float speed = controller.wholeVelocityWhileJumping.Out_GetSpeed_EqualToBeforeJump(controller.wholeVelocityBeforeJump, type);
                        float ratio = speed / maxSpeed;

                        arrow.SetActive(true);
                        SetArrowColor(GetColor(ratio));
                        arrow.transform.localScale = new Vector3(ratio, ratio, 1);
                    }
                    else
                    {
                        arrow.SetActive(false);
                    }

                }
                else
                {
                    Vector2 arrowDirection = controller.wholeVelocityBeforeJump.direction;
                    float angle = Vector2.SignedAngle(defaultArrowDirection, arrowDirection);
                    arrow.transform.localRotation = Quaternion.Euler(0, 0, angle);

                    //根据身体的移动速度，设置Arrow颜色
                    {
                        //身体的最大移动速度，等于手的最大移动速度
                        float maxSpeed = controller.fistVelocity.left.GetSpeedMaxTwoFist();
                        float ratio = controller.wholeVelocityBeforeJump.speed / maxSpeed;

                        arrow.SetActive(true);
                        SetArrowColor(GetColor(ratio));
                        arrow.transform.localScale = new Vector3(ratio, ratio, 1);
                    }
                }
            }
            else
            {
                arrow.SetActive(false);
            }
        }

        void SetArrowColor(Color c)
        {
            foreach (var s in arrowRenderers)
            {
                s.color = c;
            }
        }

        Color GetColor(float ratio)
        {
            if (ratio <= 1 / 2f)
            {
                return new Color(
                    g: 1, 
                    r: ratio / (1 / 2f),
                    b: 0, 
                    a: 1);
            }
            else
            {
                return new Color(
                    g: (1 - ratio) / (1 / 2f),
                    r: 1,
                    b: 0,
                    a: 1);
            }
        }
    }
}
