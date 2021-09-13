using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class LayerMaskExtension
{
    public static int GetMaskInTwoHandsWar(params string[] layerNames)
    {
        List<string> layerNamesList = new List<string>(layerNames);
        for(int i = 0; i < layerNamesList.Count; i++)
        {
            string value = layerNamesList[i];
            if (value == "Env")
            {
                layerNamesList.RemoveAt(i);
                layerNamesList.AddRange(new List<string>() {"EnvRock", "EnvGround", "EnvRoundRock"});
            }
        }
        return LayerMask.GetMask(layerNamesList.ToArray());
    }
    
}
