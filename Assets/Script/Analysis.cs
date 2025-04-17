using UnityEngine;
using System.IO;
using System.Linq;

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
        } else
        {
            returnString = "successfully found folder";
        }
        return returnString;
    }


    public bool validFolder()
    {
        return Directory.Exists(folderPath);
    }

    public int findTimeSlices()
    {
        int timestep = 0;
        int count = 0;

        while (true)
        {
            string fileName = $"All_mesh_data_at_timestep_{timestep}.csv";
            string fullPath = Path.Combine(folderPath, fileName);


            if (File.Exists(fullPath))
            {
                count++;
            }
            timestep++;
            if (timestep > 101) break;

        }
        return count;
    }

    public int findDataAmount()
    {
        int count = 0;
        string filePath = folderPath + "/All_mesh_data_at_timestep_0.csv";
        using (var reader = new StreamReader(filePath))
        {
            reader.ReadLine();
            reader.ReadLine();
            while (reader.EndOfStream == false)
            {
                reader.ReadLine();
                count++;
                
            }
        }
        return count-2;
    }


}
