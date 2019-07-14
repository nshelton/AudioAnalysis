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
    [AddComponentMenu("Klak/Wiring/Audio/Debug1D")]
    public class Debug1D : NodeBase
    {
        public RenderType renderType;

        private Texture2D DebugTexture;

        public string DebugName;

        private int _textureHeight = 1;

        [Inlet]
        public float[] input {
            set
            {
                if (_rawBuffer == null || _rawBuffer.Length != value.Length)
                    _rawBuffer = new float[value.Length];

                value.CopyTo(_rawBuffer,0);
            }
        }

        float[] _rawBuffer;

        private void UpdateTexture ()
        {
            if (_rawBuffer == null)
                return;

            if(DebugTexture == null || DebugTexture.width != _rawBuffer.Length)
            {
                DebugTexture = new Texture2D(_rawBuffer.Length, _textureHeight, TextureFormat.RGBA32, true);
                DebugTexture.wrapMode = TextureWrapMode.Clamp;
                DebugManager.Textures[DebugName] = new DebugManager.DebugContent();
                DebugManager.Textures[DebugName].Texture = DebugTexture;
                DebugManager.Textures[DebugName].RenderType = renderType;
            }

            for (int i = 0; i < _rawBuffer.Length; i++)
            {
                Color color = Color.white * _rawBuffer[i];
                if (renderType == RenderType.Wave)
                {
                    color = Color.white * (_rawBuffer[i] * 0.5f + 0.5f);
                }
                DebugTexture.SetPixel(i, 0, color);
            }
            DebugTexture.Apply();
        }

  

        void Update()
        {
            UpdateTexture();
        }

        public enum RenderType
        {
            Normal = 0,
            Wave = 1,
            Spectrum = 2
        }
    }
}
