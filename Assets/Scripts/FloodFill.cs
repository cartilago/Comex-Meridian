using UnityEngine;
using System.Collections;
using System.Collections.Generic;

static public class FloodFill
{
    public static Texture2D Fill(this Texture2D texture, int startX, int startY, Color newColor)
    {
        Texture2D tx = new Texture2D(texture.width, texture.height);
        tx.SetPixels(texture.GetPixels(), 0);

        Point start = new Point(startX, TransformToLeftTop_y(startY, tx.height));

        Flat2DArray copyBmp = new Flat2DArray(tx.height, tx.width, tx.GetPixels());

        Color originalColor = tx.GetPixel(start.X, start.Y);
        int width = tx.width;
        int height = tx.height;

        if (originalColor == newColor)
        {
            return null;
        }

        copyBmp[start.X, start.Y] = newColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        int i = 0;

        // TODO: remove this
        // emergency switch so it doesn't hang if something goes wrong
        int emergency = width * height;

        while (openNodes.Count > 0)
        {
            i++;

            if (i > emergency)
            {
                return null;
            }

            Point current = openNodes.Dequeue();
            int x = current.X;
            int y = current.Y;

            if (x > 0)
            {
                if (copyBmp[x - 1, y] == originalColor)
                {
                    copyBmp[x - 1, y] = newColor;
                    openNodes.Enqueue(new Point(x - 1, y));
                }
            }
            if (x < width - 1)
            {
                if (copyBmp[x + 1, y] == originalColor)
                {
                    copyBmp[x + 1, y] = newColor;
                    openNodes.Enqueue(new Point(x + 1, y));
                }
            }
            if (y > 0)
            {
                if (copyBmp[x, y - 1] == originalColor)
                {
                    copyBmp[x, y - 1] = newColor;
                    openNodes.Enqueue(new Point(x, y - 1));
                }
            }
            if (y < height - 1)
            {
                if (copyBmp[x, y + 1] == originalColor)
                {
                    copyBmp[x, y + 1] = newColor;
                    openNodes.Enqueue(new Point(x, y + 1));
                }
            }
        }

        tx.SetPixels(copyBmp.data);
        return tx;
    }



    public static Texture2D HSVFill(this Texture2D texture, int startX, int startY, Color newColor, float hueTolerance, float saturationTolerance, float valueTolerance)
    {
        Texture2D tx = new Texture2D(texture.width, texture.height);
        tx.SetPixels(texture.GetPixels(), 0);

        Point start = new Point(startX, TransformToLeftTop_y(startY, tx.height));

        Flat2DArray copyBmp = new Flat2DArray(tx.height, tx.width, tx.GetPixels());
        Flat2DArray mask = new Flat2DArray(tx.height, tx.width, new Color[tx.width * tx.height]);

        Color originalColor = tx.GetPixel(start.X, start.Y);
        HSVColor originalHSV = HSVColor.FromRGBA(originalColor.r, originalColor.g, originalColor.b, 1);
        int width = tx.width;
        int height = tx.height;

        if (originalColor == newColor)
        {
            return null;
        }

        copyBmp[start.X, start.Y] = newColor;

        Queue<Point> openNodes = new Queue<Point>();
        openNodes.Enqueue(start);

        int i = 0;

        // TODO: remove this
        // emergency switch so it doesn't hang if something goes wrong
        int emergency = width * height;

        while (openNodes.Count > 0)
        {
            i++;

            if (i > emergency)
            {
                return null;
            }

            Point current = openNodes.Dequeue();
            int x = current.X;
            int y = current.Y;
           
            if (x > 0)
            {
                HSVColor hsvColor = copyBmp.GetHSV(x - 1, y);

                if (Mathf.Abs(hsvColor.h - originalHSV.h) < hueTolerance && 
                    Mathf.Abs(hsvColor.s - originalHSV.s) < saturationTolerance && 
                    Mathf.Abs(hsvColor.v - originalHSV.v) < valueTolerance)
                {
                    copyBmp[x - 1, y] = newColor;
                    mask[x - 1, y] = Color.white;
                    openNodes.Enqueue(new Point(x - 1, y));
                }
            }
            if (x < width - 1)
            {
                HSVColor hsvColor = copyBmp.GetHSV(x + 1, y);

                if (Mathf.Abs(hsvColor.h - originalHSV.h) < hueTolerance &&
                     Mathf.Abs(hsvColor.s - originalHSV.s) < saturationTolerance &&
                     Mathf.Abs(hsvColor.v - originalHSV.v) < valueTolerance)
                {
                    copyBmp[x + 1, y] = newColor;
                    mask[x + 1, y] = Color.white;
                    openNodes.Enqueue(new Point(x + 1, y));
                }
            }
            if (y > 0)
            {
                HSVColor hsvColor = copyBmp.GetHSV(x, y - 1);

                if (Mathf.Abs(hsvColor.h - originalHSV.h) < hueTolerance &&
                    Mathf.Abs(hsvColor.s - originalHSV.s) < saturationTolerance &&
                    Mathf.Abs(hsvColor.v - originalHSV.v) < valueTolerance)
                {
                    copyBmp[x, y - 1] = newColor;
                    mask[x, y -1 ] = Color.white;
                    openNodes.Enqueue(new Point(x, y - 1));
                }
            }
            if (y < height - 1)
            {
                HSVColor hsvColor = copyBmp.GetHSV(x, y + 1);

                if (Mathf.Abs(hsvColor.h - originalHSV.h) < hueTolerance &&
                    Mathf.Abs(hsvColor.s - originalHSV.s) < saturationTolerance &&
                    Mathf.Abs(hsvColor.v - originalHSV.v) < valueTolerance)
                {
                    copyBmp[x, y + 1] = newColor;
                    mask[x, y + 1] = Color.white;
                    openNodes.Enqueue(new Point(x, y + 1));
                }
            }
        }

        tx.SetPixels(mask.data);
        return tx;
    }

    private class Flat2DArray
    {
        public Color[] data;
        private readonly int height;
        private readonly int width;

        public Flat2DArray(int height, int width, Color[] data)
        {
            this.height = height;
            this.width = width;

            this.data = data;
        }

        public Color this[int x, int y]
        {
            get
            {
                return data[x + y * width];
            }
            set
            {
                data[x + y * width] = value;
            }
        }

        public HSVColor GetHSV(int x, int y)
        {
            Color col = data[x + y * width];
            return HSVColor.FromRGBA(col.r, col.g, col.b, col.a);
        }

    }

    private struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// Transforms a point in the texture plane so that 0,0 points at left-top corner.</summary>
    private static int TransformToLeftTop_y(int y, int height)
    {
        return height - y;
    }

    /// <summary>
    /// Transforms a point in the texture plane so that 0,0 points at left-top corner.</summary>
    private static int TransformToLeftTop_y(float y, int height)
    {
        return height - (int)y;
    }
}
