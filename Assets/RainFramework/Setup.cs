#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

using static UnityEditor.AssetDatabase;
using static System.IO.Path;
using static System.IO.Directory;

namespace RainFramework
{
    public static class Setup
    {
        static string RootDirectory = "_Project";

        [MenuItem("Rain Framework/Setup/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
            Folders.CreateDefault(RootDirectory, "Models", "Materials", "Animations", 
                "Scripts", "ScriptableObjects", "Settings");
            Refresh();
        }

        static class Folders
        {
            public static void CreateDefault(string root, params string[] folders)
            {
                var fullPath = Combine(Application.dataPath, root);
                foreach (var folder in folders)
                {
                    var path = Combine(fullPath, folder);
                    if (!Exists(path))
                    {
                        CreateDirectory(path);
                    }
                }
            }
        }
    }
}

#endif