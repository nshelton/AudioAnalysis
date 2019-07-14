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
    [AddComponentMenu("Klak/Wiring/Audio/SpectralFlux")]
    public class SpectralFlux : NodeBase
    {

        private float[] lastSpectrum;
        private float flux;

        [Inlet]
        public float[] input {
            set
            {
                if (lastSpectrum == null || lastSpectrum.Length != value.Length)
                {
                    lastSpectrum = new float[value.Length];
                }

                flux = 0;

                for ( int i = 0; i < lastSpectrum.Length; i ++)
                {
                    if (lastSpectrum[i] < value[i])
                    {
                        flux += value[i] - lastSpectrum[i];
                    }
                }

                value.CopyTo(lastSpectrum, 0);
            }
        }

        [SerializeField, Outlet]
        FloatEvent _outputEvent = new FloatEvent();

        void Update()
        {
            _outputEvent.Invoke(flux);
        }
    }
}