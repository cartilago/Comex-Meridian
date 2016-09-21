using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Tastybits.NativeGallery;
using System.IO;

public class DecoratorPanel : Panel
{
    #region Class members
    public GameObject fileMenu;
    public GameObject topSection;
    public GameObject bottomSection;
    public NewImagePanel newImagePanel;
    public InputField projectNameInputField;
    public Renderer photoRenderer;
    public Renderer canvasRenderer;
    public Texture2D startPhoto;

    public Camera photoCamera;
    public Camera canvasCamera;

    public DrawingToolBase[] tools;

    private Project currentProject;
    private int currentTouch;
    private bool mouseOrFingerDown;

    private DrawingToolBase currentTool;
    private ColorBuffer HSVPixelBuffer;
    #endregion

    #region Class accessors
    static private DecoratorPanel _instance;
    static public DecoratorPanel Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<DecoratorPanel>();

            return _instance;
        }
    }
    #endregion

    #region MonoBehaviour overrides
    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        SetCurrentProject(new Project());
        SetPhoto(startPhoto);
        currentTool = tools[0];
    }

    private bool twoFinger;

    private void Update()
    {
        // Touch input
        if (Input.touchCount == 1)
        {
            Touch touchOne = Input.GetTouch(0);

            if (touchOne.phase == TouchPhase.Began)
            {
                // UI elements block touch input
                if (EventSystem.current.IsPointerOverGameObject(touchOne.fingerId))
                    return;

                currentTool.TouchDown(touchOne.position);
                mouseOrFingerDown = true;
            }
            else if (touchOne.phase == TouchPhase.Moved && mouseOrFingerDown == true)
            {
                currentTool.TouchMove(touchOne.position);
            }
            else if (touchOne.phase == TouchPhase.Ended && mouseOrFingerDown == true)
            {
                mouseOrFingerDown = false;
                currentTool.TouchUp(touchOne.position);
            }
            return;
        }

		currentTool.Update();

        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            // UI elements block mouse input
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            mouseOrFingerDown = true;
            currentTool.TouchDown(Input.mousePosition);
        }

        if (mouseOrFingerDown == true)
        {
            currentTool.TouchMove(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && mouseOrFingerDown == true)
        {
            mouseOrFingerDown = false;
            currentTool.TouchUp(Input.mousePosition);
        }
    }

    private void Reset()
    {
        FingerCanvas.Instance.ClearUndoStack();
        FingerCanvas.Instance.Clear();
      	PaintTool.ReleaseMemory();
    }
    #endregion

    #region Class implementation
    public void SetCurrentProject(Project project)
    {
        currentProject = project;
        projectNameInputField.text = currentProject.name;

        Texture2D projectPhoto = project.GetEncodedPhoto();

        if (projectPhoto != null)
            SetPhoto(projectPhoto);

        Texture2D projectMask = project.GetEncodedMask();

        if (projectMask != null)
            FingerCanvas.Instance.SetContents(projectMask);


        ColorsManager.Instance.SetColorsForButtons(currentProject.colors, currentProject.colorNames);
    }

    public Project GetCurrentProject()
    {
        return currentProject;
    }

    public void SetProjectName(string name)
    {
        currentProject.name = name;
    }

    public float GetBaseOrthographicSize()
    {
		float screenAspectRatio = (float)Screen.height / (float)Screen.width;
        Vector2 photoSize = new Vector2(currentProject.GetPhoto().width, currentProject.GetPhoto().height);
        float photoAspectRatio = photoSize.y / photoSize.x;

        return (photoSize.y / 2) * (screenAspectRatio / photoAspectRatio);
    }

    public void SetPhoto(Texture2D photo)
    {
        Reset();
       
        Debug.Log (string.Format("Photo set with size: {0}x{1}", photo.width, photo.height));

		ColorBuffer oldHSVPixelBuffer = HSVPixelBuffer;
		HSVPixelBuffer = new ColorBuffer(photo.width, photo.height, Color32Utils.ConvertToHSV(photo.GetPixels()));

        // Convert photo to internal HSV representation, shader will convert it back to RGB.
        Texture oldHSVTexture = photoRenderer.material.GetTexture("_MainTex");
        Texture2D hsvTexture = new Texture2D(photo.width, photo.height);
		hsvTexture.SetPixels(HSVPixelBuffer.data);
		hsvTexture.filterMode = FilterMode.Point;
		hsvTexture.Apply();
		photoRenderer.material.SetTexture("_MainTex", hsvTexture);

		Debug.Log (string.Format("HSV buffer size: {0}x{1}", hsvTexture.width, hsvTexture.height));

		currentProject.SetPhoto(photo);
	
		// Set correct size for both camera orthographic view & photo renderer            
		float screenAspectRatio = Screen.width / (float)Screen.height;//(float)Screen.height / (float)Screen.width;
		Vector2 photoSize = new Vector2(photo.width, photo.height);  
        float photoAspectRatio = photoSize.y / photoSize.x;
		canvasRenderer.transform.localScale = photoRenderer.transform.localScale = photoSize; 
		canvasCamera.orthographicSize = photoCamera.orthographicSize = GetBaseOrthographicSize();
		canvasCamera.aspect = photoCamera.aspect;

		ResetCameraPosition();

        FingerCanvas.Instance.SetupCanvas();
        FingerCanvas.Instance.SaveUndo();
		
        // We should wait for the UI to complete layout setup or will get wrong coordinates
        Invoke("ResetCameraPosition", .01f);

		// Release old texture memory
		DestroyImmediate(oldHSVPixelBuffer);
		DestroyImmediate(oldHSVTexture,true);
		Resources.UnloadUnusedAssets(); 
		System.GC.Collect();
    }

    public void ResetCameraPosition()
    {
		photoCamera.transform.position = canvasCamera.transform.position = Vector3.zero;
		photoCamera.orthographicSize = canvasCamera.orthographicSize = GetBaseOrthographicSize();

        Vector3[] topSectionCorners = new Vector3[4];
        Vector3[] bottomSectionCorners = new Vector3[4];

        topSection.GetComponent<RectTransform>().GetWorldCorners(topSectionCorners);
        bottomSection.GetComponent<RectTransform>().GetWorldCorners(bottomSectionCorners);

        Canvas canvas = FindObjectOfType<Canvas>();
        Rect topSectionScreenRect = GetScreenRect(topSection.GetComponent<RectTransform>(), canvas);
        Rect bottomSectionScreenRect = GetScreenRect(bottomSection.GetComponent<RectTransform>(), canvas);
        Debug.Log(topSectionScreenRect);

        Vector2 p = photoCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, (topSectionScreenRect.yMax + bottomSectionScreenRect.yMin) / 2, 0));
       	photoRenderer.transform.position = new Vector3(0, -p.y, 0);
    }

    public static Rect GetScreenRect(RectTransform rectTransform, Canvas canvas)
    {

        Vector3[] corners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[2];

        rectTransform.GetWorldCorners(corners);

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
        }
        else
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
        }

        screenCorners[0].y = Screen.height - screenCorners[0].y;
        screenCorners[1].y = Screen.height - screenCorners[1].y;

        return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
    }


    public ColorBuffer GetHSVPixelBuffer()
    {
		return HSVPixelBuffer;
    }

    public void Hide()
    {
        topSection.gameObject.SetActive(false);
        bottomSection.gameObject.SetActive(false);
        photoRenderer.gameObject.SetActive(false);
    }

    public void Show()
    {
        topSection.gameObject.SetActive(true);
        bottomSection.gameObject.SetActive(true);
        photoRenderer.gameObject.SetActive(true);
    }

    public void GetImageFromGallery()
    {
        newImagePanel.ShowProgress();
        
        NativeGalleryController.OpenGallery((Texture2D tex, ExifOrientation orientation) => 
        {  
        	if (tex != null)
        	{
        		// Deal with EXIF rotations
				Texture2D texture = new Texture2D(tex.height, tex.width);
            	Color32[] photoBuffer = texture.GetPixels32();

        		switch (orientation)
        		{
					case ExifOrientation.ORIENTATION_UNDEFINED : break;
					case ExifOrientation.ORIENTATION_NORMAL: break;
					case ExifOrientation.ORIENTATION_FLIP_HORIZONTAL: photoBuffer = Color32Utils.FlipColorArrayHorizontally(photoBuffer, tex.width, tex.height); break;
					case ExifOrientation.ORIENTATION_ROTATE_180: photoBuffer = Color32Utils.RotateColorArrayLeft(photoBuffer, tex.width, tex.height); Color32Utils.RotateColorArrayLeft(photoBuffer, tex.width, tex.height);break;
					case ExifOrientation.ORIENTATION_FLIP_VERTICAL: photoBuffer = Color32Utils.FlipColorArrayVertically(photoBuffer, tex.width, tex.height); break;
					case ExifOrientation.ORIENTATION_TRANSPOSE: photoBuffer = Color32Utils.RotateColorArrayLeft(photoBuffer, tex.width, tex.height); break; photoBuffer = Color32Utils.FlipColorArrayVertically(photoBuffer, tex.width, tex.height); break;
					case ExifOrientation.ORIENTATION_ROTATE_90: photoBuffer = Color32Utils.RotateColorArrayRight(photoBuffer, tex.width, tex.height); break;
					case ExifOrientation.ORIENTATION_TRANSVERSE: photoBuffer = Color32Utils.RotateColorArrayLeft(photoBuffer, tex.width, tex.height); break; photoBuffer = Color32Utils.FlipColorArrayHorizontally(photoBuffer, tex.width, tex.height); break;
					case ExifOrientation.ORIENTATION_ROTATE_270: photoBuffer = Color32Utils.RotateColorArrayLeft(photoBuffer, tex.width, tex.height); break;
        		}

				texture.SetPixels32(photoBuffer);
            	texture.Apply();
      
            	SetPhoto(tex);
				DestroyImmediate(tex);
				Resources.UnloadUnusedAssets(); 
				System.GC.Collect();
            }
            newImagePanel.Hide();

			// Release old texture memory
        });
    }

    public void SaveScreenShot(string path)
    {
        // Remove old snapshot file
        if (File.Exists(path))
            File.Delete(path);

        // Create a temporary render texture and asign it to the photo camera
        RenderTexture renderTexture = RenderTexture.GetTemporary((int)photoRenderer.transform.localScale.x, (int)photoRenderer.transform.localScale.y);
        photoCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        // Readjust camera orthographic size & photo position
        float orthosize = photoCamera.orthographicSize;
        photoCamera.orthographicSize = renderTexture.height / 2;
        Vector3 cameraPos = photoCamera.transform.position;
        photoCamera.transform.position = Vector3.zero;
        Vector3 photoPos = photoRenderer.transform.position;
        photoRenderer.transform.position = Vector3.zero;
        // Render
        photoCamera.Render();
        // Now grab the rendered image into a texture
        Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        screenshot.Apply();
        // Wite the texture to disk
        byte[] png = screenshot.EncodeToPNG();
        File.WriteAllBytes(path, png);
        photoCamera.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);
        // Restore camera orthographic size & photo position
        photoCamera.orthographicSize = orthosize;
        photoCamera.transform.position = cameraPos;
        photoRenderer.transform.position = photoPos;
    }

    #region Paint tools
    public void SetPaintTool(bool active)
    {
    	if (active == true)
    	{
    		currentTool = tools[1];
    	}

		if (tools[1].toggle.isOn == false && tools[2].toggle.isOn == false)
			currentTool = tools[0];
    }

    public void SetEraserTool(bool active)
   	{
   		if (active == true)
   		{
			currentTool = tools[2];
   		}

		if (tools[1].toggle.isOn == false && tools[2].toggle.isOn == false)
			currentTool = tools[0];
   	}

    public void Undo()
    {
        FingerCanvas.Instance.RestoreFromUndoStack();
    }

	public void Clear()
    {
        //	FingerCanvas.Instance.SaveUndo();
        FingerCanvas.Instance.Clear();
    }

    #endregion

    #region IO
    public void SaveFile(string filename)
    {
		Texture2D snapshot = FingerCanvas.Instance.GetSnapshot();	

        fileMenu.SetActive(false);

        currentProject.SetEncodedPhoto(currentProject.GetPhoto(), snapshot);
		currentProject.SetColors(ColorsManager.Instance.GetButtonColorWidgets());

        string serializedProject = JsonUtility.ToJson(currentProject);
        System.IO.File.WriteAllText(filename, serializedProject);

		// Release texture memory
		Destroy(snapshot);
		Resources.UnloadUnusedAssets(); 
		System.GC.Collect();
    }

    public void LoadFile(string filename)
    {
        fileMenu.SetActive(false);
        string serializedProject = (string)System.IO.File.ReadAllText(filename);

        try
        {
            Project loadedProject = JsonUtility.FromJson<Project>(serializedProject);

            if (loadedProject != null)
                SetCurrentProject(loadedProject);
            else
                SetCurrentProject(new Project());
        }
        catch (System.Exception ex)
        {
            SetCurrentProject(new Project());
            Debug.Log(ex.Message);
        }
    }
    #endregion

    #endregion
}