using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public const float screenRatio = 16f / 9f;
    const float cameraZ = -10f;

    [NonSerialized] public Camera sceneCamera;
    HandControlTool.Tool.AABB cameraRegion;
    [SerializeField] GameObject player;
    LevelManager levelManager;
    public static CameraFollow instance;
    
    void Start()
    {
        instance = this;
        levelManager = LevelManager.onlyInstance;

        //camera
        sceneCamera = gameObject.GetComponent<Camera>();


        //cameraRegion
        cameraRegion = new HandControlTool.Tool.AABB();
        float camera_half_height = sceneCamera.orthographicSize;
        float camera_half_width = camera_half_height * screenRatio;

        cameraRegion.left = levelManager.region.left + camera_half_width;
        cameraRegion.right = levelManager.region.right - camera_half_width;
        cameraRegion.bottom = levelManager.region.bottom + camera_half_height;
        cameraRegion.top = levelManager.region.top - camera_half_height;

        if (Ludo.Utility.FloatEqual_WithIn0p001(cameraRegion.left, cameraRegion.right))
        {
            cameraRegion.right = cameraRegion.left;
        }
        else if (cameraRegion.right < cameraRegion.left)
        {
            throw new System.Exception("Camer region invalid");
        }


        if (Ludo.Utility.FloatEqual_WithIn0p001(cameraRegion.bottom, cameraRegion.top))
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
}
