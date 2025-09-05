using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(MeshFilter))]
public class meow : MonoBehaviour
{
    Mesh mesh;
    public Vector3[] vertices;
    public Color[] colours;
    public int[] triangles;
    public bool drawGizmos;
    public int strength = 20;
    public int sine = 1;
    public Text text;
    public int outlineLength;
    public Gradient gradient;

    public GameObject UIface;

    private List<Vector3> verticeList = new List<Vector3>();

    void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
    }
    /*
    void readFile()
    {
        string fileName = $"csv/All_mesh_data_at_timestep_4.csv";
        verticeList = new List<Vector3>();
        if (File.Exists(fileName))
        {
            Debug.Log("Found: " + fileName);
            using (var reader = new StreamReader(fileName))
            {
                reader.ReadLine();
                var content = reader.ReadLine();
                while (reader.EndOfStream == false)
                {
                    var splitRow = content.Split(',');
                    Debug.Log(content);
                    float x = ParseFloat(splitRow[0]);
                    float y = ParseFloat(splitRow[1]);
                    verticeList.Add(new Vector3(0, y, x));
                    content = reader.ReadLine();
                }
            }
            outlineLength = verticeList.Count;
        }
    }

    public float ParseFloat(string value){
        float result;
        if (!float.TryParse(value, out result))
        {
            //Debug.LogWarning($"Invalid float format: '{value}'. Defaulting to 0.");
            result = 0f;  // Default to 0 if parsing fails
        }
        return result;
    }

    void createOutline(){
        //sort the outline
        vertices = new Vector3[outlineLength];
        LinkedList root = new LinkedList(verticeList[(int)Mathf.Floor(outlineLength / 2)]);
        verticeList.RemoveAt((int)Mathf.Floor(outlineLength / 2));

        int meow = 0;
        meow = head(root);
        while (meow == 0) { }
        tail(root);

        root = root.getRoot();

        for (int i = 0; i < outlineLength; i++)
        {
            Debug.Log(i);
            vertices[i] = root.getPosition();
            root = root.getTail();
        }
    }

    int head(LinkedList l){
        float distance = 2;
        int index = 0;
        for (int i=0; i<verticeList.Count; i++){
           if (Vector3.Distance(verticeList[i], l.getPosition()) < distance){
                distance = Vector3.Distance(verticeList[i], l.getPosition());
                l.setHead(new LinkedList(null, l, verticeList[i]));
                index = i;
           }
        }
        if (l.getHead() != null){
            verticeList.RemoveAt(index);
            return head(l.getHead());
        }
        return 1;
    }

    int tail(LinkedList l){
        float distance = 2;
        int index = 0;
        for (int i=0; i<verticeList.Count; i++){
           if (Vector3.Distance(verticeList[i], l.getPosition()) < distance){
                distance = Vector3.Distance(verticeList[i], l.getPosition());
                l.setTail(new LinkedList(l, null, verticeList[i]));
                index = i;
           }
        }
        if (l.getTail() != null){
            verticeList.RemoveAt(index);
            return tail(l.getTail());
        }
        return 1;
    }

    void wrapOutline(){
        float angle = 0;
        int count = 0;
        Vector3[] verticesF = new Vector3[outlineLength * strength];
        for (int i = 0; i < outlineLength; i++){
            angle = 0;
            float y = vertices[i].y;
            float z = vertices[i].z;
            //int a = (int)Mathf.Ceil(z) * strength/10;
            for (int j = 0; j < strength; j++){
                angle += 2 * Mathf.PI / (strength);
                verticesF[count] = new Vector3(Mathf.Cos(angle) * z, y, Mathf.Sin(angle) * z);
                count++;
            }
        }
        vertices = verticesF;
        colours = new Color[vertices.Length];
        for (int i = 0; i < colours.Length; i++){
            colours[i] = gradient.Evaluate(Mathf.Floor(i/strength)/outlineLength);
        }
    }

    void trianglesWrap(){
        //triangles = new int[outlineLength * (strength - 2) * 6];
        triangles = new int[(outlineLength-1)*2*strength*3];

            }
            outlineLength = verticeList.Count;
        }
    }
    
    public float ParseFloat(string value)
    {
        float result;
        if (!float.TryParse(value, out result))
        {
            //Debug.LogWarning($"Invalid float format: '{value}'. Defaulting to 0.");
            result = 0f;  // Default to 0 if parsing fails
        }
        return result;
    }
    */

    void createOutline()
    {
        //Here we do a naughty, just straight up grab from thinging, let's not do that lol
        verticeList = GetComponent<MainLoader>().myCollision.timeSlices[0].verticeList;
        outlineLength = verticeList.Count;
        //sort the outline
        vertices = new Vector3[outlineLength];
        LinkedList root = new LinkedList(verticeList[(int)Mathf.Floor(outlineLength / 2)]);
        verticeList.RemoveAt((int)Mathf.Floor(outlineLength / 2));

        int meow = 0;
        meow = head(root);
        while (meow == 0) { }
        tail(root);

        root = root.getRoot();

        for (int i = 0; i < outlineLength; i++)
        {
            //Debug.Log(i);
            vertices[i] = root.getPosition();
            root = root.getTail();
            if (root.getTail() == null)
            {
                outlineLength = i;
            }
        }
        //Debug.Log(outlineLength);
    }

    //Used for creating the outline, it's basically a linkedlist
    int head(LinkedList l)
    {
        //this distance variable and the one in tail needs to be dynamic, so bigger distances for higher dilution
        float distance = 50;
        int index = 0;
        for (int i = 0; i < verticeList.Count; i++)
        {
            if (Vector3.Distance(verticeList[i], l.getPosition()) < distance)
            {
                distance = Vector3.Distance(verticeList[i], l.getPosition());
                l.setHead(new LinkedList(null, l, verticeList[i]));
                index = i;
            }
        }
        if (l.getHead() != null)
        {
            verticeList.RemoveAt(index);
            return head(l.getHead());
        }
        return 1;
    }

    int tail(LinkedList l)
    {
        float distance = 50;
        int index = 0;
        for (int i = 0; i < verticeList.Count; i++)
        {
            if (Vector3.Distance(verticeList[i], l.getPosition()) < distance)
            {
                distance = Vector3.Distance(verticeList[i], l.getPosition());
                l.setTail(new LinkedList(l, null, verticeList[i]));
                index = i;
            }
        }
        if (l.getTail() != null)
        {
            verticeList.RemoveAt(index);
            return tail(l.getTail());
        }
        return 1;
    }

    void wrapOutline()
    {
        float angle = 0;
        int count = 0;
        Vector3[] verticesF = new Vector3[outlineLength * strength];
        for (int i = 0; i < outlineLength; i++)
        {
            angle = 0;
            float y = vertices[i].y;
            float z = vertices[i].z;
            //int a = (int)Mathf.Ceil(z) * strength/10;
            for (int j = 0; j < strength; j++)
            {
                angle += 2 * Mathf.PI / (strength);
                verticesF[count] = new Vector3(Mathf.Cos(angle) * z, y, Mathf.Sin(angle) * z);
                count++;
            }
        }
        vertices = verticesF;
        colours = new Color[vertices.Length];
        for (int i = 0; i < colours.Length; i++)
        {
            colours[i] = gradient.Evaluate(Mathf.Floor(i / strength) / outlineLength);
        }
    }

    void trianglesWrap()
    {
        triangles = new int[(outlineLength - 1) * 2 * strength * 3];

        for (int i = 0; i < outlineLength - 1; i++)
        {
            for (int j = 0; j < strength - 1; j++)
            {
                triangles[6 * j + 6 * strength * i] = j + strength * i;
                triangles[6 * j + 1 + 6 * strength * i] = j + 1 + strength * i;
                triangles[6 * j + 2 + 6 * strength * i] = j + 1 + strength * (i + 1);

                triangles[6 * j + 3 + 6 * strength * i] = j + strength * i;
                triangles[6 * j + 4 + 6 * strength * i] = j + 1 + strength * (i + 1);
                triangles[6 * j + 5 + 6 * strength * i] = j + strength * (i + 1);
            }

            triangles[6 * (strength - 1) + 6 * strength * i] = (strength - 1) + strength * i;
            triangles[6 * (strength - 1) + 1 + 6 * strength * i] = strength * i;
            triangles[6 * (strength - 1) + 2 + 6 * strength * i] = strength * (i + 1);

            triangles[6 * (strength - 1) + 3 + 6 * strength * i] = (strength - 1) + strength * i;
            triangles[6 * (strength - 1) + 4 + 6 * strength * i] = strength * (i + 1);
            triangles[6 * (strength - 1) + 5 + 6 * strength * i] = (strength - 1) + strength * (i + 1);

        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UIface.SetActive(false);
            createOutline();
            wrapOutline();
            trianglesWrap();
            updateMesh();
        }
    }

    void updateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colours;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}