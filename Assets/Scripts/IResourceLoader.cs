using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    public interface IResourceLoader
    {
        void Load<T>(string name, Action<T> cb);
    }

    //public class AssetBundleLoader
    //    : IResourceLoader
    //{
    //    public void Load<T>(string name, Action<T> cb)
    //    {
    //    }
    //}

    //public class EditorResourceLoader
    //    : IResourceLoader
    //{
    //    private Dictionary<string, Object> LoadedResources = new Dictionary<string, Object>();
    //    public void Load<T>(string name, Action<T> cb)
    //    {
    //        var obj = null as Object;
    //        do
    //        {
    //            if (this.LoadedResources.TryGetValue())
    //        }
    //    }
    //}
}
