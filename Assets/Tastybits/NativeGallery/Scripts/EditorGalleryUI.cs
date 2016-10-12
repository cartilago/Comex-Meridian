using UnityEngine;
using System.Collections;


namespace Tastybits.NativeGallery {


	/**
	 * The Editor Gallery UI is used to display a Gallery simulator 
	 * UI in the Unity editor.
	 */
	public class EditorGalleryUI : MonoBehaviour {
		System.Action<Texture2D,ExifOrientation> callback;

		/**
		 * function called when the gallery has been opened.
		 */
		public void OpenGallery( System.Action<Texture2D,ExifOrientation> callback ){
			this.callback = callback;	
			this.transform.SetAsLastSibling ();
			this.gameObject.SetActive(true);
		}
		   
		/**
		 * Callback invoked when the gallery items has been clicked.
		 */
		public void OnGalleryItemClicked( GameObject go ) {
			if( this.callback!=null ) {
				var rawimg = go.transform.FindChild("Image").GetComponent<UnityEngine.UI.RawImage>();
				this.callback( (Texture2D)rawimg.texture, ExifOrientation.ORIENTATION_NORMAL );
				this.gameObject.SetActive(false);
			}
		}

		/**
		 * Update UI.
		 */
		void Update(){
			for (int i = 0; i < this.gameObject.transform.childCount; i++) {
				var ch = this.gameObject.transform.GetChild (i).gameObject;;
				ch.SetActive (false);
				ch.SetActive (true);
			}
		}
	}


}