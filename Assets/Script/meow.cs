using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class meow : MonoBehaviour
{
    public int sliceAngle = 180;
    public MeshFilter outline;
    public MeshFilter interior;

    Mesh mesh;
    Mesh[] meshList;
    private Vector3[] outlineVertices;
    private Vector3[] vertices;
    private Color[] colours;
    private int[] triangles;
    public bool drawGizmos;
    private int strength = 24;
    public int sine = 1;
    public Text text;
    public int outlineLength;
    public Gradient gradient;
    private int counter = 0;
    public GameObject UIface;
    public Camera mainCamera;

    private List<Vector3> verticeList = new List<Vector3>();
    private float gridSize;
    private int dilution;
    private bool startStop = false;
    private float time = 0;
    public float thinging = 0.2f;
    private List<int> disconnections = new List<int>();
    private string[] fileNames;
    private int whichFile = 0;

    void Start()
    {
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = new Color32(0xD2, 0xE3, 0xF3, 0xFF); // light blue on initial load
        }
        if (Directory.Exists("Assets/Resources/MESHES")){

            //Find the interior
            fileNames = Directory.GetFiles("Assets/Resources/MESHES/RESULTS_0/", "*.asset");
            fileNames = fileNames.OrderBy(s => int.Parse(s.Split("/")[s.Split("/").Length - 1].Split(".")[0])).ToArray();
            Debug.Log("MESHES/RESULTS_0/" + fileNames[0].Split("/")[0].Split(".")[0]);

            mesh = Resources.Load<Mesh>("MESHES/RESULTS_0/"+ fileNames[0].Split("/")[fileNames[0].Split("/").Length-1].Split(".")[0]);
            Debug.Log("Found saved outline: " + mesh.name);
            Debug.Log("Outline length: " + mesh.vertices);
            outlineVertices = mesh.vertices;
            vertices = mesh.vertices;
            colours = mesh.colors;
            outlineLength = vertices.Length;

            //so set our mesh to an empty mesh, then run the two following functions to add the points and edges to the outline.
            mesh = new Mesh();
            wrapOutline();
            trianglesWrap();

            mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.colors = colours;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            outline.mesh = mesh;



            //Code being used to render the interior
            fileNames = Directory.GetFiles("Assets/Resources/MESHES/INTERIOR_0/", "*.asset");
            fileNames = fileNames.OrderBy(s => int.Parse(s.Split("/")[s.Split("/").Length - 1].Split(".")[0])).ToArray();
            //Debug.Log("MESHES/RESULTS_0/" + fileNames[0].Split("/")[0].Split(".")[0]);

            mesh = Resources.Load<Mesh>("MESHES/INTERIOR_0/" + fileNames[0].Split("/")[fileNames[0].Split("/").Length - 1].Split(".")[0]);
            Debug.Log("Found saved outline: " + mesh.name);
            outlineVertices = mesh.vertices;
            vertices = mesh.vertices;
            colours = mesh.colors;
            triangles = mesh.triangles;

            outlineLength = vertices.Length;
            //

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.colors = colours;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            interior.mesh = mesh;
        }
    }

    private void Update()
    {

        //Go through each timestep
        if (Input.GetKeyUp(KeyCode.N))
        {
            whichFile++;
            if (whichFile > fileNames.Length-1)
            {
                whichFile = 0;
            }
            //Find the interior
            fileNames = Directory.GetFiles("Assets/Resources/MESHES/RESULTS_0/", "*.asset");
            fileNames = fileNames.OrderBy(s => int.Parse(s.Split("/")[s.Split("/").Length - 1].Split(".")[0])).ToArray();
            Debug.Log("MESHES/RESULTS_0/" + fileNames[0].Split("/")[0].Split(".")[0]);

            mesh = Resources.Load<Mesh>("MESHES/RESULTS_0/" + fileNames[whichFile].Split("/")[fileNames[whichFile].Split("/").Length - 1].Split(".")[0]);
            Debug.Log("Found saved outline: " + mesh.name);
            Debug.Log("Outline length: " + mesh.vertices);
            outlineVertices = mesh.vertices;
            vertices = mesh.vertices;
            colours = mesh.colors;
            outlineLength = vertices.Length;

            mesh = new Mesh();

            UIface.SetActive(false);
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = new Color(0f, 0f, 0.2f); // RGB 0,0,0.2 = dark navy
            }

            //createOutline();

            wrapOutline();
            trianglesWrap();
            mesh.vertices = vertices;
            mesh.colors = colours;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            outline.mesh = mesh;


            //Code being used to render the interior
            fileNames = Directory.GetFiles("Assets/Resources/MESHES/INTERIOR_0/", "*.asset");
            fileNames = fileNames.OrderBy(s => int.Parse(s.Split("/")[s.Split("/").Length - 1].Split(".")[0])).ToArray();
            //Debug.Log("MESHES/RESULTS_0/" + fileNames[0].Split("/")[0].Split(".")[0]);

            mesh = Resources.Load<Mesh>("MESHES/INTERIOR_0/" + fileNames[whichFile].Split("/")[fileNames[0].Split("/").Length - 1].Split(".")[0]);
            Debug.Log("Found saved outline: " + mesh.name);
            outlineVertices = mesh.vertices;
            vertices = mesh.vertices;
            colours = mesh.colors;
            triangles = mesh.triangles;

            outlineLength = vertices.Length;
            //

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.colors = colours;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            interior.mesh = mesh;

        }
    }

    void wrapOutline()
    {
        int count = 0;
        strength = 24/(int)Mathf.Floor(360/sliceAngle) + 1;
        //so for 180 degrees out of 24, we need 13 rather than 12, as we are plotting both poles
        disconnections = new List<int>();
        Vector3[] verticesF = new Vector3[outlineLength * strength];
        for (int i = 0; i < outlineLength; i++)
        {
            float angle = 0;
            if ((int)vertices[i].x != 0)
            {
                //find all disconnections
                disconnections.Add(i);
            }
            float y = vertices[i].y;
            float z = vertices[i].z;

            for (int j = 0; j < (strength); j++)
            {
                //angle += 2 * Mathf.PI / (strength);
                verticesF[count] = new Vector3(Mathf.Cos(angle) * z, y, Mathf.Sin(angle) * z);
                angle += 2 * Mathf.PI / (24);
                count++;
            }
        }
        vertices = verticesF;
        //outlineLength = vertices.Length;
        Color[] coloursM = new Color[vertices.Length];
        Debug.Log("Vertice length after wrap: "+vertices.Length);
        for (int i = 0; i < coloursM.Length; i++)
        {
            coloursM[i] = colours[Mathf.FloorToInt(i / strength)];//gradient.Evaluate(Mathf.Floor(i / strength) / outlineLength);
        }
        colours = coloursM;
    }


    void trianglesWrap()
    {
        Debug.Log("Repeat " + ((outlineLength - 1) * strength * 6) + " times");
        Debug.Log("Highest Vertice should be: " + (strength * outlineLength));
        triangles = new int[(outlineLength - 1) * strength * 6];

        for (int i = 0; i < outlineLength - 1; i++)
        {
            for (int j = 0; j < strength - 1; j++)
            {
                if (!disconnections.Contains(i))
                {
                    // .   .  -- j+(i*strength), j+(i*strength)+1
                    // .   .  -- j+((i+1)*strength), j+((i+1)*strength)+1
                    triangles[6 * j + 6 * strength * i] = j + strength * i;
                    triangles[6 * j + 1 + 6 * strength * i] = j + 1 + strength * i;
                    triangles[6 * j + 2 + 6 * strength * i] = j + 1 + strength * (i + 1);

                    triangles[6 * j + 3 + 6 * strength * i] = j + strength * i;
                    triangles[6 * j + 4 + 6 * strength * i] = j + 1 + strength * (i + 1);
                    triangles[6 * j + 5 + 6 * strength * i] = j + strength * (i + 1);
                }
            }
        }
    }

}

