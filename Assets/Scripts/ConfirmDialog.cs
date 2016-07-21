using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Meridian.Framework.Utils;

public class ConfirmDialog : MonoBehaviour
{
    #region Class members
    public Text title;
    public FileBrowser fileBrowser;
    [System.NonSerialized]
    public UnityEvent onAccept = new UnityEvent();
    #endregion

    #region Class implementation
    public void SetTitle(string title)
    {
        this.title.text = title;
    }

    public void Accept()
    {
        onAccept.Invoke();
        gameObject.SetActive(false);
        fileBrowser.Hide();
    }

	public void Cancel()
    {
        gameObject.SetActive(false);
    }
    #endregion
}
