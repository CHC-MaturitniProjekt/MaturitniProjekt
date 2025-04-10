using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class PcStorage : MonoBehaviour
{
    private string ScriptsDirectory => Path.Combine(Application.persistentDataPath, "TobaScripts");

    private void EnsureDirectoryExists(string relativeFolder = "")
    {
        string fullPath = Path.Combine(ScriptsDirectory, relativeFolder);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public void SaveCodeAs(string name, string code, string folder = "")
    {
        EnsureDirectoryExists(folder);
        string path = Path.Combine(ScriptsDirectory, folder, name + ".tbs");
        File.WriteAllText(path, code);
    }

    public string LoadCodeNamed(string name, string folder = "")
    {
        string path = Path.Combine(ScriptsDirectory, folder, name + ".tbs");
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return "";
    }

    public string[] GetAllScriptNames(string folder = "", bool includeSubfolders = false)
    {
        string scriptsDirFullPath = Path.GetFullPath(ScriptsDirectory).TrimEnd(Path.DirectorySeparatorChar);
        string fullPath = Path.GetFullPath(Path.Combine(scriptsDirFullPath, folder)).TrimEnd(Path.DirectorySeparatorChar);

        if (!fullPath.StartsWith(scriptsDirFullPath))
        {
            Debug.LogWarning("Pokus o přístup mimo ScriptsDirectory zablokován.");
            return new string[0];
        }

        if (!Directory.Exists(fullPath))
        {
            return null;
        }

        SearchOption option = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        List<string> entries = new List<string>();

        string[] directories = Directory.GetDirectories(fullPath, "*", option);
        foreach (string dir in directories)
        {
            string relativePath = dir.Substring(fullPath.Length + 1);
            entries.Add(relativePath.Replace(Path.DirectorySeparatorChar, '/'));
        }

        string[] files = Directory.GetFiles(fullPath, "*.tbs", option);
        foreach (string file in files)
        {
            string relativePath = file.Substring(fullPath.Length + 1);
            entries.Add(relativePath.Replace(Path.DirectorySeparatorChar, '/'));
        }

        entries.Sort();
        return entries.ToArray();
    }

    public void DeleteScript(string name, string folder = "")
    {
        string path = Path.Combine(ScriptsDirectory, folder, name + ".tbs");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public void DeleteFolder(string folder)
    {
        string fullPath = Path.Combine(ScriptsDirectory, folder);
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
    }

    public bool ScriptExists(string name, string folder = "")
    {
        string path = Path.Combine(ScriptsDirectory, folder, name + ".tbs");
        return File.Exists(path);
    }

    public bool FolderExists(string folder)
    {
        string fullPath = Path.Combine(ScriptsDirectory, folder);
        return Directory.Exists(fullPath);
    }

    public string CreateFolder(string folder = "")
    {
        string baseName = "New Folder";
        string newFolderName = baseName;
        string fullPath = Path.Combine(ScriptsDirectory, folder, newFolderName);
        int counter = 1;

        while (Directory.Exists(fullPath))
        {
            newFolderName = $"{baseName} ({counter})";
            fullPath = Path.Combine(ScriptsDirectory, folder, newFolderName);
            counter++;
        }

        Directory.CreateDirectory(fullPath);
        return newFolderName;
    }


    public void RenameScript(string oldName, string newName, string folder = "")
    {
        string oldPath = Path.Combine(ScriptsDirectory, folder, oldName + ".tbs");
        string newPath = Path.Combine(ScriptsDirectory, folder, newName + ".tbs");

        if (File.Exists(oldPath) && !File.Exists(newPath))
        {
            File.Move(oldPath, newPath);
        }
    }

    public void RenameFolder(string oldFolder, string newFolder)
    {
        string oldPath = Path.Combine(ScriptsDirectory, oldFolder);
        string newPath = Path.Combine(ScriptsDirectory, newFolder);

        if (Directory.Exists(oldPath) && !Directory.Exists(newPath))
        {
            Directory.Move(oldPath, newPath);
        }
    }
}
