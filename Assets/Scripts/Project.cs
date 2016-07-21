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
    public List<Vector2> floodFillPoints = new List<Vector2>();
    #endregion

    #region Class implementation
    public Project()
    {
        name = "Tu proyecto Meridian";
    }

    public void AddFlodFillPoint(Vector2 point)
    {
        floodFillPoints.Add(point);
        UpdateImage();
    }

    public void SetPhoto(Texture2D photo)
    {
        encodedPhoto = Texture2DToString(photo);
    }

    public Texture2D GetPhoto()
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
