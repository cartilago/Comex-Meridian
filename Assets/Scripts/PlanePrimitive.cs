using UnityEngine;
using System.Collections;

//___________________________________________________________
// PlanePrimitive:
// Created by: Jorge Luis Chávez Herrera
//___________________________________________________________

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]

[ExecuteInEditMode]
public class PlanePrimitive : MonoBehaviour {
	
	public int columns = 1;
	public int rows = 1;
	public float width = 1;
	public float height = 1;
	public Vector3 offset = Vector3.zero;
	public Vector3 orientation = new Vector3 (0,0,0);
	public Color verticesColor = Color.white;
		
	private Mesh planeMesh;
		
	void Awake() {
		planeMesh = ((MeshFilter)transform.GetComponent("MeshFilter")).sharedMesh;
		CreateMesh();
		this.useGUILayout = false;
	}
	
		
	public void CreateMesh() {
		
		if (planeMesh == null) {
			// Create a new mesh and assign it to the mesh filter
			planeMesh = new Mesh ();
			planeMesh.name = "PlaneMesh";
			((MeshFilter)transform.GetComponent("MeshFilter")).mesh = planeMesh;
		}
		
		// Allocate memory for buffers
		// All the arrays must be the same size
		Vector3[] vertices = new Vector3[(rows+1) * (columns+1)];  		
		Vector2[] uvs = new Vector2[vertices.Length];
		Vector3[] normals = new Vector3[vertices.Length]; 
		Color[] colors = new Color[vertices.Length];
		int[] triangles = new int[(rows * columns)*6];
		
		// Create vertices 
		float w = width / columns;
		float h = height / rows;
		
		Matrix4x4 trs = Matrix4x4.TRS(offset,Quaternion.Euler(orientation),Vector3.one);
					
		int index = 0;
		for (int y = 0; y < rows+1; y++)
		for (int x = 0; x < columns+1; x++, index++) {
			
			vertices[index] = trs.MultiplyPoint( new Vector3((x*w) - (width * 0.5f), (y*h) - (height * 0.5f),0) ); // Create Vertices 
			uvs[index] = new Vector2((x*w) / width, (y*h) / height); // Create UVs
			normals[index] = trs.MultiplyVector(new Vector3(0,0,-1)); // Create Normals
			colors[index] = new Color(Random.value,Random.value,Random.value,Random.value);
		}
	
		// Create Triangles
		index = 0;
		
		for (int y = 0; y < rows; y++)
		for (int x = 0; x < columns ; x++, index+=6)
		{
			triangles[index] = (y * (columns+1)) + x+1;
			triangles[index+1] = (y * (columns+1)) + x; 
			triangles[index+2] = ((y+1) * (columns+1)) + x;
			
			triangles[index+3] = (y * (columns+1)) + x+1;
			triangles[index+4] = ((y+1) * (columns+1)) + x; 
			triangles[index+5] = ((y+1) * (columns+1)) + x+1; 
		}
		
		planeMesh.Clear();	
		planeMesh.vertices = vertices;
		planeMesh.uv = uvs;
		planeMesh.normals = normals;
		planeMesh.triangles = triangles;
		planeMesh.colors = colors;
		planeMesh.RecalculateBounds();
		planeMesh.Optimize();
	}
}