#pragma warning disable 414
using UnityEngine;
using System.Collections;


namespace Tastybits.NativeGallery {
	

	/**
	 * NativeGalleryController is the heart of the NativeGallery plugin.
	 * it's a singleton object which should not be released.
	 */
	[RequireComponent(typeof(RectTransform))]
	public class NativeGalleryController : MonoBehaviour {
		// The singleton instance.
		static NativeGalleryController _instance;

		// This variable is used to detect if an instance of the class is the singleton instance.
		[HideInInspector][UnityEngine.SerializeField]
		bool iAmSingleton = false;

		[Tooltip("Contains a referance to the Editor Only Prefab for the UI")]
		public GameObject editorUIPrefab;

		/**
		 * if you want to get more info out of the Native Gallery Assist plugin
		 * You can use this debugging system to debug potential problems.
		 */
		public bool verbose=false;
		public static bool Verbose {
			get { 
				if (_instance == null)
					return false;
				return _instance.verbose;
			}
		}


		/**
		 * This enum defines two types of LoadMthods
		 */
		public enum LoadImageMethods {
			WWWLoading = 1,
			NativeLoading = 2
		}


		/**
		 * Returns the sigleton instace.
		 */
		public static NativeGalleryController instance {
			get {
				if( _instance == null ) {
					var gos = Object.FindObjectsOfType<GameObject>();
					foreach( var go in gos ) {
						var tmp = go.GetComponentInChildren2<NativeGalleryController>(true);
						if( tmp != null ) {
							//Debug.Log("Returned the native gallery controller.");
							_instance = tmp;
							break;
						} 
					}
				}
				return _instance;
			}
		}


		/**
		 * Initializese the component and makes sure that no more than 1 instance is created.
		 */
		void Awake() {
			if( _instance != null && _instance != this ) {
				Debug.LogWarning ("The gameobject : " + this.name + " was destroyed since there is already an insteance of the Native Gallery Component loaded");
				GameObject.DestroyImmediate (this.gameObject);
				return;
			}
			Object.DontDestroyOnLoad (this.gameObject);
			if( _instance == null ) {
				_instance=this;
			}
			if( Time.frameCount < 3 ) {
				instance.gameObject.SetActive(false);
			}
			iAmSingleton = true;
		}


		void OnDestroy(){
			if( _instance==this ) {
				_instance = null;
			}
		}


		/**
		 * In the Unity Editor we can support recompile by using Update.
		 */
#if UNITY_EDITOR
		void Update() {
			if( _instance == null && iAmSingleton ){ 
				_instance=this;
			}
		}
#endif 

		// This holds an instance to some UI we will show in the editor to make
		// it easier to integrate and we don't need to run on the device ( iOS/Android ) 
		// all the time to test the integration.
		[Tooltip("Editor gallery simulator UI referance")]
		public GameObject editorGallery;


		[Tooltip("On iOS you can make the native gallery select images from diffirent types of Galleries. Photos,Saved Photos and Camera")]
		public Tastybits.NativeGallery.ImagePickerType iOSImagePickerType = Tastybits.NativeGallery.ImagePickerType.UIImagePickerControllerSourceTypeSavedPhotosAlbum;


		[Tooltip("Rotate imported image is used to counter rotate the imported image so that landscape images are looking right when imported.")]
		public bool rotateImportedImage = true;


		[Tooltip("On Android the Unity Android Player has a difficult time loading using the WWW class use Native Loading if you're experencing this")]
		public NativeGalleryController.LoadImageMethods LoadImageMethod = NativeGalleryController.LoadImageMethods.NativeLoading;


		/**
		 * This is the main method in the Native Gallery Controller.
		 * You can use this to open the gallery and show the images availble on the Device.
		 */
		public static void OpenGallery( System.Action<Texture2D,ExifOrientation> callback ) {
			if( instance == null ) {
				var prefab = Resources.Load( "NativeGallery", typeof(GameObject) );
				GameObject go = (GameObject)GameObject.Instantiate (prefab );
				go.name = "NativeGallery";
				if (instance == null) {
					Debug.LogError ("Cannot open Test gallery in editor mode since no instance of NativeGalleryController was found");
					callback (null,ExifOrientation.ORIENTATION_UNDEFINED);
					return;
				} else {
					Debug.Log("NativeGallery: We have autocreted the instance of the NativeGalleryp prefab in the scene");
				}
			}
			instance.gameObject.SetActive(true);
			if( UnityEngine.Application.isEditor == false && instance.editorGallery != null ) {
				instance.editorGallery.SetActive(false);
			}
			#if UNITY_ANDROID && (!UNITY_EDITOR || NATIVE_GALLERY_DEV)
			Tastybits.NativeGallery.AndroidGallery.LoadImageMethod = instance.LoadImageMethod;
			#endif 
			ImagePicker.OpenGallery( ( Texture2D tx, ExifOrientation orient ) => {
				instance.gameObject.SetActive(false);
				callback( tx, orient );
			}, instance.rotateImportedImage, instance.iOSImagePickerType );
		}


		/**
		 * Open Editor Gallery is a method meant not to be called directly 
		 * It's invoked when you run this in the editor to simulate the functionality exposed on iOS/Andorid.
		 */
		public static void OpenEditorGallery( System.Action<Texture2D,ExifOrientation> callback ) {
			if( instance == null ) {
				Debug.LogError("Cannot open Test gallery in editor mode since no instance of NativeGalleryController was found" );
				callback(null,ExifOrientation.ORIENTATION_UNDEFINED);
				return;
			}
			if (instance.editorGallery == null) {
				var canvas = CanvasHelper.GetCanvas (true);
				if (canvas != null && instance.editorUIPrefab != null ) {
					var go = (GameObject)GameObject.Instantiate (instance.editorUIPrefab);
					instance.editorGallery = go;
					var rt = go.GetComponent<RectTransform> ();
					rt.SetParent (canvas.GetComponent<RectTransform> (), false);
					go.name = "NativeGalleryUI";
				}
			}
			if( instance.editorGallery == null || ( instance.editorGallery.GetComponent<EditorGalleryUI>()==null ) ) {
				Debug.LogError("Cannot open Test gallery in editor mode since no referance to an EditorGalleryUI object was found" );
				return;
			}
			instance.gameObject.transform.SetAsLastSibling ();
			var edgal = instance.editorGallery.GetComponent<EditorGalleryUI>();
			edgal.OpenGallery( callback );
		}

		public static string LastPickedImagePath = "";
	}





}