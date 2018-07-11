﻿using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LuaFramework;

public class Packager {
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();

    ///-----------------------------------------------------------
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle", ".json" };
    static bool CanCopy(string ext) {   //能不能复制
        foreach (string e in exts) {
            if (ext.Equals(e)) return true;
        }
        return false;
    }

    /// <summary>
    /// 载入素材
    /// </summary>
    //static UnityEngine.Object LoadAsset(string file) {
    //    if (file.EndsWith(".lua")) file += ".txt";
    //    return AssetDatabase.LoadMainAssetAtPath("Assets/LuaFramework/Examples/Builds/" + file);
    //}

    [MenuItem("LuaFramework/Build iPhone Resource", false, 100)]
    public static void BuildiPhoneResource() {
        BuildTarget target;
#if UNITY_5
        target = BuildTarget.iOS;
#else
        target = BuildTarget.iOS;
#endif
        BuildAssetResource(target);
    }

    [MenuItem("LuaFramework/Build Android Resource", false, 101)]
    public static void BuildAndroidResource() {
        BuildAssetResource(BuildTarget.Android);
    }

    [MenuItem("LuaFramework/Build Android Resource And Copy", false, 102)]
    public static void BuildAndroidResourceAndCopy()
    {
        BuildAndroidResource();
        CopyToServerFolder(AppConst.GetStreamingAssetsTargetPathByPlatform(RuntimePlatform.Android), "E:/Apache24/htdocs/AndroidStreamingAssets");
        UnityEngine.Debug.Log("Copy Succeed!");
    }

    public static void CopyFolder(string strFromPath, string strToPath, bool isCopySelf=true)
    {
        strFromPath = strFromPath.Replace('\\', '/');
        strToPath = strToPath.Replace('\\', '/');
        //如果源文件夹不存在，则创建
        if (!Directory.Exists(strFromPath))
        {
            Directory.CreateDirectory(strFromPath);
        }
        //取得要拷贝的文件夹名
        string strFolderName = string.Empty;
        if (isCopySelf)
        {
            strFolderName = strFromPath.Substring(strFromPath.LastIndexOf("/") + 1, strFromPath.Length - strFromPath.LastIndexOf("/") - 1);
            //如果目标文件夹中没有源文件夹则在目标文件夹中创建源文件夹
            if (!Directory.Exists(strToPath + "/" + strFolderName))
            {
                Directory.CreateDirectory(strToPath + "/" + strFolderName);
            }
        }
        else
        {
            if (!Directory.Exists(strToPath))
            {
                Directory.CreateDirectory(strToPath);
            }
        }
        //创建数组保存源文件夹下的文件名
        string[] strFiles = Directory.GetFiles(strFromPath);
        //循环拷贝文件
        for (int i = 0; i < strFiles.Length; i++)
        {
            //取得拷贝的文件名，只取文件名，地址截掉。
            string strFileName = Path.GetFileName(strFiles[i]);
            //开始拷贝文件,true表示覆盖同名文件
            if (isCopySelf)
                File.Copy(strFiles[i], strToPath + "/" + strFolderName + "/" + strFileName, true);
            else
                File.Copy(strFiles[i], strToPath + "/" + strFileName, true);

        }
        //创建DirectoryInfo实例
        DirectoryInfo dirInfo = new DirectoryInfo(strFromPath);
        //取得源文件夹下的所有子文件夹名称
        DirectoryInfo[] ZiPath = dirInfo.GetDirectories();
        for (int j = 0; j < ZiPath.Length; j++)
        {
            //把得到的子文件夹当成新的源文件夹，从头开始新一轮的拷贝
            CopyFolder(ZiPath[j].FullName, strToPath + "/" + ZiPath[j].Name, false);
        }
    }

    private static void CopyToServerFolder(string fromPath, string toPath)
    {
        if (Directory.Exists(toPath)) 
            Directory.Delete(toPath, true);
        CopyFolder(fromPath, toPath, false);
    }

    [MenuItem("LuaFramework/Build Windows Resource", false, 103)]
    public static void BuildWindowsResource() {
        BuildAssetResource(BuildTarget.StandaloneWindows);
    }

