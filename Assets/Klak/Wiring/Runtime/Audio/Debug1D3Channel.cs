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

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Audio/Debug1D3Channel")]
    public class Debug1D3Channel : NodeBase
    {
        private Texture2D DebugTexture;

        public string DebugName;

        private int _textureHeight = 1;

        [Inlet]
        public float[] inputR {
            set
            {
                if (_rawBufferR == null || _rawBufferR.Length != value.Length)
                    _rawBufferR = new float[value.Length];

                value.CopyTo(_rawBufferR,0);
            }
        }

        [Inlet]
        public float[] inputG
        {
            set
            {
                if (_rawBufferG == null || _rawBufferG.Length != value.Length)
                    _rawBufferG = new float[value.Length];

                value.CopyTo(_rawBufferG, 0);
            }
        }

        [Inlet]
        public float[] inputB
        {
            set
            {
                if (_rawBufferB == null || _rawBufferB.Length != value.Length)
                    _rawBufferB = new float[value.Length];

                value.CopyTo(_rawBufferB, 0);
            }
        }

        float[] _rawBufferR;
        float[] _rawBufferG;
        float[] _rawBufferB;

        private void UpdateTexture ()
        {
            if (_rawBufferB == null || _rawBufferG == null || _rawBufferR == null)
                return;

            if(DebugTexture == null || DebugTexture.width != _rawBufferR.Length)
            {
                DebugTexture = new Texture2D(_rawBufferR.Length, _textureHeight, TextureFormat.RGBA32, true);
                DebugTexture.wrapMode = TextureWrapMode.Clamp;
                DebugManager.Textures[DebugName] = new DebugManager.DebugContent();
                DebugManager.Textures[DebugName].Texture = DebugTexture;
                DebugManager.Textures[DebugName].RenderType = Debug1D.RenderType.Normal;
            }

            for (int i = 0; i < _rawBufferR.Length; i++)
            {
                Color color = new Color(_rawBufferR[i], _rawBufferG[i], _rawBufferB[i]);
   
                DebugTexture.SetPixel(i, 0, color);
            }
            DebugTexture.Apply();
        }

  

        void Update()
        {
            UpdateTexture();
        }

    }
}
