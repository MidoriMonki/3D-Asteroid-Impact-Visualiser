using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class meow : MonoBehaviour
{
    public int sliceAngle = 180;
    public MeshFilter outline;

    public MeshFilter outline2;
    public MeshFilter interior;
    public MeshFilter interior2;


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


    public Camera mainCamera;


    public Gradient gradient2;
    private int counter = 0;

    private List<Vector3> verticeList = new List<Vector3>();
    private float gridSize;
    private int dilution;
    private bool startStop = false;
    private float time = 0;
    public float thinging = 0.2f;
    private List<int> disconnections = new List<int>();
    private string[] fileNames;
    private int whichFile = 0;

    private string name = "meow";

    void OnEnable()
    {
        Debug.Log("meow enabled!");
        //string path = "Assets/Resources/MESHES";
        string dirPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"AIV_3D/{name}");
        if (Directory.Exists(dirPath))
        {
            Debug.Log("Directory exists: " + dirPath);
            loadOutline(0);
            loadInterior(0);
        }
        else
        {
            Debug.LogWarning("Directory does NOT exist: " + dirPath);
        }
    }

    private void Update(){
        //Go through each timestep
        if (Input.GetKeyUp(KeyCode.N)){
            whichFile++;
            if (whichFile > fileNames.Length-1){
                whichFile = 0;
            }
            loadOutline(whichFile);
            loadInterior(whichFile);
        }
    }

    void loadOutline(int n){
        //Find the Outline
        string dirPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"AIV_3D/{name}/OUTLINE");
        fileNames = Directory.GetFiles(dirPath, "*.json");

        fileNames = fileNames
            .OrderBy(s => int.Parse(Path.GetFileNameWithoutExtension(s)))
            .ToArray();

        string json = File.ReadAllText(fileNames[n]);
        MeshSaveData foundMesh = JsonUtility.FromJson<MeshSaveData>(json);

        Mesh mesh = new Mesh();
        vertices = foundMesh.vertices.ToArray();
        colours = foundMesh.colours.ToArray();
        outlineLength = vertices.Length;
        
        //mesh = Resources.Load<Mesh>("MESHES/RESULTS_0/"+ fileNames[n].Split("/")[fileNames[0].Split("/").Length-1].Split(".")[0]);
        Debug.Log("Found saved outline: " + mesh.name);
        Debug.Log("Outline length: " + mesh.vertices);

        /*
        outlineVertices = mesh.vertices;
        vertices = mesh.vertices;
        colours = mesh.colors;
        outlineLength = vertices.Length;
        */
        wrapOutline();
        trianglesWrap();

        mesh.vertices = vertices;
        //Get colours through gradient
        /*
        for (int i = 0; i < colours.Length; i++)
        {
            colours[i] = gradient.Evaluate(colours[i].r);
        }
        */
        //Not crazy efficient obviously, this should have been put in global file
        float? max = 0;
        float? min = 0;

        for (int i = 0; i < vertices.Length; i++){
            if (max != null && min != null)
            {
                if (vertices[i].y > max)
                {
                    max = vertices[i].y;
                }
                if (vertices[i].y < min)
                {
                    min = vertices[i].y;
                }
            }
            else
            {
                max = vertices[i].y;
                min = vertices[i].y;
            }
        }

        //For now we will just set the colour respective to the height, rather than a parameter to make easier to see :)
        for (int i = 0; i < colours.Length; i++){
            colours[i] = gradient.Evaluate((vertices[i].y-(float)min)/(float)(max-min));
        }

        mesh.colors = colours;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        outline.mesh = mesh;

        //Reverse normals because we are lazy
        for(int i=0;i<triangles.Length; i += 3){
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];
            triangles[i+1] = v3;
            triangles[i+2] = v2;
        }

        mesh.vertices = vertices;
        mesh.colors = colours;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        outline2.mesh = mesh;
    }

    void loadInterior(int n){
        //Code being used to render the interior
        //fileNames = Directory.GetFiles("Assets/Resources/MESHES/INTERIOR_0/", "*.asset");
        //fileNames = fileNames.OrderBy(s => int.Parse(s.Split("/")[s.Split("/").Length - 1].Split(".")[0])).ToArray();
        //Debug.Log("MESHES/RESULTS_0/" + fileNames[0].Split("/")[0].Split(".")[0]);
        string dirPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"AIV_3D/{name}/INTERIOR");
        fileNames = Directory.GetFiles(dirPath, "*.json");

        fileNames = fileNames
            .OrderBy(s => int.Parse(Path.GetFileNameWithoutExtension(s)))
            .ToArray();
        string json = File.ReadAllText(fileNames[n]);
        MeshSaveData foundMesh = JsonUtility.FromJson<MeshSaveData>(json);

        //outlineVertices = foundMesh.vertices.ToArray();
        vertices = foundMesh.vertices.ToArray();
        triangles = foundMesh.triangles.ToArray();
        colours = foundMesh.colours.ToArray();
        outlineLength = vertices.Length;

        Mesh mm = new Mesh();
        Color[] colours2 = new Color[colours.Length];
        mm.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        //Get colours through gradient
        for(int i=0;i<colours2.Length;i++){
            colours2[i] = gradient2.Evaluate(colours[i].g);
        }
        mm.vertices = vertices;
        mm.colors = colours2;
        mm.triangles = triangles;
        mm.RecalculateNormals();

        interior2.mesh = mm;

        //mesh = Resources.Load<Mesh>("MESHES/INTERIOR_0/" + fileNames[n].Split("/")[fileNames[0].Split("/").Length - 1].Split(".")[0]);

        /*
        outlineVertices = mesh.vertices;
        vertices = mesh.vertices;
        colours = mesh.colors;
        Color[] colours2 = mesh.colors;
        triangles = mesh.triangles;
        */
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;

        //Get colours through gradient
        for(int i=0;i<colours.Length;i++){
            colours[i] = gradient.Evaluate(colours[i].r);
        }
        mesh.colors = colours;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        interior.mesh = mesh;
    }


    void wrapOutline()
    {
        int count = 0;
        int amount = 20;
        strength = amount/(int)Mathf.Floor(360/sliceAngle) + 1;
        //so for 180 degrees out of 24 for example, we need 13 rather than 12, as we are plotting both poles
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

                angle += 2 * Mathf.PI / (amount);

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

