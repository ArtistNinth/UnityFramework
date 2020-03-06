using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABConfig", menuName = "ABConfig")]
public class ABConfig : ScriptableObject
{
    [System.Serializable]
    public struct FolderABConfig
    {
        public string ABName;
        public string FolderPath;
    }

    public List<FolderABConfig> folderList = new List<FolderABConfig>(); //此文件夹下的所有文件都会打进一个AB包
    public List<string> prefabFolderList = new List<string>();  //此文件夹下的所有prefab都会单独打成AB包
}
