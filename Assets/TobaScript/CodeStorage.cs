using System.IO;
using UnityEngine;

public class CodeStorage : MonoBehaviour
{
    private string ScriptsDirectory => Path.Combine(Application.persistentDataPath, "TobaScripts");

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(ScriptsDirectory))
        {
            Directory.CreateDirectory(ScriptsDirectory);
        }
    }

    public void SaveCodeAs(string name, string code)
    {
        EnsureDirectoryExists();
        string path = Path.Combine(ScriptsDirectory, name + ".tbs");
        File.WriteAllText(path, code);
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
}
