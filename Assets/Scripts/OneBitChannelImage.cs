using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        // Encode rgba channels into bits 1 2 3 & 4
        for (int i = 0; i < pixels.Length; i++)
            ret.data[i] = (byte)(((pixels[i].r > 0) ? 1 : 0) | ((pixels[i].g > 0) ? 2 : 0) | ((pixels[i].b > 0) ? 4 : 0) | ((pixels[i].a > 0) ? 8 : 0));

        // Now compress data
		ret.data = Compress(ret.data);
        return ret;
    }

    public Texture2D ToTexture2D()
    {
        Texture2D ret = new Texture2D(width, height, TextureFormat.RGBA32, false);

        byte[] uncompressedData = Decompress(data);

		Color32[] pixels = new Color32[uncompressedData.Length];

		for (int i = 0; i < uncompressedData.Length; i++)
        {
			pixels[i].r = (byte)((uncompressedData[i] & (byte)1) > 0 ? 255 : 0);
			pixels[i].g = (byte)((uncompressedData[i] & (byte)2) > 0 ? 255 : 0);
			pixels[i].b = (byte)((uncompressedData[i] & (byte)4) > 0 ? 255 : 0);
			pixels[i].a = (byte)((uncompressedData[i] & (byte)8) > 0 ? 255 : 0);
        }

        ret.SetPixels32(pixels);
        ret.Apply();

        return ret;
    }

	private static byte[] Compress(byte[] source)
    {
        List<byte> dest = new List<byte>();
        byte runLength;

        for (int i = 0; i < source.Length; i++)
        {
            runLength = 1;
            while (runLength < byte.MaxValue 
                && i + 1 < source.Length 
                && source[i] == source[i + 1])
            {
                runLength++;
                i++;
            }
            dest.Add(runLength);
            dest.Add(source[i]);
        }

        return dest.ToArray();
    }

    private static byte[] Decompress(byte[] source)
    {
        List<byte> dest = new List<byte>();
        byte runLength; 

        for (int i = 1; i < source.Length; i+=2)
        {
            runLength = source[i - 1];

            while (runLength > 0)
            {
                dest.Add(source[i]);
                runLength--;
            }
        }
        return dest.ToArray();
    }
}