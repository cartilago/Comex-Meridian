using UnityEngine;
using System.Collections;

static public class BilateralFilterTexture 
{
	static public Color32[] Process(Texture2D texture)
	{
		Material material = new Material (Shader.Find("BilateralFilter"));

		RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height);
		Graphics.Blit(texture, renderTexture, material);

		Texture2D processedTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
		processedTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		processedTexture.Apply(); 

		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(renderTexture);

		Color32[] processedPixels = processedTexture.GetPixels32();

		Object.DestroyImmediate(processedTexture);

		return processedPixels;
	}
}
