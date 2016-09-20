using UnityEngine;
using System.Collections;

public class OneBitChannelImage
{
    public int width;
    public int height;
    public byte[] data;

    public OneBitChannelImage(int width, int height)
    {
        this.width = width;
        this.height = height;
        data = new byte[width * height];
    }

    static public OneBitChannelImage FromTexture2D(Texture2D texture)
    {
        OneBitChannelImage ret = new OneBitChannelImage(texture.width, texture.height);

        Color32[] pixels = texture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
            ret.data[i] = (byte)(((pixels[i].r > 0) ? 1 : 0) | ((pixels[i].g > 0) ? 2 : 0) | ((pixels[i].g > 0) ? 2 : 0) | ((pixels[i].b > 0) ? 2 : 0));

        return ret;
    }

    public Texture2D ToTexture2D()
    {
        Texture2D ret = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color32[] pixels = new Color32[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            pixels[i].r = (byte)((data[i] & (byte)1) > 0 ? 0 : 255);
            pixels[i].g = (byte)((data[i] & (byte)2) > 0 ? 0 : 255);
            pixels[i].b = (byte)((data[i] & (byte)4) > 0 ? 0 : 255);
            pixels[i].a = (byte)((data[i] & (byte)8) > 0 ? 0 : 255);
        }

        ret.SetPixels32(pixels);
        ret.Apply();

        return ret;
    }
}