using UnityEngine;
using System.Collections;
using SFB; // Standalone File Browser

public class OpenFolder : MonoBehaviour
{
    private string selectedFolderPath = "";

    // Called by button OnClick()
    public void OnClickOpenFolder()
    {
        // Opens a folder selection dialog
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedFolderPath = paths[0];
            Debug.Log("Selected folder: " + selectedFolderPath);

            // You can now use selectedFolderPath anywhere in your code
            // Example: load files from this folder
        }
        else
        {
            Debug.Log("No folder selected.");
        }
    }
}
