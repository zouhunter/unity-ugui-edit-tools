using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIAssembler
{
    public class LayerInfo
    {
        public string name;
        public string path;//路径
        public Rect rect;//坐标尺寸
        public string type;//导级类型
        public readonly ResourceDic resourceDic;//资源字典
        public readonly Dictionary<string, ResourceDic> subResourceDic;//子控件
        public readonly List<ResourceDic> sub_images;//图片资源
        public readonly List<ResourceDic> sub_rawImages;//图片资源
        public readonly List<ResourceDic> sub_texts;//文字资源

        public bool HaveSubResource(string key)
        {
            if (subResourceDic == null || string.IsNullOrEmpty(key))
            {
                return false;
            }
            return subResourceDic.ContainsKey(key);
        }
        public LayerInfo()
        {
            resourceDic = new ResourceDic();
            subResourceDic = new Dictionary<string, ResourceDic>();
            sub_images = new List<ResourceDic>();
            sub_rawImages = new List<ResourceDic>();
            sub_texts = new List<ResourceDic>();
        }
    }
}