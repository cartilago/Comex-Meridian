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

public class CameraManager : MonoSingleton<CameraManager>
{
    #region Class members
    public Decorator decorator;
    public Renderer previewRenderer;
    public AudioClipSource photoSFX;

    private WebCamTexture webCamTexture;
    private WebCamAuthorizationStatus aurhorizationStatus = WebCamAuthorizationStatus.Off;
    private Vector2 photoSize;
    private float photoAngle;
    #endregion

    #region Class accessors
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
        previewRenderer.gameObject.SetActive(true);

        // Restart WebCam
        if (aurhorizationStatus == WebCamAuthorizationStatus.Off || aurhorizationStatus == WebCamAuthorizationStatus.Denied)
        {
            StartCoroutine(StartCamera());
        }

        if (webCamTexture != null && aurhorizationStatus == WebCamAuthorizationStatus.Authorized)
        {
            webCamTexture.Play();
            previewRenderer.material.SetTexture("_MainTex", webCamTexture);
        }

        decorator.Hide();
    }

    private void OnDisable()
    {
        previewRenderer.gameObject.SetActive(false);

        // Pause WebCam
        if (webCamTexture != null && aurhorizationStatus == WebCamAuthorizationStatus.Authorized)
        {
            webCamTexture.Pause();
        }

        decorator.Show();
    }

    override public void OnDestroy()
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
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone))
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
            if (webcamDevices[i].isFrontFacing == false)
                backCamName = webcamDevices[i].name;

        // Set WebCam resolution, ensure to get a texture at least the double size of a screen
        webCamTexture = new WebCamTexture(backCamName);
        webCamTexture.requestedWidth = Screen.width * 4;
        webCamTexture.requestedHeight = Screen.height * 4;
        webCamTexture.requestedFPS = 60;

        webCamTexture.Play();

        photoAngle = (webCamTexture.videoRotationAngle != 0) ? webCamTexture.videoRotationAngle + 180 : 0;

        // Adjust preview to match photo orientation & size
        float screenAspectRatio = (float)Screen.height / (float)Screen.width;
        photoSize = new Vector2(webCamTexture.width, webCamTexture.height);
        float photoAspectRatio = photoSize.y / photoSize.x;
  
        if (photoAngle == 0)
            Camera.main.orthographicSize = (photoSize.y / 2) * (screenAspectRatio / photoAspectRatio);
        else
            Camera.main.orthographicSize = (photoSize.x / 2) * (screenAspectRatio / (photoSize.x / photoSize.y));

        previewRenderer.transform.localScale = photoSize;
        previewRenderer.transform.localEulerAngles = new Vector3(0, 0, photoAngle);
        previewRenderer.material.SetTexture("_MainTex", webCamTexture);
    }

    public void TakeSnapshot()
    {
        StartCoroutine(TakeSnapshotCoroutine());
    }

    private IEnumerator TakeSnapshotCoroutine()
    {
        Color32[] photoBuffer = null;

        // Grab the current Webcam's image
        webCamTexture.Pause();

        photoBuffer = webCamTexture.GetPixels32();
        photoSFX.Play(cachedAudioSource);

        if (photoAngle == 0)
        {
            Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height);
            texture.SetPixels32(photoBuffer);
            texture.Apply();
            Decorator.Instance.SetPhoto(texture, photoAngle);
        }
        else
        {
            Texture2D texture = new Texture2D(webCamTexture.height, webCamTexture.width);
            photoBuffer = Color32Utils.RotateColorArrayLeft(photoBuffer, webCamTexture.width, webCamTexture.height);
            texture.SetPixels32(photoBuffer);
            texture.Apply();
            Decorator.Instance.SetPhoto(texture, photoAngle);
        }

        yield return new WaitForSeconds(1);
        decorator.Show();
        gameObject.SetActive(false);
    }
    #endregion
}
