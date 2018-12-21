using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace UGUIAssembler.Binding
{
    public class PrefabBindingInfo
    {
        [System.Serializable]
        public class ScriptItem
        {
            public string path;
            public Type type;
            public ResourceDic resources = new ResourceDic();
        }

        public string name;
        public List<ScriptItem> scriptItems = new List<ScriptItem>();

        public PrefabBindingInfo(string name)
        {
            this.name = name;
        }
    }

    public static class PrefabBindingInfoTableExtend
    {
        public const string key_Path = "相对路径";
        public const string key_Assemble = "程序集";
        public const string key_Type = "类型";
        public const string key_resourceDic = "资源字典";

        public static string[] titles = { key_Path, key_Assemble, key_Type, key_resourceDic };

        public static CsvTable CreateTable(this PrefabBindingInfo bindingInfo)
        {
            var table = new CsvTable(bindingInfo.name);
            table.Columns = new List<string>(titles);
            for (int i = 0; i < bindingInfo.scriptItems.Count; i++)
            {
                var scriptItem = bindingInfo.scriptItems[i];
                var infos = new string[titles.Length];
                infos[0] = scriptItem.path;
                infos[1] = scriptItem.type.Assembly.FullName;
                infos[2] = scriptItem.type.FullName;
                infos[3] = ParamAnalysisTool.FromDictionary(scriptItem.resources);
                table.Rows.Add(infos);
            }
            return table;
        }

        public static PrefabBindingInfo LoadPrefabBindingInfo(this CsvTable table)
        {
            if (table == null) return null;

            if (table.Columns.Count < 4)
            {
                Debug.LogErrorFormat("表列数和需求不一致，请目标表{0}检查是否为绑定信息！", table.name);
                return null;
            }

            var prefabBindingInfo = new PrefabBindingInfo(table.name);
            var count = table.Rows.Count;

            for (int i = 0; i < count; i++)
            {
                var row = table.Rows[i];
                var scriptItem = new PrefabBindingInfo.ScriptItem();
                scriptItem.path = row[0];
                var assemble = Assembly.Load(row[1]);
                if (assemble != null)
                {
                    scriptItem.type = assemble.GetType(row[2]);
                }
                scriptItem.resources = ParamAnalysisTool.ToDictionary(row[3]);
                prefabBindingInfo.scriptItems.Add(scriptItem);
            }

            return prefabBindingInfo;
        }
    }
}