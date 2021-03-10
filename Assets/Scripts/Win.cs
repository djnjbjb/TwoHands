using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour
{
    public GameObject playerBody;
    public GameObject playerRHFist;
    public GameObject playerLHFist;
    public GameObject win;
    GameObject winBody;
    GameObject winRHFist;
    GameObject winLHFist;
    public GameObject bodySign;
    public GameObject rhFistSign;
    public GameObject lhFistSign;

    public float threshold = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        winBody = win.transform.Find("Body").gameObject;
        winRHFist = win.transform.Find("RFist").gameObject;
        winLHFist = win.transform.Find("LFist").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        bool _bodySign = Vector3.Distance(playerBody.transform.position, winBody.transform.position) <= threshold;
        bool _rhSign = Vector3.Distance(playerRHFist.transform.position, winRHFist.transform.position) <= threshold;
        bool _lhSign = Vector3.Distance(playerLHFist.transform.position, winLHFist.transform.position) <= threshold;

        bodySign.SetActive(_bodySign);
        rhFistSign.SetActive(_rhSign);
        lhFistSign.SetActive(_lhSign);
    }
}