    public static RuntimePlatform BuildTargetToPlatform(BuildTarget target)
    {
        if (target == BuildTarget.StandaloneWindows)
            return RuntimePlatform.WindowsEditor;
        else if (target == BuildTarget.Android)
            return RuntimePlatform.Android;
        else if (target == BuildTarget.iOS)
            return RuntimePlatform.IPhonePlayer;
        else
            return RuntimePlatform.WindowsEditor;
    }

    /// <summary>
    /// 生成绑定素材
    /// </summary>
    public static void BuildAssetResource(BuildTarget target) {
        string streamPath = AppConst.GetStreamingAssetsTargetPathByPlatform(BuildTargetToPlatform(target));
        if (Directory.Exists(streamPath))
        {
            Directory.Delete(streamPath, true);
        }

        if (Directory.Exists(streamPath))
        {
            Directory.Delete(streamPath, true);
        }
        Directory.CreateDirectory(streamPath);
        AssetDatabase.Refresh();

        maps.Clear();

        //把Lua文件先存放在一个临时文件夹,然后针对此文件夹打包
        string dataPath = Application.dataPath.Replace("/Assets", "");
        string tempLuaDir = dataPath + "/" + AppConst.LuaTempDir;
        if (AppConst.LuaBundleMode)
            HandleLuaBundle(tempLuaDir);
        else
            HandleLuaFile(streamPath);
        if (AppConst.SprotoBinMode)
            HandleSprotoBundle(streamPath);


        HandleUIBundles();

        BuildPipeline.BuildAssetBundles(streamPath, maps.ToArray(), BuildAssetBundleOptions.None, target);
        BuildFileIndex(streamPath);

        if (Directory.Exists(tempLuaDir)) Directory.Delete(tempLuaDir, true);
        AssetDatabase.Refresh();
    }

    static void AddBuildMap(string bundleName, string pattern, string path) {
        string[] files = Directory.GetFiles(path, pattern);
        if (files.Length == 0) return;

        for (int i = 0; i < files.Length; i++) {
            files[i] = files[i].Replace('\\', '/');
        }
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = bundleName;
        build.assetNames = files;
        maps.Add(build);
    }

    /// <summary>
    /// 处理Lua代码包
    /// </summary>
    static void HandleLuaBundle(string tempLuaDir) {
        //string tempLuaDir = AppConst.AppDataPath.ToLower() + "/"+AppConst.LuaTempDir+"/";
        if (!Directory.Exists(tempLuaDir)) Directory.CreateDirectory(tempLuaDir);

        string[] srcDirs = { AppConst.LuaAssetsDir };
        for (int i = 0; i < srcDirs.Length; i++) {
            if (AppConst.LuaByteMode) {
                string sourceDir = srcDirs[i];
                string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
                int len = sourceDir.Length;

                if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\') {
                    --len;
                }
                for (int j = 0; j < files.Length; j++) {
                    string str = files[j].Remove(0, len);
                    string dest = tempLuaDir + str + ".bytes";
                    string dir = Path.GetDirectoryName(dest);
                    Directory.CreateDirectory(dir);
                    EncodeLuaFile(files[j], dest);
                }    
            } else {
                ToLuaMenu.CopyLuaBytesFiles(srcDirs[i], tempLuaDir);
            }
        }
        string[] dirs = Directory.GetDirectories(tempLuaDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++) {
            string name = dirs[i].Replace(tempLuaDir, string.Empty);
            name = name.Replace('\\', '_').Replace('/', '_');
            name = "lua/lua_" + name.ToLower() + AppConst.ExtName;

            string path = "Assets" + dirs[i].Replace(Application.dataPath, "");
            AddBuildMap(name, "*.bytes", path);
        }
        AddBuildMap("lua/lua" + AppConst.ExtName, "*.bytes", AppConst.LuaTempDir);

        //-------------------------------处理非Lua文件----------------------------------
        string luaPath = AppConst.AppDataPath.ToLower() + "/StreamingAssets/lua/";
        for (int i = 0; i < srcDirs.Length; i++) {
            paths.Clear(); files.Clear();
            string luaDataPath = srcDirs[i].ToLower();
            Recursive(luaDataPath);
            foreach (string f in files) {
                if (f.EndsWith(".meta") || f.EndsWith(".lua")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string path = Path.GetDirectoryName(luaPath + newfile);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string destfile = path + "/" + Path.GetFileName(f);
                File.Copy(f, destfile, true);
            }
        }
        AssetDatabase.Refresh();
    }

