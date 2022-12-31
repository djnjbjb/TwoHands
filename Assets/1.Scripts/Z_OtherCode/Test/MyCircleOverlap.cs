using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCircleOverlap : MonoBehaviour
{
    [SerializeField] bool overlapped;
    [SerializeField] string[] overlappedNames;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, transform.lossyScale.x/2);
        string[] overlappedNames1 = new string[colliders.Length];
        int i = 0;
        foreach (var collider in colliders)
        {
            overlappedNames1[i] = collider.gameObject.name;
            i++;
        }
        overlappedNames = overlappedNames1;
    }
}
