using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Reflection;

public static class PrefabBindingUtility
{
    private const string Menu_PrefabBindingWindow = "Window/PrefabBinding";
    [MenuItem(Menu_PrefabBindingWindow)]
    private static void CreatePreabBindingWindow()
    {
        PrefabBindingWindow.GetWindow<PrefabBindingWindow>();
    }

    public static PrefabBindingObj CreatePreabBindingObj()
    {
        var obj = ScriptableObject.CreateInstance<PrefabBindingObj>();
        ProjectWindowUtil.CreateAsset(obj, "PrefabBindingObj.asset");
        return obj;
    }

    public static void ExargeRentItems(List<PrefabBindingObj.ScriptItem> scripts, List<PrefabBindingWindow.ObjHolder> objHolds)
    {
        var currCount = objHolds.Count;
        for (int i = 0; i < currCount; i++)
        {
            var objHold = objHolds[i];
            var monobs = objHold.obj.GetComponents<MonoBehaviour>();
            foreach (var mitem in monobs)
            {
                var monoscript = MonoScript.FromMonoBehaviour(mitem);
                if (monoscript != null)
                {
                    var scriptItem = SurchItemOrInit(scripts, objHold.index, Assembly.GetAssembly(mitem.GetType()).ToString(), monoscript.name);
                    var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(monoscript));
                    Debug.Log(mitem.name + ":" + guid);
                    Type type = mitem.GetType();
                    scriptItem.name = objHold.name;
                    scriptItem.nodes = new List<PrefabBindingObj.ValueNode>();
                    FieldInfo[] infos = type.GetFields(BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public);
                    foreach (var item in infos)
                    {
                        PrefabBindingObj.ValueNode node = new PrefabBindingObj.ValueNode();
                        scriptItem.nodes.Add(node);
                        node.valueName = item.Name;
                        if (item.FieldType == typeof(float) || item.FieldType == typeof(double)
                            || item.FieldType == typeof(int) || item.FieldType == typeof(long)
                            || item.FieldType == typeof(string) || item.FieldType == typeof(String))
                        {
                            node.type = PrefabBindingObj.NodeType.Single;
                            node.singleValue = item.GetValue(mitem).ToString();
                        }

                        else if (item.FieldType == typeof(Vector3))
                        {
                            node.type = PrefabBindingObj.NodeType.Vector3;
                            node.vector3Value = (Vector3)item.GetValue(mitem);
                        }
                        else if (item.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                        {
                            node.type = PrefabBindingObj.NodeType.Object;
                            var obj = ((UnityEngine.Object)item.GetValue(mitem));
                            var targetItem = SurchItemOrInit(objHolds, obj);
                            node.index = targetItem.index;
                            var targetScript = SurchItemOrInit(scripts, node.index, Assembly.GetAssembly(obj.GetType()).ToString(), obj.GetType().Name);
                            targetScript.scriptName = obj.GetType().ToString();
                            targetScript.name = targetItem.name;
                        }

                    }
                }
            }
        }
    }

    private static PrefabBindingObj.ScriptItem SurchItemOrInit(List<PrefabBindingObj.ScriptItem> scripts, int index, string assemble, string scriptName)
    {
        var item = scripts.Find(x => x.index == index && x.scriptName == scriptName);
        if (item == null)
        {
            item = new PrefabBindingObj.ScriptItem();
            scripts.Add(item);
        }
        item.index = index;
        item.scriptName = scriptName;
        item.assemble = assemble;
        return item;
    }
    private static PrefabBindingWindow.ObjHolder SurchItemOrInit(List<PrefabBindingWindow.ObjHolder> objHolds, UnityEngine.Object obj)
    {
        PrefabBindingWindow.ObjHolder targetItem = null;
        GameObject gameObj = null;
        if (obj is GameObject)
        {
            gameObj = obj as GameObject;
            targetItem = objHolds.Find(x => x.obj == obj);
        }
        else if (obj is Component)
        {
            gameObj = (obj as Component).gameObject;
            targetItem = objHolds.Find(x => x.obj.GetComponent(obj.GetType()) == obj);
        }

        if (targetItem == null)
        {
            targetItem =new PrefabBindingWindow.ObjHolder();
            objHolds.Add(targetItem);
            targetItem.name = gameObj.name;
        }

        targetItem.obj = gameObj;

        return targetItem;
    }
    /// <summary>
    /// 加载记录的信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="scripts"></param>
    public static void InstallInfomation(List<PrefabBindingWindow.ObjHolder> objHolds, List<PrefabBindingObj.ScriptItem> scripts)
    {
        foreach (var objHold in objHolds)
        {
            var scriptsNow = scripts.FindAll(x => x.index == objHold.index);
            foreach (var item in scriptsNow)
            {
                Component script = null;
                if (Assembly.Load(item.assemble).GetType(item.scriptName).IsSubclassOf(typeof(Component)))
                {
                    script = AddComponentSingle(objHold.obj, item.assemble, item.scriptName);
                }

                if (item.nodes != null && item.nodes.Count > 0 && script != null)
                {
                    Type type = script.GetType();
                    foreach (var valueItem in item.nodes)
                    {
                        if (valueItem.type == PrefabBindingObj.NodeType.Single)
                        {
                            Type filedType = type.GetField(valueItem.valueName).FieldType;
                            object value = Convert.ChangeType(valueItem.singleValue, filedType);
                            type.InvokeMember(valueItem.valueName, BindingFlags.SetField | BindingFlags.Instance | BindingFlags.Public,
                                null, script, new object[] { value }, null, null, null);
                        }
                        else if (valueItem.type == PrefabBindingObj.NodeType.Vector3)
                        {
                            type.InvokeMember(valueItem.valueName, BindingFlags.SetField | BindingFlags.Instance | BindingFlags.Public,
                                null, script, new object[] { valueItem.vector3Value }, null, null, null);
                        }
                        else if (valueItem.type == PrefabBindingObj.NodeType.Object)
                        {
                            if (type.GetField(valueItem.valueName).FieldType == typeof(GameObject))
                            {
                                var obj = objHolds[valueItem.index].obj;
                                type.InvokeMember(valueItem.valueName, BindingFlags.SetField | BindingFlags.Instance | BindingFlags.Public,
                              null, script, new object[] { obj }, null, null, null);
                            }
                            else if (type.GetField(valueItem.valueName).FieldType.IsSubclassOf(typeof(Component)))
                            {
                                var obj = objHolds[valueItem.index].obj;
                                var com = AddComponentSingle(obj, type.GetField(valueItem.valueName).FieldType);
                                type.InvokeMember(valueItem.valueName, BindingFlags.SetField | BindingFlags.Instance | BindingFlags.Public, null, script, new object[] { com }, null, null, null);
                            }

                        }
                    }

                }

            }
        }

    }

    private static Component AddComponentSingle(GameObject obj, string assemble, string scriptName)
    {
        Debug.Log(scriptName);
        var asb = Assembly.Load(assemble);
        var comp = obj.GetComponent(asb.GetType(scriptName));
        if (comp == null)
        {
            comp = obj.AddComponent(asb.GetType(scriptName));
        }
        return comp;
    }
    private static Component AddComponentSingle(GameObject obj, Type scriptType)
    {
        Debug.Log(scriptType);
        var comp = obj.GetComponent(scriptType);
        if (comp == null)
        {
            comp = obj.AddComponent(scriptType);
        }
        return comp;
    }
}
