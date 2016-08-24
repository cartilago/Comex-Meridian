using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Meridian.Framework.Utils;

public class DecoratorPanel :  Panel
{
    #region Class members
    public GameObject fileMenu;
    public GameObject topSection;
    public GameObject bottomSection;
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

    private void Update()
    {
        // Touch input
        if (Input.touchCount == 1)
        {
            Touch touchZero = Input.GetTouch(0);

            if (touchZero.phase == TouchPhase.Began)
            {
                // UI elements block touch input
                if (EventSystem.current.IsPointerOverGameObject(touchZero.fingerId))
                    return;

                currentTool.TouchDown(touchZero.position);
                mouseOrFingerDown = true;
            }
            else if (touchZero.phase == TouchPhase.Moved && mouseOrFingerDown == true)
            {
                currentTool.TouchMove(touchZero.position);
            }
            else if (touchZero.phase == TouchPhase.Ended && mouseOrFingerDown == true)
            {
                mouseOrFingerDown = false;
                currentTool.TouchUp(touchZero.position);
            }
        }

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

		if (currentProject.colors != null)
		{
			photoRenderer.material.SetColor("_Color1", currentProject.colors[0]);
			photoRenderer.material.SetColor("_Color2", currentProject.colors[1]);
			photoRenderer.material.SetColor("_Color3", currentProject.colors[2]);
		}
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
		FingerCanvas.Instance.Clear();

		HSVPixelBuffer = new ColorBuffer(photo.width, photo.height, Color32Utils.ConvertToHSV(photo.GetPixels()));

		// Convert photo to internal HSV representation, shader will convert it back to RGB.
		Texture2D hsvTexture = new Texture2D(photo.width, photo.height);
		hsvTexture.SetPixels(HSVPixelBuffer.data);
		hsvTexture.Apply();
		photoRenderer.material.SetTexture("_MainTex", hsvTexture);

		currentProject.SetPhoto(photo);
	
		// Set correct size for both camera orthographic view & photo renderer            
		float screenAspectRatio = Screen.width / (float)Screen.height;//(float)Screen.height / (float)Screen.width;
		Vector2 photoSize = new Vector2(photo.width, photo.height);  
        float photoAspectRatio = photoSize.y / photoSize.x;
		canvasRenderer.transform.localScale = photoRenderer.transform.localScale = photoSize; 
		canvasCamera.orthographicSize = photoCamera.orthographicSize = GetBaseOrthographicSize();
		canvasCamera.aspect = photoCamera.aspect;
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
    	FingerCanvas.Instance.SaveUndo();
        FingerCanvas.Instance.Clear();
    }

    #endregion

    #region IO
    public void SaveFile(string filename)
    {
        fileMenu.SetActive(false);

        currentProject.SetEncodedPhoto(currentProject.GetPhoto(), FingerCanvas.Instance.GetSnapshot());
		currentProject.SetColors(new Color[]{photoRenderer.material.GetColor("_Color1"), photoRenderer.material.GetColor("_Color2"), photoRenderer.material.GetColor("_Color3")});

        string serializedProject = JsonUtility.ToJson(currentProject);
        System.IO.File.WriteAllText(filename, serializedProject);
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