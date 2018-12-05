using System;
using System.Collections;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using UnityEngine.Networking;

using Version = System.Version;

namespace Assets.Scripts.UpdateService
{
    public enum UpdateStatus
    {
        CheckRemoteVersion,
        Download,
        Finished,
        Error,
    }

    public class UpdateMgr
    {
        /// <summary>
        /// 平台
        /// </summary>
#if UNITY_EDITOR
        public const string kCurrentPlatform = "windows";
#elif UNITY_IOS
        public const string kCurrentPlatform = "ios";
#elif UNITY_ANDROID
        public const string kCurrentPlatform = "android";
#else
        public const string kCurrentPlatform = "windows";
#endif

        /// <summary>
        /// 渠道
        /// </summary>
        public const string CurrentChannel = "wx";

        public const string kBaseURL = "http://makeappicon.online/";
        public static readonly string sAssetBundlePersistentPath = Application.persistentDataPath + "/AssetBundles/";

        public UpdateStatus UpdateStatus { get; private set; }
        public ResourceVersion CurrentResVersion { get; private set; }
        public UpdateMgr()
        {
            this.CurrentResVersion = new ResourceVersion();
        }

        public IEnumerator TryUpdate()
        {
            this.UpdateStatus = UpdateStatus.CheckRemoteVersion;
            yield return this.GetRemoteResVersion();

            if (this.currentRemoteVersion == null)
            {
                // 读取远程版本信息失败
                this.UpdateStatus = UpdateStatus.Error;
            }
            else if (this.currentRemoteVersion != null &&
                this.currentRemoteVersion > this.CurrentResVersion.Version)
            {
                this.UpdateStatus = UpdateStatus.Download;

                yield return this.DownLoadUpdatePackage();
            }
            else if (this.currentRemoteVersion != null &&
                this.currentRemoteVersion == this.CurrentResVersion.Version)
            {
                this.UpdateStatus = UpdateStatus.Finished;
            }
        }

        private Version currentRemoteVersion;
        private IEnumerator GetRemoteResVersion()
        {
            do
            {
                var versionURL = kBaseURL + kCurrentPlatform + "/" + CurrentChannel + "/version";
                var www = UnityWebRequest.Get(versionURL);
                yield return www.SendWebRequest();

                if (www.isHttpError ||
                    www.isNetworkError ||
                    www.downloadedBytes == 0)
                {
                    break;
                }

                var version = www.downloadHandler.data.ToAsciiString();
                if (string.IsNullOrEmpty(version))
                {

                    break;
                }

                try
                {
                    this.currentRemoteVersion = new Version(version);
                }
                catch (Exception err)
                {
                    // 版本格式不正确
                    break;
                }
            } while (false);
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
        private IEnumerator DownLoadUpdatePackage()
        {
            do
            {
                var url = kBaseURL + kCurrentPlatform + "/" + CurrentChannel + "/" + this.CurrentResVersion.Version.ToString();
                var www = UnityWebRequest.Get(url + ".md5");
                yield return www.SendWebRequest();

                if (www.isHttpError ||
                    www.isNetworkError ||
                    www.downloadedBytes == 0)
                {
                    // 下载错误
                    this.UpdateStatus = UpdateStatus.Error;
                    break;
                }

                var md5Value = www.downloadHandler.data.ToAsciiString().Trim();
                if (string.IsNullOrEmpty(md5Value))
                {
                    this.UpdateStatus = UpdateStatus.Error;
                    break;
                }

                www = UnityWebRequest.Get(url + ".zip");
                yield return www.SendWebRequest();

                if (www.isHttpError ||
                    www.isNetworkError ||
                    www.downloadedBytes == 0)
                {
                    // 下载错误
                    this.UpdateStatus = UpdateStatus.Error;
                    break;
                }

                using (var ms = new MemoryStream(www.downloadHandler.data))
                {
                    var md5 = Utils.CalcStreamMD5(ms);
                    if (string.Compare(md5Value, md5, true) != 0)
                    {
                        // MD5检验失败
                        this.UpdateStatus = UpdateStatus.Error;
                        break;
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    this.UnzipFromStream(ms);
                }

                this.CurrentResVersion.ReloadVersion();
                if (this.CurrentResVersion.Version == this.currentRemoteVersion)
                {
                    // 本地版本与服务器版本一致
                    this.UpdateStatus = UpdateStatus.Finished;
                    break;
                }
            } while (true);
        }

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
