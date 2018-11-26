using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMgr
{
    public const string ScenePath = "Scenes/";
    public const string PrefabPath = "Prefabs/";
    public const string TexturePath = "Textures/";
    public const string AssetBundleDirectory = "Assets/AssetBundles/";

    private bool useBundle = true;
    private static ResourceMgr instance = new ResourceMgr();
    public static ResourceMgr Instance { get { return instance; } }

    public void LoadPrefab(string name, Action<GameObject> cb)
    {
        var path = "Assets/Prefabs/" + name + ".ab";
        if (this.useBundle)
        {
            var ab = AssetBundle.LoadFromFile(AssetBundleDirectory + path.Replace("/", "."));
            var go = ab.LoadAsset<GameObject>(name);

            cb.Invoke(GameObject.Instantiate(go));
        }
    }
}
