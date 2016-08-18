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
        if (currentTool == null)
            return;

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
            return;
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

        Debug.Log("Current project drawing actions " + currentProject.drawingActions.Count);

        if (projectPhoto != null)
            SetPhoto(projectPhoto);
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
        Vector2 photoSize = new Vector2(currentProject.GetPhoto().width, currentProject.GetPhoto().height);
        float screenAspectRatio = (float)Screen.height / (float)Screen.width;
        float photoAspectRatio = photoSize.y / photoSize.x;
        return (photoSize.y / 2) * (screenAspectRatio / photoAspectRatio);
    }

    public void SetPhoto(Texture2D photo)
    {
    	/*
		Color32[] pixels = photo.GetPixels32();
		pixels = Color32Utils.ConvertToHSV(pixels);
		Texture2D hsvTexture = new Texture2D(photo.width, photo.height);
		hsvTexture.SetPixels32(pixels);
		hsvTexture.Apply();*/

        currentProject.SetPhoto(photo);
        Vector2 photoSize = new Vector2(photo.width, photo.height);      
        float screenAspectRatio = (float)Screen.height / (float)Screen.width;
        float photoAspectRatio = photoSize.y / photoSize.x;
		canvasRenderer.transform.localScale = photoRenderer.transform.localScale = photoSize; 
		canvasCamera.orthographicSize = photoCamera.orthographicSize = GetBaseOrthographicSize();
		canvasCamera.aspect = photoCamera.aspect;
		photoRenderer.material.SetTexture("_MainTex", photo);
		//photoRenderer.material.SetTexture("_MainTex", hsvTexture);

        currentProject.ClearDrawingActions();
    }

    public void Hide()
    {
        topSection.gameObject.SetActive(false);
        bottomSection.gameObject.SetActive(false);
        currentProject.Hide();
        photoRenderer.gameObject.SetActive(false);
    }

    public void Show()
    {
        topSection.gameObject.SetActive(true);
        bottomSection.gameObject.SetActive(true);
        currentProject.Show();
        photoRenderer.gameObject.SetActive(true);
    }

    #region Paint tools
    public void SetCurrentTool(DrawingToolBase drawingTool)
    {
        currentTool = drawingTool;
    }

    public void Undo()
    {
        currentProject.RemoveLastDrawingAction();
    }

    #endregion

    #region IO
    public void SaveFile(string filename)
    {
        fileMenu.SetActive(false);

        if (currentProject.GetPhoto() != null)
            currentProject.SetEncodedPhoto(currentProject.GetPhoto());

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