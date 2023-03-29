using System;
using Ludo.TwoHandsWar;
using UnityEngine;
using NaughtyAttributes;
using Ludo.Types;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow onlyInstance = null;

    public const float screenRatio = 16f / 9f;
    const float cameraZ = -10f;

    [NonSerialized] public Camera sceneCamera;
    GameObject player;
    LevelManager levelManager;
    AABB cameraRegion; //相机可以活动的范围

    void Start()
    {
        onlyInstance = this;
        player = THWController.singleton.gameObject;
        levelManager = LevelManager.onlyInstance;

        //camera
        sceneCamera = gameObject.GetComponent<Camera>();

        //cameraRegion
        cameraRegion = new AABB();
        float camera_half_height = sceneCamera.orthographicSize;
        float camera_half_width = camera_half_height * screenRatio;

        cameraRegion.left = levelManager.region.left + camera_half_width;
        cameraRegion.right = levelManager.region.right - camera_half_width;
        cameraRegion.bottom = levelManager.region.bottom + camera_half_height;
        cameraRegion.top = levelManager.region.top - camera_half_height;

        if (Ludo.Utility.Algebra.FloatEqual_WithIn0p001(cameraRegion.left, cameraRegion.right))
        {
            cameraRegion.right = cameraRegion.left;
        }
        else if (cameraRegion.right < cameraRegion.left)
        {
            throw new System.Exception("Camera region invalid");
        }


        if (Ludo.Utility.Algebra.FloatEqual_WithIn0p001(cameraRegion.bottom, cameraRegion.top))
        {
            cameraRegion.top = cameraRegion.bottom;
        }
        else if (cameraRegion.top < cameraRegion.bottom)
        {
            throw new System.Exception("Camer region invalid");
        }
    }
    void Update()
    {
        float x = player.transform.position.x;
        float y = player.transform.position.y;

        x = Mathf.Clamp(x, cameraRegion.left, cameraRegion.right);
        y = Mathf.Clamp(y, cameraRegion.bottom, cameraRegion.top);

        gameObject.transform.position = new Vector3(x, y, cameraZ);
    }

    #region External
    public Bounds viewBounds 
    { 
        get
        {
            float h = sceneCamera.orthographicSize;
            float w = h * screenRatio;
            return new Bounds(new Vector3(transform.position.x, transform.position.y, 0), new Vector3(w, h, 0));
        }
    }

    public Bounds bufferedViewBounds
    {
        get
        {
            const float bufferRatio = 1.1f;

            Bounds b0 = viewBounds;
            return new Bounds(b0.center, b0.size * bufferRatio);
        }
    }
    #endregion

    #region Editor
    [ShowNativeProperty]
    public Bounds View_Bounds
    {
        get
        {
            if (Application.isPlaying)
            {
                return viewBounds;
            }
            else
            {
                return new Bounds(new Vector3(0,0,0), new Vector3(0,0,0));
            }
        }
    }

    [ShowNativeProperty]
    public Bounds Buffered_View_Bounds
    {
        get
        {
            if (Application.isPlaying)
            {
                return bufferedViewBounds;
            }
            else
            {
                return new Bounds(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            }
        }
    }
    #endregion
}
