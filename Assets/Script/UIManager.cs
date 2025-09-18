using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject openingScreen;    // Canvas 1
    public GameObject fileLoaderScreen; // Canvas 2
    public GameObject controllerScreen; // Canvas 3
    public GameObject hiddenScreen;
    public Camera mainCamera;           
    public meow controllerRenderer;

    void Start()
    {
        ShowOpening(); // Start on opening screen
        controllerRenderer.enabled = false;
        hiddenScreen.SetActive(false);
    }

    public void ShowOpening()
    {
        openingScreen.SetActive(true);
        fileLoaderScreen.SetActive(false);
        controllerScreen.SetActive(false);

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = new Color(0f, 0f, 0.2f);
        }
        controllerRenderer.enabled = false;
    }

    public void ShowFileLoader()
    {
        openingScreen.SetActive(false);
        fileLoaderScreen.SetActive(true);
        controllerScreen.SetActive(false);

        if (mainCamera != null)
        {
            // Light blue main menu
            mainCamera.backgroundColor = new Color32(0xD2, 0xE3, 0xF3, 0xFF);
        }
        controllerRenderer.enabled = false;
    }

    public void ShowController()
    {
        openingScreen.SetActive(false);
        fileLoaderScreen.SetActive(false);
        controllerScreen.SetActive(true);

        if (mainCamera != null)
        {
            // Dark navy
            mainCamera.backgroundColor = new Color(0f, 0f, 0.2f);
        }
        controllerRenderer.enabled = true;
    }
}
