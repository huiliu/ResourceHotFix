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
                this.updatedFiles.Add(f.Name.TrimEnd('.', 'a', 'b'));
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

        private readonly StringBuilder sbPath = new StringBuilder(256);
        private string GetResourceFullPath(string name, ResourceType type)
        {
            this.sbPath.Clear();
            var rname = ResourcePath.GetResourceFullPath(name, ResourceType.Prefab).Replace("/", "_").Replace(".", "_").ToLower();
            if (this.isUpdated(rname))
            {
                this.sbPath.Append(Application.persistentDataPath);
            }
            else
            {
#if UNITY_EDITOR
                this.sbPath.Append(Application.dataPath);
#else
                this.sbPath.Append(Application.streamingAssetsPath);
#endif

            }

            this.sbPath.Append("/");
            this.sbPath.Append(ResourcePath.kAssetBundlesPath);
            this.sbPath.Append(rname);
            this.sbPath.Append(".ab");

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

                var path = this.GetResourceFullPath(name, ResourceType.Prefab);
                var assetbundle = AssetBundle.LoadFromFile(path);
                if (assetbundle != null)
                {
                    var manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    if (manifest != null)
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

        public void LoadTexture(string name, Action<Sprite> cb)
        {
        }
    }
}
