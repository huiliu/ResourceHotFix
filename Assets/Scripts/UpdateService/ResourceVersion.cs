using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.UpdateService
{
    public class ResourceVersion
    {
        // 发布包时的资源版本
        public static Version sResVersion = new Version(1, 0, 0);

        const string kVersionFile = "version.txt";
        private static readonly string sVersionFileFullPath = Application.persistentDataPath + "/AssetBundles/" + kVersionFile;

        public Version Version { get; private set; }
        public ResourceVersion()
        {
            this.Version = sResVersion;
            this.DoLoadVersionFromFile();
        }

        private void DoLoadVersionFromFile()
        {
            var c = this.LoadVersionFromFile();
            if (!string.IsNullOrEmpty(c))
                this.Version = new Version(c);
        }

        /// <summary>
        /// 更新版本号并写入文件
        /// </summary>
        /// <param name="version"></param>
        public void UpdateAndRefreshVersion(Version version)
        {
            this.Version = version;
            this.WriteVersionToFile();
        }

        /// <summary>
        /// 重新从文件加载版本号
        /// </summary>
        public void ReloadVersion()
        {
            this.DoLoadVersionFromFile();
        }

        private string LoadVersionFromFile()
        {
            if (File.Exists(sVersionFileFullPath))
            {
                using (var sr = new StreamReader(sVersionFileFullPath))
                {
                    return sr.ReadLine();
                }
            }

            return null;
        }

        private void WriteVersionToFile()
        {
            using (var sw = new StreamWriter(sVersionFileFullPath))
            {
                sw.Write(this.Version.ToString());
            }
        }
    }
}
