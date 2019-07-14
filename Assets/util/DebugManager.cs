using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{

    public class DebugContent
    {
        public Texture2D Texture;
        public Klak.Wiring.Debug1D.RenderType RenderType;
    }

    public static Dictionary<string, DebugContent> Textures = new Dictionary<string, DebugContent>();

    public Material blitMaterial;

    private void OnGUI()
    {
        int yOffset = 30;
        foreach (var tex in Textures)
        {
            blitMaterial.SetFloat("_RenderType", (int)tex.Value.RenderType);
            Graphics.DrawTexture(new Rect(0, yOffset, tex.Value.Texture.width, 100), tex.Value.Texture, blitMaterial);
             yOffset += 100;

            GUI.contentColor = Color.green;
            GUI.Label(new Rect(0, yOffset - 20f, tex.Value.Texture.width, 20f), tex.Key);
            GUI.contentColor = Color.white;
        }
    }
}
