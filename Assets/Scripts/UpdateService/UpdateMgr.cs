using System.IO;
using System.Security.Cryptography;
using System.Net;
using UnityEngine;
using System;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;

namespace Assets.Scripts.UpdateService
{
    public class UpdateMgr
    {
        /// <summary>
        /// 平台
        /// </summary>
#if UNITY_EDITOR
        public const string CurrentPlatform = "windows";
#elif UNITY_IOS
        public const string CurrentPlatform = "ios";
#elif UNITY_ANDROID
        public const string CurrentPlatform = "android";
#endif

        /// <summary>
        /// 渠道
        /// </summary>
        public const string CurrentChannel = "wx";

        public const string BaseURL = "http://makeappicon.com/";
        private static string sAssetBundlePersistentPath = Application.persistentDataPath + "/AssetBundles/";

        public ResourceVersion CurrentResVersion { get; private set; }
        public UpdateMgr()
        {
            this.CurrentResVersion = new ResourceVersion();
        }

        public void TryUpdate()
        {
            this.currentRemoteVersion = this.GetRemoteResVersion();
            if (this.currentRemoteVersion == null)
            {
                // 读取失败
                return;
            }

            // 服务器资源不可降级
            System.Diagnostics.Debug.Assert(this.currentRemoteVersion <= this.CurrentResVersion.Version);

            if (this.currentRemoteVersion < this.CurrentResVersion.Version)
            {
                // 需要更新
                // TODO: start update resource
            }
        }

        private Version currentRemoteVersion;
        private Version GetRemoteResVersion()
        {
            const string versionURL = BaseURL + CurrentPlatform + "/" + CurrentChannel + "/version";
            var req = HttpWebRequest.Create(versionURL);
            var rep = req.GetResponse();

            var version = null as Version;
            using (var sr = new StreamReader(rep.GetResponseStream()))
            {
                var c = sr.ReadLine();
                if (!string.IsNullOrEmpty(c))
                    version = new Version(c);
            }

            return version;
        }

        /// <summary>
        /// 执行更新操作
        /// 
        /// 如果本地版本为1.0.0
        /// 远程已经升级为(即服务器资源已经发布了三次更新1.0.0 -> 1.0.1；1.0.1 -> 1.0.3; 1.0.3 -> 1.0.5): 1.0.5
        /// 那么升级过程为：
        /// 
        ///     1. 客户端会先请求1.0.0的升级包，完成后本地资源版本为1.0.1
        ///     2. 然后本地会再次比较资源版本，继续升级，完成后本地版本为: 1.0.3
        ///     3. 重复上面步骤直到本地版本也为1.0.5
        ///
        /// </summary>
        public void DoUpdate()
        {
            do
            {
                var r = this.DownLoadUpdatePackage(this.CurrentResVersion.Version);
                if (!r)
                {
                    // 更新失败！
                    break;
                }

                this.CurrentResVersion.ReloadVersion();
                if (this.CurrentResVersion.Version == this.currentRemoteVersion)
                {
                    // 本地版本与服务器版本一致
                    break;
                }

                /// Warning: 危险
            } while (true);
        }

        private bool DownLoadUpdatePackage(Version version)
        {
            // Version.ToString()感觉是个隐患，如果返回值不同平台，不同版本不一样会是一个麻烦
            string updateMd5Url = BaseURL + CurrentPlatform + "/" + CurrentChannel + "/" + version.ToString() + ".md5";
            var hw = HttpWebRequest.Create(updateMd5Url);
            var md5Vaule = null as string;
            using(var sr = new StreamReader(hw.GetResponse().GetResponseStream()))
            {
                md5Vaule = sr.ReadLine().Trim();
            }


            string updateUrl = BaseURL + CurrentPlatform + "/" + CurrentChannel + "/" + version.ToString();
            var httpclient = HttpWebRequest.Create(updateUrl);
            var rep = httpclient.GetResponse();

            using (var f = File.Create(Application.temporaryCachePath + "/update.zip"))
            {
                using (var s = rep.GetResponseStream())
                {
                    s.CopyTo(f);
                }

                var cmd5 = CalcStreamMD5(f);
                if (string.Compare(md5Vaule, cmd5, true) != 0)
                {
                    // MD5检验失败
                    return false;
                }

                f.Seek(0, SeekOrigin.Begin);
                this.UnzipFromStream(f);
            }

            return true;
        }

        #region 计算MD5
        // https://stackoverflow.com/questions/10520048/calculate-md5-checksum-for-a-file
        // https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
        // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?redirectedfrom=MSDN&view=netframework-4.7.2
        private static StringBuilder hashsb = new StringBuilder(32);
        private static string CalcStreamMD5(Stream s)
        {
            hashsb.Clear();

            var md5 = MD5.Create();
            var hash = md5.ComputeHash(s);
            foreach (var bt in hash)
                hashsb.Append(bt.ToString("x2"));

            return hashsb.ToString();
        }

        private static string CalcStreamMD5Another(Stream s)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(s);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
        #endregion

        private void UnzipFromStream(Stream stream)
        {
            var zStream = new ZipInputStream(stream);
            var buffer = new byte[4096];
            do
            {
                Array.Clear(buffer, 0, buffer.Length);

                var zEntry = zStream.GetNextEntry();
                if (zEntry == null)
                    break;

                var path = sAssetBundlePersistentPath + zEntry.Name;
                if (zEntry.IsDirectory)
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    continue;
                }

                var fileName = zEntry.Name;
                using(var fs=File.OpenWrite(path))
                {
                    StreamUtils.Copy(zStream, fs, buffer);
                }
            } while (true);
        }
    }
}
