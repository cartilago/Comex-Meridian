using UnityEngine;
using UnityEngine.UI;
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

    private Project currentProject;
    private Texture2D photo;
    private Vector3 photoScale;
    private float photoAngle;
    #endregion

    #region MonoBehaviour overrides
    private void Start()
    {
        SetCurrentProject(new Project());
    }
    #endregion

    #region Class implementation
    public void SetCurrentProject(Project project)
    {
        currentProject = project;
        projectNameInputField.text = currentProject.name;

        Texture2D projectPhoto = project.GetPhoto();

        if (projectPhoto != null)
            SetPhoto(projectPhoto, project.photoSize, project.photoAngle);
    }

    public Project GetCurrentProject()
    {
        return currentProject;
    }

    public void SetProjectName(string name)
    {
        currentProject.name = name;
    }

    public void SetPhoto(Texture2D photo, Vector2 size, float angle)
    {
        this.photo = photo;
        currentProject.photoSize = size;
        currentProject.photoAngle = angle;
        photoRnderer.material.SetTexture("_MainTex", this.photo);
        photoRnderer.transform.localScale = size;
        photoRnderer.transform.localEulerAngles = new Vector3(0, 0, angle);
        Camera.main.orthographicSize = Screen.height / 2;
    }

    public void Hide()
    {
        topSection.gameObject.SetActive(false);
        bottomSection.gameObject.SetActive(false);
    }

    public void Show()
    {
        topSection.gameObject.SetActive(true);
        bottomSection.gameObject.SetActive(true);
    }

    #region IO
    public void SaveFile(string filename)
    {
        fileMenu.SetActive(false);

        if (photo != null)
            currentProject.SetPhoto(photo);

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