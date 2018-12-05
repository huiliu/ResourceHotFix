using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.UpdateService;
using UnityEngine;

namespace Assets.Scripts
{
    public class LoadedAssetBundle
    {
        public AssetBundle AssetBundle { get; private set; }
        public int ReferencedCount { get; set; }

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            this.AssetBundle = assetBundle;
            this.ReferencedCount = 1;
        }
    }

    public class AssetBundleMgr
    {
        public AssetBundleMgr()
        {

        }

        private HashSet<string> updatedFiles;
        private void InitialUpdateList()
        {
            this.updatedFiles = new HashSet<string>();
            var dirInfo = new DirectoryInfo(UpdateMgr.sAssetBundlePersistentPath);
            foreach(var f in dirInfo.GetFiles("*.ab"))
            {
                this.updatedFiles.Add(f.Name);
            }
        }

        private readonly StringBuilder sbPath = new StringBuilder(256);
        private string GetPrefabFileName(string name)
        {
            this.sbPath.Clear();
            this.sbPath.Append(ResourcePath.PrefabPath);
            this.sbPath.Append(name);
            this.sbPath.Append(".prefab");

            return this.sbPath.ToString();
        }

        private string GetTextureFileName(string name)
        {
            this.sbPath.Clear();
            this.sbPath.Append(ResourcePath.TexturePath);
            this.sbPath.Append(name);
            this.sbPath.Append(".png");

            return this.sbPath.ToString();
        }

        public void LoadPrefab(string name, Action<GameObject> cb)
        {
            var loadedAssetbundle = null as LoadedAssetBundle;
            do
            {
                if (this.LoadedAssetBundles.TryGetValue(name, out loadedAssetbundle))
                {
                    break;
                }

                var path = UpdateMgr.sAssetBundlePersistentPath + "_prefab.ab";
                var assetbundle = AssetBundle.LoadFromFile(path);
                if (assetbundle != null)
                {
                    var manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    this.LoadDependencies(manifest, name);

                    this.LoadedAssetBundles.Add(name, new LoadedAssetBundle(assetbundle));
                }
            } while (false);

            var go = null as GameObject;
            if (loadedAssetbundle != null)
            {
                go = GameObject.Instantiate(loadedAssetbundle.AssetBundle.LoadAsset<GameObject>(name));
            }

            cb.SafeInvoke(go);
        }

        private Dictionary<string, LoadedAssetBundle> LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
        public void LoadDependencies(AssetBundleManifest manifest, string assetBundleName)
        {
            var deps = manifest.GetAllDependencies(assetBundleName);
            foreach(var dep in deps)
            {
                if (this.LoadedAssetBundles.ContainsKey(dep))
                    continue;

                var assetBundle = AssetBundle.LoadFromFile(Path.Combine(UpdateMgr.sAssetBundlePersistentPath, dep));
                if (assetBundle != null)
                    this.LoadedAssetBundles.Add(dep, new LoadedAssetBundle(assetBundle));
            }
        }
    }
}
