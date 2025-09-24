using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class StorageScroll : MonoBehaviour
{
    public GameObject savedCollisionPrefab; 
    public Transform contentParent;
    public UIManager uiManager;

    private string indexFilePath;

    [System.Serializable]
    public class ProcessedFileData
    {
        public string name;
        public int timeSlices;
        public int cells;
    }

    void Start()
    {
        indexFilePath = Path.Combine(Directory.GetCurrentDirectory(), "AIV_3D/fileIndex.txt");
        LoadFileList();
    }

    void LoadFileList()
    {
        if (!File.Exists(indexFilePath))
        {
            Debug.LogWarning("No file index found at " + indexFilePath);
            return;
        }

        string[] lines = File.ReadAllLines(indexFilePath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            ProcessedFileData data = ParseLine(line);
            if (data != null)
                CreateBlock(data);
        }
    }

    ProcessedFileData ParseLine(string line)
    {
        ProcessedFileData data = new ProcessedFileData();
        string[] parts = line.Split(';');
        foreach (string part in parts)
        {
            string[] kv = part.Split('=');
            if (kv.Length != 2) continue;

            switch (kv[0].Trim())
            {
                case "name": data.name = kv[1].Trim(); break;
                case "timeslices": int.TryParse(kv[1], out data.timeSlices); break;
                case "cells": int.TryParse(kv[1], out data.cells); break;
            }
        }
        return data;
    }

    void CreateBlock(ProcessedFileData data)
    {
        GameObject block = Instantiate(savedCollisionPrefab, contentParent);

        block.transform.localScale = Vector3.one;
        block.transform.localPosition = Vector3.zero;

        // Find children in prefab
        TMP_Text nameText = block.transform.Find("Container/NameText").GetComponent<TMP_Text>();
        TMP_Text infoText = block.transform.Find("Container/InfoText").GetComponent<TMP_Text>();
        Button renderButton = block.transform.Find("Container/RenderButton").GetComponent<Button>();
        Button deleteButton = block.transform.Find("Container/DeleteButton").GetComponent<Button>();

        nameText.text = data.name;
        infoText.text = $"Time Slices: {data.timeSlices}\nCells: {data.cells}";

        renderButton.onClick.AddListener(() => OnRenderClicked(data.name));
        deleteButton.onClick.AddListener(() => DeleteFile(data.name, block));

    }

    void OnRenderClicked(string fileName)
    {
        Debug.Log("Render requested for: " + fileName);

        
        meow renderer = FindObjectOfType<meow>();
        if (renderer != null)
        {
            renderer.SetCollisionToBeRendered(fileName); 
        }

        if (uiManager != null)
            uiManager.ShowController();
    }

    public void RefreshList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        LoadFileList();
    }

    void DeleteFile(string folderName, GameObject block)
    {
        string storageDir = Path.Combine(Directory.GetCurrentDirectory(), "AIV_3D");
        string indexFilePath = Path.Combine(storageDir, "fileIndex.txt");


        if (File.Exists(indexFilePath))
        {
            List<string> lines = new List<string>(File.ReadAllLines(indexFilePath));
            lines.RemoveAll(l => l.Contains($"name={folderName};"));
            File.WriteAllLines(indexFilePath, lines.ToArray());
        }

        string folderPath = Path.Combine(storageDir, folderName);
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true); // true = delete recursively
            Debug.Log($"Deleted folder: {folderPath}");
        }



        Destroy(block);

        
    }

}
