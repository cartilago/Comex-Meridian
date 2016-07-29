using UnityEngine;
using System.Collections;


public class FloodFillPainter : MonoBehaviour {

    public Vector2 fillPoint;
    private Color32 byteColorBuffer;
    private Texture2D texture;

    private bool fill = false;

    // Use this for initialization
    /*
	void Start ()
    {
        Texture2D tx;
        texture = GetComponent<MeshRenderer>().material.mainTexture as Texture2D;
        tx = FloodFill.HSVFill(texture, (int)fillPoint.x, (int)fillPoint.y, Color.blue, .02f);
        tx.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_TintMask", tx);
    }*/

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && fill == true)
        {
            fill = false;
            Vector2 normalizedPos = new Vector2(Input.mousePosition.x / (float)Screen.width, Input.mousePosition.y / (float)Screen.height);
            FillAt(normalizedPos);
        }
    }

    public void PrepareToFill()
    {
        fill = true;
    }

    public void FillAt(Vector2 pos)
    {
        Texture2D tx;
        texture = GetComponent<MeshRenderer>().material.mainTexture as Texture2D;

        tx = FloodFill.HSVFill(texture, (int)((1 - pos.y) * texture.width), (int)((1 - pos.x) * texture.height), Color.blue, .03f, 0.9f, 1);
        tx.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_TintMask", tx);

        Debug.Log("Filling at ");
    }
}
