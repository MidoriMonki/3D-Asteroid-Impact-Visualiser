using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject openingScreen;    // Canvas 1
    public GameObject fileLoaderScreen; // Canvas 2
    public GameObject controllerScreen; // Canvas 3
    public GameObject hiddenScreen;
    public Camera mainCamera;
    public meow controllerRenderer;

    [Header("Note Toggle UI")]
    public GameObject notePanel;        
    public Button noteToggleButton;     
    public TMPro.TMP_Text noteToggleText; 

    void Start()
    {
        ShowOpening(); // Start on opening screen
        hiddenScreen.SetActive(false);

        if (notePanel != null)
            notePanel.SetActive(false); // Start hidden

        if (noteToggleButton != null)
            noteToggleButton.onClick.AddListener(ToggleNotePanel);

        UpdateNoteButtonText();
    }

    public void ShowOpening()
    {
        openingScreen.SetActive(true);
        fileLoaderScreen.SetActive(false);
        controllerScreen.SetActive(false);

        if (mainCamera != null)
            mainCamera.backgroundColor = new Color(0f, 0f, 0.2f);

        controllerRenderer.enabled = false;
    }

    public void ShowFileLoader()
    {
        openingScreen.SetActive(false);
        fileLoaderScreen.SetActive(true);
        controllerScreen.SetActive(false);

        if (mainCamera != null)
            mainCamera.backgroundColor = new Color(0f, 0f, 0.2f);

        controllerRenderer.enabled = false;
    }

    public void ShowController()
    {
        openingScreen.SetActive(false);
        fileLoaderScreen.SetActive(false);
        controllerScreen.SetActive(true);

        if (mainCamera != null)
            mainCamera.backgroundColor = new Color(0f, 0f, 0.2f);

        controllerRenderer.enabled = true;
    }

    public void ToggleNotePanel()
    {
        if (notePanel == null) return;

        bool isActive = notePanel.activeSelf;
        notePanel.SetActive(!isActive);

        UpdateNoteButtonText();
    }

    private void UpdateNoteButtonText()
    {
        if (noteToggleText == null) return;

        if (notePanel != null && notePanel.activeSelf)
            noteToggleText.text = "Hide Note";
        else
            noteToggleText.text = "Show Note";
    }
}
