//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using NAudio.Dsp;
using System;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Audio/BeatDetect")]
    public class BeatDetect : NodeBase
    {


        [Inlet]
        public float input {
            set
            {
                if (value > 0.6)
                    beatTimes.PushBack(Time.time);
            }
        }

        CircularBuffer<float> beatTimes = new CircularBuffer<float>(16);

        private void Start()
        {
            for(int i = 0; i < beatTimes.Capacity; i++)
            {
                beatTimes.PushBack(0);
            }
        }

        float IntervalToBPM(float seconds)
        {
            return seconds * 60;
        }

        void Update()
        {
            float avgDiff= 0;
            var diffstring = string.Empty;

           for (int i = 0; i < beatTimes.Capacity-1; i ++)
           {
                avgDiff += Mathf.Abs(beatTimes[i] - beatTimes[i + 1]);
                diffstring += Mathf.Abs(beatTimes[i] - beatTimes[i + 1]) + ", ";
            }

            avgDiff /= (beatTimes.Capacity - 1);

            Debug.Log(diffstring);
            Debug.Log(IntervalToBPM(avgDiff));
        }
    }
}
