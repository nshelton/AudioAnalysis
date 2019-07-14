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
    [AddComponentMenu("Klak/Wiring/Audio/Spectrum")]
    public class Spectrum : NodeBase
    {

        public float DB_SCALE_FACTOR = 20f;
        public float m_frequencyResacle = 1f;

        [Inlet]
        public float[] input {
            set
            {
                value.CopyTo(_rawAudioBuffer, 0);
            }
        }

        [SerializeField, Outlet]
        ArrayEvent _outputEvent = new ArrayEvent();

        float[] _rawAudioBuffer;

        float[] _rawBufferReal;
        float[] _rawBufferComplex;

        float[] _fftResult;

        float windowFreq = 44100f / AudioAnalysisSettings.BufferSize;
        float maxFreq = 44100f / 2f;

        private float[] CalcSpectrum()
        {
            _rawAudioBuffer.CopyTo(_rawBufferReal, 0);

            for (int i = 0; i < _rawBufferComplex.Length; i++)
                _rawBufferComplex[i] = 0f;

            var spectrumSizeM = (int)Mathf.Round(Mathf.Log(_rawBufferReal.Length, 2));

            FastFourierTransform.FFT(true, spectrumSizeM, _rawBufferReal, _rawBufferComplex);

            for (int i = 0; i < _fftResult.Length; i ++)
            {
                float frequency = ((float)i * windowFreq) / maxFreq;

                var magnitude = Mathf.Sqrt(_rawBufferReal[i] * _rawBufferReal[i] + _rawBufferComplex[i] * _rawBufferComplex[i]); 
                _fftResult[i] = Mathf.Log10(magnitude + 1) * DB_SCALE_FACTOR;
                _fftResult[i] *= Mathf.Lerp(1, frequency, m_frequencyResacle);
            }

            return _fftResult;
        }

        private void Start()
        {
            _rawAudioBuffer = new float[AudioAnalysisSettings.BufferSize];

            _rawBufferReal = new float[AudioAnalysisSettings.BufferSize];
            _rawBufferComplex = new float[AudioAnalysisSettings.BufferSize];
            _fftResult = new float[AudioAnalysisSettings.BufferSize / 2];
        }

        void Update()
        {
            _outputEvent.Invoke(CalcSpectrum());
        }
    }
}
