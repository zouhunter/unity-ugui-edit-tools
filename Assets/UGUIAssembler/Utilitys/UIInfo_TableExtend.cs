using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler
{
    public static class UIInfo_TableExtend
    {
        public const string key_name = "名称";
        public const string key_layerPath = "层级";
        public const string key_controlType = "控件类型";
        public const string key_rect = "坐标尺寸";
        public const string key_resourceDic = "参数字典";
        public const string key_sub_resourceDic = "子控件参数字典列表";//[Optional]

        public static string[] uiInfoHead = { key_name, key_layerPath, key_controlType, key_rect, key_resourceDic, key_sub_resourceDic };

        public static bool IsUIInfoTable(this CsvTable table, bool compireHead = true)
        {
            if (table == null || table.Columns == null || table.Columns.Count > uiInfoHead.Length)
                return false;

            if (compireHead)
            {
                for (int i = 0; i < uiInfoHead.Length; i++)
                {
                    if (table.Columns.Count > i && uiInfoHead[i] != table.Columns[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static UIInfo LoadUIInfo(this CsvTable table)
        {
            if (table.IsUIInfoTable(false))
            {
                var uiInfo = new UIInfo(table.name);

                if (table.Rows != null && table.Columns != null)
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        var layerInfo = new LayerInfo();
                        layerInfo.name = table[0, i];
                        layerInfo.path = table[1, i];
                        layerInfo.type = table[2, i];
                        layerInfo.rect = ParamAnalysisTool.StringToRect(table[3, i]);
                        var resourceDic = ParamAnalysisTool.ToDictionary(table[4, i]);
                        if (resourceDic != null)
                        {
                            ChargeDic(layerInfo.resourceDic, resourceDic);
                        }
                       

                        if (table.Columns.Count > 5)
                        {
                            List<ResourceDic> sub_images;
                            List<ResourceDic> sub_texts;
                            List<ResourceDic> sub_rawImages;
                            var subResourceDic = ParamAnalysisTool.ToDictionary_Sub(table[5, i], out sub_images, out sub_texts, out sub_rawImages);
                            if (subResourceDic != null)
                            {
                                ChargeDic(layerInfo.subResourceDic, subResourceDic);
                            }
                            ChargeList(layerInfo.sub_images, sub_images);
                            ChargeList(layerInfo.sub_texts, sub_texts);
                            ChargeList(layerInfo.sub_rawImages, sub_rawImages);
                        }

                        uiInfo.layers.Add(layerInfo);
                    }
                }

                return uiInfo;
            }
            return null;
        }

        private static void ChargeDic(Dictionary<string, ResourceDic> target, Dictionary<string, ResourceDic> source)
        {
            target.Clear();
            foreach (var keyvalue in source)
            {
                target.Add(keyvalue.Key, keyvalue.Value);
            }
        }
        private static void ChargeDic(ResourceDic target, ResourceDic source)
        {
            target.Clear();
            foreach (var keyvalue in source)
            {
                target.Add(keyvalue.Key, keyvalue.Value);
            }
        }
        private static void ChargeList(List<ResourceDic> target, List<ResourceDic> source)
        {
            target.Clear();

            if(source != null)
            {
                foreach (var value in source)
                {
                    target.Add(value);
                }
            }
        }
        public static CsvTable UIInfoToTable(this UIInfo info)
        {
            var table = new CsvTable(info.name);
            table.Columns = new List<string>(uiInfoHead);
            for (int i = 0; i < info.layers.Count; i++)
            {
                var layer = info.layers[i];
                var raw = new string[table.Columns.Count];
                raw[0] = layer.name;
                raw[1] = layer.path;
                raw[2] = layer.type;
                raw[3] = ParamAnalysisTool.RectToString(layer.rect);
                raw[4] = ParamAnalysisTool.FromDictionary(layer.resourceDic);
                raw[5] = ParamAnalysisTool.FromDictionarySub(layer.subResourceDic, layer.sub_images, layer.sub_texts, layer.sub_rawImages);
                table.Rows.Add(raw);
            }
            return table;
        }
    }
}