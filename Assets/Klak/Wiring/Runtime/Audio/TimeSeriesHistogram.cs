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
using Klak.Math;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Audio/TimeSeriesHistogram")]
    public class TimeSeriesHistogram : NodeBase
    {
        public float threshold;
        private CircularBuffer<float> impulses;

        public int memory = 16;
        public int harmionics = 4;
        public float scale = 1.1f;
        public float decay = 0.999f;
        public float blur = 0f;

        private int cooldownLatch = 0;

        // 0 to 200 BPM
        private float[] histogram = new float[400];

        [Inlet]
        public float input {
            set
            {
                if (value > threshold && cooldownLatch <=0)
                {
                    impulses.PushFront(Time.time);
                    UpdateHistogram();
                    cooldownLatch = 10;
                }
            }
        }

        [SerializeField, Outlet]
        ArrayEvent _outputEvent = new ArrayEvent();


        private int bucketFromBPM(float bpm)
        {
            return (int) Mathf.Round(bpm * 2);
        }


        private bool isValid(int bucket)
        {
            return bucket > 140 && bucket < 300;
        }

        private void UpdateHistogram()
        {
            if (!impulses.IsFull)
                return;



            var lastImpulse = impulses.Front();

            for(int i = 1; i < impulses.Capacity; i++)
            {
                float secondsInterval = lastImpulse - impulses[i];
                for ( int ii = 0; ii <= harmionics; ii++)
                {
                    float bpm = 60 * secondsInterval;
                    int bucket = bucketFromBPM(bpm);

                    if ( isValid(bucket))
                    {
                        if (histogram[bucket] == 0)
                            histogram[bucket] = 1f;
                        else
                            histogram[bucket]++;
                    }

                    secondsInterval *= 2f;
                }
            }


        }

        private void Start()
        {
            impulses = new CircularBuffer<float>(memory);

        }

        void Update()
        {

            for (int i = 1; i < histogram.Length - 1; i++)
            {
                histogram[i] = 
                    blur * histogram[i - 1] +
                    decay * histogram[i] +
                    blur * histogram[i + 1];
            }

            cooldownLatch--;


            int maxIndex = 0;
            float maxValue = 0;

            for (int i = 0; i < histogram.Length; i++)
            {
                if ( histogram[i] > maxValue)
                {
                    maxIndex = i;
                    maxValue = histogram[i];
                }
            }
            if (maxValue > 1)
            {
                for (int i = 0; i < histogram.Length; i++)
                {
                    histogram[i] /= maxValue;
                }
            }

            Debug.Log((float)maxIndex / 2f);

            _outputEvent.Invoke(histogram);
        }
    }
}
