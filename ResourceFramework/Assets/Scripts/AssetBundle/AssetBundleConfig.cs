using System.Collections.Generic;
using System.Xml.Serialization;

[System.Serializable]
public class AssetBundleConfig
{
    [XmlElement]
    public List<ABBase> abList { get; set; }
}

//文件而非文件夹
[System.Serializable]
public class ABBase
{
    [XmlAttribute]
    public string ABName { get; set; }
    [XmlAttribute]
    public string Path { get; set; }
    [XmlAttribute]
    public uint Crc { get; set; }
    [XmlAttribute]
    public List<string> DependencyABName { get; set; }
}