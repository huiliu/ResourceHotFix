using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Components
{
    public class TestComponent
        : MonoBehaviour
    {
        public void LoadAnimationPrefab()
        {
            ResourceMgr.Instance.LoadPrefab("coin.prefab", go =>
            {
                go.transform.SetParent(this.transform, false);
                go.SetActive(true);
            });

        }

        public void ReadFromWeb()
        {
            //var mgr = new UpdateService.UpdateMgr();
            //var go = new GameObject("www");
            //var text = go.AddComponent<Text>();
            //text.text = mgr.Data;

            //go.transform.SetParent(this.transform.parent, false);
        }
    }
}
