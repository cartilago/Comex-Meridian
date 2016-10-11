#pragma warning disable 414
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Linq;

namespace Tastybits.NativeGallery {

	// This is an interface to use the androd gallery and the image picker to pick an image in unity.
	// You can still use the classes UIImagePicker directly for a more specific iOS related
	// set of functionality.
	public class ImagePicker {
		static System.Action<Texture2D,ExifOrientation> _callback;
		public static void OpenGallery( System.Action<Texture2D,ExifOrientation> callback, bool rotateImportedImage, Tastybits.NativeGallery.ImagePickerType iOSImagePickerType ) {
			if (NativeGalleryController.Verbose)
				Debug.Log ("Opening Image Picker");
			_callback = callback;

#if UNITY_IPHONE && !UNITY_EDITOR
			if (NativeGalleryController.Verbose)
				Debug.Log ("Opening Image Picker - iOS");
			UIImagePicker.OpenPhotoAlbum( ( Texture2D texture, bool ok, string errMsg )=>{
				_callback(texture, UIImagePicker.GetLastImportedOrientation() );
			}, rotateImportedImage, iOSImagePickerType );
#elif UNITY_ANDROID && !UNITY_EDITOR
			if (NativeGalleryController.Verbose)
				Debug.Log ("Opening Image Picker - Android");
			AndroidGallery.OpenGallery( (Texture2D tex)=>{
				_callback(tex,AndroidGallery.GetLastImportedOrientation());
			}, rotateImportedImage );
#elif UNITY_EDITOR
			if (NativeGalleryController.Verbose)
				Debug.Log ("Opening Image Picker - Editor");
			NativeGalleryController.OpenEditorGallery( callback );
#else
			Debug.LogError("OpenGallery: Function not implemented for platform " + Application.platform );
#endif
		}
	}



}