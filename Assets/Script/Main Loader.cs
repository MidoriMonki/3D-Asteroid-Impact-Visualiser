using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MainLoader : MonoBehaviour
{
    public int timeSliceDilation = 1;
    public int coordinateDilation = 1;
    public TMPro.TMP_Text timeSliceDilationDisplay;
    public TMPro.TMP_Text coordinateDilationDisplay;


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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TimeSliceDilationUp()
    {
        if (!(timeSliceDilation == 10)) //code upper bound
        {
            timeSliceDilation++;
            timeSliceDilationDisplay.text = timeSliceDilation.ToString();
        }
    }

    public void TimeSliceDilationDown()
    {
        if (!(timeSliceDilation == 1)) 
        {
            timeSliceDilation--;
            timeSliceDilationDisplay.text = timeSliceDilation.ToString();
        }

    }

    public void CoordinateDilationUp()
    {
        if (!(coordinateDilation == 10))
        {
            coordinateDilation++;
            coordinateDilationDisplay.text = coordinateDilation.ToString();
        }
    }

    public void CoordinateDilationDown()
    {
        if (!(coordinateDilation == 1))
        {
            coordinateDilation--;
            coordinateDilationDisplay.text = coordinateDilation.ToString();
        }
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

        }
        else
        {
            fileStructure.textComponent.color = Color.red;
            LoadButton.interactable = false;
        }   
    }

    public void LoadButtonClick()
    {
        Collision myCollision = new Collision();
        csvPath = fileStructure.text;
        myCollision.Loader(csvPath, timeSliceDilation, coordinateDilation);
        TestCollisionData(myCollision);
    }

    void TestCollisionData(Collision collision)
    {
        foreach (var timeSlice in collision.timeSlices)
        {
            Debug.Log($"Testing TimeSlice from file: {timeSlice.filePath}");
            foreach (var coordinate in timeSlice.coordinates2D)
            {
                if (coordinate != null)
                {
                    Debug.Log($"Coordinate: x = {coordinate.x}, y = {coordinate.y}, " +
                          $"Density = {coordinate.density}, Pressure = {coordinate.pressure}, " +
                          $"Temperature = {coordinate.temperature}, Vel_x = {coordinate.vel_x}, Vel_y = {coordinate.vel_y}");
                }
                
            }
        }
    }



}

