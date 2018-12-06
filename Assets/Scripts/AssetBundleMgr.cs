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
        : IResourceMgr
    {
        public AssetBundleMgr()
        {
        }
        public void Init()
        {
            this.InitialUpdateList();
            this.InitDependenciesData();
        }

        public void Fini()
        {
        }

        private HashSet<string> updatedFiles;
        /// <summary>
        /// 记录所有已更新的资源
        /// </summary>
        private void InitialUpdateList()
        {
            this.updatedFiles = new HashSet<string>();
            var dirInfo = new DirectoryInfo(UpdateMgr.sAssetBundlePersistentPath);
            foreach(var f in dirInfo.GetFiles("*.ab"))
            {
                this.updatedFiles.Add(f.Name);
            }
        }

        /// <summary>
        /// 检查资源是否有更新
        /// </summary>
        /// <param name="name">ab资源名。</param>
        /// <returns></returns>
        private bool isUpdated(string name)
        {
            return this.updatedFiles.Contains(name);
        }

        /// <summary>
        /// 返回资源全路径
        /// </summary>
        /// <param name="name">ab资源名</param>
        /// <returns></returns>
        private string GetPathByUpdate(string name)
        {
            string path;
            if (this.isUpdated(name))
            {
                path = Application.persistentDataPath;
            }
            else
            {
#if UNITY_EDITOR
                path = Application.dataPath;
#else
                path = Application.streamingAssetsPath;
#endif
            }

            this.sbPath.Clear();
            this.sbPath.Append(path);
            this.sbPath.Append("/");
            this.sbPath.Append(ResourcePath.kAssetBundlesPath);
            this.sbPath.Append(name);

            return this.sbPath.ToString();
        }

        private readonly StringBuilder sbPath = new StringBuilder(256);
        private string GetResourceFullPath(string name, ResourceType type)
        {
            var rname = ResourcePath.GetResourceFullPath(name, ResourceType.Prefab)
                            .Replace("/", "_")
                            .Replace(".", "_")
                            .ToLower() + ".ab";

            return this.GetPathByUpdate(rname);
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

                var path = this.GetResourceFullPath(name, ResourceType.Prefab);
                var assetbundle = AssetBundle.LoadFromFile(path);
                if (assetbundle != null)
                {
                    this.LoadDependencies(Path.GetFileName(path));

                    loadedAssetbundle = new LoadedAssetBundle(assetbundle);
                    this.LoadedAssetBundles.Add(name, loadedAssetbundle);
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
        public void LoadDependencies(string assetBundleName)
        {
            var deps = this.AssetBundleManifest.GetAllDependencies(assetBundleName);
            foreach(var dep in deps)
            {
                if (this.LoadedAssetBundles.ContainsKey(dep))
                    continue;

                var path = this.GetPathByUpdate(dep);
                var assetBundle = AssetBundle.LoadFromFile(path);
                if (assetBundle != null)
                    this.LoadedAssetBundles.Add(dep, new LoadedAssetBundle(assetBundle));
            }
        }

        private static readonly string sManifestFileName = "AssetBundles";
        private AssetBundleManifest AssetBundleManifest;
        private void InitDependenciesData()
        {
            var assetbundle = AssetBundle.LoadFromFile(this.GetPathByUpdate(sManifestFileName));
            this.AssetBundleManifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        public void LoadTexture(string name, Action<Sprite> cb)
        {
        }
    }
}
