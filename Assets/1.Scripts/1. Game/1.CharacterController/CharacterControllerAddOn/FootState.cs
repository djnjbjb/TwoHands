using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    实现思路，见notion文档
    https://www.notion.so/FootState-97ff1a3278824cdfb9e64372a299aa06
*/

public enum FootState
{
    Air,
    Surface,
    EnvGround,
    EnvRock
}

public class FootStatePlus
{
    public FootState state { get; private set; }
    public FootState pre { get; private set; }
    private float surfaceUpDowOffset;
    private float surfaceLeftRightInaccuracy;

    public FootStatePlus(FootState footState, float surfaceUpDownOffset, float surfaceLeftRightInaccuracy)
    {
        state = footState;
        pre = footState;
        this.surfaceUpDowOffset = surfaceUpDownOffset;
        this.surfaceLeftRightInaccuracy = surfaceLeftRightInaccuracy;
    }

    public static implicit operator FootState(FootStatePlus footState) => footState.state;

    public void FixedUpdateManually(GameObject bottomLeftPoint, GameObject bottomRightPoint)
    {
        pre = state;

        {
            LayerMask LMRockOrGround = LayerMask.GetMask("EnvRock", "EnvGround");
            int layerMaskGround = LayerMask.GetMask("EnvGround");

            Vector2 pointBLUp = (Vector2)bottomLeftPoint.transform.position + new Vector2(0, surfaceUpDowOffset);
            Vector2 pointBRUp = (Vector2)bottomRightPoint.transform.position + new Vector2(0, surfaceUpDowOffset);
            RaycastHit2D hitUp = Physics2D.Linecast(pointBLUp, pointBRUp, LMRockOrGround);

            Vector2 pointBLDown = (Vector2)bottomLeftPoint.transform.position - new Vector2(0, surfaceUpDowOffset);
            Vector2 pointBRDown = (Vector2)bottomRightPoint.transform.position - new Vector2(0, surfaceUpDowOffset);
            RaycastHit2D hitDown = Physics2D.Linecast(pointBLDown, pointBRDown, LMRockOrGround);

            if (hitUp.collider == null && hitDown.collider == null)
                state = FootState.Air;
            else if (hitUp.collider == null && hitDown.collider != null)
                state = FootState.Surface;
            else
            {
                //hitUp hitDown 都有。但依然有可能为surface。这段代码处理这个情况。
                {
                    bool leftSomeAir = false;
                    float leftSomeAirLength = 0f;
                    {
                        RaycastHit2D hitUpLeftToRight = hitUp;
                        float upLeftX = bottomLeftPoint.transform.position.x;
                        float upLeftHitX = hitUpLeftToRight.point.x;
                        if (Mathf.Abs(upLeftHitX - upLeftX) > surfaceLeftRightInaccuracy)
                        {
                            leftSomeAir = true;
                            leftSomeAirLength = Mathf.Abs(upLeftHitX - upLeftX);
                        }
                    }
                    bool rightSomeAir = false;
                    float rightSomeAirLength = 0f;
                    {
                        RaycastHit2D hitupRightToLeft = Physics2D.Linecast(pointBRUp, pointBLUp, LMRockOrGround);
                        float upRightX = bottomRightPoint.transform.position.x;
                        float upRightHitX = hitupRightToLeft.point.x;
                        if (Mathf.Abs(upRightX - upRightHitX) > surfaceLeftRightInaccuracy)
                        {
                            rightSomeAir = true;
                            rightSomeAirLength = Mathf.Abs(upRightX - upRightHitX);
                        }
                    }

                    if (leftSomeAir)
                    {
                        RaycastHit2D hitDownLeftToRight = hitDown;
                        float downLeftX = bottomLeftPoint.transform.position.x;
                        float downLeftHitX = hitDownLeftToRight.point.x;
                        if (Mathf.Abs(downLeftX - downLeftHitX) < leftSomeAirLength)
                        {
                            state = FootState.Surface;
                            return;
                        }
                    }
                    if (rightSomeAir)
                    {
                        RaycastHit2D hitDownRightToLeft = Physics2D.Linecast(pointBRDown, pointBLDown, LMRockOrGround);
                        float downRightX = bottomRightPoint.transform.position.x;
                        float downRightHitX = hitDownRightToLeft.point.x;
                        if (Mathf.Abs(downRightX - downRightHitX) < rightSomeAirLength)
                        {
                            state = FootState.Surface;
                            return;
                        }
                    }
                }
                
                state = FootState.EnvRock;

                RaycastHit2D hitDownGround = Physics2D.Linecast(pointBLDown, pointBRDown, layerMaskGround);
                RaycastHit2D hitUpGround = Physics2D.Linecast(pointBLUp, pointBRUp, layerMaskGround);
                if (hitDownGround.collider != null || hitUpGround.collider != null)
                    state = FootState.EnvGround;
            }
        }
        
    }
}
