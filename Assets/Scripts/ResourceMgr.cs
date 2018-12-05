using System;
using UnityEngine;

public interface IResourceMgr
{
    void Init();
    void Fini();

    void LoadPrefab(string name, Action<GameObject> cb);
    void LoadTexture(string name, Action<Sprite> cb);
}
