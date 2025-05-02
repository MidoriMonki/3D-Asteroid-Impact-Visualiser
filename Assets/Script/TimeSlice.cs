using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst;
using System;
using Unity.Collections;

public class TimeSlice
{
    public Coordinate[,] coordinates;
    public List<Vector3> verticeList = new List<Vector3>();
    public int dilution;
    public int outlineLength;
    public string filePath;
    public int gridSize = 0;
    public int cellsPerRow;
    public int cellsPerCol;
    public int ignoreColA;
    public int ignoreColB;
    public int ignoreRowB;

    public TimeSlice(string pFilePath, int pDilution, int cellsPerCol, int cellsPerRow, int gridSize, int ignoreColA, int ignoreColB, int ignoreRowB)
    {
        this.dilution = pDilution;
        this.filePath = pFilePath;
        this.cellsPerRow = cellsPerRow;
        this.cellsPerCol = cellsPerCol;
        this.gridSize = gridSize;
        this.ignoreColA = ignoreColA;
        this.ignoreColB = ignoreColB;
        this.ignoreRowB = ignoreRowB;
        //createOutline();
        //this.Create1DArray();
    }
    /*
    public struct Data
    {
        public NativeArray<Coordinate> coordinates;
        public int dilution;
        public string filePath;
        public float gridSize;
        public int rows;
        public int cols;

        public Data(TimeSlice t)
        {
            this.dilution = t.dilution;
            this.filePath = t.filePath;
            this.rows = t.rows;
            cols = t.cols;
            gridSize = t.gridSize;
            coordinates = new NativeArray<Coordinate>();
            //coordinates = new Coordinate[(int)cols / dilution, (int)rows / dilution];
            //createOutline();
            //this.Create1DArray();
        }

        public void Create2DArray(int fileSize)
        {
            //put entries into 2D array
            //UnmanagedMemoryStream
            using (var reader = new UnmanagedMemoryStream(filePath, )) 
            {
                int colDil = (int)cols / dilution;
                int rowDil = (int)rows / dilution;
                int jDil = (dilution - 1);
                int iDil = (jDil) * (rows / (jDil + 1));

                Debug.Log("colDil: " + colDil + "\nrowDil: " + rowDil);
                //coordinates = new Coordinate[colDil, rowDil];

                var content = "0,0,0";//reader.ReadLine();
                //reader.ReadLine();
                //read file for all entries, putting them into an array
                for (int i = 0; i < colDil; i++)
                {
                    //dilute
                    for (int k = 0; k < iDil; k++)
                    {
                        //reader.ReadLine();
                    }
                    for (int j = 0; j < rowDil; j++)
                    {
                        //dilute
                        for (int k = 0; k < jDil; k++)
                        {
                            //reader.ReadLine();
                        }
                        //content = reader.ReadLine();
                        var splitRow = content.Split(',');
                        float x = ParseFloat(splitRow[0]);
                        float y = ParseFloat(splitRow[1]);
                        float density = ParseFloat(splitRow[2]);
                        //float pressure = ParseFloat(splitRow[3]);
                        //float temperature = ParseFloat(splitRow[4]);
                        //float vel_x = ParseFloat(splitRow[5]);
                        //float vel_y = ParseFloat(splitRow[6]);
                        coordinates[i*rowDil + j] = (new Coordinate(x, y, density));
                    }
                }
                //reader.Close();
                reader.Dispose();
                coordinates.Dispose();
            }
        }
        */
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



