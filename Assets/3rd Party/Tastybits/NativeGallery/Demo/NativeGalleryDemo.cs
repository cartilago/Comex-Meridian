#pragma warning disable 414
using UnityEngine;
using System.Collections;

namespace Tastybits.NativeGallery {

	public class NativeGalleryDemo : MonoBehaviour {
		string lastFileNamePicked = "";
		Texture2D textureGotten;
		public bool useOnGUI=false;
		public UnityEngine.UI.RawImage preview;

		public bool rotateImportedImage = true;
		public Tastybits.NativeGallery.ImagePickerType iOSImagePickerType = Tastybits.NativeGallery.ImagePickerType.UIImagePickerControllerSourceTypeSavedPhotosAlbum;

		public UnityEngine.UI.Text orientationLabel;


		public void OnOpenGalleryButtonClicked() {
			NativeGalleryController.OpenGallery( ( Texture2D tex, ExifOrientation orientation )=>{
				orientationLabel.text = "Orientation: " + orientation;
				ShowTexture(tex);
			} ); 
		}


		void ShowTexture( Texture2D tex ) {
			if( tex != null ) {
				textureGotten = tex;
				lastFileNamePicked = tex.name;
				Debug.Log("You picked the file : " + textureGotten.name );
				UpdatePreview();
			} else {
				textureGotten = null;
				lastFileNamePicked = "";
				UpdatePreview();
			}
		}


		void UpdatePreview() {
			this.preview.texture = textureGotten;
		}
			

		public GUIStyle buttonstyle;
		void OnGUI() {
			if( !useOnGUI ) {
				return;
			}
			var re = new Rect( Screen.width / 4f, Screen.width / 4f, Screen.width / 2f, Screen.width / 2f );
			bool cl = GUI.Button( re, "Start Gallery\nPick image from Galery" );
			if( cl )  {
				ImagePicker.OpenGallery( ( Texture2D tex, ExifOrientation orientation )=>{
					if( tex != null ) {
						textureGotten = tex;
						lastFileNamePicked = tex.name;
						orientationLabel.text = "Orientation: " + orientation;
						Debug.LogError("you picked the file : " + textureGotten.name );
					} else {
						textureGotten = null;
						lastFileNamePicked = "";
					}
				}, rotateImportedImage,  iOSImagePickerType ); 
			}
				
			if( null!=textureGotten ) {
				float boxH = Screen.height / 2f;
				if( buttonstyle == null ) { 
					buttonstyle = new GUIStyle( GUI.skin.button );
				}
				buttonstyle.normal.background = textureGotten;
				var r = new Rect(0,Screen.height-(boxH), Screen.width, boxH);
				GUI.Box( r, textureGotten );
			}
			
		}
	}

}