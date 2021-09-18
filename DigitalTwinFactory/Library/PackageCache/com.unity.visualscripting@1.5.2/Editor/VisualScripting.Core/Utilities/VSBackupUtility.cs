using System;
using System.Collections.Generic;
using UnityEditor;
using Ionic.Zip;
using System.IO;
using UnityEngine;

namespace Unity.VisualScripting
{
    public class VSBackupUtility
    {
        public static void Backup()
        {
            if (!IsGeneratedFolderPresent())
            {
                Debug.Log($"Creating the Visual Scripting Generated folder for backup: '{PluginPaths.ASSETS_FOLDER_BOLT_GENERATED}'");

                try
                {
                    Directory.CreateDirectory(PluginPaths.ASSETS_FOLDER_BOLT_GENERATED);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }
            }

            BackupAssetsFolder(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
        }

        private static bool IsGeneratedFolderPresent()
        {
            string[] result = AssetDatabase.GetSubFolders(PluginPaths.ASSETS_FOLDER_BOLT_GENERATED);

            return result.Length > 0;
        }

        public static List<string> Find<T>() where T : UnityEngine.Object
        {
            List<string> assets = new List<string>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                assets.Add(assetPath);
            }

            return assets;
        }

        public static void BackupAssetsFolder(string backupLabel)
        {
            backupLabel = PathUtility.MakeSafeFilename(backupLabel, '_');

            PathUtility.CreateDirectoryIfNeeded(Paths.backups);

            var fileName = $"Assets_{backupLabel}.zip";

            var addEntryIndex = 0;
            var saveEntryIndex = 0;

            using (var zip = new ZipFile())
            {
                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;

                zip.AddProgress += (sender, e) => { EditorUtility.DisplayProgressBar("Creating Backup...", e.CurrentEntry != null ? e.CurrentEntry.FileName : "...", (float)(addEntryIndex++) / e.EntriesTotal); };

                zip.SaveProgress += (sender, e) => { EditorUtility.DisplayProgressBar("Creating Backup...", e.CurrentEntry != null ? e.CurrentEntry.FileName : "...", (float)(saveEntryIndex++) / e.EntriesTotal); };

                List<string> listOfAssets = Find<LudiqScriptableObject>();

                foreach (string assetPath in listOfAssets)
                {
                    zip.AddFile(assetPath);
                }

                var zipPath = Path.Combine(Paths.backups, fileName);

                VersionControlUtility.Unlock(zipPath);

                zip.Save(zipPath);

                EditorUtility.ClearProgressBar();
            }
        }
    }
}
