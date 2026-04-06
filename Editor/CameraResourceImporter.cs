#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Handles automatic copying of assets and include files from the Core package
/// into the project's <c>Assets/Resources</c> directory. This ensures assets are properly
/// included in Unity builds and accessible at runtime via <see cref="Resources.Load"/>.
/// </summary>
[InitializeOnLoad]
public static class CameraResourceImporter
{
    /// <summary>
    /// Name of the importer module.
    /// </summary>
    private const string importerName = "CameraResourceImporter";

    /// <summary>
    /// Path used when developing the package locally (under the Assets folder).
    /// </summary>
    private const string DevelopmentAssetsPath = "Assets/Core/UI/unity-camera-controller/Runtime/Assets/";

    /// <summary>
    /// Path used when the package is installed via UPM.
    /// </summary>
    private const string PackageAssetsPath = "Packages/com.pihkura.unity-camera-controller/Runtime/Assets/";

    /// <summary>
    /// Target directory where assets will be copied for inclusion in builds.
    /// </summary>
    private const string TargetResourcesPath = "Assets/Resources/Pihkura/unity-camera-controller/Assets";

    /// <summary>
    /// Supported asset file extensions to be copied.
    /// </summary>
    private static readonly string[] AssetExtensions = new[]
    {
        ".inputactions"
    }; 

    /// <summary>
    /// Static constructor initializes the importer and schedules asset synchronization on editor load.
    /// </summary>
    static CameraResourceImporter()
    {
        EditorApplication.delayCall += EnsureAssetsCopied;
    }

    /// <summary>
    /// Ensures that all relevant asset files from the source package or development path
    /// are copied to the target Resources folder. Updates existing files if modified.
    /// </summary>
    private static void EnsureAssetsCopied()
    {
        try
        {
            string AssetsPath = GetAssetSourcePath();
            if (!Directory.Exists(AssetsPath))
            {
                Debug.LogWarning($"[{importerName}] Package asset path not found: {AssetsPath}");
                return;
            }

            if (!Directory.Exists(TargetResourcesPath))
                Directory.CreateDirectory(TargetResourcesPath);

            foreach (var ext in AssetExtensions)
            {
                string[] files = Directory.GetFiles(AssetsPath, $"*{ext}", SearchOption.AllDirectories);
                foreach (string srcFile in files)
                {
                    // Create corresponding subdirectories mirroring the package structure
                    string relativePath = srcFile.Replace(AssetsPath, "");
                    string destFile = Path.Combine(TargetResourcesPath, relativePath);
                    string destDir = Path.GetDirectoryName(destFile)!;

                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    // Copy only if file is new or modified
                    if (!File.Exists(destFile) ||
                        File.GetLastWriteTimeUtc(srcFile) > File.GetLastWriteTimeUtc(destFile))
                    {
                        File.Copy(srcFile, destFile, true);
                        Debug.Log($"[{importerName}] Copied asset: {relativePath}");
                    }
                }
            }

            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[{importerName}] Failed to copy assets: {ex.Message}");
        }
    }

    /// <summary>
    /// Determines whether to use the local development asset path or the installed UPM package path.
    /// Logs a warning if neither exists.
    /// </summary>
    /// <returns>The asset source path string.</returns>
    private static string GetAssetSourcePath()
    {
        if (Directory.Exists(DevelopmentAssetsPath))
            return DevelopmentAssetsPath;
        if (Directory.Exists(PackageAssetsPath))
            return PackageAssetsPath;

        Debug.LogWarning($"[{importerName}] Asset source folder not found.");
        return DevelopmentAssetsPath; // fallback
    }
}
#endif
