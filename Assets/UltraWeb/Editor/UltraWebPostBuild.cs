using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;

public class UltraWebPostBuild
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.StandaloneWindows && target != BuildTarget.StandaloneWindows64)
        {
            return;
        }

        string buildDir = Path.GetDirectoryName(pathToBuiltProject);
        string dataDirName = Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data";
        string targetPluginDir = Path.Combine(buildDir, dataDirName, "Plugins", "x86_64");

        string sourceDir = Path.Combine(Application.dataPath, "UltraWeb/Plugins/x86_64");
        string resourcesSourceDir = Path.Combine(Application.dataPath, "UltraWeb/Resources");
        string resourcesTargetDir = Path.Combine(targetPluginDir, "resources");


        if (!Directory.Exists(sourceDir))
        {
            return;
        }

        CopyDirectoryFiltered(sourceDir, targetPluginDir, ".meta");

        if (Directory.Exists(resourcesSourceDir))
        {
            CopyDirectoryFiltered(resourcesSourceDir, resourcesTargetDir, ".meta");
        }

    }

    private static void CopyDirectoryFiltered(string sourceDir, string targetDir, string excludeExtension)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var filePath in Directory.GetFiles(sourceDir))
        {
            if (Path.GetExtension(filePath).Equals(excludeExtension, System.StringComparison.OrdinalIgnoreCase))
                continue;

            string fileName = Path.GetFileName(filePath);
            string destFile = Path.Combine(targetDir, fileName);
            File.Copy(filePath, destFile, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(dir);
            string newTargetDir = Path.Combine(targetDir, dirName);
            CopyDirectoryFiltered(dir, newTargetDir, excludeExtension);
        }
    }
}
