using UnityEngine;
using Ludo.Extensions;

public class TestLayerMaskExtension : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUI.Button(new Rect(50,50,100,100), "TestLayerMaskExtention"))
        {
            print("Env:" + LayerMaskExtension.GetMaskInTwoHandsWar("Env"));
            print("Env,Stuff,Player:" + LayerMaskExtension.GetMaskInTwoHandsWar("Env", "Stuff", "Player"));
        }
    }
}
