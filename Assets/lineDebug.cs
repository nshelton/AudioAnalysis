using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lineDebug : MonoBehaviour
{

    [SerializeField] LineRenderer fftLine;

    public float[] Data
    {
        set
        {
            SetLine(fftLine, value);
        }
    }
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

}
