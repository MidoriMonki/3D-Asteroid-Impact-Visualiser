using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class Collision
{
    public List<TimeSlice> timeSlices;
    public string folderPath;
    public int timeSliceDilation = 1;
    public int coordinateDilation = 1;

    public Collision()
    {
        timeSlices = new List<TimeSlice>();
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

        if (File.Exists(fFullPath))
        {
            newSlice = new TimeSlice(fFullPath, pCoordinateDilation);
            timeSlices.Add(newSlice);
        }

        while (true)
        {
            string fileName = $"All_mesh_data_at_timestep_{timestep}.csv";
            string fullPath = Path.Combine(pFolderPath, fileName);


            if (File.Exists(fullPath))
            {
                if (skippedCount == (pTimeSliceDilation - 1))
                {
                    newSlice = new TimeSlice(fullPath, pCoordinateDilation);
                    timeSlices.Add(newSlice);
                    skippedCount = 0;
                } else
                {
                    skippedCount++;
                }
                
            }
            timestep++;
            if (timestep > 101) break;

        }
        //Debug.Log("Total slices loaded: " + timeSlices.Count);
    }
}
