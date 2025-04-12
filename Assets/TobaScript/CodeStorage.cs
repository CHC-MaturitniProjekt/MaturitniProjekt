using System.IO;
using UnityEngine;

public class CodeStorage : MonoBehaviour
{
    private string ScriptsDirectory => Path.Combine(Application.persistentDataPath, "TobaScripts");
    private string TobberDirectory => Path.Combine(Application.persistentDataPath, "Tobber");

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(ScriptsDirectory))
        {
            Directory.CreateDirectory(ScriptsDirectory);
        }


        if (!Directory.Exists(TobberDirectory))
        {
            Directory.CreateDirectory(TobberDirectory);
        }
    }

    public void SaveCodeAs(string name, string code)
    {
        EnsureDirectoryExists();
        string path = Path.Combine(ScriptsDirectory, name + ".tbs");
        File.WriteAllText(path, code);
    }


    public string[] GetAllTobbertScripts()
    {
        EnsureDirectoryExists();
        string[] files = Directory.GetFiles(TobberDirectory);
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        return files;
    }

    public void uploadToTobber(string name, string code)
    {
        EnsureDirectoryExists();
        string path = Path.Combine(TobberDirectory, name + ".tbs");
        File.WriteAllText(path, code);
    }

    public string loadFromTobber(string name)
    {
        string path = Path.Combine(TobberDirectory, name);
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return "";
    }

    public string LoadCodeNamed(string name)
    {
        string path = Path.Combine(ScriptsDirectory, name + ".tbs");
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return "";
    }

    public string[] GetAllScriptNames()
    {
        EnsureDirectoryExists();
        string[] files = Directory.GetFiles(ScriptsDirectory, "*.tbs");
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        return files;
    }

    public void DeleteScript(string name)
    {
        string path = Path.Combine(ScriptsDirectory, name + ".tbs");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    public void DeleteTobberScript(string name)
    {
        string path = Path.Combine(TobberDirectory, name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

}
