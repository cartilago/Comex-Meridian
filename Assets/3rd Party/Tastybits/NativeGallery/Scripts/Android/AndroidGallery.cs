using System;
using UnityEngine;


#if UNITY_ANDROID && (!UNITY_EDITOR || NATIVE_GALLERY_DEV)

namespace Tastybits.NativeGallery {
	
	/**
	 * This is the implemnetation of the NativeGallery class for Android.
 	 * You can use this to ope
	 */
	public class AndroidGallery {
		public static NativeGalleryController.LoadImageMethods LoadImageMethod = NativeGalleryController.LoadImageMethods.NativeLoading;

	
		static AndroidJavaClass _AndroidCls=null;
		static AndroidJavaClass AndroidCls {
			get { 
				if( _AndroidCls == null ) {
					_AndroidCls = new AndroidJavaClass("com.NativeGallery.AndroidGallery");
				}
				return _AndroidCls;
			}
		}



		public static void OpenGallery( System.Action<Texture2D> callback, bool rotateImportedImage = true ) { 
			if( Application.platform != RuntimePlatform.Android ) {
				Debug.LogError("Cannot do this on a non Android platform");
			}

			string callbackId=
			NativeIOSDelegate.CreateNativeIOSDelegate( ( System.Collections.Hashtable args )=>{
				Debug.Log("NativeCallback returned");
				bool succeeded = System.Convert.ToBoolean(args["succeeded"]);
				bool cancelled = System.Convert.ToBoolean(args["cancelled"]);
				string path = System.Convert.ToString( args["path"] );

				Debug.Log("AndroidGallery returned with succeeded = " + succeeded + " cancelled = " + cancelled + " path = " + path );

				if( succeeded && !cancelled ) {
					LoadAssetFromAssetLibrary( path, ( Texture2D tex )=> {
						callback( tex );
					} );
				} else {
					Tastybits.NativeGallery.NativeGalleryController.LastPickedImagePath = "";
					//string msg = cancelled ? "cancelled" : "";
					callback( null );
				}


			}).name;

			AndroidCls.CallStatic("OpenGallery", new object[] { callbackId, rotateImportedImage } );
		}

	
		public static ExifOrientation GetLastImportedOrientation() {
			int ret = AndroidCls.CallStatic<int>("getLastImportedOrientation", new object[] {  } );
			return (ExifOrientation)ret;
		}

		
		static byte[] LoadPngBytesFromPath( string assetUrl, int maxTextureSize ) {
			if (assetUrl.StartsWith ("file://")) {
				assetUrl = assetUrl.Replace("file://","");
			}
			var obj = AndroidCls.CallStatic<AndroidJavaObject>("LoadPngBytesFromPath", new object[] { assetUrl, maxTextureSize } );
			bool ok = true;
			if (obj == null) {
				Debug.LogError( "Error the returned object was null" );
				ok = false;
			} else {
				if( obj.GetRawObject() == IntPtr.Zero ) {
					Debug.LogError ("Error the raw object returned was null");
					ok = false;
				} else if (obj.GetRawObject ().ToInt32 () == 0) {
					Debug.LogError("Error cannot convert raw object to Int32" );
					ok = false;
				}
			}
			if( ok ) {
				Debug.Log ("Converting raw object to byte array");
				byte[] result = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(obj.GetRawObject());
				if( result == null || result.Length == 0 ) {
					Debug.LogError("Error readig bytes from the asset library (1)");
				}
				AndroidCls.CallStatic ("ClearPngByteBuffer", new object[]{ });
				return result;	
			}
				Debug.LogError("Error reading bytes from the asset library (2)");
			return null;
		} 


		/**
		 * Loads the asset from the assetlibrary.
		 */
		static void LoadAssetFromAssetLibrary( string assetUrl, System.Action<Texture2D> callback ) {
			// Load using Native Method.
			if( LoadImageMethod == NativeGalleryController.LoadImageMethods.NativeLoading ) { 
				var bytes = LoadPngBytesFromPath( assetUrl, GetMaxTextureSize() );
				var tx = new Texture2D( 2, 2 );
				tx.LoadImage( bytes );
				callback( tx );
			} else { // Load using WWW.
				string path = assetUrl;
				var www = new WWW( path );
				Tastybits.NativeGallery.NativeGalleryController.LastPickedImagePath = path;
				WWWUtil.Wait( www, ( WWW w, bool www_ok ) => {
					//string msg = ( www_ok ? "" : "error loading file" );
					if( www_ok && w.texture == null ) {
						www_ok=false;
						//msg = "texture is null";
					}
					callback( w.texture );
				} );
			}
		}


		/**
		 * Returns the Texture size 
	 	 * source: http://answers.unity3d.com/questions/299405/how-can-i-detect-the-maximum-supported-texture-siz.html
		 */
		static int GetMaxTextureSize( int cap = 2048 ) {
			try {
				using( AndroidJavaClass glClass = new AndroidJavaClass( "android.opengl.GLES20" ) ) {
				using(AndroidJavaClass intBufferClass = new AndroidJavaClass("java.nio.IntBuffer") ) {
				AndroidJavaObject intBuffer = intBufferClass.CallStatic<AndroidJavaObject>("allocate", 1);
				int MAX_TEXTURE_CODE = glClass.GetStatic<int>("GL_MAX_TEXTURE_SIZE");
				glClass.CallStatic("glGetIntegerv", MAX_TEXTURE_CODE, intBuffer);
				int maxSize = intBuffer.Call<int>("get", 0);
				if( maxSize >= cap ) {
				maxSize = cap;
				}
				return maxSize;
				}
				}
			}catch (System.Exception e){
				Debug.LogError ("Error getting maximum texture size : " + e.ToString ());
				return 1024;
			}
		}


		
	}

}


#endif
