/// <summary>
/// Color32Utils.
/// A set of functions for transforming Color32 arrays.
/// 
/// By Jorge L. Chavez Herrera
/// </summary>
using UnityEngine;
using System.Collections;

public static class Color32Utils
{
    /// <summary>
    /// Flips horizontally a Color32 linear array.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    static public Color32[] FlipColorArrayHorizontally(Color32[] array, int width, int height)
    {
        Color32[] flippedArray = new Color32[array.Length];

        int sourceIndex = 0;
        int destIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sourceIndex = (y * width) + ((width - 1) - x);
                flippedArray[destIndex] = array[sourceIndex];
                destIndex++;
            }
        }

        return flippedArray;
    }

    /// <summary>
    /// Flips vertically a Color32 linear array 90.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    static public Color32[] FlipColorArrayVertically(Color32[] array, int width, int height)
    {
        Color32[] flippedArray = new Color32[array.Length];

        int sourceIndex = 0;
        int destIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sourceIndex = (((height - 1) - y) * width) + x;
                flippedArray[destIndex] = array[sourceIndex];
                destIndex++;
            }
        }

        return flippedArray;
    }

    /// <summary>
    /// Rotates a Color32 linear array 90 degrees to the left.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    static public Color32[] RotateColorArrayLeft(Color32[] array, int width, int height)
    {
        Color32[] rotatedArray = new Color32[array.Length];

        int sourceIndex = 0;
        int destIndex = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                sourceIndex = (y * width) + ((width - 1) - x);
                rotatedArray[destIndex] = array[sourceIndex];
                destIndex++;
            }
        }

        return rotatedArray;
    }

    /// <summary>
    /// /// Rotates a Color32 linear array 90 degrees to the right.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    static public Color32[] RotateColorArrayRight(Color32[] array, int width, int height)
    {
        Color32[] rotatedArray = new Color32[array.Length];

        int sourceIndex = 0;
        int destIndex = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                sourceIndex = (((height - 1) * width) - (y * width)) + x;
                rotatedArray[destIndex] = array[sourceIndex];
                destIndex++;
            }
        }

        return rotatedArray;
    }

    /// <summary>
    /// Converts all colors to ther HSV representation.
    /// </summary>
    /// <returns>The to HS.</returns>
    /// <param name="array">Array.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
	static public Color[] ConvertToHSV(Color[] array)
	{
		Color[] convertedArray = new Color[array.Length];

		for (int i = 0; i < array.Length; i++)
		{
			Color c = array[i];

			Vector4 K = new Vector4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
			Vector4 p = c.g < c.b ? new Vector4(c.b, c.g, K.w, K.z) : new Vector4(c.g, c.b, K.x, K.y);
			Vector4 q = c.r < p.x ? new Vector4(p.x, p.y, p.w, c.r) : new Vector4(c.r, p.y, p.z, p.x);

	    	float d = q.x - Mathf.Min(q.w, q.y);
	    	float e = 1.0e-10f;
	    
			convertedArray[i] = new Color (Mathf.Abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x, c.a);
		}

		return convertedArray;
	}

	static public Color ConvertToHSV(Color c)
	{
		Vector4 K = new Vector4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
		Vector4 p = c.g < c.b ? new Vector4(c.b, c.g, K.w, K.z) : new Vector4(c.g, c.b, K.x, K.y);
		Vector4 q = c.r < p.x ? new Vector4(p.x, p.y, p.w, c.r) : new Vector4(c.r, p.y, p.z, p.x);

    	float d = q.x - Mathf.Min(q.w, q.y);
    	float e = 1.0e-10f;
    
		return new Color(Mathf.Abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x, c.a);
	}
}

public class ColorBuffer
{
	public Color[] data;
	public readonly int height;
	public readonly int width;

	public ColorBuffer(int width, int height, Color[] colorData)
	{
		this.width = width;
		this.height = height;
		this.data = colorData;
	}

	public Color this[int x, int y]
	{
		get
		{
			return data[(y * width) + x];
		}
		set
		{
			data[(y * width) + x] = value;
		}
	}
}
