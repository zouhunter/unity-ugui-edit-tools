using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UGUIAssembler
{
    public class TextureInfo
    {
        public string texturePath;
        public TextureImporterType type;
        public ResourceDic resourceDic;
        public List<ResourceDic> spritesheetList;
    }
    public class TextureInfoTable
    {
        public List<TextureInfo> textureInfos;
    }

    public static class TextureInfoTableExtend
    {
        public static string[] titles = { "路径", "类型", "参数" ,"切图"};

        public static void LoadFromCsvTable(this TextureInfoTable infoTable, CsvTable table)
        {
            var columsCount = table.Columns.Count;
            if (columsCount >= 3)
            {
                if (table.Rows != null && table.Columns != null)
                {
                    if (infoTable.textureInfos == null)
                    {
                        infoTable.textureInfos = new List<TextureInfo>();
                    }
                    else
                    {
                        infoTable.textureInfos.Clear();
                    }

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        var textureInfo = new TextureInfo();
                        textureInfo.texturePath = table[0, i];
                        textureInfo.type = (TextureImporterType)System.Enum.Parse(typeof(TextureImporterType), table[1, i]);
                        textureInfo.resourceDic = ParamAnalysisTool.ToDictionary(table[2, i]);

                        if(columsCount > 3)
                        {
                            textureInfo.spritesheetList = ParamAnalysisTool.ToDictionaryArray(table[3,i]);
                        }

                        infoTable.textureInfos.Add(textureInfo);
                    }
                }
            }
            else
            {
                Debug.LogError("csv文档参数不足3个，请检查");
            }
        }
    }
}