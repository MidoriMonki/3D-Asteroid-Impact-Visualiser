using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class Collision
{
    public List<TimeSlice> timeSlices;
    public string folderPath;

    public Collision()
    {
        timeSlices = new List<TimeSlice>();
    }

    public void AddTimeSlice(TimeSlice slice)
    {
        timeSlices.Add(slice);
    }

    public void Loader(string pFolderPath)
    {
        folderPath = pFolderPath;
        int timestep = 0;
        

        while (true)
        {
            string fileName = $"All_mesh_data_at_timestep_{timestep}.csv";
            string fullPath = Path.Combine(pFolderPath, fileName);


            if (File.Exists(fullPath))
            {
                Debug.Log("Found: " + fileName);
                TimeSlice newSlice = new TimeSlice(fullPath);
                timeSlices.Add(newSlice);
            }
            else
            {
                Debug.Log("No file found :(");
            }
            timestep++;
            if (timestep > 101) break;

        }
        //Debug.Log("Total slices loaded: " + timeSlices.Count);
    }
}
