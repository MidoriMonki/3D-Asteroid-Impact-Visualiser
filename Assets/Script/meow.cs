using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class meow : MonoBehaviour{

    Mesh mesh;
    public Vector3[] vertices;
    public int[] triangles;
    public bool drawGizmos;
    public int strength = 20;
    public Text text;

    void Start(){
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        createTriangle();
        updateMesh();
    }

    private void Update()
    {
        text.text = ""+strength;
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("doing it");
            strength++;
            createTriangle();
            updateMesh();
            Debug.Log("done it?");
        }
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

            //need to do final one differently
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
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void OnDrawGizmos(){
        if (drawGizmos){
            for (int i=0;i<vertices.Length;i++){
                Gizmos.DrawSphere(vertices[i], .1f);
            }
        }
    }
}
