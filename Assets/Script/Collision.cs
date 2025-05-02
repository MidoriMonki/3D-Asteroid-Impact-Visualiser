using UnityEngine;
using System.IO;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using System.Linq;

[BurstCompile]
public class Collision
{
    public List<TimeSlice> timeSlices;
    public string folderPath;
    public int timeSliceDilation = 1;
    public int coordinateDilation = 1;
    public int fileSize;

    public Collision(int fileSize)
    {
        timeSlices = new List<TimeSlice>();
        this.fileSize = fileSize;
    }

    public void AddTimeSlice(TimeSlice slice)
    {
        timeSlices.Add(slice);
    }

    public void Loader(string pFolderPath, int pTimeSliceDilation, int pCoordinateDilation)
    {
        folderPath = pFolderPath;
        int timestep = 1;
        int skippedCount = 0;
        TimeSlice newSlice;

        string fFileName = "All_mesh_data_at_timestep_0.csv";
        string fFullPath = Path.Combine(pFolderPath, fFileName);

        int cellsPerRow = 0;// = this.countRows();
        int cellsPerCol = -2;// = this.countCols();
        int colIgnoreA = 0;
        int colIgnoreB = 0;
        int rowIgnoreA = 0;
        int rowIgnoreB = 0;
        int gridSize = 0;

        if (File.Exists(fFullPath)){
            //find gridsize, rows, and cols, algorithm assumes at least 3 rows and 3 cols (I think anyway)
            using (var reader = new StreamReader(fFullPath))
            {
                int content = 0;
                int tempContent = 0;
                reader.ReadLine();
                reader.ReadLine();

                //---------------- I will update this later Adam --------------------

                //read file until first x changes by gridsize for the first time
                /*while (content == 0){
                    content = (int)ParseFloat(reader.ReadLine().Split(',')[0]);
                    cellsPerRow++;
                }
                gridSize = content;
                //next, read file to find cells per col, assume at least one cell of stretch
                content = (int)ParseFloat(reader.ReadLine().Split(',')[1]);
                while (content%gridSize != 0) {
                    content = (int)ParseFloat(reader.ReadLine().Split(',')[1]);
                    //colIgnoreA++;
                    cellsPerCol++;
                    cellsPerRow++;
                }
                while (content % gridSize == 0){
                    content = (int)ParseFloat(reader.ReadLine().Split(',')[1]);
                    cellsPerCol++;
                    cellsPerRow++;
                }
                while (content % gridSize != 0){
                    content = (int)ParseFloat(reader.ReadLine().Split(',')[1]);
                    //colIgnoreB++;
                    cellsPerCol++;
                    cellsPerRow++;
                }
                while (content % gridSize == 0){
                    content = (int)ParseFloat(reader.ReadLine().Split(',')[0]);
                    cellsPerRow++;
                }
                while (reader.ReadLine() != null){
                    //rowIgnoreB++;
                    cellsPerRow++;
                }

                cellsPerRow /= cellsPerCol;
                rowIgnoreB /= (cellsPerRow + colIgnoreA + colIgnoreB);*/
                while (content == 0)
                {
                    content = (int)ParseFloat(reader.ReadLine().Split(',')[0]);
                    cellsPerCol++;
                    cellsPerRow++;
                }
                gridSize = content;
                while (reader.ReadLine() != null)
                {
                    cellsPerRow++;
                }
                //cellsPerRow--;
                cellsPerRow /= cellsPerCol;
                reader.Close();
            }

            //newSlice = new TimeSlice(fFullPath, pCoordinateDilation, fileSize, cols, rows, gridSize);
            //timeSlices.Add(newSlice);
        }

        while (true)
        {
            timestep = 14;
            string fileName = $"All_mesh_data_at_timestep_{timestep}.csv";
            string fullPath = Path.Combine(pFolderPath, fileName);

            newSlice = new TimeSlice(fullPath, pCoordinateDilation, cellsPerCol, cellsPerRow, gridSize, colIgnoreA, colIgnoreB, rowIgnoreB);
            timeSlices.Add(newSlice);
            /*if (File.Exists(fullPath))
            {
                if (skippedCount == (pTimeSliceDilation - 1))
                {
                    newSlice = new TimeSlice(fullPath, pCoordinateDilation, cellsPerCol, cellsPerRow, gridSize, colIgnoreA, colIgnoreB, rowIgnoreB);
                    timeSlices.Add(newSlice);
                    skippedCount = 0;
                } else
                {
                    skippedCount++;
                }

            }
            timestep++;
            if (timestep > 101) break;*/
            break;

        }
        Debug.Log("Total slices loaded: " + timeSlices.Count);

        foreach(TimeSlice t in timeSlices)
        {
           t.Create2DArray();
        }

        //Job system stuff
        /*
        var slices = new NativeArray<TimeSlice.Data>(timeSlices.Count, Allocator.TempJob);
        for (var i = 0; i < timeSlices.Count; i++)
        {
            slices[i] = new TimeSlice.Data(timeSlices[i]);
        }
        var job = new coordinateFileRead<Coordinate>
        { Slices = slices };
    
        var jobHandle = job.Schedule(timeSlices.Count, 1);
        //ensure finishes each frame
        jobHandle.Complete();
        slices.Dispose();
        */
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
}
/*
[BurstCompile]
public struct coordinateFileRead<T> : IJobParallelFor where T : struct
{
    [ReadOnly] public int fileSize;
    public NativeArray<TimeSlice.Data> Slices;
    //public NativeArray<T> coordinates; //treat 1D array as 2D array
    public void Execute(int i)
    {
        var data = Slices[i];
        data.Create2DArray(fileSize);
        Slices[i] = data;
    }
}
*/