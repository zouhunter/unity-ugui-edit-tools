using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UGUIAssembler
{
    public class PictureImportItem
    {
        public string picturePath;//图片路径
        public TextureImporterType importType;//导入类型
        public Dictionary<string,string> paramDic;//参数信息
    }
}