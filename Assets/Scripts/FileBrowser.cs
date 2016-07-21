using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Meridian.Framework.Utils;

public enum EFileBrowserMode { Save, Load, Delete };

public class FileBrowser : MonoBehaviour
{
    #region Class members
    public Text title;
    public GameObject fileButtonPrefab;
    public Transform fileButtonsRoot;
    public ConfirmDialog confirmDialog;

    private List<GameObject> fileButtons = new List<GameObject>();
    private EFileBrowserMode mode;
    #endregion

    #region MonoBehaviour overrides
    private void OnEnable()
    {
        ClearFileButtons();
        GetFileButtons(Application.persistentDataPath);
    }
    #endregion

    #region Class implementation
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetMode(EFileBrowserMode mode)
    {
        this.mode = mode;

        switch (this.mode)
        {
            case EFileBrowserMode.Save: title.text = "Guardar proyecto";
                break;
            case EFileBrowserMode.Load: title.text = "Cargar proyecto";
                break;
            case EFileBrowserMode.Delete: title.text = "Borrar proyecto";
                break;
        }
    }

    private void ClearFileButtons()
    {
        for (int i = 0; i < fileButtons.Count; i++)
            Destroy(fileButtons[i]);

        fileButtons.Clear();
    }

    private void GetFileButtons(string path)
    {
        string[] files = Directory.GetFiles(path);

        foreach (string file in files)
            AddFileButton(file);
    }

    private void AddFileButton(string fileName)
    {
        GameObject go = GameObject.Instantiate(fileButtonPrefab);
        go.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(fileName);
        go.transform.SetParent(fileButtonsRoot);
        go.transform.localScale = Vector3.one;
        fileButtons.Add(go);

        switch (this.mode)
        {
            case EFileBrowserMode.Save:
                go.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonSaveDelegate(fileName));
                break;
            case EFileBrowserMode.Load:
                go.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonLoadDelegate(fileName));
                break;
            case EFileBrowserMode.Delete:
                go.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonDeleteDelegate(fileName));
                break;
        }
    }

    public void ButtonSaveDelegate(string filename)
    {
        confirmDialog.SetTitle("El proyecto " + Path.GetFileNameWithoutExtension(filename) + " será sobre escrito.\n¿Estás seguro(a)?");
        confirmDialog.gameObject.SetActive(true);
        confirmDialog.onAccept.RemoveAllListeners();
        confirmDialog.onAccept.AddListener(() => AcceptedFileOverwriteDelegate(filename));
    }

    public void ButtonLoadDelegate(string filename)
    {
        confirmDialog.SetTitle("¿Estás seguro(a) de querer cargar el proyecto " + Path.GetFileNameWithoutExtension(filename) + "?");
        confirmDialog.gameObject.SetActive(true);
        confirmDialog.onAccept.RemoveAllListeners();
        confirmDialog.onAccept.AddListener(() => AcceptedFileLoadDelegate(filename));
    }

    public void ButtonDeleteDelegate(string filename)
    {
        confirmDialog.SetTitle("Estás seguro(a) de querer borrar el proyecto " + Path.GetFileNameWithoutExtension(filename) + "?");
        confirmDialog.gameObject.SetActive(true);
        confirmDialog.onAccept.RemoveAllListeners();
        confirmDialog.onAccept.AddListener(() => AcceptedFileDeleteDelegate(filename));
    }

    public void AcceptedFileOverwriteDelegate(string filename)
    {
        Decorator.Instance.SaveFile(filename);
    }

    public void AcceptedFileLoadDelegate(string filename)
    {
        Decorator.Instance.LoadFile(filename);
    }

    public void AcceptedFileDeleteDelegate(string filename)
    {
        File.Delete(filename);
    }
    #endregion
}
