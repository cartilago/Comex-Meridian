/// <summary>
/// CameraManager.
/// Defines functionality for taking pgotos with the device's Webcam.
/// 
/// By Jorge L. Chavez Herrera
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Meridian.Framework.Utils;
using Meridian.Framework.Managers;

public enum WebCamAuthorizationStatus { Off, Requested, Authorized, Denied };

public class CameraPanel : Panel
{
    #region Class members
    public DecoratorPanel decorator;
    public AudioClipSource photoSFX;
    public Renderer previewRenderer;
    public GameObject initializingMessage;

    private WebCamTexture webCamTexture;
    private WebCamAuthorizationStatus aurhorizationStatus = WebCamAuthorizationStatus.Off;
    private Vector2 photoSize;
    private float photoAngle;
    private bool verticallyMirrored;
    #endregion

	#region Class accessors
	static private CameraPanel _instance;
	static public CameraPanel Instance
    {
    	get
    	{
    		if (_instance == null)
				_instance = FindObjectOfType<CameraPanel>();

    		return _instance;
    	}
    }

    private AudioSource _cachedAudioSource;
    private AudioSource cachedAudioSource
    {
        get
        {
            if (_cachedAudioSource == null)
            {
                // Try finding an already exixting AudioSource
                _cachedAudioSource = GetComponent<AudioSource>();

                // If no AudioSource was found, add a new one
                if (_cachedAudioSource == null)
                {
                    _cachedAudioSource = gameObject.AddComponent<AudioSource>();
                    _cachedAudioSource.playOnAwake = false;
                }
            }

            return _cachedAudioSource;
        }
    }
    #endregion

    #region MonoBehaviour overrides
    private void OnEnable()
    {
		previewRenderer.gameObject.SetActive(false);
		initializingMessage.SetActive(true);
		DecoratorPanel.Instance.ResetView();

        // Start WebCam
        if (aurhorizationStatus == WebCamAuthorizationStatus.Off || aurhorizationStatus == WebCamAuthorizationStatus.Denied)
        {
            StartCoroutine(StartCamera());
        }

        // Playback webcam texture 
        if (webCamTexture != null && aurhorizationStatus == WebCamAuthorizationStatus.Authorized)
        {
            webCamTexture.Play();
			initializingMessage.SetActive(false);
			previewRenderer.gameObject.SetActive(true);

			Vector2 photoSize = new Vector2(webCamTexture.width, webCamTexture.height);      

			if (webCamTexture.videoVerticallyMirrored)
				photoSize.y *= -1;

			previewRenderer.transform.localScale = photoSize;
			previewRenderer.material.SetTexture("_MainTex", webCamTexture);
        }

        decorator.Hide();
    }

    private void OnDisable()
    {
        // Pause WebCam
        if (webCamTexture != null && webCamTexture.isPlaying == true)
        {
            webCamTexture.Pause();
        }

		previewRenderer.gameObject.SetActive(false);
        decorator.Show();
    }

    public void OnDestroy()
    {
        // Stop WebCam
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }
    }
    #endregion

    #region Class implementation
    IEnumerator StartCamera()
    {
        // Request WebCam's user authorization
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            aurhorizationStatus = WebCamAuthorizationStatus.Authorized;
                
        }
        else
        {
            aurhorizationStatus = WebCamAuthorizationStatus.Denied;
            yield break;
        }

        // Setup WEBCam texture
        string backCamName = "";
        WebCamDevice[] webcamDevices = WebCamTexture.devices;

        // Find back camera
        for (int i = 0; i < webcamDevices.Length; i++)
        {
            if (webcamDevices[i].isFrontFacing == false)
                backCamName = webcamDevices[i].name;
        }

        // Set WebCam resolution, ensure to get a texture at least the double size of a screen
        webCamTexture = new WebCamTexture(backCamName, Screen.width, Screen.height);//10000, 10000);//, Screen.width, Screen.height, 60);
        webCamTexture.Play();
       
		while ( webCamTexture.width < 100 )
		{
			//Debug.Log("Still waiting another frame for correct info...");
			yield return null;
		}

		initializingMessage.gameObject.SetActive(false);
		previewRenderer.gameObject.SetActive(true);

		// Get video rotation first so we can compute aspect ratio & screen fitting correctly.
		photoAngle = webCamTexture.videoRotationAngle;
		previewRenderer.transform.localEulerAngles = new Vector3(0,0, -photoAngle);


		if (photoAngle == 0)
		{
			Vector2 photoSize = new Vector2(webCamTexture.width, webCamTexture.height);      
	        float screenAspectRatio = (float)Screen.height / (float)Screen.width;
	        float photoAspectRatio = photoSize.y / photoSize.x;
	        previewRenderer.transform.localScale = photoSize;
			previewRenderer.material.SetTexture("_MainTex", webCamTexture);

			Camera.main.orthographicSize = (photoSize.y / 2) * (screenAspectRatio / photoAspectRatio);
		}
		else
		{
			Vector2 photoSize = new Vector2(webCamTexture.width, webCamTexture.height);      
	        float screenAspectRatio = (float)Screen.height / (float)Screen.width;
	        float photoAspectRatio = photoSize.x / photoSize.y;

			if (webCamTexture.videoVerticallyMirrored)
				photoSize.y *= -1;

			previewRenderer.transform.localScale = photoSize;

			previewRenderer.material.SetTexture("_MainTex", webCamTexture);

			Camera.main.orthographicSize = (photoSize.x / 2) * (screenAspectRatio / photoAspectRatio);
		}
    }

    public void TakeSnapshot()
    {
        // Grab the current Webcam's image
        webCamTexture.Pause();
        StartCoroutine(TakeSnapshotCoroutine());
    }

    private IEnumerator TakeSnapshotCoroutine()
    {
        Color32[] photoBuffer = null;

        photoBuffer = webCamTexture.GetPixels32();
        photoSFX.Play(cachedAudioSource);

        Debug.Log(string.Format("WebcamTexture size {0}x{1}", webCamTexture.width, webCamTexture.height));

        if (photoAngle == 0)
        {
            Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height);
            texture.SetPixels32(photoBuffer);
            texture.Apply();
            DecoratorPanel.Instance.SetPhoto(texture);
        }
        else
        {
            Texture2D texture = new Texture2D(webCamTexture.height, webCamTexture.width);
            photoBuffer = Color32Utils.RotateColorArrayLeft(photoBuffer, webCamTexture.width, webCamTexture.height);
            texture.SetPixels32(photoBuffer);
            texture.Apply();
            DecoratorPanel.Instance.SetPhoto(texture);
        }

        yield return new WaitForSeconds(1);
        decorator.Show();
        gameObject.SetActive(false);
    }
    #endregion
}