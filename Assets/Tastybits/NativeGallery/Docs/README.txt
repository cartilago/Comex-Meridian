Thank you for buying Native Gallery !  ( for iOS / Android )

Native Gallery for iOS and Android is a plugin that helps you to quickly integrate the Native Gallery functionalty of iOS or Android Device

You can use to to select an image from the gallery and import that into Unity for usage as a texture, profile image or whatever makes sense in your awesome project. If you need support, have questions or if you have a request for a new feature you want to have added please don't hessitate to contract us at tastybits8+support@gmail.com - we will do our best to help you.


----------------------------------------------------------------------------------------
  How to use - with Unity UI Prefab
----------------------------------------------------------------------------------------

1. The package contains a NativeGallery prefab that you can drag into an existing Canvas object in Unity. 
You can use that to open the Gallery an example of such usage can be seen in the Demo scene.

2. To open the gallery and pick a image simply run :
NativeGalleryController.OpenGallery( ( Texture2D tex )=>{ } ); 

NOTE: We wanted to make it easy for you to test getting images from the Native Gallery so we have included a simple "Editor Test UI".
That way you can also test the integration in your project. Try running the Demo in the Editor and then on device. 


----------------------------------------------------------------------------------------
  How to use - Without Unity UI
----------------------------------------------------------------------------------------

If you are not interested in using Unity UI you can also open the Gallery (on device) by adding this one liner to your code somewhere.

Tastybits.NativeGallery.ImagePicker.OpenGallery( ( Texture2D tex ) => { Debug.Log("here is your texture : " + tex.name );  } );





----------------------------------------------------------------------------------------
  Notes regarding Manifest for Android
----------------------------------------------------------------------------------------

To be able to open the gallery on Android you need to tell the Android OS that the Application is allowed to access images on the device.

-

If you are using no other plugins for Android you can copy the template manifest from location :

Assets/Tastybits/NativeGallery/AndroidManifest.xml 

to location:

Assets/Plugins/Android/AndroidManifest.xml

-

If you are using other plugins relying on an Android Manifest, you'll need to paste this into the existing manifest file.
in the 'application' scope.

<activity android:name="com.NativeGallery.UnityProxyActivity"
	  android:launchMode="singleTask"
	  android:label="@string/app_name"
	  android:configChanges="fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen">

	<intent-filter>
    		<action android:name="androidnativeactions.UnityProxyActivity" />
    		<category android:name="android.intent.category.DEFAULT" />
	 	</intent-filter>
</activity>

You also need to add the following permission to the 'manifest' scope after the 'application' scope.

<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />







