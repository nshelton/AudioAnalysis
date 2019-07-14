using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{

    

    [SerializeField] private AudioOSCHandler m_source;
    [SerializeField] private LineRenderer m_attackLine;


    private void SetLine(LineRenderer line, float[] data)
    {
        if (line.positionCount != data.Length)
        {
            line.positionCount = data.Length;
        }

        for (int i = 0; i < data.Length; i++)
        {
            line.SetPosition(i,
                new Vector3((float)i / data.Length, data[i], 0f));
        }
    }


    void Update()
    {
        SetLine(m_attackLine, m_source.AttackData);
    }
}
