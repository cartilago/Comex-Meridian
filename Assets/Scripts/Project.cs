using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Project
{
    #region Class members
    public string name;
    public string encodedPhoto;
    public Vector2 photoSize;
    public float photoAngle;
    public List<DrawingActionBase> drawingActions = new List<DrawingActionBase>();

    [System.NonSerialized]
    public Texture2D photo;
    #endregion

    #region Class implementation
    public Project()
    {
        name = "Tu proyecto Meridian";
    }

    public void Hide()
    {
        for (int i = 0; i < drawingActions.Count; i++)
            drawingActions[i].gameObject.SetActive(false);
    }

    public void Show()
    {
        for (int i = 0; i < drawingActions.Count; i++)
            drawingActions[i].gameObject.SetActive(true);
    }

    public void AddDrawingAction(DrawingActionBase drawingAction)
    {
        drawingActions.Add(drawingAction);
    }

    public void RemoveLastDrawingAction()
    {
        DrawingActionBase lastDrawingAction = drawingActions[drawingActions.Count - 1];

        drawingActions.Remove(lastDrawingAction);
        GameObject.Destroy(lastDrawingAction.gameObject);
    }

    public void ClearDrawingActions()
    {
        for (int i = 0; i < drawingActions.Count; i++)
            GameObject.Destroy(drawingActions[i].gameObject);

        drawingActions.Clear();
    }

    public void SetPhoto(Texture2D photo)
    {
        this.photo = photo;
        ClearDrawingActions();
    }

    public Texture2D GetPhoto()
    {
        return photo;
    }

    public void SetEncodedPhoto(Texture2D photo)
    {
        encodedPhoto = Texture2DToString(photo);
    }

    public Texture2D GetEncodedPhoto()
    {
        if (encodedPhoto != null)
            return Texture2DFromString(encodedPhoto);

        return null;
    }

    public void UpdateImage()
    {
    }

    static public string Texture2DToString(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        return System.Convert.ToBase64String(bytes);
    }

    static public Texture2D Texture2DFromString(string base64EncodedData)
    {
        byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(base64EncodedBytes);
        return texture;
    }
    #endregion
}
