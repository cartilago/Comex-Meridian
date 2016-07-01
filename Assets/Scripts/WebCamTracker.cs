using UnityEngine;
using System.Collections;
using System.Threading;

public enum WebCamAuthorizationStatus {Requested, Authorized, Denied};

[System.Serializable]
public class ColorTracker {
	// Color tracking values
	public float minHue = .9f;
	public float maxHue = 1.1f;
	
	public float minSaturation = 0.8f;
	public float maxSaturation = 1;
	
	public float minValue = 0.5f;
	public float maxValue = 1;
	
	[System.NonSerialized]
	public float valueAverage;
	[System.NonSerialized]
	public int withinValueRangePixelCount;
	[System.NonSerialized]
	public float targetValue;
	
	[System.NonSerialized]
	public int pixelCount;
	
	[System.NonSerialized]
	public Vector2 centroid;
	
	[System.NonSerialized]
	public Vector3 targetPos;
	
	public bool autoAdjustBrigthness = false;
}

/// <summary>
/// Web cam tracker.
/// </summary>
public class WebCamTracker : MonoBehaviour {
	
	bool DEBUG = false;	
	const int SIZE = 256;
	int width, height;
	
	public ColorTracker[] colorTrackers;
	public int minPixelCoverage = 1;
	
	private WebCamTexture webCamTexture;
	
	[System.NonSerialized]
	public Texture2D displayTexture;
	
	private Color32[] currentFrame = null, displayFrame = null;
	
	private Thread mdThread;
	private bool isQuit = false;
	static public WebCamAuthorizationStatus aurhorizationStatus = WebCamAuthorizationStatus.Requested;
	
	IEnumerator Start() {
		
		// Request user authorization for using the webcam if running as webplayer
		if (Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer ||
			Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor ) {
			
			yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);
        	
			if (Application.HasUserAuthorization(UserAuthorization.WebCam))
				aurhorizationStatus = WebCamAuthorizationStatus.Authorized;
			else
				aurhorizationStatus = WebCamAuthorizationStatus.Denied; 
		} 
		else 
			aurhorizationStatus = WebCamAuthorizationStatus.Authorized;
		
		// Setup WEBCam texture
		string frontCamName = "";
		WebCamDevice[] webcamDevices = WebCamTexture.devices;
		
		for (int i = 0; i < webcamDevices.Length; i++)
			if (webcamDevices[i].isFrontFacing)
				frontCamName = webcamDevices[i].name;
	
		webCamTexture = new WebCamTexture(frontCamName,SIZE,SIZE);  
		
		OnEnable();
	}
	
	void OnEnable () {
		if(webCamTexture != null && aurhorizationStatus == WebCamAuthorizationStatus.Authorized) {
			webCamTexture.Play();
		}
	}
	
	void OnDisable () {
		if(webCamTexture != null && aurhorizationStatus == WebCamAuthorizationStatus.Authorized) {
			webCamTexture.Pause();
		}
	}
	
	void OnDestroy () {
		if(webCamTexture != null) {
			
			if (mdThread != null)
				mdThread.Abort();
			
			webCamTexture.Stop();
		}
	}
	
	// It's better to stop the thread by itself rather than abort it.
	void OnApplicationQuit () {
		isQuit = true;
	}
	
	Color32 clear = new Color32(0,0,0,0);
	
	const float BYTE_TO_FLOAT_MULTIPLIER = 1.0f / 256.0f;
	
	void TrackColor () {
		
		while(true) {
			
			if (isQuit) 
				break;
			 
			// Initialize tracker centroids && pixel count
			foreach(ColorTracker tracker in colorTrackers) {
				tracker.pixelCount = 0;;
				tracker.centroid = Vector2.zero;
				tracker.valueAverage = 0;
				tracker.withinValueRangePixelCount = 0;
			}
		
			// Analize image pixels
			for (int i = 0; i < currentFrame.Length; i++) {
				Color32 pixel = currentFrame[i];
				
				HSVColor hsv = HSVColor.FromRGBA(pixel.r * BYTE_TO_FLOAT_MULTIPLIER, pixel.g * BYTE_TO_FLOAT_MULTIPLIER, pixel.b * BYTE_TO_FLOAT_MULTIPLIER, 1); 
				
				foreach(ColorTracker tracker in colorTrackers) {
					
					// Add pixel coorinates to the centroid if H & S are withing the range
					if (hsv.h >= tracker.minHue && hsv.h <= tracker.maxHue && hsv.s >= tracker.minSaturation && hsv.s <= tracker.maxSaturation) {
						// We must average v because ligth changes a lot depending on the current environment
						tracker.valueAverage+= hsv.v;
						tracker.withinValueRangePixelCount++;
						
						if (hsv.v >= tracker.minValue && hsv.v <= tracker.maxValue) { 
							tracker.centroid.x+= i % width;
							tracker.centroid.y+= i / width;
									
							tracker.pixelCount++;
							displayFrame[i] = pixel;
						}
						else
							displayFrame[i] = clear;
					}
				}
			}		
				
			// Analize bounding boxes
			foreach(ColorTracker tracker in colorTrackers) {
				// normalize value average
				if (tracker.withinValueRangePixelCount > 0)
					tracker.targetValue = tracker.valueAverage/= tracker.withinValueRangePixelCount;
				
				// If we computed enough data when getting the tracked object's centroid
				if (tracker.pixelCount > minPixelCoverage) {
					// average the sum of all pixel coordinathes within the hsv threshold
					tracker.centroid/= tracker.pixelCount;
					
					// Finaly set the target tracked position
					tracker.targetPos = new Vector3((tracker.centroid.x / width) - 0.5f, (tracker.centroid.y / height) - 0.5f);
				}
			}
		}
    }
	
	void Update() {
		
		if (webCamTexture != null && webCamTexture.didUpdateThisFrame) {
			currentFrame = webCamTexture.GetPixels32();
			
			if (displayFrame == null) {
				
				// Create the display texture
				width =  webCamTexture.width;
				height = webCamTexture.height;
				displayTexture = new Texture2D(width, height);
				displayFrame = new Color32[width * height];
				
				// Start the motion detection thread
				mdThread = new Thread(TrackColor);
				mdThread.Priority = System.Threading.ThreadPriority.Highest;
				mdThread.Start();
			} 
			else {
				if (DEBUG == true) {
					displayTexture.SetPixels32(displayFrame);
					displayTexture.Apply();
				}
			}
		}
		// Autoadjust brigthness values
		foreach (ColorTracker tracker in colorTrackers) {
			if (tracker.autoAdjustBrigthness) {
				tracker.minValue = Mathf.Lerp(tracker.minValue, tracker.targetValue - 0.2f, Time.deltaTime);
				tracker.maxValue = Mathf.Lerp(tracker.maxValue, tracker.targetValue + 0.2f, Time.deltaTime);
			}
		}
	}
}
