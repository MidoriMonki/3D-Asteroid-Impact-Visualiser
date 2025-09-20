using UnityEngine;
using System.IO;
using System;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;

public class Collision
{
    public List<TimeSlice> timeSlices;
    public string folderPath;
    public int timeSliceDilation = 1;
    public int coordinateDilation = 1;
    public int fileSize;
    public int filesCompletionStatus;
    public int fileNumber;
    private Gradient gradient;

    public Collision(int fileSize, Gradient g)
    {
        gradient = g;
        timeSlices = new List<TimeSlice>();
        this.fileSize = fileSize;
    }

    public void AddTimeSlice(TimeSlice slice)
    {
        timeSlices.Add(slice);
    }
    public async Task Loader(string pFolderPath, int pTimeSliceDilation, int pCoordinateDilation)
    {
        /* BELOW DOESNT WORK BECAUSE OF / VS \
        await Task.Run(() =>{
            folderPath = pFolderPath;
            int timestep = 0;
            int skippedCount = 0;
            TimeSlice newSlice;

            string[] fileNames = Directory.GetFiles(pFolderPath);
            //sorting by numeric value goddamn, who would have thought something so simple would be so complicated?
            //Okay so assume all file names are exactly the same except for the index/numerical part WHICH THEY END WITH
            //Find starting string
            string prefix = "";
            string ss = fileNames[0].Split("/")[fileNames[0].Split("/").Length - 1].Split(".")[0];
            for (int k = 0; k < ss.Length; k++)
            {
                if (!char.IsDigit(ss[k]))
                {
                    prefix += ss[k];
                }
            }
            //now we know where to split as we known length of prefix to numbers
            fileNames = fileNames.OrderBy(s =>
                int.Parse(s.Split("/")[s.Split("/").Length - 1].Split(".")[0].Substring(prefix.Length))
            ).ToArray();
            */
        await Task.Run(() => {
            folderPath = pFolderPath;
            int timestep = 0;
            int skippedCount = 0;
            TimeSlice newSlice;

            string[] fileNames = Directory.GetFiles(pFolderPath);
            //sorting by numeric value goddamn, who would have thought something so simple would be so complicated?
            //Okay so assume all file names are exactly the same except for the index/numerical part WHICH THEY END WITH
            //Find starting string
            string prefix = "";
            string ss = Path.GetFileNameWithoutExtension(fileNames[0]);
            for (int k = 0; k < ss.Length; k++)
            {
                if (!char.IsDigit(ss[k]))
                {
                    prefix += ss[k];
                }
            }
            //now we know where to split as we known length of prefix to numbers
            fileNames = fileNames.OrderBy(s =>
            {
                string name = Path.GetFileNameWithoutExtension(s);
                string numberPart = name.Substring(prefix.Length);
                int number;
                if (!int.TryParse(numberPart, out number))
                {
                    number = 0; // default if parsing fails
                }
                return number;
            }).ToArray();


            string fFullPath = fileNames[0];

            int rows = 0;// = this.countRows();
            int cols = -1;// = this.countCols(); //was set to -2?
            int colIgnoreA = 0;
            int colIgnoreB = 0;
            int rowIgnoreA = 0;
            int rowIgnoreB = 0;
            int gridSize = 0;

            if (File.Exists(fFullPath))
            {
                //find gridsize, rows, and cols, algorithm assumes at least 3 rows and 3 cols (I think anyway)
                using (var reader = new StreamReader(fFullPath))
                {
                    /* We need to read the file and determine which column represents which value
                     * Read each column, do this by reading first line and splitting
                     * Here is the format:
                     * 
                     *  and empty space is ignored
                     *  split all column names by a space, the first part is the name, second is the unit
                     */
                    int content = 0;
                    int tempContent = 0;
                    int xColumn = 0;

                    string[] columnLegend = reader.ReadLine().Split(",");
                    for(int i = 0; i < columnLegend.Length; i++)
                    {
                        if(columnLegend[i].Split(" ")[0].Equals("x"))
                        {
                            xColumn = i;
                        }
                    }
                    //Just printing stuff for testing
                        string storeCols = "";
                        for (int i = 0; i < columnLegend.Length; i++)
                        {
                            storeCols += columnLegend[i].Split(" ")[0] + " ";
                        }
                        Debug.Log(storeCols);
                        storeCols = "";
                        for (int i = 0; i < columnLegend.Length; i++)
                        {
                            if(columnLegend[i].Split(" ").Length > 1)
                            {
                                storeCols += columnLegend[i].Split(" ")[1] + " ";
                            }
                        }
                        Debug.Log(storeCols);

                    //set initial x position, find when change happens to determine grid size
                    string[] tempCheck = reader.ReadLine().Split(",");
                    int initX = (int)ParseFloat(tempCheck[xColumn]);
                    content = initX;

                    
                    //ignore extra row if it exists
                    bool extraRow = true;
                    foreach(string item in tempCheck)
                    {
                        if (!item.Equals("0"))
                        {
                            extraRow = false;
                            //Debug.Log("no extra row");
                        }
                    }
                    if (extraRow)
                    {
                        tempCheck = reader.ReadLine().Split(",");
                        initX = (int)ParseFloat(tempCheck[xColumn]);
                        content = initX;
                        rows--;
                    }
                    else
                    {
                        //rows+=2;
                    }
                    
                    while (content == initX)
                    {
                        content = (int)ParseFloat(reader.ReadLine().Split(",")[xColumn]);
                        rows++;
                        cols++;
                    }

                    gridSize = content- initX;
                    while (!reader.EndOfStream)
                    {
                        reader.ReadLine();
                        cols++;
                    }
                    //cellsPerRow--;
                    cols /= rows;
                    reader.Close();
                }

                //newSlice = new TimeSlice(fFullPath, pCoordinateDilation, fileSize, cols, rows, gridSize);
                //timeSlices.Add(newSlice);
            }
            timestep = 0;
            int pp = 0;
            for(int i=0;i<fileNames.Length;i++)
            {
                //string fileName = $"All_mesh_data_at_timestep_{timestep}.csv";
                string fullPath = fileNames[i];
                string[] parameters = {"Pressure", "Temperature"};

                newSlice = new TimeSlice(fullPath, ""+ i, pCoordinateDilation, rows, cols, gridSize, colIgnoreA, colIgnoreB, rowIgnoreB, parameters);
                if (File.Exists(fullPath))
                {
                    if (skippedCount == (pTimeSliceDilation - 1))
                    {
                        Debug.Log("Index " + pp + ": " + fullPath);
                        newSlice = new TimeSlice(fullPath, ""+ i, pCoordinateDilation, rows, cols, gridSize, colIgnoreA, colIgnoreB, rowIgnoreB, parameters);
                        timeSlices.Add(newSlice);
                        skippedCount = 0;
                        pp++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                else
                {
                    //Debug.Log("No file found :(");
                }
            }
        });

        //Set up Directory stuff
        //fhegyufgyuehufi34i
        int dirNumber = 0;
        if (!Directory.Exists("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!Directory.Exists("Assets/Resources/MESHES"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "MESHES");
        }
        while (Directory.Exists($"Assets/Resources/MESHES/RESULTS_{dirNumber}"))
        {
            AssetDatabase.DeleteAsset($"Assets/Resources/MESHES/RESULTS_{dirNumber}");
            //dirNumber++;
        }
        while (Directory.Exists($"Assets/Resources/MESHES/INTERIOR_{dirNumber}"))
        {
            AssetDatabase.DeleteAsset($"Assets/Resources/MESHES/INTERIOR_{dirNumber}");
            //dirNumber++;
        }
        AssetDatabase.CreateFolder("Assets/Resources/MESHES", $"RESULTS_{dirNumber}");
        AssetDatabase.CreateFolder("Assets/Resources/MESHES", $"INTERIOR_{dirNumber}");


        filesCompletionStatus = 0;
        fileNumber = timeSlices.Count;
        foreach (TimeSlice t in timeSlices)
        {
            //t.Create2DArray();
            //await t.Create2DArrayTask();
            await t.setUpSlice(dirNumber, gradient);
            filesCompletionStatus++;
            Debug.Log("TimeSlice "+filesCompletionStatus+" completed out of "+fileNumber);
        }
        filesCompletionStatus = 0;
        foreach (TimeSlice t in timeSlices)
        {
            await t.saveOutlineTask();
            filesCompletionStatus++;
            Debug.Log("Saving all outlines " + filesCompletionStatus + " completed out of " + fileNumber);
        }
        filesCompletionStatus = 0;
        foreach (TimeSlice t in timeSlices)
        {
            await t.updateGlobal();
            filesCompletionStatus++;
            Debug.Log("Applying global parameter bounds... " + filesCompletionStatus + " completed out of " + fileNumber);
        }
         
        Debug.Log("Total slices loaded: " + timeSlices.Count);

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
