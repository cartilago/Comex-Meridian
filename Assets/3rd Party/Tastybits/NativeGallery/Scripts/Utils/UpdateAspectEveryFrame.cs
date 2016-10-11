using UnityEngine;
using System.Collections;

public class UpdateAspectEveryFrame : MonoBehaviour {

	// Use this for initialization
	void Start () {
		UpdateAspect ();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateAspect ();
	}


	void LateUpdate() {
		UpdateAspect ();
	}


	void UpdateAspect() {
		var arf = this.GetComponent<UnityEngine.UI.AspectRatioFitter> ();
		if (arf == null)
			return;
		UnityEngine.UI.Image img = this.GetComponent<UnityEngine.UI.Image> ();
		float aspect = 1f;
		if (img == null) {
			UnityEngine.UI.RawImage rawimg = this.GetComponent<UnityEngine.UI.RawImage> ();
			if (rawimg.texture == null)
				return;
			aspect = (float)rawimg.texture.width / (float)rawimg.texture.height;
		} else {
			if (img.sprite == null)
				return;
			aspect = (float)img.sprite.texture.width / (float)img.sprite.texture.height;
		}
		arf.aspectRatio = aspect;
	}
}
