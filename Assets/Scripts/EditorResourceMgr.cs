using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using System.Text;
using Assets.Scripts.UpdateService;

namespace Assets.Scripts
{
    public class EditorResourceMgr
    {
        public EditorResourceMgr()
        {

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

        private Dictionary<string, Object> LoadedResource;
        public void LoadPrefab(string name, Action<GameObject> cb)
        {
            var go = null as GameObject;
            do
            {
                var path = this.GetPrefabFileName(name);
                var obj = null as Object;
                if (this.LoadedResource.TryGetValue(path, out obj))
                {
                    go = GameObject.Instantiate(obj as GameObject);
                    break;
                }

                go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null)
                {
                    break;
                }

                this.LoadedResource.Add(path, go);
                go = GameObject.Instantiate(go);
            } while (false);

            cb.SafeInvoke(go);
        }

        public void LoadSprite(string name, Action<Sprite> cb)
        {
            var go = null as Sprite;
            do
            {
                var path = this.GetTextureFileName(name);
                var obj = null as Object;
                if (this.LoadedResource.TryGetValue(path, out obj))
                {
                    go = obj as Sprite;
                    break;
                }

                go = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (go == null)
                {
                    break;
                }

                this.LoadedResource.Add(path, go);
            } while (false);

            cb.SafeInvoke(go);
        }

        private T GetFromCache<T>(string key) where T : Object
        {
            var obj = null as Object;
            this.LoadedResource.TryGetValue(key, out obj);

            return obj as T;
        }
    }
}
