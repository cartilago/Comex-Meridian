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
}
