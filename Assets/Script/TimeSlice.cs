using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst;
using System;
using Unity.Collections;
using Unity.Jobs;
using System.Threading.Tasks;
using System.Threading;

public class TimeSlice
{
    public float?[,][] coordinates;
    public List<Coordinate> outlineList;
    public Vector3[] vertices;
    private Color[] outlineColours;


    private static float?[] minParameter;
    private static float?[] maxParameter;

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
    private string[] parameter;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private List<int> outlineCorrespondingIndex;

    public Gradient gradient = new Gradient();
    private List<Vector2> Edges;

    private string name;

    public TimeSlice(string pFilePath, string timestep, int pDilution, int rows, int cols, int gridSize, int ignoreColA, int ignoreColB, int ignoreRowB, string[] parameter, string name){
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
        this.name = name;
    }
   
    public float ParseFloat(string value){
        float result;
        if (!float.TryParse(value, out result)){
            //Debug.LogWarning($"Invalid float format: '{value}'. Defaulting to 0.");
            result = 0f;  // Default to 0 if parsing fails
        }
        return result;
    }

    async public Task setUpSlice(int dirNum, Gradient g) {
        this.dirNum = dirNum;
        gradient = g;
        await Task.Run(() =>{
            Create2DArrayTask();
            createOutlineArray();
            createOutlineMesh();
            //smooth it out
            for (int i = 0; i < rows/dilution/10; i++){
                curveFitting(0.33f);
                curveFitting(-0.34f);
            }
        });
        saveInteriorMesh();
        //part one done
    }


    async public Task saveOutlineTask()
    {
        await Task.Run(() => {

        });
        saveOutlineMesh();
        saveTextFile();
        //part two done
    }

    async public Task updateGlobal()
    {
        //Mesh mesh = Resources.Load<Mesh>($"/INTERIOR{dirNum}/{timestep}");
        string dirPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"AIV_3D/{name}/INTERIOR/{timestep}.json");
        string json = File.ReadAllText(dirPath);
        MeshSaveData foundMesh = JsonUtility.FromJson<MeshSaveData>(json);

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = foundMesh.vertices.ToArray();
        mesh.triangles = foundMesh.triangles.ToArray();
        Color[] col = foundMesh.colours.ToArray();
        
