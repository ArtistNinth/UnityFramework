using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class ABGenerator
{
    private static string BuildTargetPath = Application.streamingAssetsPath;
    private static Dictionary<string, string> folderDic = new Dictionary<string, string>(); //key:AB包名，value:文件夹路径
    private static Dictionary<string, List<string>> prefabDic = new Dictionary<string, List<string>>(); //key:AB包名，value:prefab及其依赖的文件的路径

    private static List<string> markedABPath = new List<string>();    //被标记为AB的文件夹路径和prefab及其依赖的文件的路径，用作过滤

    [MenuItem("Tools/Generate AB")]
    public static void Build()
    {
        folderDic.Clear();
        prefabDic.Clear();
        markedABPath.Clear();

        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(Constants.ABConfigPath);

        #region 文件夹
        foreach (ABConfig.FolderABConfig folder in abConfig.folderList)
        {
            if (folderDic.ContainsKey(folder.ABName))
            {
                Debug.LogError("重复的ABName " + folder.ABName);
            }
            else
            {
                folderDic.Add(folder.ABName, folder.FolderPath);
                markedABPath.Add(folder.FolderPath);
            }
        }
        #endregion

        #region Prefab
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab",abConfig.prefabFolderList.ToArray());
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            if (!hasMarked(prefabPath))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabDic.ContainsKey(go.name))
                {
                    Debug.LogError("重复的Prefab " + go.name);
                }

                string[] dependenciesPath = AssetDatabase.GetDependencies(prefabPath);
                List<string> dependenciesPathList = new List<string>();
                for (int j = 0; j < dependenciesPath.Length; j++)
                {
                    string dependencyItem = dependenciesPath[j];
                    if (!hasMarked(dependencyItem) && !dependencyItem.EndsWith(Constants.CSharpExtension))
                    {
                        markedABPath.Add(dependencyItem);
                        dependenciesPathList.Add(dependencyItem);
                    }
                }
                
                prefabDic.Add(go.name, dependenciesPathList);
            }
        }
        #endregion

        #region 真正设置AB
        foreach (var ABName in folderDic.Keys)
        {
            AssetImporter importer = AssetImporter.GetAtPath(folderDic[ABName]);
            importer.assetBundleName = ABName;
        }
        foreach (var ABName in prefabDic.Keys)
        {
            foreach (var path in prefabDic[ABName])
            {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                importer.assetBundleName = ABName;
            }
        }
        #endregion

        #region 删除原来生成的AB
        DirectoryInfo directory = new DirectoryInfo(BuildTargetPath);
        FileInfo[] files = directory.GetFiles();
        for (int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i].FullName);
        }
        #endregion

        GenerateConfig();
        BuildPipeline.BuildAssetBundles(BuildTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        #region 清除设置的AB
        string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < bundleNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(bundleNames[i], true);
        }
        AssetDatabase.Refresh();
        #endregion
    }

    private static bool hasMarked(string assetPath){
        for (int i = 0; i < markedABPath.Count; i++)
        {
            string markedItem = markedABPath[i];
            if (assetPath.StartsWith(markedItem))
            {
                return true;
            }
        }
        return false;
    }

    private static void GenerateConfig()
    {
        #region 生成配置表字典
        Dictionary<string, string> path2ABNameDic = new Dictionary<string, string>();

        string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < bundleNames.Length; i++)
        {
            string abName = bundleNames[i];
            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);
            for (int j = 0; j < assetPaths.Length; j++)
            {
                path2ABNameDic.Add(assetPaths[j], abName);
            }
        }
        #endregion

        #region 生成配置表列表
        AssetBundleConfig abConfig = new AssetBundleConfig();
        abConfig.abList = new List<ABBase>();
        foreach (var path in path2ABNameDic.Keys)
        {
            ABBase abBase = new ABBase();
            abBase.ABName = path2ABNameDic[path];
            abBase.Path = path;
            abBase.Crc = Crc32.GetCrc32(path);

            abBase.DependencyABName = new List<string>();
            string[] dependencyPaths = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < dependencyPaths.Length; i++)
            {
                string dependencyPath = dependencyPaths[i];

                string abName = string.Empty;
                if (path2ABNameDic.TryGetValue(dependencyPath, out abName))
                {
                    if (abName == path2ABNameDic[path])
                    {
                        continue;
                    }
                    else
                    {
                        if (!abBase.DependencyABName.Contains(abName))
                        {
                            abBase.DependencyABName.Add(abName);
                        }
                    }
                }
            }
            abConfig.abList.Add(abBase);
        }
        #endregion

        #region 生成XML
        string xmlPath = Constants.ABConfigXMLPath;
        if (File.Exists(xmlPath))
        {
            File.Delete(xmlPath);
        }

        FileStream fs = new FileStream(xmlPath, FileMode.Create);
        StreamWriter writer = new StreamWriter(fs);
        XmlSerializer serializer = new XmlSerializer(abConfig.GetType());
        serializer.Serialize(writer, abConfig);
        writer.Close();
        fs.Close();
        #endregion

        #region 生成二进制
        string binPath = Constants.ABConfigBinPath;
        if (File.Exists(binPath))
        {
            File.Delete(binPath);
        }

        FileStream binFs = new FileStream(binPath, FileMode.Create);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(binFs, abConfig);
        binFs.Close();
        #endregion

        #region 给二进制设置AB
        AssetDatabase.Refresh();
        AssetImporter importer = AssetImporter.GetAtPath(binPath);
        importer.assetBundleName = Constants.ABConfigBinABName;
        #endregion
    }
}

