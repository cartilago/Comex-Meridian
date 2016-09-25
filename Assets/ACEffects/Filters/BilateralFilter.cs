using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("ACEffects/Filters/Median")]
public class BilateralFilter : MonoBehaviour {

	private Material material = null;

	void Start () {
		material = new Material (Shader.Find("BilateralFilter"));
	}
	
	void OnDisable () {
		if (!material) {
			return;
		}
		DestroyImmediate (material);
	}
	
	void OnRenderImage (RenderTexture src, RenderTexture dest) {
		if (!material) {
			return;
		}
		material.SetFloat ("_TexWidth", src.width);
		material.SetFloat ("_TexHeight", src.height);
		Graphics.Blit (src, dest, material);
	}

}
