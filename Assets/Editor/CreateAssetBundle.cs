using System.IO;
using UnityEngine;
using UnityEditor;

public class CreateAssetBundle
{

    [MenuItem("Tool/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        BuildAllPrefabs();

        if (!Directory.Exists(ResourceMgr.AssetBundleDirectory))
        {
            Directory.CreateDirectory(ResourceMgr.AssetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(ResourceMgr.AssetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    static void BuildAllPrefabs()
    {
        var prefabPath = "Assets/" + ResourceMgr.PrefabPath;
        var dir = new DirectoryInfo(prefabPath);
        foreach (var f in dir.GetFiles("*.prefab"))
        {
            var rpath = prefabPath + f.Name;
            var ai = AssetImporter.GetAtPath(rpath);
            ai.SetAssetBundleNameAndVariant(rpath.Replace("/", "."), "ab");
            ai.SaveAndReimport();
        }

        BuildPipeline.BuildAssetBundles(ResourceMgr.AssetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
}
