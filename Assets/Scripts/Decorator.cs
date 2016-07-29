using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Meridian.Framework.Utils;

public class Decorator :  MonoSingleton<Decorator>
{
    #region Class members
    public GameObject fileMenu;
    public GameObject topSection;
    public GameObject bottomSection;
    public InputField projectNameInputField;
    public Renderer photoRnderer;

    private float zoom;
    private Vector2 pan;

    public Texture2D startPhoto;

    private Project currentProject;
    //private Texture2D photo;

    private int currentTouch;
    private bool touchOrMouseDown;

    private DrawingToolBase currentDrawingTool;
    #endregion

    #region MonoBehaviour overrides
    private void Start()
    {
        SetCurrentProject(new Project());
        SetPhoto(startPhoto,0);
    }

    Touch fisrtTouch;
    Touch secondTouch;

    Vector2 startPos;
    bool panOrZoomStarted = false;

    private void Update()
    {
        // One finger input, drawing tools
        if (Input.touches.Length == 1)
        {
            if (currentDrawingTool == null)
                return;
        }

        // Two finger input, zoom & pan
        if (Input.touches.Length == 2 && panOrZoomStarted == false)
        {
            panOrZoomStarted = true;
        }



        // Touch input
        for (int i = 0; i < Input.touches.Length; i++)
        {
            if (Input.touches[i].phase == TouchPhase.Began)
            {
                // UI elements block mouse input
                if (EventSystem.current.IsPointerOverGameObject(i))
                    return;

                touchOrMouseDown = true;
                currentDrawingTool.TouchDown(Input.touches[i].position);
            }
            else if (Input.touches[i].phase == TouchPhase.Moved && touchOrMouseDown == true)
            {
                currentDrawingTool.TouchMove(Input.touches[i].position);
            }
            else if (Input.touches[i].phase == TouchPhase.Ended && touchOrMouseDown == true)
            {
                currentDrawingTool.TouchUp(Input.touches[i].position);
            }
        }

        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            // UI elements block mouse input
            if (EventSystem.current.IsPointerOverGameObject())
                return;
         
            touchOrMouseDown = true;
            currentDrawingTool.TouchDown(Input.mousePosition);
            return;
        }

        if (touchOrMouseDown == true)
        {
            currentDrawingTool.TouchMove(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && touchOrMouseDown == true)
        {
            currentDrawingTool.TouchUp(Input.mousePosition);
            touchOrMouseDown = false;
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
            SetPhoto(projectPhoto, project.photoAngle);
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

    public void SetPhoto(Texture2D photo, float angle)
    {
        currentProject.SetPhoto(photo);
        Vector2 photoSize = new Vector2(photo.width, photo.height);      
        float screenAspectRatio = (float)Screen.height / (float)Screen.width;
        float photoAspectRatio = photoSize.y / photoSize.x;
        photoRnderer.transform.localScale = photoSize;
        photoRnderer.material.SetTexture("_MainTex", photo);
        Camera.main.orthographicSize = GetBaseOrthographicSize(); // (photoSize.y / 2) * (screenAspectRatio / photoAspectRatio);

        currentProject.ClearDrawingActions();
    }

    public void Hide()
    {
        topSection.gameObject.SetActive(false);
        bottomSection.gameObject.SetActive(false);
        currentProject.Hide();
        photoRnderer.gameObject.SetActive(false);
    }

    public void Show()
    {
        topSection.gameObject.SetActive(true);
        bottomSection.gameObject.SetActive(true);
        currentProject.Show();
        photoRnderer.gameObject.SetActive(true);
    }

    #region Paint tools
    public void SetCurrentDrawingTool(DrawingToolBase drawingTool)
    {
        currentDrawingTool = drawingTool;
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