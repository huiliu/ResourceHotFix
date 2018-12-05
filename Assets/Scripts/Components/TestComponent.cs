using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.UpdateService;

namespace Assets.Scripts.Components
{
    public class TestComponent
        : MonoBehaviour
    {
        [SerializeField] private Text StatusText;

        private UpdateMgr UpdateMgr;
        private void Awake()
        {
            this.UpdateMgr = new UpdateMgr();
        }

        public void LoadAnimationPrefab()
        {
            ResourceMgr.Instance.LoadPrefab("coin.prefab", go =>
            {
                go.transform.SetParent(this.transform, false);
                go.SetActive(true);
            });

        }

        protected void Update()
        {
            this.StatusText.text = this.UpdateMgr.UpdateStatus.ToString();
        }

        public void ReadFromWeb()
        {
            //var mgr = new UpdateService.UpdateMgr();
            //var go = new GameObject("www");
            //var text = go.AddComponent<Text>();
            //text.text = mgr.Data;

            //go.transform.SetParent(this.transform.parent, false);
        }

        public void UpdateResource()
        {
            this.StartCoroutine(this.UpdateMgr.TryUpdate());
        }
    }
}
