using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIAssembler
{
    public class LayerInfo
    {
        public string[] path;//路径
        public Rect rect;//坐标尺寸
        public string type;//导级类型
        public Dictionary<string, string> resourceDic;//资源字典
        public Dictionary<string, string> paramDic;//参数字典
    }
}