using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SFB; // Standalone File Browser

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
    public Button OpenFolderButton;
    public Gradient gradient;

    public TMPro.TMP_Text resultTimeSlice;
    public TMPro.TMP_Text resultCoordinate;

    public TMPro.TMP_InputField fileStructure;
    public TMPro.TMP_Text foundTimeSlice;
    public TMPro.TMP_Text foundCoordinate;
    public TMPro.TMP_Text foundStatus;
    public Button LoadButton;
    public string csvPath;

    private string selectedFolderPath = "";

    void Start()
    {
        LoadButton.interactable = false;
        timeSliceDilationDisplay.interactable = false;
        coordinateDilationDisplay.interactable = false;
        TimeSliceUp.interactable = false;
        TimeSliceDown.interactable = false;
        CoordinateUp.interactable = false;
        CoordinateDown.interactable = false;

        if (OpenFolderButton != null)
            OpenFolderButton.onClick.AddListener(OnClickOpenFolder);

        // Automatically validate when the input field is changed
        fileStructure.onEndEdit.AddListener((value) => ValidateFolder(value));
    }

    // ---------------------- Folder selection ----------------------
    public void OnClickOpenFolder()
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedFolderPath = paths[0];
            fileStructure.text = selectedFolderPath; // Update input field
            Debug.Log("Selected folder: " + selectedFolderPath);

            ValidateFolder(selectedFolderPath);
        }
        else
        {
            Debug.Log("No folder selected.");
        }
    }

    // ---------------------- Folder validation ----------------------
    private void ValidateFolder(string pathToCheck)
    {
        if (string.IsNullOrEmpty(pathToCheck) || !Directory.Exists(pathToCheck))
        {
            SetFolderInvalid();
            return;
        }

        Analysis myAnalysis = new Analysis(pathToCheck);
        foundTimeSlice.text = myAnalysis.foundTimeSlice();
        foundCoordinate.text = myAnalysis.foundCoordinate();
        foundStatus.text = myAnalysis.foundStatus();

        if (myAnalysis.validFolder())
            SetFolderValid();
        else
            SetFolderInvalid();
    }

    private void SetFolderValid()
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

    private void SetFolderInvalid()
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

    // ---------------------- Time & Coordinate updates ----------------------
    public void TimeSliceResultUpdate()
    {
        if (int.TryParse(foundTimeSlice.text, out int foundValue))
        {
            int result = (foundValue / timeSliceDilation) + 1;
            if (result > foundValue) result = foundValue;
            resultTimeSlice.text = result.ToString();
        }
        else resultTimeSlice.text = "Invalid";
    }

    public void CoordinateResultUpdate()
    {
        if (int.TryParse(foundCoordinate.text, out int foundValue))
        {
            int result = foundValue / (coordinateDilation * coordinateDilation);
            resultCoordinate.text = result.ToString();
        }
        else resultCoordinate.text = "Invalid";
    }

    public void TimeSliceDilationUpdate()
    {
        if (int.TryParse(timeSliceDilationDisplay.text, out int parsedValue) && parsedValue > 0)
            timeSliceDilation = parsedValue;
        else
        {
            timeSliceDilation = 1;
            timeSliceDilationDisplay.text = "1";
        }
        TimeSliceResultUpdate();
    }

    public void CoordinateDilationUpdate()
    {
        if (int.TryParse(coordinateDilationDisplay.text, out int parsedValue) && parsedValue > 0)
            coordinateDilation = parsedValue;
        else
        {
            coordinateDilation = 1;
            coordinateDilationDisplay.text = "1";
        }
        CoordinateResultUpdate();
    }

    public void TimeSliceDilationUp()
    {
        if (timeSliceDilation < 9999)
        {
            timeSliceDilation++;
            timeSliceDilationDisplay.text = timeSliceDilation.ToString();
        }
        TimeSliceResultUpdate();
    }

    public void TimeSliceDilationDown()
    {
        if (timeSliceDilation > 1)
        {
            timeSliceDilation--;
            timeSliceDilationDisplay.text = timeSliceDilation.ToString();
        }
        TimeSliceResultUpdate();
    }

    public void CoordinateDilationUp()
    {
        if (coordinateDilation < 9999)
        {
            coordinateDilation++;
            coordinateDilationDisplay.text = coordinateDilation.ToString();
        }
        CoordinateResultUpdate();
    }

    public void CoordinateDilationDown()
    {
        if (coordinateDilation > 1)
        {
            coordinateDilation--;
            coordinateDilationDisplay.text = coordinateDilation.ToString();
        }
        CoordinateResultUpdate();
    }

    // ---------------------- Load ----------------------
    public async void LoadButtonClick()
    {
        string pathToLoad = string.IsNullOrEmpty(fileStructure.text) ? selectedFolderPath : fileStructure.text;

        Analysis myAnalysis = new Analysis(pathToLoad);
        myCollision = new Collision(myAnalysis.findDataAmount(), gradient);
        csvPath = pathToLoad;
        await myCollision.Loader(csvPath, timeSliceDilation, coordinateDilation);
    }
}
