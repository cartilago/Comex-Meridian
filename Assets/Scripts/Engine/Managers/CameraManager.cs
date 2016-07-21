using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Meridian.Framework.Utils;
using Meridian.Framework.Managers;

public enum WebCamAuthorizationStatus { Off,Requested, Authorized, Denied };

public class CameraManager : MonoSingleton<CameraManager>
{
    #region Class members
    public Decorator decorator;
    public Renderer photoRnderer;
    public AudioClipSource photoSFX;
    private WebCamTexture webCamTexture;
    private WebCamAuthorizationStatus aurhorizationStatus = WebCamAuthorizationStatus.Off;
    private Color32[] currentFrame = null;

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
        if (aurhorizationStatus == WebCamAuthorizationStatus.Off)
        {
            StartCoroutine(StartCamera());
        }

        if (webCamTexture != null && aurhorizationStatus == WebCamAuthorizationStatus.Authorized)
        {
            photoRnderer.material.SetTexture("_MainTex", webCamTexture);
            webCamTexture.Play();
        }

        decorator.Hide();
    }

    private void OnDisable()
    {
        if (webCamTexture != null && aurhorizationStatus == WebCamAuthorizationStatus.Authorized)
        {
            webCamTexture.Pause();
        }

        decorator.Show();
    }

    override public void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }
    }
    #endregion

    #region Class implementation
    IEnumerator StartCamera()
    {
        // Request user authorization for using the webcam if running as webplayer
        if (Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer ||
            Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
        {

            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);

            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
                aurhorizationStatus = WebCamAuthorizationStatus.Authorized;
            else
                aurhorizationStatus = WebCamAuthorizationStatus.Denied;
        }
        else
            aurhorizationStatus = WebCamAuthorizationStatus.Authorized;

        // Setup WEBCam texture
        string backCamName = "";
        WebCamDevice[] webcamDevices = WebCamTexture.devices;

        for (int i = 0; i < webcamDevices.Length; i++)
            if (webcamDevices[i].isFrontFacing == false)
                backCamName = webcamDevices[i].name;

        webCamTexture = new WebCamTexture(backCamName);
        webCamTexture.requestedWidth = Screen.width * 2;
        webCamTexture.requestedHeight = Screen.height * 2;
        webCamTexture.requestedFPS = 60;

        webCamTexture.Play();
       
        if (webCamTexture.videoRotationAngle > 0)
        {
            photoSize = new Vector2(Screen.height, Screen.width);
            photoAngle = webCamTexture.videoRotationAngle + 180;
           
        }
        else
        {
            photoSize = new Vector2(Screen.width, Screen.height);
            photoAngle = 0;
        }

        Camera.main.orthographicSize = Screen.height / 2;
        photoRnderer.transform.localScale = photoSize;
        photoRnderer.transform.localEulerAngles = new Vector3(0, 0, photoAngle);
        photoRnderer.material.SetTexture("_MainTex", webCamTexture);
        


        /*
        DebugManager.Log("Dimensions: " + webCamTexture.width + "x" + webCamTexture.height);
        DebugManager.Log("Video rotation angle: " + webCamTexture.videoRotationAngle);
        DebugManager.Log("Video vertically mirrored" + webCamTexture.videoVerticallyMirrored);
        */
    }

    public void TakeSnapshot()
    {
        StartCoroutine(TakeSnapshotCoroutine());
    }

    private IEnumerator TakeSnapshotCoroutine()
    {
        photoSFX.Play(cachedAudioSource);
        webCamTexture.Pause();
        currentFrame = webCamTexture.GetPixels32();

        Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height);
        texture.SetPixels32(currentFrame);
        texture.Apply();

        Decorator.Instance.SetPhoto(texture, photoSize, photoAngle);
        //photoRnderer.material.SetTexture("_MainTex", texture);

        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
        decorator.Show();
    }

    #endregion
}
