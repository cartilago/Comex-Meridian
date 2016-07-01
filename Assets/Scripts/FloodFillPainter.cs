using UnityEngine;
using System.Collections;


public class FloodFillPainter : MonoBehaviour {

    public Vector2 fillPoint;
    private Color32 byteColorBuffer;
    private Texture2D texture;

	// Use this for initialization
	void Start ()
    {
        Texture2D tx;
        texture = GetComponent<MeshRenderer>().material.mainTexture as Texture2D;
        tx = FloodFill.HSVFill(texture, (int)fillPoint.x, (int)fillPoint.y, Color.blue, .02f);
        tx.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_TintMask", tx);
    }
}
