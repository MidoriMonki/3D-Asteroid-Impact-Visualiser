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
    public async Task Loader(string pFolderPath, int pTimeSliceDilation, int pCoordinateDilation, int singleIndex = -1)
    {
        await Task.Run(() => {
            folderPath = pFolderPath;
            TimeSlice newSlice;

            // Get and sort files numerically (only csv)
            string[] fileNames = Directory.GetFiles(pFolderPath, "*.csv");
            if (fileNames == null || fileNames.Length == 0) return;

            // determine prefix (non-digit prefix)
            string prefix = "";
            string ss = Path.GetFileNameWithoutExtension(fileNames[0]);
            for (int k = 0; k < ss.Length; k++)
            {
                if (!char.IsDigit(ss[k]))
                    prefix += ss[k];
            }
            fileNames = fileNames.OrderBy(s =>
            {
                string name = Path.GetFileNameWithoutExtension(s);
                string numberPart = name.Length > prefix.Length ? name.Substring(prefix.Length) : "";
                int number;
                return int.TryParse(numberPart, out number) ? number : 0;
            }).ToArray();

            // Read grid info from the first file (keep original behavior)
            string fFullPath = fileNames[0];
            int rows = 0, cols = -1, colIgnoreA = 0, colIgnoreB = 0, rowIgnoreA = 0, rowIgnoreB = 0, gridSize = 0;
            if (File.Exists(fFullPath))
            {
                using (var reader = new StreamReader(fFullPath))
                {
                    string[] columnLegend = reader.ReadLine().Split(",");
                    int xColumn = Array.FindIndex(columnLegend, c => c.Split(" ")[0] == "x");

                    string[] tempCheck = reader.ReadLine().Split(",");
                    int initX = (int)ParseFloat(tempCheck[xColumn]);
                    int content = initX;

                    bool extraRow = tempCheck.All(item => item == "0");
                    if (extraRow)
                    {
                        tempCheck = reader.ReadLine().Split(",");
                        initX = (int)ParseFloat(tempCheck[xColumn]);
                        content = initX;
                        rows--;
                    }

                    while (content == initX && !reader.EndOfStream)
                    {
                        content = (int)ParseFloat(reader.ReadLine().Split(",")[xColumn]);
                        rows++;
                        cols++;
                    }

                    gridSize = content - initX;
                    while (!reader.EndOfStream)
                    {
                        reader.ReadLine();
                        cols++;
                    }
                    if (rows != 0)
                        cols /= Math.Max(1, rows);
                }
            }

            // --- Single-index branch: only include that specific file (by sorted index) ---
            if (singleIndex >= 0)
            {
                // clamp to valid range
                int idx = Mathf.Clamp(singleIndex, 0, fileNames.Length - 1);
                string singlePath = fileNames[idx];
                if (File.Exists(singlePath))
                {
                    string[] parameters = { "Pressure", "Temperature" };
                    newSlice = new TimeSlice(singlePath, idx.ToString(), pCoordinateDilation, rows, cols, gridSize,
                                             colIgnoreA, colIgnoreB, rowIgnoreB, parameters);
                    timeSlices.Add(newSlice);
                    Debug.Log($"Loaded single slice index {idx}: {singlePath}");
                }
            }
            else
            {
                // --- Sampling logic: include 0 then every 'step' ---
                int step = Mathf.Max(1, pTimeSliceDilation);

                for (int i = 0; i < fileNames.Length; i += step)
                {
                    string fullPath = fileNames[i];
                    if (!File.Exists(fullPath)) continue;

                    string[] parameters = { "Pressure", "Temperature" };
                    newSlice = new TimeSlice(fullPath, i.ToString(), pCoordinateDilation, rows, cols, gridSize,
                                             colIgnoreA, colIgnoreB, rowIgnoreB, parameters);

                    timeSlices.Add(newSlice);
                    Debug.Log($"Loaded slice index {i}: {fullPath}");
                }
            }
        });

        // rest of your existing post-processing (resource folders, setUpSlice, saveOutlineTask, updateGlobal) unchanged...
        int dirNumber = 0;
        if (!Directory.Exists("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!Directory.Exists("Assets/Resources/MESHES"))
            AssetDatabase.CreateFolder("Assets/Resources", "MESHES");

        while (Directory.Exists($"Assets/Resources/MESHES/RESULTS_{dirNumber}"))
            AssetDatabase.DeleteAsset($"Assets/Resources/MESHES/RESULTS_{dirNumber}");
        while (Directory.Exists($"Assets/Resources/MESHES/INTERIOR_{dirNumber}"))
            AssetDatabase.DeleteAsset($"Assets/Resources/MESHES/INTERIOR_{dirNumber}");

        AssetDatabase.CreateFolder("Assets/Resources/MESHES", $"RESULTS_{dirNumber}");
        AssetDatabase.CreateFolder("Assets/Resources/MESHES", $"INTERIOR_{dirNumber}");

        // Process time slices
        filesCompletionStatus = 0;
        fileNumber = timeSlices.Count;
        foreach (TimeSlice t in timeSlices)
        {
            await t.setUpSlice(dirNumber, gradient);
            filesCompletionStatus++;
            Debug.Log($"TimeSlice {filesCompletionStatus} completed out of {fileNumber}");
        }

        filesCompletionStatus = 0;
        foreach (TimeSlice t in timeSlices)
        {
            await t.saveOutlineTask();
            filesCompletionStatus++;
            Debug.Log($"Saving outlines {filesCompletionStatus} / {fileNumber}");
        }

        filesCompletionStatus = 0;
        foreach (TimeSlice t in timeSlices)
        {
            await t.updateGlobal();
            filesCompletionStatus++;
            Debug.Log($"Applying global bounds {filesCompletionStatus} / {fileNumber}");
        }

        Debug.Log($"Total slices loaded: {timeSlices.Count}");
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
