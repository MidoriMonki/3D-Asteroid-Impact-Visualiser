using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[RequireComponent(typeof(MeshFilter))]
public class meow : MonoBehaviour{

    Mesh mesh;
    public Vector3[] vertices;
    public Color[] colours;
    public int[] triangles;
    public bool drawGizmos;
    public int strength = 20;
    public int sine = 1;
    public Text text;
    public Gradient gradient;

    private List<Vector3> verticeList = new List<Vector3>();

    void Start(){



        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        //createTriangle();
        //updateMesh();

        createOut();
        //createOutline();
        //wrapOutline();
    }

    void createOutline(){
        float angle = 0;
        //vertices = new Vector3[strength];

        for (int i = 0; i < strength; i++){
            angle += Mathf.PI / strength;
            verticeList.Add(new Vector3(0, Mathf.Cos(angle) * 6, Mathf.Sin(angle) * 6));
        }
    
    }

    int head(LinkedList l){
        float distance = 1;
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
        float distance = 1;
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


    void createOut(){
        /*
        float angle = 0;
        vertices = new Vector3[strength];
        for (int i = 0; i < strength; i++){
            angle += Mathf.PI / strength;
            vertices[i] = new Vector3(0, 6-(i/(float)strength*12), Mathf.Sin(angle*sine) * 3 + 3);
        }*/
        int count = 0;
        vertices = new Vector3[strength];
        for (int i = 0; i < strength; i++){
            while (16 * ((Mathf.Sin(3*(i+count)) - 3*Mathf.Sin(i+count)) / 4) < 0){
                count++;
            }
            verticeList.Add(new Vector3(0, 13*Mathf.Cos(i+count) - 5*Mathf.Cos(2*(i+count)) - 2*Mathf.Cos(3*(i+count)) - Mathf.Cos(4*(i+count)), 16*(Mathf.Sin(3*(i+count)) - 3*Mathf.Sin(i+count))/4));
        }
      
        //sort the outline
        LinkedList root = new LinkedList(verticeList[(int)Mathf.Floor(strength/2)]);
        verticeList.RemoveAt((int)Mathf.Floor(strength/2));
        
        int meow = 0;
        meow = head(root);
        while(meow==0){}
        tail(root);

        root = root.getRoot();

        for(int i=0; i<strength; i++){
            vertices[i] = root.getPosition();
            root = root.getTail();
        }


    }

    void wrapOutline(){
        float angle = 0;
        int count = 0;
        Vector3[] verticesF = new Vector3[strength * strength];
        for (int i = 0; i < strength; i++){
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
            colours[i] = gradient.Evaluate(i / (float)colours.Length);
        }
    }

    void trianglesWrap(){
        triangles = new int[strength * (strength - 2) * 6];
        for (int i = 0; i < strength - 2; i++)
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

            //need to do final one differently to wrap around
            
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
        text.text = ""+strength;
        if (Input.GetKeyDown(KeyCode.R))
        {
            //Debug.Log("doing it");
            strength++;
            createOut();
            wrapOutline();
            trianglesWrap();
            updateMesh();
            //createTriangle();
            //Debug.Log("done it?");
        }
    }

    public static void CacheItem(string url, Mesh mesh)
    {
        string path = Path.Combine(Application.persistentDataPath, url);
        byte[] bytes = MeshSerializer.WriteMesh(mesh, true);
        File.WriteAllBytes(path, bytes);
    }

    public static Mesh GetCacheItem(string url)
    {
        string path = Path.Combine(Application.persistentDataPath, url);
        if (File.Exists(path) == true)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return MeshSerializer.ReadMesh(bytes);
        }
        return null;
    }

    void createTriangle(){
        float angle0 = 0;
        float angle1 = 0;
        vertices = new Vector3[strength * (strength-1)];
        triangles = new int[strength * (strength-2) * 6];

        for (int i = 0; i < strength-1; i++)
        {
            angle1 += 3.14f / strength;
            for (int j = 0; j < strength; j++)
            {
                angle0 += 2 * 3.14f / strength;
                vertices[strength * i + j] = new Vector3(Mathf.Sin(angle1) * Mathf.Sin(angle0) * 10, Mathf.Cos(angle1) * 10, Mathf.Sin(angle1)* Mathf.Cos(angle0) * 10);
            }
            angle0 = 0;

        }

        for (int i=0;i<strength-2; i++)
        {
            for (int j=0; j<strength-1; j++)
            {
                triangles[6 * j +     6 * strength * i] = j + strength * i;
                triangles[6 * j + 1 + 6 * strength * i] = j + 1 + strength * i;
                triangles[6 * j + 2 + 6 * strength * i] = j + 1 + strength * (i + 1);
               
                triangles[6 * j + 3 + 6 * strength * i] = j + strength * i;
                triangles[6 * j + 4 + 6 * strength * i] = j + 1 + strength * (i + 1);
                triangles[6 * j + 5 + 6 * strength * i] = j + strength * (i + 1);
            }

            //need to do final one differently to wrap around
            
            triangles[6 * (strength-1) + 6 * strength * i] = (strength - 1) + strength * i;
            triangles[6 * (strength - 1) + 1 + 6 * strength * i] = strength * i;
            triangles[6 * (strength - 1) + 2 + 6 * strength * i] = strength * (i + 1);

            triangles[6 * (strength - 1) + 3 + 6 * strength * i] = (strength - 1) + strength * i;
            triangles[6 * (strength - 1) + 4 + 6 * strength * i] = strength * (i + 1);
            triangles[6 * (strength - 1) + 5 + 6 * strength * i] = (strength - 1) + strength * (i + 1);
            
        }
        
        /*
        vertices = new Vector3[strength*2];
        triangles = new int[strength*3*2];
        for (int i = 0; i < strength; i++)
        {
            vertices[2*i] = new Vector3(Mathf.Sin(angle)*10, Mathf.Cos(angle)*10, 0);
            vertices[2*i+1] = new Vector3(Mathf.Sin(angle)*10, Mathf.Cos(angle)*10, 2);
            angle += 2*Mathf.PI/strength;
        }
        for (int i=1; i<strength; i++)
        {
            triangles[6 * (i - 1)] = 2 * (i - 1);
            triangles[6 * (i - 1) + 1] = 2 * (i - 1) + 3;
            triangles[6 * (i - 1) + 2] = 2 * (i - 1) + 2;

            triangles[6 * (i - 1) + 3] = 2 * (i - 1);
            triangles[6 * (i - 1) + 4] = 2 * (i - 1) + 1;
            triangles[6 * (i - 1) + 5] = 2 * (i - 1) + 3;
        }*/
        /*
        vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0)
        };
        */
        /*
        triangles = new int[]{
            0, 1, 2
        };
        */
    }

    void updateMesh(){
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colours;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void OnDrawGizmos(){
        if (drawGizmos){
            for (int i=0;i<vertices.Length;i++){
                Gizmos.color = gradient.Evaluate(i/(float)vertices.Length);
                Gizmos.DrawSphere(vertices[i], .1f);
            }
        }
    }
}