        await Task.Run(() =>
        {
            col = updateMeshGlobal(col);
        });
        mesh.colors = col;
        saveInteriorMesh(mesh);
        saveTextFile();
        //part three final part done
    }


    public void Create2DArrayTask(){
        //Ensure correction dilution
        int remainderA = rows%dilution;
        int remainderB = cols%dilution;

        int dilRows = Mathf.FloorToInt(rows/dilution);
        int dilCols = Mathf.FloorToInt(cols/dilution);

        ignoreRowB = remainderA;


        /*
        if (remainderA > 0){
            dilRows-=dilution;
        }
        if (remainderB > 0){
            dilCols-=dilution;
        }
        */

        /*
        while(remainderA != 0 || remainderB != 0){
            int ratio = (int)rows/cols;
            if (remainderA != 0){
                rows - remainderA;

            }
        }
        }
        int dilRows = (int)Math.Ceiling((float)rows/dilution);
        int dilCols = (int)Math.Ceiling((float)cols/dilution);

        if (remainderA > 0){
            dilRows-=dilution;
        }
        if (remainderB > 0){
            dilCols-=dilution;
        }
        */

        int densityColumn = 0;
        int[] parameterColumn = new int[0];
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
        coordinates = new float?[dilRows, dilCols][];
        outlineList = new List<Coordinate>();

        string[] columnLegend = reader.ReadLine().Split(",");
        for (int i = 0; i < columnLegend.Length; i++){
            if (columnLegend[i].Split(" ")[0].Equals("Density")){
                densityColumn = i;
            }
            //find all the interesting columns
            String s = columnLegend[i].Split(" ")[0];
            if (!s.Equals("") && !s.Equals("Density") && !s.Equals("x") && !s.Equals("y")){
                //make new array with an one extra array
                int[] temp = new int[parameterColumn.Length+1];
                for(int m=0;m<parameterColumn.Length;m++){
                    temp[m] = parameterColumn[m];
                }
                temp[parameterColumn.Length] = i;
                parameterColumn = temp;
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
                    float?[] para = new float?[4];
                    for(int m=0;m<parameterColumn.Length;m++){
                        para[m] = ParseFloat(splitRow[parameterColumn[m]]);
                        //read first line to find a starting reference point for temps if not set yet
                        if (minParameter == null){
                            minParameter = new float?[4];
                            maxParameter = new float?[4];
                            minParameter[m] = para[m];
                            maxParameter[m] = para[m];
                        }else if(minParameter[m] == null){
                            minParameter[m] = para[m];
                            maxParameter[m] = para[m];
                        }else if (para[m] > maxParameter[m]){
                            maxParameter[m] = para[m];
                        }else if(para[m]<minParameter[m]){
                            minParameter[m] = para[m];
                        }
                    }
                    coordinates[j, i] = para;
                }
            }
            //dilute, skip rows by the number of remainder from cols
            for (int k = 0; k < ignoreRowB; k++){
                reader.ReadLine();
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
        outlineCorrespondingIndex = new List<int>();

        //set up bounds for parameter
        //float b = Mathf.Abs((float)maxParameter - (float)minParameter);
        //float a = (float)minParameter * -1;

        //Go down column for each point, then go to the right and start from the top again
        //Given this, define each point as their column (j, repeat row times for each y position in the column),
        //and their row (i * row, repeat for each x position so each column) added together.
        //A point can then be defined as such: (j*rows)+i

        for (int i = 0; i < cols; i++){
            for (int j = 0; j < rows; j++){
                //For each point
                interiorVertices[(i*rows)+j] = new Vector3(0, grid * j, grid * i);
                //This algorithm assumes coordinates that touch an empty space horizontally or vertically (not diagonally) are outline points
                if (coordinates[j, i] != null){

                    //testPoints.Add(new Vector3(0, grid * j, grid * i));

                    //colour for interior
                    //colours[(i * rows) + j] = gradient.Evaluate((c + a) / b);
                    /*for(int m=0; m<parameterColumn.length;m++){
                        if(coordinates[j, i].parameter[m] == null){
                            c = pa
                        }
                    }*/
                    float?[] c = coordinates[j,i];
                    colours[(i * rows) + j] = new Color(c[0]??0, c[1]??0, c[2]??0, c[3]??0);
                    //Debug.Log(colours[(i * rows) + j]);

                    /* 
                        4 main scenarios for a outline to exist, one to the left/right/above/below is null
                    */
                    if (i > 0 && coordinates[j, i-1] == null){
                            //one to the left is null
                            //We check if the below and left-below are possible
                            if (j > 0 && i > 0 && coordinates[j-1, i] != null && coordinates[j-1, i-1] != null){
                                triangles.Add((i * rows) + j); //our point
                                triangles.Add((i * rows) + j - 1); //below point
                                triangles.Add(((i - 1) * rows) + j -1); //left-below point
                            }
                            outlineList.Add(new Coordinate(new Vector3(0, j, i), c));
                    }else if (i < cols - 1 && coordinates[j, i + 1] == null) {
                            //one to the right is null
                            //We check if the below and right-below are possible
                            if (j > 0 && coordinates[j-1, i+1] != null && coordinates[j-1, i] != null){
                                triangles.Add((i * rows) + j); //our point
                                triangles.Add(((i + 1) * rows) + j - 1); //below and right point 
                                triangles.Add((i * rows) + j-1); //below point
                            }
                            //Then we check if the above and left-above are possible
                            if (j < rows - 1 && i > 0 && coordinates[j+1, i-1] != null && coordinates[j+1, i] != null){
                                triangles.Add((i * rows) + j); //our point
                                triangles.Add(((i - 1) * rows) + j + 1); //above and left point 
                                triangles.Add((i * rows) + j + 1); //above point

                                triangles.Add((i * rows) + j); //our point
                                triangles.Add(((i - 1) * rows) + j); //left point 
                                triangles.Add(((i - 1) * rows) + j + 1); //above and left point
                            }
                            //Then we check if the below and left-below are possible
                            if (j > 0 && i > 0 && coordinates[j-1, i] != null){
                                triangles.Add((i * rows) + j); //our point
                                triangles.Add((i * rows) + j - 1); //below point
                                triangles.Add(((i - 1) * rows) + j); //left point
                                
                                if(coordinates[j-1, i-1] != null){
                                    triangles.Add(((i - 1) * rows) + j - 1); //below and left point
                                    triangles.Add(((i - 1) * rows) + j); //left point
                                    triangles.Add((i * rows) + j - 1); //below point
                                }
                            }
                            if(j > 0 && i > 0 && coordinates[j-1, i-1] != null){
                                triangles.Add((i * rows) + j); //our point
                                triangles.Add(((i - 1) * rows) + j-1); //left-below point
                                triangles.Add(((i - 1) * rows) + j); //left point
                            }
                            
                            outlineList.Add(new Coordinate(new Vector3(0, j, i), c));
                    }else if(j < rows - 1 && coordinates[j + 1, i] == null){
                            //one above is null
                            //We check if the below and right-below are possible
                            if (j > 0 && i < cols -1 && coordinates[j-1, i+1] != null && coordinates[j-1, i] != null){
                                triangles.Add((i * rows) + j); //our point
                                triangles.Add(((i + 1) * rows) + j - 1); //below and right point 
                                triangles.Add((i * rows) + j-1); //below point

                                triangles.Add((i * rows) + j); //our point
                                triangles.Add(((i + 1) * rows) + j); //right point
                                triangles.Add(((i + 1) * rows) + j - 1); //below and right point 
                            }
                            //Then we check if the below and left-below are possible
                            if (j > 0 && i > 0 && coordinates[j, i-1] != null && coordinates[j-1, i-1] != null){
                                triangles.Add((i * rows) + j); //our point
                                triangles.Add((i * rows) + j - 1); //below point
                                triangles.Add(((i - 1) * rows) + j); //left point
                                if (coordinates[j-1, i-1] != null){
                                    triangles.Add(((i - 1) * rows) + j - 1); //below and left point
                                    triangles.Add(((i - 1) * rows) + j); //left point
                                    triangles.Add((i * rows) + j - 1); //below point
                                }
                            }
                            outlineList.Add(new Coordinate(new Vector3(0, j, i), c));
                    }else if(j > 0 && coordinates[j - 1, i] == null){
                            //one below is null
                            //We check if the left-below is possible
                            if (j > 0 && i > 0 && coordinates[j-1, i-1] != null){
                                triangles.Add((i * rows) + j); //our point
                                triangles.Add(((i - 1) * rows) + j -1); //left-below point
                                triangles.Add(((i-1) * rows) + j); //below point
                            }
                            outlineList.Add(new Coordinate(new Vector3(0, j, i), c));
                    }else{
                        //found point that is not an outline, so form connections to next point (right) and below point
                        if (i > 0 && j > 0){
                            triangles.Add((i * rows) + j); //our point
                            triangles.Add((i * rows) + j - 1); //below point
                            triangles.Add(((i - 1) * rows) + j); //left point
                            if (coordinates[j-1, i-1] != null){
                                triangles.Add(((i - 1) * rows) + j - 1); //below and left point
                                triangles.Add(((i - 1) * rows) + j); //left point
                                triangles.Add((i * rows) + j - 1); //below point
                            }
                        }
                    }
                }
            }
        }
       /* Debug.Log("begin creating tree");
        LinkedList tree = kdtree(testPoints, 0);
        Vector3 point = new Vector3(0, grid * 400, grid * 400);
        Vector3?[] eight = new Vector3?[8];
        Debug.Log("begin searching tree");
        for(int i=0;i<1000000;i++){
              eight = find8KNN(tree, point, eight, 0);
        }
        foreach (Vector3? v in eight){
            if(v!=null){
                Vector3 i = (Vector3)v;
                Debug.Log(i.x+", "+i.y+", "+i.z);
            }
        }
        */
        coordinates = null;
    }


    private void createOutlineMesh(){
        List<Vector3> verticeList = new List<Vector3>();
        List<Color> colourList = new List<Color>();
        outlineLength = outlineList.Count;
        //set up bounds for parameter
        //float b = Mathf.Abs((float)maxParameter - (float)minParameter);
        //float a = (float)minParameter * -1;

        while (outlineList.Count > 0){
            LinkedList root = new LinkedList(outlineList[(int)Mathf.Floor(outlineLength / 2)]);
            outlineList.RemoveAt((int)Mathf.Floor(outlineLength / 2));
            Mhead(root, false);
            root = root.getRoot();

            //reverse the list if is wrong way
            /*if(root.getEnd().getPosition().y > root.getPosition().y){
                root = root.reverseList();
            }*/
            
            //automatically goes through each section, checking if it is facing the right way
            //root.reverseList();


            while (root.getTail() != null){
                verticeList.Add(new Vector3(0, root.getPosition().y*grid, root.getPosition().z*grid));

                float?[] c = root.getParameter();
                colourList.Add(new Color(c[0]??0, c[1]??0, c[2]??0, c[3]??0));
                //colourList.Add(gradient.Evaluate((root.getParameter()+a)/b));
                outlineCorrespondingIndex.Add(((int)root.getPosition().z * rows) + (int)root.getPosition().y);
                root = root.getTail();
            }

            if (verticeList.Count > 0)
            {
                int last = verticeList.Count - 1;
                verticeList[last] = new Vector3(10f, verticeList[last].y, verticeList[last].z);
            }
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
        //float distance = Mathf.Sqrt(2.2f * (grid) * (grid));
        float distance = Mathf.Sqrt(2.5f*(dilution+1));
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
        //float distance = Mathf.Sqrt(2.2f * (grid) * (grid));
        float distance = 2*Mathf.Sqrt(2.5f*(dilution+1));
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


    public void saveOutlineMesh(){
        Mesh mm = new Mesh();
        mm.vertices = vertices;

        float?[] a = new float?[4];
        float?[] b = new float?[4];

        for(int i=0;i<4;i++){
            if(minParameter[i]!=null){
                b[i] = Mathf.Abs((float)maxParameter[i] - (float)minParameter[i]);
                a[i] = (float)minParameter[i] * -1;
            }
        }
        for(int i=0;i<outlineColours.Length;i++){
            Color c = outlineColours[i];
            outlineColours[i] = new Color((c.r+a[0])/b[0]??0, (c.b+a[1])/b[1]??0, (c.g+a[2])/b[2]??0, (c.a+a[3])/b[3]??0);
            //mesh.colors[i] = new Color(??0, c[1]??0, c[2]??0, c[3]??0);
        }
        mm.colors = outlineColours;

        // Create mesh
        MeshSaveData saveData = new MeshSaveData{
            vertices = new List<Vector3>(mm.vertices),
            triangles = new List<int>(mm.triangles),
            colours = new List<Color>(mm.colors)
        };

        string json = JsonUtility.ToJson(saveData);
        string dirPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"AIV_3D");
        //Create our directories
        Directory.CreateDirectory(dirPath);
        dirPath = Path.Combine(dirPath, name);
        Directory.CreateDirectory(dirPath);
        Directory.CreateDirectory(Path.Combine(dirPath, $"OUTLINE"));
        string thePath = Path.Combine(dirPath, $"OUTLINE/{timestep}.json");
        try{
            File.WriteAllText(thePath, json);
        }catch(IOException e){
            Debug.Log(e);
        }

        //AssetDatabase.CreateAsset(mm, $"Assets/Resources/MESHES/RESULTS_{dirNum}/{timestep}.asset");


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


    public void saveTextFile(){
        //save csv file with global info
        string content = "";
        try{
            File.WriteAllText("Assets/Resources/MESHES/global.csv", content);
        }catch(Exception e){
            Debug.Log($"Failed to save global info: {e}");
        }
    }

    public Color[] updateMeshGlobal(Color[] meshColours){
        float?[] a = minParameter;
        float?[] b = maxParameter;

        float average = 0;
        for (int i = 0; i < meshColours.Length; i++)
        {
            Color c = meshColours[i];
            average += (c.g - a[1]) / (b[1] - a[1])/meshColours.Length ?? 0;
            meshColours[i] = new Color((c.r - a[0]) / (b[0] - a[0]) ?? 0, (c.g - a[1]) / (b[1] - a[1]) ?? 0, (c.b - a[2]) / (b[2] - a[2]) ?? 0, (c.a - a[3]) / (b[3] - a[3]) ?? 0);
            //meshColours[i] = new Color((c.r+a[0])/b[0]??0, (c.b+a[1])/b[1]??0, (c.g+a[2])/b[2]??0, (c.a+a[3])/b[3]??0);
            //mesh.colors[i] = new Color(??0, c[1]??0, c[2]??0, c[3]??0);
        }
        Debug.Log(average);
        return meshColours;
    }


    public void saveInteriorMesh()
    {
        //first fix up the outline points in the interior mesh
        //to fix this, go through each item in vertices (outline vertices)
        //then given their 
        for(int i=0;i<vertices.Length;i++){
            interiorVertices[outlineCorrespondingIndex[i]] = new Vector3(0, vertices[i].y, vertices[i].z);
        }

        //Mesh mm = new Mesh();
        //mm.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        //mm.vertices = interiorVertices;
        //mm.triangles = triangles.ToArray();
        //mm.colors = colours;
        //mm.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Create mesh
        MeshSaveData saveData = new MeshSaveData{
            vertices = new List<Vector3>(interiorVertices),
            triangles = new List<int>(triangles.ToArray()),
            colours = new List<Color>(colours)
        };

        string json = JsonUtility.ToJson(saveData);
        string dirPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"AIV_3D");
        //Create our directories
        Directory.CreateDirectory(dirPath);
        dirPath = Path.Combine(dirPath, name);
        Directory.CreateDirectory(dirPath);
        Directory.CreateDirectory(Path.Combine(dirPath, $"INTERIOR"));
        string thePath = Path.Combine(dirPath, $"INTERIOR/{timestep}.json");
        try{
            File.WriteAllText(thePath, json);
        }catch(IOException e){
            Debug.Log(e);
        }
        
        //AssetDatabase.CreateAsset(mm, $"Assets/Resources/MESHES/INTERIOR_{dirNum}/{timestep}.asset");
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

    public void saveInteriorMesh(Mesh mesh){

        // Create mesh
        MeshSaveData saveData = new MeshSaveData{
            vertices = new List<Vector3>(mesh.vertices),
            triangles = new List<int>(mesh.triangles),
            colours = new List<Color>(mesh.colors)
        };

        string json = JsonUtility.ToJson(saveData);
        string dirPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"AIV_3D");
        //Create our directories
        Directory.CreateDirectory(dirPath);
        dirPath = Path.Combine(dirPath, name);
        Directory.CreateDirectory(dirPath);
        Directory.CreateDirectory(Path.Combine(dirPath, $"INTERIOR"));
        string thePath = Path.Combine(dirPath, $"INTERIOR/{timestep}.json");
        try{
            File.WriteAllText(thePath, json);
        }catch(IOException e){
            Debug.Log(e);
        }
        //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        //EditorUtility.SetDirty(mesh);
        //AssetDatabase.SaveAssets();
        //AssetDatabase.SaveAssets(mesh, $"Assets/Resources/MESHES/INTERIOR_{dirNum}/{timestep}.asset");
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










    /*

    private LinkedList kdtree (List<Vector3> pointList, int depth)
    {
        // Select axis based on depth so that axis cycles through all valid values
        //as only 2 axis
        int axis = depth%2;
        List<Vector3> sortedVectors;
        Vector3 median;

        // Sort point list and choose median as pivot element, also make sure list has more than one item
        if(depth%2 == 0){
            sortedVectors = new List<Vector3>(pointList.OrderBy(v => v.y).ToArray<Vector3>());
        }else{
            sortedVectors = new List<Vector3>(pointList.OrderBy(v => v.z).ToArray<Vector3>());
        }

        median = sortedVectors[(int)Mathf.Floor(sortedVectors.Count/2)];
        // Create node and construct subtree
        LinkedList node = new LinkedList(new Coordinate(median, new float?[]{null, null, null, null}));

        //left = head, //right = tail
        if(pointList.Count>2){
            node.setHead(kdtree(new List<Vector3>(sortedVectors.Take((int)Mathf.Floor(sortedVectors.Count/2)).ToArray<Vector3>()), depth+1));
            node.setTail(kdtree(new List<Vector3>(sortedVectors.Skip((int)Mathf.Floor(sortedVectors.Count/2)).ToArray<Vector3>()), depth+1));
        }else if(pointList.Count>1){
            node.setHead(kdtree(new List<Vector3>(sortedVectors.Take((int)Mathf.Floor(sortedVectors.Count/2)).ToArray<Vector3>()), depth+1));
        }

        //node.leftChild := kdtree(points in pointList before median, depth+1);
        //node.rightChild := kdtree(points in pointList after median, depth+1);
        return node;
    }





    private Vector3?[] find8KNN(LinkedList node, Vector3 point, Vector3?[] eight, int depth){
        int axis = depth%2;


        //    atm an niave approach, sort of just duplicate code to check left and right side
   

        //check if point is on left (head) of our node while making sure we have a left (head)
        //either y or z, y == 0, z == 1
        if(node.getHead()!=null && (node.getPosition().y <= point.y && depth%2 == 0 || node.getPosition().z <= point.z && depth%2 == 1)){
            eight = find8KNN(node.getHead(), point, eight, depth+1);
            //check to see if we are a neighbour
            if(Vector3.Distance(node.getPosition(), point) < Mathf.Sqrt(2.2f * (grid) * (grid))){
                //this node is a neighbour, so now find where to add in list
                for(int i=0;i<8;i++){
                    if(eight[i]==null){
                        eight[i] = node.getPosition();
                        break;
                    }
                }
            }
            //now check to see if we need to go down the other way
            if(node.getTail()!=null){
                for(int i=0;i<8;i++){
                    bool check = false;
                    if(eight[i]!=null){
                        //see for each eight if the current node is closer than that point, if so then it may be worth going down that path
                        if(Mathf.Abs(point.y-node.getPosition().y) <= Mathf.Abs(point.y-((Vector3)eight[i]).y) && depth%2 == 0 
                        || Mathf.Abs(point.z-node.getPosition().z) <= Mathf.Abs(point.z-((Vector3)eight[i]).z) && depth%2 == 1){
                            check = true;
                        }
                    }else{
                        if(check || i==0){
                            //if it is null, we know we havent found all points so now we check to see if points are possibly on other side
                            eight = find8KNN(node.getTail(), point, eight, depth+1);
                        }
                        break;
                    }
                }
            }
        }
        else if(node.getTail()!=null){
            //else is on our right (tail)
            eight = find8KNN(node.getTail(), point, eight, depth+1);
            //check to see if we are a neighbour
            if(Vector3.Distance(node.getPosition(), point) < Mathf.Sqrt(2.1f * (grid) * (grid))){
                //this node is a neighbour, so now find where to add in list
                for(int i=0;i<8;i++){
                    if(eight[i]==null){
                        eight[i] = node.getPosition();
                        break;
                    }
                }
            }
            //now check to see if we need to go down the other way
            if(node.getHead()!=null){
                for(int i=0;i<8;i++){
                    bool check = false;
                    if(eight[i]!=null){
                        //see for each eight if the current node is closer than that point, if so then it may be worth going down that path
                        if(Mathf.Abs(point.y-node.getPosition().y) <= Mathf.Abs(point.y-((Vector3)eight[i]).y) && depth%2 == 0 
                        || Mathf.Abs(point.z-node.getPosition().z) <= Mathf.Abs(point.z-((Vector3)eight[i]).z) && depth%2 == 1){
                            check = true;
                        }
                    }else if(check){
                        //if it is null, we know we havent found all points so now we check to see if points are possibly on other side
                        eight = find8KNN(node.getHead(), point, eight, depth+1);
                        break;
                    }else{
                        break;
                    }
                }
            }
        }

        return eight;
    }
    */

}





//          /Users/zac/Desktop/NPSC/Timed field outputs planet
  //        /Users/zac/Desktop/NPSC/Planet2D


[System.Serializable]
public class MeshSaveData
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Color> colours;
}