    public void createOutline()
    {
        //get the now updated number of cols and rows
        int cols = coordinates.GetLength(0);
        int rows = coordinates.GetLength(1);
        //rows = (int)rows / dilution;
        int grid = 1;
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if(coordinates[i, j] != null)
                {
                    if (i > 0 && coordinates[i - 1, j] == null)
                    {
                        verticeList.Add(new Vector3(0, grid * j, grid * i));
                    }
                    else if (i < cols - 1 && coordinates[i + 1, j] == null)
                    {
                        verticeList.Add(new Vector3(0, grid * j, grid * i));
                    }
                    else if (j < rows - 1 && coordinates[i, j + 1] == null)
                    {
                        verticeList.Add(new Vector3(0, grid * j, grid * i));
                    }
                    else if (j > 0 && coordinates[i, j - 1] == null)
                    {
                        verticeList.Add(new Vector3(0, grid * j, grid * i));
                    }
                }
            }
        }
        coordinates = null;

        //Debug.Log("Done :)");
    }

    public void Create2DArray()
    {
        String temp = "";
        Debug.Log(cellsPerCol/dilution + ", " + cellsPerRow/dilution + ", " + gridSize);
        coordinates = new Coordinate[cellsPerRow / dilution, cellsPerCol / dilution];
        //read file for all entries, putting them into an array
        using (var reader = new StreamReader(filePath))
        {
            var content = reader.ReadLine();
            reader.ReadLine();
            for (int i = 0; i < cellsPerRow / dilution; i++)
            {
                //dilute
                for (int k = 0; k < (dilution-1) * cellsPerCol; k++)
                {
                    reader.ReadLine();
                }
                for (int j = 0; j < cellsPerCol / dilution; j++)
                {
                    //dilute
                    for (int k = 0; k < dilution-1; k++)
                    {
                        reader.ReadLine();
                    }
                    content = reader.ReadLine();
                    var splitRow = content.Split(',');
                    float density = ParseFloat(splitRow[2]);
                    float pressure = ParseFloat(splitRow[3]);
                    float temperature = ParseFloat(splitRow[4]);
                    //float vel_x = ParseFloat(splitRow[5]);
                    //float vel_y = ParseFloat(splitRow[6]);
                    if (density == 0)
                    {
                        coordinates[i, j] = null;
                        temp += "0, ";
                    }
                    else
                    {
                        coordinates[i, j] = new Coordinate(pressure, temperature);
                        temp += "1, ";
                        //temp += density+", ";
                    }
                }
                Debug.Log(temp);
                temp = "";
            }
        }
        createOutline();
    }
        /* --------
         public void Create2DArray(){
             /*int cellsPerRow = -1;// = this.countRows();
             int cellsPerCol = 0;// = this.countCols();
             fileSize = -1;

             //find gridsize, rows, and cols
             using (var reader = new StreamReader(filePath)){
                 int content = 0;
                 int tempContent = 0;
                 //read file until first x changes by gridsize for the first time
                 reader.ReadLine();
                 reader.ReadLine();
                 while(content == 0.0f){
                     content = (int)ParseFloat(reader.ReadLine().Split(',')[0]);
                     cellsPerRow++;
                     fileSize++;
                 }
                 tempContent = content;
                 while(!(tempContent > content + gridSize)){
                     content = tempContent;
                     tempContent = (int)ParseFloat(reader.ReadLine().Split(',')[0]);
                     fileSize++;
                 }
                 cellsPerCol = fileSize / cellsPerRow;
                 reader.Close();
             }*/
        /* --------
         //int rows = 0;
         //int cols = 0;
         //put entries into 2D array
         using (var reader = new StreamReader(filePath))
         {
             //int colDil = (int)cols/dilution;
             //int rowDil = (int)rows/dilution;
             //int jDil = (dilution-1);
             //int iDil = (jDil) * (rows/(jDil+1));
             //String temp = "";
             //Debug.Log("colDil: " + colDil + "\nrowDil: " + rowDil);
             //coordinates = new Coordinate[100, 100];
             //coordinates = new Coordinate[1350, 1620];
             coordinates = new Coordinate[450, 540];
             var content = reader.ReadLine();
             reader.ReadLine();
             //read file for all entries, putting them into an array
             /*
             for (int i = 0; i < colDil; i++){
                 //dilute
                 for (int k = 0; k < iDil; k++){
                     reader.ReadLine();
                 }
                 for (int j = 0; j < rowDil; j++){
                     //dilute
                     for (int k = 0; k < jDil; k++){
                         reader.ReadLine();
                     }
                     content = reader.ReadLine();
                     var splitRow = content.Split(',');
                     float density = ParseFloat(splitRow[2]);
                     float pressure = ParseFloat(splitRow[3]);
                     float temperature = ParseFloat(splitRow[4]);
                     //float vel_x = ParseFloat(splitRow[5]);
                     //float vel_y = ParseFloat(splitRow[6]);
                     if (density == 0)
                     {
                         coordinates[i, j] = null;
                         temp += "0, ";
                     }
                     else
                     {
                         coordinates[i, j] = new Coordinate(density, pressure, temperature);
                         temp += "1, ";
                         //temp += density+", ";
                     }
                 }
                 Debug.Log(temp);
                 temp = "";
             }*/

        /* Explaining how the following code segment works:
         * - Ignore the stretched colums in the data, these are points outside of the simulation
         * - Repeat the following for all colums that will survive the diluting:
         * - Dilute following colums
         * - Apply dilution to our colum rows
         * - Save those points that weren't diluted
         * - Keep a count of how far we have travelled the col, don't want to overstep into rows to be ignored (stretched rows)
         * - Read the file until we reach the next colum, repeat the process (This next colum will be the first to be diluted)
        */
        /* --------
        //First, get rid of the ones to ignore (far most left colums)
        //number of colums to ignore on left side * ( cells per col + cells per col to be ignored )
        Debug.Log(cellsPerCol + ", " + cellsPerRow + ", " + ignoreColA + ", " + ignoreRowB);
        for(int i = 0; i < ignoreColA * (cellsPerCol + ignoreRowB); i++){
            reader.ReadLine();
        }
        String balls = "";
        //repeat for each undiluted col
        for (int j = 0; j < cellsPerRow / dilution; j++){
            //Apply dilution to cols (skip cols)
            //May have to ensure no overstepping i'm not too sure...
            for (int k = 0; k < (cellsPerCol + ignoreRowB); k++){
                reader.ReadLine();
            }
            //go through next col
            for (int i = 0; i < cellsPerCol / dilution; i++) {
                int count = 0; //make sure we don't overstep with diluting
                if (count < cellsPerCol / dilution + dilution)
                {
                    //Dilute through the col (skip rows)
                    for (int k = 0; k < dilution - 1; k++)
                    {
                        reader.ReadLine();
                        count++;
                    }
                    content = reader.ReadLine();
                    count++;
                    var splitRow = content.Split(',');
                    float density = ParseFloat(splitRow[2]);
                    float pressure = ParseFloat(splitRow[3]);
                    float temperature = ParseFloat(splitRow[4]);
                    if (density == 0)
                    {
                        coordinates[j, i] = null;
                        balls += "0,";
                    }
                    else
                    {
                        coordinates[j, i] = new Coordinate(pressure, temperature);
                        balls += "1,";
                    }
                }
                else
                {
                    //readfile until we reach next colum, then start over
                    while (count < cellsPerCol + ignoreColB)
                    {
                        reader.ReadLine();
                        count++;
                    }
                }
            }
            Debug.Log(balls);
            balls = "";
        }
        reader.Close();
        createOutline();
    }
    }
    -------- */

        /*
        public void Create1DArray()
        {
            this.coordinates1D = new List<Coordinate>();
            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine();
                reader.ReadLine();
                while (reader.EndOfStream == false)
                {
                    for (int i = 0; i < (dilution - 1) ; i++)
                    {
                        reader.ReadLine();
                    }
                    var content = reader.ReadLine();
                    if (content == null) break;
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
        }*/




        /*
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
        */
        /*
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
        }*/
}

