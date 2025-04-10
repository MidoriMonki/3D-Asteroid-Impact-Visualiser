using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MainLoader : MonoBehaviour
{
    public TMPro.TMP_InputField fileStructure;
    public string csvPath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SubmitButton()
    {
        Collision myCollision = new Collision();
        csvPath = fileStructure.text;
        myCollision.Loader(csvPath);
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

