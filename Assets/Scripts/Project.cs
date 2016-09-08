using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Project
{
    #region Class members
    public string name;
    public string encodedPhoto;
    public string encodedMask;

    public Color[] colors;
    public string[] colorNames;

    [System.NonSerialized]
    public Texture2D photo;
    #endregion

    #region Class implementation
    public Project()
    {
        name = "Tu proyecto Meridian";
        colors = new Color[3];
        colorNames = new string[3];
    }

    public void SetPhoto(Texture2D photo)
    {
        this.photo = photo;
    }

    public Texture2D GetPhoto()
    {
        return photo;
    }

    public void SetEncodedPhoto(Texture2D photo, Texture2D mask)
    {
        encodedPhoto = Texture2DToString(photo);
        encodedMask = Texture2DToString(mask);
    }

    public Texture2D GetEncodedPhoto()
    {
        if (encodedPhoto != null)
            return Texture2DFromString(encodedPhoto);

        return null;
    }

	public Texture2D GetEncodedMask()
    {
        if (encodedMask != null)
            return Texture2DFromString(encodedMask);

        return null;
    }

    public void SetColors(ColorWidget[] colorWidgets)
    {
        this.colors = new Color[3];
        this.colorNames = new string[3];

        for (int i = 0; i < 3; i++)
        {
            if (colorWidgets[i] != null)
            {
                this.colors[i] = colorWidgets[i].color;
                this.colorNames[i] = colorWidgets[i].label.text;
            }
        }
    }

	public Color[] GetColors()
    {
        	return colors;
    }

    public string[] GetColorNames()
    {
        return colorNames;
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
