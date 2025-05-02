using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MainLoader : MonoBehaviour
{
    public Collision myCollision;

    public int timeSliceDilation = 1;
    public int coordinateDilation = 1;
    public TMPro.TMP_InputField timeSliceDilationDisplay;
    public TMPro.TMP_InputField coordinateDilationDisplay;

    public Button TimeSliceUp;
    public Button TimeSliceDown;
    public Button CoordinateUp;
    public Button CoordinateDown;

    public TMPro.TMP_Text resultTimeSlice;
    public TMPro.TMP_Text resultCoordinate;

    public TMPro.TMP_InputField fileStructure;
    public TMPro.TMP_Text foundTimeSlice;
    public TMPro.TMP_Text foundCoordinate;
    public TMPro.TMP_Text foundStatus;
    public Button LoadButton;
    public string csvPath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadButton.interactable = false;
        timeSliceDilationDisplay.interactable = false;
        coordinateDilationDisplay.interactable = false;
        TimeSliceUp.interactable = false;
        TimeSliceDown.interactable = false;
        CoordinateUp.interactable = false;
        CoordinateDown.interactable = false;

    }

    public void TimeSliceResultUpdate()
    {

        int foundValue;
        if (int.TryParse(foundTimeSlice.text, out foundValue))
        {
            int result = (foundValue / timeSliceDilation) +1;
            if (result > foundValue)
            {
                result = foundValue;
            }
            resultTimeSlice.text = result.ToString();
        }
        else
        {
            resultTimeSlice.text = "Invalid";
        }
    }

    public void CoordinateResultUpdate()
    {

        int foundValue;
        if (int.TryParse(foundCoordinate.text, out foundValue))
        {
            int result = foundValue / (coordinateDilation*coordinateDilation);
            resultCoordinate.text = result.ToString();
        }
        else
        {
            resultCoordinate.text = "Invalid";
        }
    }

    public void TimeSliceDilationUpdate()
    {
        int parsedValue;
        if (int.TryParse(timeSliceDilationDisplay.text, out parsedValue) && parsedValue > 0)
        {
            timeSliceDilation = parsedValue;
        }
        else
        {
            timeSliceDilation = 1;
            timeSliceDilationDisplay.text = "1";
        }

        TimeSliceResultUpdate();
        //resultTimeSlice.text = timeSliceDilation;
    }

    public void CoordinateDilationUpdate()
    {
        int parsedValue;
        if (int.TryParse(coordinateDilationDisplay.text, out parsedValue) && parsedValue > 0)
        {
            coordinateDilation = parsedValue;
        }
        else
        {
            coordinateDilation = 1;
            coordinateDilationDisplay.text = "1";
        }
        CoordinateResultUpdate();
    }

    public void TimeSliceDilationUp()
    {
        if (!(timeSliceDilation == 9999)) //code upper bound
        {
            timeSliceDilation += 1;
            timeSliceDilationDisplay.text = timeSliceDilation.ToString();
        }
        TimeSliceResultUpdate();
    }

    public void TimeSliceDilationDown()
    {
        if (!(timeSliceDilation == 1)) 
        {
            timeSliceDilation--;
            timeSliceDilationDisplay.text = timeSliceDilation.ToString();
        }
        TimeSliceResultUpdate();
    }

    public void CoordinateDilationUp()
    {
        if (!(coordinateDilation == 9999))
        {
            coordinateDilation+= 1;
            coordinateDilationDisplay.text = coordinateDilation.ToString();
        }
        CoordinateResultUpdate();
    }

    public void CoordinateDilationDown()
    {
        if (!(coordinateDilation == 1))
        {
            coordinateDilation--;
            coordinateDilationDisplay.text = coordinateDilation.ToString();
        }
        CoordinateResultUpdate();
    }

    public void FindButton()
    {
        Analysis myAnalysis = new Analysis(fileStructure.text);
        foundTimeSlice.text = myAnalysis.foundTimeSlice();
        foundCoordinate.text = myAnalysis.foundCoordinate();
        foundStatus.text = myAnalysis.foundStatus();
        if (myAnalysis.validFolder())
        {
            fileStructure.textComponent.color = Color.green;
            LoadButton.interactable = true;
            timeSliceDilationDisplay.interactable = true;
            coordinateDilationDisplay.interactable = true;
            TimeSliceUp.interactable = true;
            TimeSliceDown.interactable = true;
            CoordinateUp.interactable = true;
            CoordinateDown.interactable = true;

        }
        else
        {
            fileStructure.textComponent.color = Color.red;
            LoadButton.interactable = false;
            timeSliceDilationDisplay.interactable = false;
            coordinateDilationDisplay.interactable = false;
            TimeSliceUp.interactable = false;
            TimeSliceDown.interactable = false;
            CoordinateUp.interactable = false;
            CoordinateDown.interactable = false;
        }   
    }

    public void LoadButtonClick()
    {
        Analysis myAnalysis = new Analysis(fileStructure.text);
        myCollision = new Collision(myAnalysis.findDataAmount());
        csvPath = fileStructure.text;
        myCollision.Loader(csvPath, timeSliceDilation, coordinateDilation);
        TestCollisionData(myCollision);
    }

    void TestCollisionData(Collision collision)
    {
        /*
        foreach (var timeSlice in collision.timeSlices)
        {
            Debug.Log($"Testing TimeSlice from file: {timeSlice.filePath}");
            Debug.Log(timeSlice.coordinates.GetLength(1));
            for (int i = 0; i < timeSlice.coordinates.GetLength(0); i++)
            {
                for (int j = 0; j < timeSlice.coordinates.GetLength(1); j++)
                {
                    Coordinate coordinate = timeSlice.coordinates[i, j];
                    Debug.Log(coordinate);
                    if (coordinate != null)
                    {
                        Debug.Log($"Coordinate: x = {timeSlice.gridSize * i}, y = {timeSlice.gridSize * j}, " +
                              $"Density = {coordinate.density}, Pressure = {coordinate.pressure}, " +
                              $"Temperature = {coordinate.temperature}, Vel_x = {coordinate.vel_x}, Vel_y = {coordinate.vel_y}");
                    }
                }
            }
        }*/
        Debug.Log("Guess it worked ayy");
        foundStatus.text = "Done bro.";
    }



}