    public static void HandleUIBundles(string dataPath="")
    {
        string ui_path = "Assets/AssetBundleRes/ui/";
        string ui_prefab_path = ui_path + "prefab";
        string ui_texture_path = ui_path + "texutre";
        string[] ui_dirs = Directory.GetDirectories(ui_prefab_path);
        if (ui_dirs.Length == 0)
            return;
        for (int i = 0; i < ui_dirs.Length; i++)
        {
            string asset_name = "ui_prefab_" + Path.GetFileName(ui_dirs[i]);

            List<string> prefab_list = new List<string>();//预制文件列表
            paths.Clear(); files.Clear(); Recursive(ui_dirs[i]);
            foreach (string f in files)
            {
                string name = Path.GetFileName(f);
                string ext = Path.GetExtension(f);
                if (ext.Equals(".prefab"))
                {
                    prefab_list.Add(f);
                    prefab_list.Add(f + ".meta");
                }
            }
            if (prefab_list.Count > 0)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = asset_name + AppConst.ExtName;

                //DeleteUICache(dataPath, build.assetBundleName);

                build.assetNames = prefab_list.ToArray();
                maps.Add(build);

                //string temp = asset_name.ToLower();
                //assets_list.Add(build.assetBundleName);
            }
        }
        ui_dirs = Directory.GetDirectories(ui_texture_path);
        if (ui_dirs.Length == 0)
            return;
        for (int i = 0; i < ui_dirs.Length; i++)
        {
            string asset_name = "ui_texture_" + Path.GetFileName(ui_dirs[i]);

            List<string> asset_list = new List<string>();//资源文件列表
            paths.Clear(); files.Clear(); Recursive(ui_dirs[i]);
            foreach (string f in files)
            {
                string name = Path.GetFileName(f);
                string ext = Path.GetExtension(f);
                if (ext.Equals(".png"))
                {
                    asset_list.Add(f);
                    asset_list.Add(f + ".meta");
                }
            }
            if (asset_list.Count > 0)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = asset_name + "_asset" + AppConst.ExtName;

                //DeleteUICache(dataPath, build.assetBundleName);

                build.assetNames = asset_list.ToArray();
                maps.Add(build);

                //string temp = asset_name.ToLower();
                //assets_list.Add(build.assetBundleName);
            }
        }

        //string single_path = "Assets/" + AppConst.AppName + "/AssetBundleRes/ui/alphaSingle";
        //paths.Clear(); files.Clear(); Recursive(single_path);
        //foreach (string f in files)
        //{
        //    string file_name = Path.GetFileNameWithoutExtension(f);
        //    string abName = "alphaSingle_" + file_name + AppConst.ExtName;
        //    DeleteUICache(dataPath, abName);

        //    AddBuildMap(abName, "*.png", f);
        //    assets_list.Add(abName);
        //}
    }
    /// <summary>
    /// 处理框架实例包
    /// </summary>
    //static void HandleExampleBundle() {
    //    string resPath = AppDataPath + "/" + AppConst.AssetDir + "/";
    //    if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);

    //    AddBuildMap("prompt" + AppConst.ExtName, "*.prefab", "Assets/LuaFramework/Examples/Builds/Prompt");
    //    AddBuildMap("message" + AppConst.ExtName, "*.prefab", "Assets/LuaFramework/Examples/Builds/Message");

    //    AddBuildMap("prompt_asset" + AppConst.ExtName, "*.png", "Assets/LuaFramework/Examples/Textures/Prompt");
    //    AddBuildMap("shared_asset" + AppConst.ExtName, "*.png", "Assets/LuaFramework/Examples/Textures/Shared");
    //}

    //[MenuItem("Test/Build Sproto BinFile")]
    public static void TestHandleSprotoBundle()
    {
        string streamPath = AppConst.StreamingAssetsTargetPath;
        HandleSprotoBundle(streamPath);
    }

    public static void HandleSprotoBundle(string streamPath)
    {
        string tool_path = Application.dataPath.Replace("/Assets", "") + "/Tools/sprotodumper/";
        string names = Util.GetFileNamesInFolder(AppConst.LuaAssetsDir + "/Common/Proto", " ");

        Process p = new Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
        p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
        p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
        p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
        p.StartInfo.CreateNoWindow = true;//不显示程序窗口
        p.Start();//启动程序
        //向cmd窗口发送输入信息
        p.StandardInput.WriteLine("cd \\");
        p.StandardInput.WriteLine("cd I:");
        p.StandardInput.WriteLine(@"cd "+ tool_path);
        p.StandardInput.WriteLine("lua sprotodumper.lua " + names+ "&exit");
        
        p.StandardInput.AutoFlush = true;
        //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
        //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令
        //获取cmd窗口的输出信息
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();//等待程序执行完退出进程
        p.Close();

        if (output.Contains("succeed!"))
        {
            UnityEngine.Debug.Log("generate sproto spb succeed!");
            File.Copy(tool_path + "/sproto_c2s.spb", streamPath + "/sproto_c2s.spb");
            File.Copy(tool_path + "/sproto_s2c.spb", streamPath + "/sproto_s2c.spb");
        }
        else
        {
            UnityEngine.Debug.Log(output);
            UnityEngine.Debug.Log("generate sproto spb failed! please check up line for detail");
        }
    }

    /// <summary>
    /// 处理Lua文件
    /// </summary>
    static void HandleLuaFile(string toPath)
    {
        string resPath = toPath;
        string luaPath = resPath + "/lua/";

        //----------复制Lua文件----------------
        if (!Directory.Exists(luaPath)) {
            Directory.CreateDirectory(luaPath); 
        }
        string rootPath = Application.dataPath.Replace("/Assets", "");
        string[] luaPaths = { rootPath + "/Lua/",};

        for (int i = 0; i < luaPaths.Length; i++) {
            paths.Clear();
            files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            int n = 0;
            foreach (string f in files) {
                if (f.EndsWith(".meta")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string newpath = luaPath + newfile;
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (File.Exists(newpath)) {
                    File.Delete(newpath);
                }
                if (AppConst.LuaByteMode) {
                    EncodeLuaFile(f, newpath);
                } else {
                    File.Copy(f, newpath, true);
                }
                UpdateProgress(n++, files.Count, newpath);
            } 
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    static void BuildFileIndex(string toPath) {
        string resPath = toPath;
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++) {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close(); fs.Close();
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path) {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names) {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs) {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    static void UpdateProgress(int progress, int progressMax, string desc) {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    public static void EncodeLuaFile(string srcFile, string outFile) {
        if (!srcFile.ToLower().EndsWith(".lua")) {
            File.Copy(srcFile, outFile, true);
            return;
        }
        bool isWin = true; 
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        string dataPath = Application.dataPath.Replace("/Assets", "");
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            isWin = true;
            luaexe = "luajit.exe";
            args = "-b -g " + srcFile + " " + outFile;
            exedir = dataPath + "Tools/LuaEncoder/luajit/";
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
            isWin = false;
            luaexe = "./luajit";
            args = "-b -g " + srcFile + " " + outFile;
            exedir = dataPath + "Tools/LuaEncoder/luajit_mac/";
        }
        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.UseShellExecute = isWin;
        info.ErrorDialog = true;
        Util.Log(info.FileName + " " + info.Arguments);

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    //[MenuItem("LuaFramework/Build Protobuf-lua-gen File")]
    //public static void BuildProtobufFile() {
    //    if (!AppConst.ExampleMode) {
    //        UnityEngine.Debug.LogError("若使用编码Protobuf-lua-gen功能，需要自己配置外部环境！！");
    //        return;
    //    }
    //    string dir = AppDataPath + "/Lua/3rd/pblua";
    //    paths.Clear(); files.Clear(); Recursive(dir);

    //    string protoc = "d:/protobuf-2.4.1/src/protoc.exe";
    //    string protoc_gen_dir = "\"d:/protoc-gen-lua/plugin/protoc-gen-lua.bat\"";

    //    foreach (string f in files) {
    //        string name = Path.GetFileName(f);
    //        string ext = Path.GetExtension(f);
    //        if (!ext.Equals(".proto")) continue;

    //        ProcessStartInfo info = new ProcessStartInfo();
    //        info.FileName = protoc;
    //        info.Arguments = " --lua_out=./ --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name;
    //        info.WindowStyle = ProcessWindowStyle.Hidden;
    //        info.UseShellExecute = true;
    //        info.WorkingDirectory = dir;
    //        info.ErrorDialog = true;
    //        Util.Log(info.FileName + " " + info.Arguments);

    //        Process pro = Process.Start(info);
    //        pro.WaitForExit();
    //    }
    //    AssetDatabase.Refresh();
    //}
}