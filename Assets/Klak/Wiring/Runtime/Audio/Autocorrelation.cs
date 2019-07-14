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
    [AddComponentMenu("Klak/Wiring/Audio/Autocorrelation")]
    public class Autocorrelation : NodeBase
    {

        [Inlet]
        public float[] input {
            set
            {
                value.CopyTo(_rawAudioBuffer, 0);
            }
        }

        public float freq = 0;

        const int AUTOCORR_FFT_SIZE = 1024;

        [SerializeField, Outlet]
        ArrayEvent _outputEvent = new ArrayEvent();

        float[] _rawAudioBuffer;

        float[] _rawBufferReal;
        float[] _rawBufferComplex;

        float[] _fftResult;

        float updateFrequency = 0f;
        public float xOffset = 0f;

        bool isValid (float freq)
        {
            return freq > 60 && freq < 200;
        }

        private float[] CalcSpectrum()
        { 
            _rawAudioBuffer.CopyTo(_rawBufferReal, 0);

            for (int i = 0; i < _rawBufferComplex.Length; i++)
                _rawBufferComplex[i] = 0f;

            var spectrumSizeM = (int)Mathf.Round(Mathf.Log(_rawBufferReal.Length, 2));

            FastFourierTransform.FFT(true, spectrumSizeM, _rawBufferReal, _rawBufferComplex);

            float maxVal = 0;
            int maxIndex = 10;
            float maxfrequency = 10;

            for (int i = 10; i < _fftResult.Length; i ++)
            {
                float frequency = (float)i * updateFrequency / ( AUTOCORR_FFT_SIZE / 2f);

                var magnitude = Mathf.Sqrt(_rawBufferReal[i] * _rawBufferReal[i] + _rawBufferComplex[i] * _rawBufferComplex[i]);
                if (magnitude > maxVal && isValid(frequency))
                {
                    maxVal = magnitude;
                    maxIndex = i;
                    maxfrequency = frequency;
                }
                _fftResult[i] = magnitude;
            }

            for (int i = 0; i < _fftResult.Length; i++)
            {
                _fftResult[i] /= maxVal;
            }
            freq = maxfrequency;

            return _fftResult;
        }

        private void Start()
        {
            _rawAudioBuffer = new float[AUTOCORR_FFT_SIZE];

            _rawBufferReal = new float[AUTOCORR_FFT_SIZE];
            _rawBufferComplex = new float[AUTOCORR_FFT_SIZE];
            _fftResult = new float[AUTOCORR_FFT_SIZE / 2];
        }

        int counter = 0;
        void Update()
        {
            updateFrequency = 60f / Time.fixedDeltaTime;
            _outputEvent.Invoke(CalcSpectrum());
        }

        private void OnGUI()
        {
            GUI.Label(new Rect( xOffset, 0,100, 20), freq.ToString());
        }
    }
}
