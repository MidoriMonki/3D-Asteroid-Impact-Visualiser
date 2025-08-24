using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst;
using System;
using Unity.Collections;
using Unity.Jobs;
using System.Threading.Tasks;
using UnityEditor;
using System.Threading;

public class TimeSlice
{
    public float?[,] coordinates;
    public List<Coordinate> outlineList;
    public Vector3[] vertices;
    private Color[] outlineColours;


    private static float? minParameter;
    private static float? maxParameter;

    public int dilution;
    private string timestep;
    public int outlineLength;
    public string filePath;
    public int gridSize = 0;
    public int rows;
    public int cols;
    public int ignoreColA;
    public int ignoreColB;
    public int ignoreRowB;
    public float grid;
    private int dirNum = 0;
    private List<Color> colourList;

    private Vector3[] interiorVertices;
    private Color[] colours;
    private List<int> triangles;
    private string parameter;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


    public Gradient gradient = new Gradient();
    private List<Vector2> Edges;

    public TimeSlice(string pFilePath, string timestep, int pDilution, int rows, int cols, int gridSize, int ignoreColA, int ignoreColB, int ignoreRowB, string parameter){
        this.dilution = pDilution;
        this.timestep = timestep;
        this.filePath = pFilePath;
        this.cols = cols;
        this.rows = rows;
        this.gridSize = gridSize;
        this.ignoreColA = ignoreColA;
        this.ignoreColB = ignoreColB;
        this.parameter = parameter;
        this.ignoreRowB = ignoreRowB;
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

    async public Task setUpSlice(int dirNum, Gradient g) {
        this.dirNum = dirNum;
        gradient = g;
        await Task.Run(() =>
        {
            Create2DArrayTask();
            createOutlineArray();
            createOutlineMesh();
        });
        saveInteriorMesh();
        //part one done
    }


    async public Task saveOutlineTask()
    {
        await Task.Run(() => {
            //smooth it out
            for (int i = 0; i < rows/dilution/12; i++)
            {
                //curveFitting(0.33f);
                //curveFitting(-0.34f);
            }
        });
        saveOutlineMesh();
        //part two done
    }



    public void Create2DArrayTask(){
        int dilRows = rows/dilution;
        int dilCols = cols/dilution;
        int densityColumn = 0;
        int temperatureColumn = 0;
        int count = 0;

        using var reader = new StreamReader(filePath);
        /* Explaining how the following code segment works:
            * - Ignore the stretched colums in the data, these are points outside of the simulation
            * - Repeat the following for all colums that will survive the diluting:
            * - Dilute following colums
            * - Apply dilution to our colum rows
            * - Save those points that weren't diluted
            * - Keep a count of how far we have travelled the col, don't want to overstep into rows to be ignored (stretched rows)
            * - Read the file until we reach the next colum, repeat the process (This next colum will be the first to be diluted)
            */
        Debug.Log("rows/y: " + dilRows + ", cols/x: " + dilCols);
        grid = 1000 / (float)((dilRows + (2 * dilCols)) / 2);
        //grid given as average pixel size, so all x + y divided by 2.
        coordinates = new float?[dilRows, dilCols];
        outlineList = new List<Coordinate>();

        string[] columnLegend = reader.ReadLine().Split(",");
        for (int i = 0; i < columnLegend.Length; i++){
            if (columnLegend[i].Split(" ")[0].Equals("Density")){
                densityColumn = i;
            }else if (columnLegend[i].Split(" ")[0].Equals(parameter)){
                temperatureColumn = i;
            }
        }
        //Go down each column, as the first dimension is y
        var content = reader.ReadLine();


        //reader.ReadLine();
        for (int i = 0; i < dilCols; i++)
        {
            //dilute, skip columns by dilution-1 (go to the right by dilution-1 times)
            for (int k = 0; k < (dilution - 1) * rows; k++)
            {
                reader.ReadLine();
            }
            for (int j = 0; j < dilRows; j++)
            {
                //dilute, skip rows by dilution-1 (go down dilution-1 times)
                for (int k = 0; k < (dilution - 1); k++)
                {
                    reader.ReadLine();
                }
                //now start actually creating a point
                content = reader.ReadLine();
                //Debug.Log(j + ", " + i);
                var splitRow = content.Split(",");
                float density = ParseFloat(splitRow[densityColumn]);
                //check if point is empty
                if (density == 0.0f)
                {
                    coordinates[j, i] = null;
                    count++;
                }
                else
                {
                    float temperature = ParseFloat(splitRow[temperatureColumn]);
                    //read first line to find a starting reference point for temps if not set yet
                    if (minParameter == null)
                    {
                        minParameter = temperature;
                        maxParameter = temperature;
                    }else if (temperature > maxParameter)
                    {
                        maxParameter = temperature;
                    }
                    else if(temperature<minParameter)
                    {
                        minParameter = temperature;
                    }
                    coordinates[j, i] = temperature;
                }
            }
        }
        //Debug.Log("percentage of empty: " + (float)count * 100 / (dilRows * dilCols) + "%");
    }
    

    public void createOutlineArray(){
        //get the now updated number of cols and rows
        rows = coordinates.GetLength(0);
        cols = coordinates.GetLength(1);
        triangles = new List<int>();
        interiorVertices = new Vector3[rows*cols];
        colours = new Color[rows*cols];
        colourList = new List<Color>();
        //set up bounds for parameter
        float b = Mathf.Abs((float)maxParameter - (float)minParameter);
        float a = (float)minParameter * -1;
        //Go down column for each point, then go to the right and start from the top again
        //Given this, define each point as their column (j, repeat row times for each y position in the column),
        //and their row (i * row, repeat for each x position so each column) added together.
        //A point can then be defined as such: (j*rows)+i
        for (int i = 0; i < cols; i++){
            for (int j = 0; j < rows; j++){
                //For each point
                interiorVertices[(i*rows)+j] = new Vector3(0, grid * j, grid * i);
                //This outline assumes coordinates that touch an empty space horizontally or vertically (not diagonally)
                if (coordinates[j, i] != null){
                    //colour for interior
                    float c = (float)coordinates[j, i];
                    colours[(i * rows) + j] = gradient.Evaluate((c + a) / b);
                    if (i > 0 && coordinates[j, i-1] == null){
                        //one to the right is null
                        outlineList.Add(new Coordinate(new Vector3(0, grid * j, grid * i), c));
                    }else if (i < cols - 1 && coordinates[j, i + 1] == null)
                    {
                        //one to the left is null
                        outlineList.Add(new Coordinate(new Vector3(0, grid * j, grid * i), c));
                    }
                    else if(j < rows - 1 && coordinates[j + 1, i] == null)
                    {
                        //one above is null
                        outlineList.Add(new Coordinate(new Vector3(0, grid * j, grid * i), c));
                    }
                    else if(j > 0 && coordinates[j - 1, i] == null)
                    {
                        //one below is null
                        //first check above, then left above, then finally left
                        if (i > 0 && coordinates[j-1, i + 1] != null)
                        {
                            //if above isn't null, ignore cause that's the other node's job now.
                        }else if (i > 0 && coordinates[j - 1, i + 1] != null)
                        {
                            //one above and left isn't null
                            triangles.Add((i * rows) + j); //our point
                            triangles.Add(((i + 1) * rows) + j); //left point
                            triangles.Add(((i + 1) * rows) + j + 1); //point above and left
                        }
                        else if(i > 0 && coordinates[j - 1, i + 1] != null)
                        {
                            //left isn't null
                            triangles.Add((i * rows) + j); //our point
                            triangles.Add(((i + 1) * rows) + j); //left point
                            triangles.Add(((i + 1) * rows) + j + 1); //point above and left
                        }
                        outlineList.Add(new Coordinate(new Vector3(0, grid * j, grid * i), c));
                    }
                    else
                    {
                        //found point that is not an outline, so form connections to next point and above point
                        if (i > 0 && j > 0)
                        {
                            triangles.Add((i * rows) + j); //our point
                            triangles.Add(((i - 1) * rows) + j); //right point
                            triangles.Add((i * rows) + j - 1); //point above

                            triangles.Add(((i - 1) * rows) + j - 1); //left and up point
                            triangles.Add((i * rows) + j - 1); //point above
                            triangles.Add(((i - 1) * rows) + j); //right point
                        }
                        //triangles = new int[(outlineLength - 1) * strength * 6];
                        //do clockwise, we know that up and left are safe, this does lead to missing points on right and down
                    }
                }
            }
        }
        coordinates = null;
        /*
            Debug.Log(outlineList.Count);
            interiorVertices[0] = new Vector3(0, 0, 0);
            interiorVertices[1] = new Vector3(0, grid * rows, 0);
            interiorVertices[2] = new Vector3(0, 0, grid * cols);
            interiorVertices[3] = new Vector3(0, grid * rows, grid * cols);
        */
        //Debug.Log("Done :)");

    }


    private void createOutlineMesh(){
        List<Vector3> verticeList = new List<Vector3>();
        List<Color> colourList = new List<Color>();
        outlineLength = outlineList.Count;
        //set up bounds for parameter
        float b = Mathf.Abs((float)maxParameter - (float)minParameter);
        float a = (float)minParameter * -1;

        while (outlineList.Count > 0){
            LinkedList root = new LinkedList(outlineList[(int)Mathf.Floor(outlineLength / 2)]);
            outlineList.RemoveAt((int)Mathf.Floor(outlineLength / 2));
            Mhead(root, false);
            root = root.getRoot();

            while (root.getTail() != null){
                verticeList.Add(root.getPosition());
                colourList.Add(gradient.Evaluate((root.getParameter()+a)/b));
                root = root.getTail();
            }

            verticeList[verticeList.Count-1] = new Vector3(10f, verticeList[verticeList.Count - 1].y, verticeList[verticeList.Count - 1].z);
            outlineLength = outlineList.Count;
        }
        vertices = verticeList.ToArray();
        outlineColours = colourList.ToArray();
        outlineLength = vertices.Length;
    }




    //Given a scenario where head always goes first, if head and tail are neighbours, but head finds
    //that it has a closer neighbour, there is no point in tail then trying to validate if they are neighbours

    void Mhead(LinkedList list, bool done)
    {
        //this distance variable and the one in tail needs to be dynamic, so bigger distances for higher dilution
        float distance = 2 * Mathf.Sqrt(2 * (grid) * (grid));
        int nextNodeIndex = -1;

        //Go through all points, find closest one
        for (int i = 0; i < outlineList.Count; i++)
        {
            if (Vector3.Distance(outlineList[i].pos, list.getPosition()) < distance)
            {
                distance = Vector3.Distance(outlineList[i].pos, list.getPosition());
                nextNodeIndex = i;
            }
        }
        //found
        if (nextNodeIndex != -1)
        {
            /*if (list.getTail() != null && Vector3.Distance(list.getEnd().getPosition(), list.getPosition()) <= distance && !list.getEnd().getPosition().Equals(list.getPosition()) )
            {
                //if we found that tail is our nearest neighbour
                list.setHead(new LinkedList(null, list, list.getEnd().getPosition()));
                Mtail(list.getEnd(), false);
            }*/
            list.setHead(new LinkedList(null, list, outlineList[nextNodeIndex]));
            outlineList.RemoveAt(nextNodeIndex);
            if (!done)
            {
                Mtail(list.getEnd(), false);
            }
            else
            {
                Mhead(list.getRoot(), true);
            }
        }
        else if (!done)
        {
            Mtail(list.getEnd(), true);
        }
    }



    void Mtail(LinkedList list, bool done)
    {
        //this distance variable and the one in tail needs to be dynamic, so bigger distances for higher dilution
        float distance = 2 * Mathf.Sqrt(2 * (grid) * (grid));
        int nextNodeIndex = -1;

        //Go through all points, find closest one
        for (int i = 0; i < outlineList.Count; i++)
        {
            if (Vector3.Distance(outlineList[i].pos, list.getPosition()) < distance)
            {
                distance = Vector3.Distance(outlineList[i].pos, list.getPosition());
                nextNodeIndex = i;
            }
        }
        //found
        if (nextNodeIndex != -1)
        { //make sure it's not a first order neighbour
            /*
            if (list.getHead().getHead() != null && Vector3.Distance(list.getRoot().getPosition(), list.getPosition()) <= distance && !list.getRoot().getPosition().Equals(list.getPosition()) )
            {
                //if we found that head is our nearest neighbour
                list.setTail(new LinkedList(list, null, list.getRoot().getPosition()));
                Mhead(list.getRoot(), false);
            }*/
            list.setTail(new LinkedList(list, null, outlineList[nextNodeIndex]));
            outlineList.RemoveAt(nextNodeIndex);
            //outlineList.RemoveAt(head);
            if (!done)
            {
                Mhead(list.getRoot(), false);
            }
            else
            {
                Mtail(list.getEnd(), true);
            }
        }
        else if(!done)
        {
            Mhead(list.getRoot(), true);
        }
    }
   

    private void curveFitting(float scale)
    {
        Vector3[] verticesF = new Vector3[outlineLength];

        for (int i = 0; i < outlineLength - 1; i++)
        {
            if (i != 0 && i != outlineLength - 1)
            {
                if(vertices[i+1].x == 0f && vertices[i - 1].x == 0f && vertices[i].x == 0f)
                {
                    //add the two neighbours with weights, should be inverse of number of neighbours
                    float z = vertices[i].z + scale * (0.5f * ((vertices[i - 1].z - vertices[i].z) + (vertices[i + 1].z - vertices[i].z)));
                    float y = vertices[i].y + scale * (0.5f * ((vertices[i - 1].y - vertices[i].y) + (vertices[i + 1].y - vertices[i].y)));
                    vertices[i] = new Vector3(vertices[i].x, y, z);
                }
                //verticesF[i] = new Vector3(0, (verticesF[i - 1].y + vertices[Mathf.FloorToInt((i + 1) / 2)].y) / 2, (verticesF[i - 1].z + vertices[Mathf.FloorToInt((i + 1) / 2)].z) / 2);
                //verticesF[i] += new Vector3(0, (verticesF[i - 1].y + vertices[Mathf.FloorToInt((i + 1) / 2)].y) / 2, (verticesF[i - 1].z + vertices[Mathf.FloorToInt((i + 1) / 2)].z) / 2);
            }
            else
            {
                verticesF[0] = vertices[0];
            }

        }
        //vertices = verticesF;

    }


    public void saveOutlineMesh()
    {
        Mesh mm = new Mesh();
        mm.vertices = vertices;
        mm.colors = outlineColours;
        AssetDatabase.CreateAsset(mm, $"Assets/Resources/MESHES/RESULTS_{dirNum}/{timestep}.asset");
        //Debug.Log("Saved new timestamp");
        /*if (!File.Exists("Assets/Resources/MESHES/timestamp_0.asset"))
        {
            Debug.Log("Created new outline 'meow'");
            AssetDatabase.CreateAsset(mm, "Assets/Resources/MESHES/timestamp_0.asset");
        }
        else
        {
            Debug.Log("Saved outline 'meow'");
            AssetDatabase.SaveAssets();
            AssetDatabase.SaveAssetIfDirty(mm);
        }*/
    }



    public void saveInteriorMesh()
    {
        Mesh mm = new Mesh();
        mm.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mm.vertices = interiorVertices;
        mm.triangles = triangles.ToArray();
        mm.colors = colours;
        mm.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        AssetDatabase.CreateAsset(mm, $"Assets/Resources/MESHES/INTERIOR_{dirNum}/{timestep}.asset");
        triangles = null;
        interiorVertices = null;
        //Debug.Log("Saved new timestamp");
        /*if (!File.Exists("Assets/Resources/MESHES/timestamp_0.asset"))
        {
            Debug.Log("Created new outline 'meow'");
            AssetDatabase.CreateAsset(mm, "Assets/Resources/MESHES/timestamp_0.asset");
        }
        else
        {
            Debug.Log("Saved outline 'meow'");
            AssetDatabase.SaveAssets();
            AssetDatabase.SaveAssetIfDirty(mm);
        }*/
    }

}