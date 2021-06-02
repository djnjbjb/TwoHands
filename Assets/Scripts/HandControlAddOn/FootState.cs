using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float surfaceTolerance;

    public FootStatePlus(FootState footState, float surfaceToleranceIn)
    {
        state = footState;
        pre = footState;
        surfaceTolerance = surfaceToleranceIn;
    }

    public static implicit operator FootState(FootStatePlus footState) => footState.state;

    public void FixedUpdateManually(GameObject bottomLeftPoint, GameObject bottomRightPoint)
    {
        pre = state;

        LayerMask LMRockOrGround = LayerMask.GetMask("EnvRock", "EnvGround");

        Vector2 pointBL = bottomLeftPoint.transform.position;
        Vector2 pointBR = bottomRightPoint.transform.position;
        RaycastHit2D hit = Physics2D.Linecast(pointBL, pointBR, LMRockOrGround);

        Vector2 pointBLUp = (Vector2)bottomLeftPoint.transform.position + new Vector2(0, surfaceTolerance);
        Vector2 pointBRUp = (Vector2)bottomRightPoint.transform.position + new Vector2(0, surfaceTolerance);
        RaycastHit2D hitUp = Physics2D.Linecast(pointBLUp, pointBRUp, LMRockOrGround);

        Vector2 pointBLDown = (Vector2)bottomLeftPoint.transform.position - new Vector2(0, surfaceTolerance);
        Vector2 pointBRDown = (Vector2)bottomRightPoint.transform.position - new Vector2(0, surfaceTolerance);
        RaycastHit2D hitDown = Physics2D.Linecast(pointBLDown, pointBRDown, LMRockOrGround);

        if (!hitUp.collider && !hitDown.collider)
            state = FootState.Air;
        else if (!hitUp.collider && hitDown.collider)
            state = FootState.Surface;
        else
        {
            state = FootState.EnvRock;

            int layerMaskGround = LayerMask.GetMask("EnvGround");
            RaycastHit2D hitDownGround = Physics2D.Linecast(pointBLDown, pointBRDown, layerMaskGround);
            RaycastHit2D hitUpGround = Physics2D.Linecast(pointBLUp, pointBRUp, layerMaskGround);
            if (hitDownGround.collider)
                state = FootState.EnvGround;
        }
    }
}
