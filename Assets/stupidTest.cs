using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stupidTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    float lastTime;

    void Update()
    {
        var t = Time.time;

        Debug.Log($"&&&&&&&: {t - lastTime}");

        lastTime = t;
    }
}
