using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SFB; // Standalone File Browser

public class MainLoader : MonoBehaviour
{
    public Collision myCollision;

    public TMPro.TMP_Text title1;
    public TMPro.TMP_Text title2;
    public TMPro.TMP_Text title3;
    public TMPro.TMP_InputField extraInputField; 

    // Frame Rate Buttons
    public Button frameRateSingleButton;
    public Button frameRate25Button;
    public Button frameRate50Button;
    public Button frameRate100Button;
    public TMPro.TMP_InputField singleFrameInput;

    // Frame Quality Buttons
    public Button quality6Button;   // 6.25%
    public Button quality11Button;  // 11%
    public Button quality25Button;  // 25%
    public Button quality100Button; // 100%

    // Results Text Fields
    public TMPro.TMP_Text resultFrameRate;
    public TMPro.TMP_Text resultFrameQuality;

    public Button OpenFolderButton;
    public Button LoadButton;
    public TMPro.TMP_InputField fileStructure;
    public TMPro.TMP_Text foundStatus;
    public Gradient gradient;

    private enum FrameRateOption { Single, P25, P50, P100 }
    private enum QualityOption { Q6, Q11, Q25, Q100 }

    private FrameRateOption selectedFrameRate = FrameRateOption.P100;
    private QualityOption selectedQuality = QualityOption.Q25;

    private int analysisTimeSlices = 0;
    private int analysisCoordinates = 0;

    private string selectedFolderPath = "";

    public TMPro.TMP_InputField userGivenName;

    void Start()
    {
        // Frame rate button listeners
        frameRateSingleButton.onClick.AddListener(() => SelectFrameRate(FrameRateOption.Single));
        frameRate25Button.onClick.AddListener(() => SelectFrameRate(FrameRateOption.P25));
        frameRate50Button.onClick.AddListener(() => SelectFrameRate(FrameRateOption.P50));
        frameRate100Button.onClick.AddListener(() => SelectFrameRate(FrameRateOption.P100));

        // Quality button listeners
        quality6Button.onClick.AddListener(() => SelectQuality(QualityOption.Q6));
        quality11Button.onClick.AddListener(() => SelectQuality(QualityOption.Q11));
        quality25Button.onClick.AddListener(() => SelectQuality(QualityOption.Q25));
        quality100Button.onClick.AddListener(() => SelectQuality(QualityOption.Q100));

        // Single frame input listener
        if (singleFrameInput != null)
        {
            singleFrameInput.onEndEdit.AddListener((value) =>
            {
                int frame;
                if (int.TryParse(value, out frame) && frame > 0)
                {
                    selectedFrameRate = FrameRateOption.Single;
                }
                UpdateFrameRateResult();
                UpdateFrameRateButtonColors();
            });
        }

        // Folder selection button
        if (OpenFolderButton != null)
            OpenFolderButton.onClick.AddListener(OnClickOpenFolder);

        if (fileStructure != null)
            fileStructure.onEndEdit.AddListener((value) => ValidateFolder(value));

        // Disable all new UI initially
        SetNewUIInteractable(false);
        SetFolderInvalid();

        // Set defaults (100% options and color)
        UpdateFrameRateButtonColors();
        UpdateQualityButtonColors();
    }

