using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TimeSlice
{
    public Coordinate[,] coordinates2D;
    public List<Coordinate> coordinates1D;
    public int dilution;
    public string filePath;

    public TimeSlice(string pFilePath)
    {
        this.filePath = pFilePath;
        this.Create1DArray();
        this.Create2DArray();
    }

    public void Create2DArray()
    {
        int rows = this.countRows();
        int columns = this.countCols();
        int currentCol = 0;
        int currentRow = 0;

        Coordinate[,] coordinates = new Coordinate[rows, columns];

        for (int i = 0; i < coordinates1D.Count; i++)
        {
            Coordinate coordinate = coordinates1D[i];

            // Check if we need to move to a new row
            if (i > 0 && coordinates1D[i - 1].x < coordinate.x)
            {
                currentRow++;
                currentCol = 0; // Reset to the first column
            }
            coordinates[currentRow, currentCol] = coordinate;

            currentCol++;
        }
        this.coordinates2D = coordinates;


    }

    public void Create1DArray()
    {
        this.coordinates1D = new List<Coordinate>();
        using (var reader = new StreamReader(filePath))
        {
            reader.ReadLine();
            reader.ReadLine();
            while (reader.EndOfStream == false)
            {
                var content = reader.ReadLine();
                var splitRow = content.Split(',');

                if (RowHasData(splitRow))
                {
                    // Parse values, with defaulting to 0 for missing values
                    float x = ParseFloat(splitRow[0]);
                    float y = ParseFloat(splitRow[1]);
                    float density = ParseFloat(splitRow[2]);
                    float pressure = ParseFloat(splitRow[3]);
                    float temperature = ParseFloat(splitRow[4]);
                    float vel_x = ParseFloat(splitRow[5]);
                    float vel_y = ParseFloat(splitRow[6]);

                    // Create new Coordinate object with parsed data
                    Coordinate newCoordinate = new Coordinate(x, y, density, pressure, temperature, vel_x, vel_y);
                    coordinates1D.Add(newCoordinate);
                }

            }

        }
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

    static bool RowHasData(string[] cells)
    {
        return cells.Any(x => x.Length > 0);
    }

    public int countRows()
    {
        int rowCount = 1;
        Coordinate heldCoordinate = coordinates1D[0];
        for (int i = 0; i < coordinates1D.Count(); i++)
        {
            Coordinate iterateCoordinate = coordinates1D[i];
            if (heldCoordinate.x < iterateCoordinate.x)
            {
                heldCoordinate = iterateCoordinate;
                rowCount++;
            }

        }
        return rowCount;
    }

    public int countCols()
    {
        int maxFound = 0;
        int colCount = 1;
        Coordinate heldCoordinate = coordinates1D[0];



        for (int i = 1; i < coordinates1D.Count; i++) 
        {
            Coordinate iterateCoordinate = coordinates1D[i];

            if (heldCoordinate.x < iterateCoordinate.x)
            {
                if (colCount > maxFound)
                {
                    maxFound = colCount;
                }
                colCount = 1; 
            }
            else
            {
                colCount++;
            }
            heldCoordinate = iterateCoordinate;
        }

        if (colCount > maxFound)
        {
            maxFound = colCount;
        }
        return maxFound;
    }
}

