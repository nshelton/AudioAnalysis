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
    [AddComponentMenu("Klak/Wiring/Audio/HistoryCollector")]
    public class HistoryCollector : NodeBase
    {
        public int HistoryLength = 128;

        private CircularBuffer<float> _floatHistory;
        private CircularBuffer<float[]> _rawArrayHistory;

        [Inlet]
        public float[] ArrayInput {
            set
            {
              if (_rawArrayHistory == null)
                {
                    _rawArrayHistory = new CircularBuffer<float[]>(HistoryLength);
                }

                _rawArrayHistory.PushFront(value);
            }
        }

        [Inlet]
        public float FloatInput
        {
            set
            {
                if (_floatHistory == null)
                {
                    _floatHistory = new CircularBuffer<float>(HistoryLength);
                }

                _floatHistory.PushFront(value);
            }
        }

        [SerializeField, Outlet]
        ArrayEvent _outputArrayEvent = new ArrayEvent();

        [SerializeField, Outlet]
        Array2DEvent _output2DEvent = new Array2DEvent();

        void Update()
        {
            if (_floatHistory != null && _floatHistory != null)
                _outputArrayEvent.Invoke(_floatHistory.ToArray());

            if (_floatHistory != null && _rawArrayHistory != null)
                _output2DEvent.Invoke(_rawArrayHistory.ToArray());
        }
    }
}