    // ---------------------- Folder selection ----------------------
    public void OnClickOpenFolder()
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedFolderPath = paths[0];
            if (fileStructure != null) fileStructure.text = selectedFolderPath;
            ValidateFolder(selectedFolderPath);
        }
    }

    private void ValidateFolder(string pathToCheck)
    {
        if (string.IsNullOrEmpty(pathToCheck) || !Directory.Exists(pathToCheck))
        {
            SetFolderInvalid();
            return;
        }

        Analysis myAnalysis = new Analysis(pathToCheck);

        // Safely parse numbers
        analysisTimeSlices = myAnalysis.findTimeSlices();        // now returns true file count
        analysisCoordinates = myAnalysis.findDataAmount();       // rows per file

        if (foundStatus != null) foundStatus.text = myAnalysis.foundStatus();

        if (myAnalysis.validFolder())
            SetFolderValid();
        else
            SetFolderInvalid();

        // Update results immediately
        UpdateFrameRateResult();
        UpdateQualityResult();
    }

    private void SetFolderValid()
    {
        if (fileStructure != null) fileStructure.textComponent.color = Color.green;
        if (LoadButton != null) LoadButton.interactable = true;
        SetNewUIInteractable(true);

        // Enable titles + extra field
        if (title1 != null) title1.color = Color.white;
        if (title2 != null) title2.color = Color.white;
        if (title3 != null) title3.color = Color.white;
        if (resultFrameQuality != null) resultFrameQuality.color = Color.white;
        if (resultFrameRate != null) resultFrameRate.color = Color.white;

        if (extraInputField != null) extraInputField.interactable = true;

        // Force refresh highlights now that buttons are interactable
        UpdateFrameRateButtonColors();
        UpdateQualityButtonColors();
    }

    private void SetFolderInvalid()
    {
        if (fileStructure != null) fileStructure.textComponent.color = Color.red;
        if (LoadButton != null) LoadButton.interactable = false;
        SetNewUIInteractable(false);

        // Grey out titles + extra field
        Color greyedOut = new Color(0.5f, 0.5f, 0.5f, 1f);
        if (title1 != null) title1.color = greyedOut;
        if (title2 != null) title2.color = greyedOut;
        if (title3 != null) title3.color = greyedOut;
        if (resultFrameQuality != null) resultFrameQuality.color = greyedOut;
        if (resultFrameRate != null) resultFrameRate.color = greyedOut;

        if (extraInputField != null) extraInputField.interactable = false;
    }

    private void SetNewUIInteractable(bool interactable)
    {
        if (frameRateSingleButton != null) frameRateSingleButton.interactable = interactable;
        if (frameRate25Button != null) frameRate25Button.interactable = interactable;
        if (frameRate50Button != null) frameRate50Button.interactable = interactable;
        if (frameRate100Button != null) frameRate100Button.interactable = interactable;

        if (quality6Button != null) quality6Button.interactable = interactable;
        if (quality11Button != null) quality11Button.interactable = interactable;
        if (quality25Button != null) quality25Button.interactable = interactable;
        if (quality100Button != null) quality100Button.interactable = interactable;

        if (singleFrameInput != null) singleFrameInput.interactable = interactable;
    }

    // ---------------------- Frame rate / quality selection ----------------------
    private void SelectFrameRate(FrameRateOption option)
    {
        selectedFrameRate = option;
        UpdateFrameRateResult();
        UpdateFrameRateButtonColors();
    }

    private void SelectQuality(QualityOption option)
    {
        selectedQuality = option;
        UpdateQualityResult();
        UpdateQualityButtonColors();
    }

    // Helper to get frame-percent (0..1) for each option
    private float FrameRatePercent(FrameRateOption option)
    {
        switch (option)
        {
            case FrameRateOption.Single: return 0f; // single is handled separately
            case FrameRateOption.P25: return 0.25f;
            case FrameRateOption.P50: return 0.5f;
            case FrameRateOption.P100: return 1f;
            default: return 1f;
        }
    }

    // Helper to get quality-percent (0..1)
    private float QualityPercent(QualityOption option)
    {
        switch (option)
        {
            case QualityOption.Q6: return 0.0625f;   // 6.25%
            case QualityOption.Q11: return 0.11f;    // 11%
            case QualityOption.Q25: return 0.25f;    // 25%
            case QualityOption.Q100: return 1f;      // 100%
            default: return 1f;
        }
    }

    private void UpdateFrameRateResult()
    {
        int totalFrames = 0;

        if (selectedFrameRate == FrameRateOption.Single)
        {
            // single frame: only 1 frame will be rendered
            totalFrames = 1;
        }
        else
        {
            if (analysisTimeSlices > 0)
            {
                float pct = FrameRatePercent(selectedFrameRate);
                totalFrames = Mathf.CeilToInt(analysisTimeSlices * pct);
            }
        }

        if (resultFrameRate != null) resultFrameRate.text = totalFrames.ToString();
    }

    private void UpdateQualityResult()
    {
        int totalCells = 0;
        if (analysisCoordinates > 0)
        {
            float pct = QualityPercent(selectedQuality);
            totalCells = Mathf.CeilToInt(analysisCoordinates * pct);
        }
        if (resultFrameQuality != null) resultFrameQuality.text = totalCells.ToString();
    }

    private void UpdateFrameRateButtonColors()
    {
        Color selectedColor = Color.green;
        Color defaultColor = Color.white;

        List<Button> frameButtons = new List<Button>
    {
        frameRateSingleButton,
        frameRate25Button,
        frameRate50Button,
        frameRate100Button
    };

        foreach (var btn in frameButtons)
        {
            if (btn == null) continue;

            // Only manually set color if the button is interactable
            if (btn.interactable)
                btn.image.color = defaultColor;
        }

        // Reset input field background
        if (singleFrameInput != null && singleFrameInput.interactable)
        {
            var inputImage = singleFrameInput.GetComponent<Image>();
            if (inputImage != null) inputImage.color = defaultColor;
        }

        // Highlight the selected option
        switch (selectedFrameRate)
        {
            case FrameRateOption.Single:
                if (frameRateSingleButton.interactable)
                    frameRateSingleButton.image.color = selectedColor;

                if (singleFrameInput != null && singleFrameInput.interactable)
                {
                    var inputImage = singleFrameInput.GetComponent<Image>();
                    if (inputImage != null) inputImage.color = selectedColor;
                }
                break;

            case FrameRateOption.P25:
                if (frameRate25Button.interactable) frameRate25Button.image.color = selectedColor;
                break;

            case FrameRateOption.P50:
                if (frameRate50Button.interactable) frameRate50Button.image.color = selectedColor;
                break;

            case FrameRateOption.P100:
                if (frameRate100Button.interactable) frameRate100Button.image.color = selectedColor;
                break;
        }
    }

    private void UpdateQualityButtonColors()
    {
        Color selectedColor = Color.green;
        Color defaultColor = Color.white;

        List<(Button btn, QualityOption option)> qualityButtons = new List<(Button, QualityOption)>
    {
        (quality6Button, QualityOption.Q6),
        (quality11Button, QualityOption.Q11),
        (quality25Button, QualityOption.Q25),
        (quality100Button, QualityOption.Q100)
    };

        foreach (var (btn, option) in qualityButtons)
        {
            if (btn == null) continue;

            if (btn.interactable)
                btn.image.color = (selectedQuality == option) ? selectedColor : defaultColor;
        }
    }


    // ---------------------- Backend / Load ----------------------
    public async void LoadButtonClick()
    {
        // Defensive checks
        if (gradient == null)
        {
            Debug.LogError("Gradient is not assigned in the inspector!");
            return;
        }
        if (fileStructure == null || string.IsNullOrEmpty(fileStructure.text))
        {
            Debug.LogError("No folder path provided in fileStructure field!");
            return;
        }
        if (!Directory.Exists(fileStructure.text))
        {
            Debug.LogError("Provided folder path does not exist!");
            return;
        }

        // re-run analysis just before load to ensure values are fresh
        Analysis myAnalysis = new Analysis(fileStructure.text);
        analysisTimeSlices = myAnalysis.findTimeSlices();
        analysisCoordinates = myAnalysis.findDataAmount();

        if (analysisTimeSlices <= 0)
        {
            Debug.LogError("No time-slice files found in folder; aborting load.");
            return;
        }

        // Determine arguments for loader:
        int timeDilationForBackend = 1; 
        int coordinateDilationForBackend = 1;

        
        
        int singleIndex = -1; // default: not single

        // Map quality selection -> backend coordinate dilation 
        switch (selectedQuality)
        {
            case QualityOption.Q100: coordinateDilationForBackend = 1; break;
            case QualityOption.Q25: coordinateDilationForBackend = 2; break;
            case QualityOption.Q11: coordinateDilationForBackend = 3; break;
            case QualityOption.Q6: coordinateDilationForBackend = 4; break;
        }

        // Map frame rate selection -> dilation or singleIndex
        switch (selectedFrameRate)
        {
            case FrameRateOption.P100:
                timeDilationForBackend = 1;
                break;
            case FrameRateOption.P50:
                timeDilationForBackend = 2;
                break;
            case FrameRateOption.P25:
                timeDilationForBackend = 4;
                break;
            case FrameRateOption.Single:
                // Try parse the single frame input as 1-based from the user, then convert to zero-based index
                int requestedFrame = -1;
                if (singleFrameInput != null && int.TryParse(singleFrameInput.text, out requestedFrame))
                {
                    // clamp to valid range [1..analysisTimeSlices], then convert to zero-based index
                    requestedFrame = Mathf.Clamp(requestedFrame, 1, Mathf.Max(1, analysisTimeSlices));
                    singleIndex = requestedFrame - 1;
                }
                else
                {
                    Debug.LogWarning("Single frame selected but no valid number entered � defaulting to first frame (index 0).");
                    singleIndex = 0;
                }
                break;
            

        }

        UpdateFileIndex(userGivenName.text);
        StorageScroll scroll = FindObjectOfType<StorageScroll>();
        if (scroll != null) scroll.RefreshList();

        // Create collision and start loader
        int fileSize = myAnalysis.findDataAmount();
        myCollision = new Collision(fileSize, gradient);
        
        string csvPath = fileStructure.text;
        Debug.Log(userGivenName.text);
        
        await myCollision.Loader(csvPath, timeDilationForBackend, coordinateDilationForBackend, userGivenName.text, singleIndex);

        
        UpdateFrameRateResult();
        UpdateQualityResult();
    }

    private void UpdateFileIndex(string datasetName)
    {
        string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "AIV_3D");
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        string indexFilePath = Path.Combine(dirPath, "fileIndex.txt");

        int resultingTimeSlices = 0;
        int resultingCells = 0;

        int.TryParse(resultFrameRate.text, out resultingTimeSlices);
        int.TryParse(resultFrameQuality.text, out resultingCells);

        string newLine = $"name={datasetName};timeslices={resultingTimeSlices};cells={resultingCells}";

        List<string> lines = new List<string>();
        if (File.Exists(indexFilePath))
            lines.AddRange(File.ReadAllLines(indexFilePath));

        lines.RemoveAll(l => l.Contains($"name={datasetName};"));

        lines.Add(newLine);

        File.WriteAllLines(indexFilePath, lines.ToArray());

        Debug.Log($"Updated file index with dataset '{datasetName}' - TimeSlices: {resultingTimeSlices}, Cells: {resultingCells}");
    }



}
