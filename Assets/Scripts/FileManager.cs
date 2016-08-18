using UnityEngine;
using System.Collections;
using System.IO;

public class FileManager : MonoBehaviour
{
    #region Class members
    public FileBrowser fileBrowser;
    public ConfirmDialog confirmDialog;
    #endregion

    #region Class implementation
    public void SaveProject()
    {
        string filename = Application.persistentDataPath + "/" + DecoratorPanel.Instance.GetCurrentProject().name + ".project";

        if (System.IO.File.Exists(filename) == true)
        {
            confirmDialog.SetTitle("El proyecto " + Path.GetFileNameWithoutExtension(filename) + " será sobre escrito.\n¿Estás seguro(a)?");
            confirmDialog.gameObject.SetActive(true);
            confirmDialog.onAccept.RemoveAllListeners();
            confirmDialog.onAccept.AddListener(() => AcceptedFileOverwriteDelegate(filename));
        }
        else
            DecoratorPanel.Instance.SaveFile(filename);
    }

    public void LoadProject()
    {
        fileBrowser.SetMode(EFileBrowserMode.Load);
        fileBrowser.gameObject.SetActive(true);
    }

    public void DeleteProject()
    {
        fileBrowser.SetMode(EFileBrowserMode.Delete);
        fileBrowser.gameObject.SetActive(true);
    }

    public void AcceptedFileOverwriteDelegate(string filename)
    {
        DecoratorPanel.Instance.SaveFile(filename);
    }
    #endregion
}
