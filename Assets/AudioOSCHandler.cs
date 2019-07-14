using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class AudioOSCHandler : MonoBehaviour
{
    public class TimeSample<T>
    {
        public float Time;
        public T Sample;
    }

    private const string ATTACK_KEY = "/audio/loud";

    public float[] AttackData
    {
        get
        {
            if (m_incomingData.ContainsKey(ATTACK_KEY))
            {
                return m_incomingData[ATTACK_KEY].ToArray();
            }
            return new float[m_historyLength];
        }
    }

    private Dictionary<string, CircularBuffer<float>> m_incomingData = new Dictionary<string, CircularBuffer<float>>();

    private int m_historyLength = 60 * 10;

    private OSCReciever reciever;
    public int port = 8338;

    void Start()
    {
        reciever = new OSCReciever();
        reciever.Open(port);
    }
    bool gotAttack;
    void Update()
    {
        gotAttack = false;
        while (reciever.hasWaitingMessages())
        {
            HandleMessage(reciever.getNextMessage());
        }

       // if(!gotAttack)
       // {
       //     PushValue(ATTACK_KEY, 0f);
       // }
    }

    private void HandleMessage(OSCMessage data)
    {
        var key = data.Address;
        float value = (float)data.Data[0];


        if (key == ATTACK_KEY)
        {
            gotAttack = true;
        }
        PushValue(key, value);
    }

    private void PushValue(string key, float val)
    {
        if (!m_incomingData.ContainsKey(key))
        {
            m_incomingData[key] = new CircularBuffer<float>(m_historyLength);
        }
        m_incomingData[key].PushFront(val);
    }

}