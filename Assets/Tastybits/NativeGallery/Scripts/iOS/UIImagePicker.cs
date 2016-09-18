using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
 

namespace Tastybits.NativeGallery {

	/**
	 * 
	 */
	public enum ImagePickerType {
		UIImagePickerControllerSourceTypePhotoLibrary = 0,
		UIImagePickerControllerSourceTypeCamera = 1,
		UIImagePickerControllerSourceTypeSavedPhotosAlbum = 2
	}


#if UNITY_IPHONE && (!UNITY_EDITOR || NATIVE_GALLERY_DEV)

	/** 
	 * UIImagePicker integration for iOS.
	 **/
	public class UIImagePicker  {
		// iOS platform implementation.
		[DllImport ("__Internal")]
		private static extern string _ImagePickerOpen ( int type, bool frontFacingIsDefault, bool transformOnImport, string nativeCallbackId );
		
		[DllImport ("__Internal")]
		private static extern string _ImagePickerGetPath ();
		
		[DllImport ("__Internal")]
		private static extern int _ImagePickerGetCallCount ();

		[DllImport ("__Internal")]
		private static extern bool _CheckCameraAvailable ();

		[DllImport ("__Internal")]
		private static extern int _ImagePickerGetLastImportedOrientation();


		public static bool CheckCameraAvailable() {
			//return false;
			if( Application.platform != RuntimePlatform.IPhonePlayer ) {
				return true;
			}
			return _CheckCameraAvailable();
		}


		public static void OpenPhotoAlbum( System.Action<Texture2D,bool,string> callback, bool rotateImageOnImport, ImagePickerType galleryType = ImagePickerType.UIImagePickerControllerSourceTypeSavedPhotosAlbum  ) {
			UIImagePicker.Open( galleryType, rotateImageOnImport, ( bool succeeded, bool cancelled, string path ) => {
				Debug.Log ("Native Camera Saved image at path : " + path );
				bool ok = succeeded;
				var www = new WWW( path );
				if( ok && !cancelled ) {
					Tastybits.NativeGallery.NativeGalleryController.LastPickedImagePath = path;
					WWWUtil.Wait( www, ( WWW w, bool www_ok ) => {
						string msg = ( www_ok ? "" : "error loading file" );
						if( www_ok && w.texture == null ) {
							www_ok=false;
							msg = "texture is null";
						}
						callback( w.texture, www_ok, msg );
					} );
				} else {
					Tastybits.NativeGallery.NativeGalleryController.LastPickedImagePath = "";
					string msg = cancelled ? "cancelled" : "";
					callback( null, false, msg );
				}
			} );
		}



		public static ExifOrientation GetLastImportedOrientation() {
			return (ExifOrientation)_ImagePickerGetLastImportedOrientation();
		}



		private static string Open( ImagePickerType type, bool transformOnImport, System.Action<bool,bool,string> deleg ) 
		{
			if( Application.platform != RuntimePlatform.IPhonePlayer ) {
				Debug.LogError( "Error not running on iOS" );
				deleg( false, false, "" );
				return "";
			}
			Debug.Log("Opening UIImagePicker with type:" + type );
			return _ImagePickerOpen( (int)type, true, transformOnImport, NativeIOSDelegate.CreateNativeIOSDelegate( ( System.Collections.Hashtable args )=>{
				bool succeeded = System.Convert.ToBoolean(args["succeeded"]);
				bool cancelled = System.Convert.ToBoolean(args["cancelled"]);
				string path = System.Convert.ToString( args["path"] );
				Debug.Log("UIImagePicker returned with succeeded = " + succeeded + " cancelled = " + cancelled + " path = " + path );
				deleg( succeeded, cancelled, path );
			}).name );
		}
		
		public static string GetPath() {
			if( Application.platform != RuntimePlatform.IPhonePlayer ) {
				Debug.LogError( "Error not running on iOS" );
				return "";
			}
			return _ImagePickerGetPath();
		}

		
		public static int GetCallCount() {
			if( Application.platform != RuntimePlatform.IPhonePlayer ) {
				Debug.LogError( "Error not running on iOS" );
				return 0;
			}
			return _ImagePickerGetCallCount();
		}


	}


#endif

}
