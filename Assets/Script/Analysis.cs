using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class Analysis
{
    public string folderPath;

    public Analysis(string folderPath)
    {
        this.folderPath = folderPath;
    } 

    public string foundTimeSlice()
    {
        string resultString = "";
        if (validFolder())
        {
            resultString += $"{findTimeSlices()}";
        }
        return resultString;
    }

    public string foundCoordinate()
    {
        string resultString = "";
        if (validFolder())
        {
            resultString += $"{findDataAmount()}";
        }
        return resultString;
    }

    public string foundDetails()
    {
        string resultString = "";
        if (validFolder())
        {
            resultString += $"File Count (Time Slices): {findTimeSlices()} \n \n";
            resultString += $"Rows per File (Coordinates): {findDataAmount()} \n";
        }
        return resultString;
    }

    public string foundStatus()
    {
        string returnString = "";
        if (!validFolder())
        {
            returnString = "failure to find folder";
        } 
        else
        {
            returnString = "successfully found folder";
        }
        return returnString;
    }

    public bool validFolder()
    {
        return Directory.Exists(folderPath);
    }

    // ---- FIXED: return the actual number of files (time-slices) in the folder ----
    public int findTimeSlices()
    {
        if (!validFolder()) return 0;
        try
        {
            // Get all files in the folder and filter case-insensitively for .csv
            var files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                                 .Where(f => f != null && f.Length > 0 && Path.GetExtension(f).ToLowerInvariant() == ".csv")
                                 .ToArray();

            // Optionally only count files that end with digits (common timestep pattern).
            // If your files are named like "..._123.csv" this helps avoid stray CSVs.
            var trailingDigits = new Regex(@"\d+$");
            var matched = files.Where(f => trailingDigits.IsMatch(Path.GetFileNameWithoutExtension(f))).ToArray();

            // If you want to count *all* CSVs (regardless of trailing digits) use files.Length,
            // otherwise use matched.Length — below we prefer matched (safer).
            return matched.Length;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error counting files in folder '{folderPath}': {e.Message}");
            return 0;
        }
    }


    public int findDataAmount()
    {
        if (!validFolder()) return 0;

        try
        {
            string[] fileNames = Directory.GetFiles(folderPath, "*.csv", SearchOption.TopDirectoryOnly);
            if (fileNames.Length == 0) return 0;

            string filePath = fileNames[0];
            int count = 0;
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    reader.ReadLine();
                    count++;
                }
            }
            return Math.Max(0, count - 2);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading file for data amount in '{folderPath}': {e.Message}");
            return 0;
        }
    }

    public string[] listTimeSliceFiles()
    {
        if (!validFolder()) return new string[0];
        try
        {
            var files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                                 .Where(f => Path.GetExtension(f).ToLowerInvariant() == ".csv")
                                 .ToArray();

            var trailingDigits = new Regex(@"\d+$");
            var matched = files
                .Where(f => trailingDigits.IsMatch(Path.GetFileNameWithoutExtension(f)))
                .OrderBy(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    var m = Regex.Match(name, @"(\d+)$");
                    int n = 0;
                    if (m.Success) int.TryParse(m.Value, out n);
                    return n;
                })
                .ToArray();

            return matched;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error listing time-slice files in '{folderPath}': {e.Message}");
            return new string[0];
        }
    }

    // Add also an "all csvs" listing for debugging
    public string[] listAllCsvFiles()
    {
        if (!validFolder()) return new string[0];
        try
        {
            return Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                            .Where(f => Path.GetExtension(f).ToLowerInvariant() == ".csv")
                            .OrderBy(f => Path.GetFileName(f))
                            .ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error listing all CSVs in '{folderPath}': {e.Message}");
            return new string[0];
        }
    }


}
