using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TTCS.EditorTools
{
    /// <summary>
    /// Keeps runtime Resources mirrors in sync before a player build.
    /// DataManager loads JSON through Resources in builds, while Editor play mode
    /// can read directly from Assets/Data. This bridge prevents Editor-only success.
    /// </summary>
    public sealed class BuildResourceSync : IPreprocessBuildWithReport
    {
        private const string SourceDataRoot = "Assets/Data";
        private const string ResourcesDataRoot = "Assets/Resources/Data";
        private const string SourcePortraitRoot = "Assets/Sprites/Characters";
        private const string ResourcesPortraitRoot = "Assets/Resources/Sprites/Characters";

        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            SyncAll();
        }

        [MenuItem("TTCS/Build/Sync Runtime Resources")]
        public static void SyncAll()
        {
            SyncDirectory(SourceDataRoot, ResourcesDataRoot, "*.json");
            SyncDirectory(SourcePortraitRoot, ResourcesPortraitRoot, "*.png");
            SyncDirectory(SourcePortraitRoot, ResourcesPortraitRoot, "*.jpg");
            SyncDirectory(SourcePortraitRoot, ResourcesPortraitRoot, "*.jpeg");
            CreateSpritePathAliases();

            AssetDatabase.Refresh();
            Debug.Log("[TTCS BuildResourceSync] Synced Assets/Data and character portraits into Assets/Resources for runtime builds.");
        }

        private static void SyncDirectory(string sourceRoot, string destinationRoot, string searchPattern)
        {
            if (!Directory.Exists(sourceRoot))
            {
                Debug.LogWarning($"[TTCS BuildResourceSync] Source folder missing: {sourceRoot}");
                return;
            }

            Directory.CreateDirectory(destinationRoot);

            foreach (var sourcePath in Directory.GetFiles(sourceRoot, searchPattern, SearchOption.AllDirectories))
            {
                if (sourcePath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                var relativePath = sourcePath.Substring(sourceRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var destinationPath = Path.Combine(destinationRoot, relativePath);
                var destinationDirectory = Path.GetDirectoryName(destinationPath);

                if (!string.IsNullOrEmpty(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);

                File.Copy(sourcePath, destinationPath, overwrite: true);
            }
        }

        private static void CreateSpritePathAliases()
        {
            if (!Directory.Exists(ResourcesPortraitRoot))
                return;

            foreach (var portraitPath in Directory.GetFiles(ResourcesPortraitRoot, "*_portrait.*", SearchOption.TopDirectoryOnly))
            {
                if (portraitPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                var name = Path.GetFileNameWithoutExtension(portraitPath);
                var extension = Path.GetExtension(portraitPath);
                var aliasName = name.EndsWith("_portrait", StringComparison.OrdinalIgnoreCase)
                    ? name.Substring(0, name.Length - "_portrait".Length) + "_sprite"
                    : name + "_sprite";
                var destinationPath = Path.Combine(ResourcesPortraitRoot, aliasName + extension);
                File.Copy(portraitPath, destinationPath, overwrite: true);

                var lowerAliasPath = Path.Combine(ResourcesPortraitRoot, aliasName.ToLowerInvariant() + extension);
                File.Copy(portraitPath, lowerAliasPath, overwrite: true);
            }
        }
    }
}
