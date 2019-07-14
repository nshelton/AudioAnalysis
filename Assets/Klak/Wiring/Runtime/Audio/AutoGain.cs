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
    [AddComponentMenu("Klak/Wiring/Audio/AutoGain")]
    public class AutoGain : NodeBase
    {

        private float Gain = 1.0f;
        private float Peak = 1.0f;
        private float _rawInput;


        public float Decay = 0.99f;

        [Inlet]
        public float input {
            set { _rawInput = value; }
        }

        [SerializeField, Outlet]
        FloatEvent _outputEvent = new FloatEvent();

        public float NormalizeVal(float rawVal)
        {
            // NGS: prevent numerical instability
            if (rawVal < 0.000001f)
            {
                return 0;
            }

            float decayingPeak = this.Peak * Mathf.Exp(-this.Decay * Time.deltaTime);
            this.Peak = Mathf.Max(decayingPeak, rawVal);

            if (this.Peak > 0.001f)
            {
                this.Gain = 1.0f / this.Peak;
            }

            return this.Gain * rawVal;
        }

        void Update()
        {
            _outputEvent.Invoke(NormalizeVal(_rawInput));
        }
    }
}
