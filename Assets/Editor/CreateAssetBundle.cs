using System.IO;
using UnityEngine;
using UnityEditor;

public class CreateAssetBundle
{
    const string kAssetBundlesExportDir = "Assets/AssetBundles";

    [MenuItem("Tool/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        if (!Directory.Exists(kAssetBundlesExportDir))
        {
            Directory.CreateDirectory(kAssetBundlesExportDir);
        }

        TagPrefabs();
        TagTextures();
        BuildPipeline.BuildAssetBundles(kAssetBundlesExportDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    static void TagPrefabs()
    {
        var path = "Assets/" + ResourcePath.kPrefabPath;
        var dir = new DirectoryInfo(path);
        foreach (var f in dir.GetFiles("*.prefab"))
        {
            var rpath = ResourcePath.kPrefabPath + f.Name;
            var ai = AssetImporter.GetAtPath("Assets/" + ResourcePath.kPrefabPath + f.Name);
            ai.SetAssetBundleNameAndVariant(rpath.Replace("/", "_").Replace(".", "_"), "ab");
            ai.SaveAndReimport();
        }
    }

    static void TagTextures()
    {
        var path = "Assets/" + ResourcePath.kTexturePath;
        var dir = new DirectoryInfo(path);
        foreach (var f in dir.GetFiles("*.png"))
        {
            var rpath = ResourcePath.kTexturePath + f.Name;
            var ai = AssetImporter.GetAtPath("Assets/" + ResourcePath.kTexturePath + f.Name);
            ai.SetAssetBundleNameAndVariant(rpath.Replace("/", "_").Replace(".", "_"), "ab");
            ai.SaveAndReimport();
        }
    }
}
