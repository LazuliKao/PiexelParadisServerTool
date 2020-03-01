using Newtonsoft.Json.Linq;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PiexelParadisServerTool
{
    public class SSH
    {
        public struct ServerInfo
        {
            public int index;
            public string name, port, dirPath, screen;
            public SshClient ssh;
            public SftpClient sftp;
        }
        public static ServerInfo[] Servers = new ServerInfo[]
        {
                new ServerInfo() { index=0, name="登录服", port="19132",dirPath= "login", screen="nk",ssh=new SshClient("47.103.140.3", "root", "xsba123."){ KeepAliveInterval=TimeSpan.FromSeconds(30)}             ,             sftp   =null                       },
                new ServerInfo() { index=1, name="生存服",port="19132",dirPath= "survival",screen= "mc" ,ssh=new SshClient("114.67.116.188", "root", "Xsba1357@") { KeepAliveInterval = TimeSpan.FromSeconds(30)}             ,                   sftp   =null         },
                new ServerInfo() { index=2, name= "海岛服",port= "19132",dirPath= "skyland",screen= "mc" ,ssh=new SshClient("122.51.133.206", "root", "040129Gxh2004") { KeepAliveInterval = TimeSpan.FromSeconds(30)}     ,        sftp   =null           },
                new ServerInfo() { index=3, name="小游戏服",port="19130",dirPath= "games",screen= "games",   ssh=new SshClient("122.51.133.206", "root", "040129Gxh2004") { KeepAliveInterval = TimeSpan.FromSeconds(30)}    ,      sftp   =null       }
        };
        public static void SFTPinfo(ref ServerInfo Server)
        {
            if (Server.sftp == null)
            {
                Server.sftp = new SftpClient(Server.ssh.ConnectionInfo);
            }
        }
        public static void DisConnect(ref ServerInfo Server)
        {
            try
            {
                if (Server.ssh.IsConnected)
                {
                    Server.ssh.Disconnect();
                }
            }
            catch (Exception) { }
        }
        public static void DisConnectSFTP(ref ServerInfo Server)
        {
            SFTPinfo(ref Server);
            try
            {
                if (Server.sftp.IsConnected)
                {
                    Server.sftp.Disconnect();
                }
            }
            catch (Exception) { }
        }
        public static bool Connect(ref ServerInfo Server)
        {
            try
            {
                if (!Server.ssh.IsConnected)
                {
                    Server.ssh.ConnectionInfo.Timeout = TimeSpan.FromSeconds(8);
                    Server.ssh.Connect();
                }
                if (Server.ssh.IsConnected)
                { return true; }
            }
            catch (Exception) { }
            return false;
        }
        public static bool ConnectSFTP(ref ServerInfo Server)
        {
            SFTPinfo(ref Server);
            try
            {
                if (!Server.sftp.IsConnected)
                {
                    Server.sftp.ConnectionInfo.Timeout = TimeSpan.FromSeconds(8);
                    Server.sftp.Connect();
                }
                if (Server.sftp.IsConnected)
                { return true; }
            }
            catch (Exception) { }
            return false;
        }
        public static string RunShell(ref ServerInfo Server, string shellString)
        {
            if (!Server.ssh.IsConnected) { if (!Connect(ref Server)) { return "error"; } }
            SshCommand result = Server.ssh.RunCommand(shellString);
            return string.IsNullOrEmpty(result.Result) ? result.Error : result.Result;
        }
        public static void ScreenCommand(ref ServerInfo Server, string commandString)
        {
            if (!Server.ssh.IsConnected) { if (!Connect(ref Server)) { return; } }
            Server.ssh.RunCommand($"screen -x {Server.screen} -p 0 -X stuff \"{commandString.Replace("\"", "\\\"")}\"");
            Server.ssh.RunCommand($"screen -x {Server.screen} -p 0 -X stuff '\n'");
        }


        #region SFTP上传文件
        /// <summary>
        /// SFTP上传文件
        /// </summary>
        /// <param name="localPath">本地路径</param>
        /// <param name="remotePath">远程路径</param>
        public static void UploadFile(ref ServerInfo Server, string localPath, string remotePath, string fileName)
        {
            try
            {
                using (var file = File.OpenRead(localPath))
                {
                    if (!ConnectSFTP(ref Server)) { return; };
                    //判断路径是否存在
                    if (!Server.sftp.Exists(remotePath))
                    {
                        Server.sftp.CreateDirectory(remotePath);
                    }
                    Server.sftp.UploadFile(file, remotePath + (remotePath.EndsWith("/") ? null : "/") + fileName);
                    DisConnectSFTP(ref Server);
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region SFTP获取文件
        /// <summary>
        /// SFTP获取文件
        /// </summary>
        /// <param name="remotePath">远程路径</param>
        /// <param name="localPath">本地路径</param>
        public static void DownloadFile(ref ServerInfo Server, string remotePath, string localPath)
        {
            try
            {
                if (!ConnectSFTP(ref Server)) { return; };
                byte[] byt = Server.sftp.ReadAllBytes(remotePath);
                DisConnectSFTP(ref Server);

                if (!Directory.Exists(Path.GetDirectoryName(localPath))) { Directory.CreateDirectory(Path.GetDirectoryName(localPath)); }
                File.WriteAllBytes(localPath, byt);
            }
            catch (Exception) { }
        }
        #endregion

        //    #region 获取SFTP文件列表
        //    /// <summary>
        //    /// 获取SFTP文件列表
        //    /// </summary>
        //    /// <param name="remotePath">远程目录</param>
        //    /// <param name="fileSuffix">文件后缀</param>
        //    /// <returns></returns>
        //    public ArrayList GetFileList(string remotePath, string fileSuffix)
        //    {
        //        try
        //        {
        //            Connect();
        //            var files = sftp.ListDirectory(remotePath);
        //            Disconnect();
        //            var objList = new ArrayList();
        //            foreach (var file in files)
        //            {
        //                string name = file.Name;
        //                if (name.Length > (fileSuffix.Length + 1) && fileSuffix == name.Substring(name.Length - fileSuffix.Length))
        //                {
        //                    objList.Add(name);
        //                }
        //            }
        //            return objList;
        //        }
        //        catch (Exception ex)
        //        {
        //            // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP文件列表获取失败，原因：{0}", ex.Message));
        //            throw new Exception(string.Format("SFTP文件列表获取失败，原因：{0}", ex.Message));
        //        }
        //    }
        //    #endregion

        //    #region 移动SFTP文件
        //    /// <summary>
        //    /// 移动SFTP文件
        //    /// </summary>
        //    /// <param name="oldRemotePath">旧远程路径</param>
        //    /// <param name="newRemotePath">新远程路径</param>
        //    public void Move(string oldRemotePath, string newRemotePath)
        //    {
        //        try
        //        {
        //            Connect();
        //            sftp.RenameFile(oldRemotePath, newRemotePath);
        //            Disconnect();
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(string.Format("SFTP文件移动失败，原因：{0}", ex.Message));
        //        }
        //    }
        //    #endregion

        //    #region 删除SFTP文件
        //    public void Delete(string remoteFile)
        //    {
        //        try
        //        {
        //            Connect();
        //            sftp.Delete(remoteFile);
        //            Disconnect();
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(string.Format("SFTP文件删除失败，原因：{0}", ex.Message));
        //        }
        //    }
        //    #endregion

        //    #region 创建目录
        //    /// <summary>
        //    /// 循环创建目录
        //    /// </summary>
        //    /// <param name="remotePath">远程目录</param>
        //    private void CreateDirectory(string remotePath)
        //    {
        //        try
        //        {
        //            string[] paths = remotePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        //            string curPath = "/";
        //            for (int i = 0; i < paths.Length; i++)
        //            {
        //                curPath += paths[i];
        //                if (!sftp.Exists(curPath))
        //                {
        //                    sftp.CreateDirectory(curPath);
        //                }
        //                if (i < paths.Length - 1)
        //                {
        //                    curPath += "/";
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(string.Format("创建目录失败，原因：{0}", ex.Message));
        //        }
        //    }
        //    #endregion
        //}
    }
}
