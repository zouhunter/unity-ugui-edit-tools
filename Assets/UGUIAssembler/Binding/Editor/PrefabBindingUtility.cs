using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace UGUIAssembler.Binding
{
    public static class PrefabBindingUtility
    {
        private static PreferString prefer_ConfigPath = new PreferString("UGUIAssembler.PrefabBindingUtility.configPath", "");
        private const BindingFlags set_flags = BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        private const BindingFlags get_flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.FlattenHierarchy;

        #region EditorMethod
        private const string Menu_AnalysisBinding = "GameObject/代码绑定/导出配制";
        private const string Menu_InstallBinding = "GameObject/代码绑定/装载配制";
        private const string Menu_InstantiateBinding = "GameObject/代码绑定/临时对象";
        private const string Menu_CopyBinding = "GameObject/代码绑定/复制绑定信息";
        private const string Menu_PastInstantiateBinding = "GameObject/代码绑定/粘贴临时对象";
        private const string Menu_PastBinding = "GameObject/代码绑定/粘贴绑定信息";
        private static PrefabBindingInfo bindingInfo;
        [MenuItem(Menu_AnalysisBinding, false)]
        private static void AnalysisBinding()
        {
            if (Selection.activeTransform != null)
            {
                var bindingInfo = DecompressionBindingInfo(Selection.activeTransform);

                var table = bindingInfo.CreateTable();

                if (table != null)
                {
                    if (string.IsNullOrEmpty(prefer_ConfigPath.Value))
                    {
                        prefer_ConfigPath.Value = Application.dataPath;
                    }
                    var filePath = EditorUtility.SaveFilePanel("另存为配制文档", prefer_ConfigPath.Value, table.name, "csv");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var dir = System.IO.Path.GetDirectoryName(filePath);
                        prefer_ConfigPath.Value = dir;
                        CsvHelper.SaveCSV(table, filePath, System.Text.Encoding.GetEncoding("gb2312"));
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
        [MenuItem(Menu_AnalysisBinding, true)]
        private static bool JudgeAnalysisBinding()
        {
            if (Selection.activeTransform != null)
            {
                return true;
            }
            return false;
        }
        [MenuItem(Menu_InstallBinding, false)]
        private static void InstallBinding()
        {
            var filePath = EditorUtility.OpenFilePanel("选择绑定配制文档", prefer_ConfigPath.Value, "csv");
            if (!string.IsNullOrEmpty(filePath))
            {
                prefer_ConfigPath.Value = System.IO.Path.GetDirectoryName(filePath);
                var table = CsvHelper.ReadCSV(filePath, System.Text.Encoding.GetEncoding("gb2312"));
                if (table != null)
                {
                    var bindingInfo = table.LoadPrefabBindingInfo();
                    if (bindingInfo != null)
                    {
                        InstallInfomation(bindingInfo, Selection.activeTransform);
                    }
                }
            }
        }
        [MenuItem(Menu_InstallBinding, true)]
        private static bool JudgeInstallBinding()
        {
            if (Selection.activeTransform != null)
            {
                return true;
            }
            return false;
        }
        [MenuItem(Menu_InstantiateBinding)]
        private static void InstantiateBinding()
        {
            if (Selection.activeObject != null)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(path))
                {
                    var table = CsvHelper.ReadCSV(path, System.Text.Encoding.GetEncoding("gb2312"));
                    var bindingInfo = table.LoadPrefabBindingInfo();
                    if (bindingInfo != null)
                    {
                        CreateBindingTemplate(bindingInfo);
                        return;
                    }
                }
            }

            var filePath = EditorUtility.OpenFilePanel("选择绑定配制文档", prefer_ConfigPath.Value, "csv");
            if (!string.IsNullOrEmpty(filePath))
            {
                prefer_ConfigPath.Value = System.IO.Path.GetDirectoryName(filePath);
                var table = CsvHelper.ReadCSV(filePath, System.Text.Encoding.GetEncoding("gb2312"));
                if (table != null)
                {
                    var bindingInfo = table.LoadPrefabBindingInfo();
                    if (bindingInfo != null)
                    {
                        CreateBindingTemplate(bindingInfo);
                    }
                }
            }
        }
        [MenuItem(Menu_CopyBinding)]
        private static void CopyBininding()
        {
            bindingInfo = DecompressionBindingInfo(Selection.activeTransform);
            if (bindingInfo != null && bindingInfo.scriptItems != null && bindingInfo.scriptItems.Count > 0)
            {
                EditorUtility.DisplayDialog("小提示", "复制信息成功，可选中目标后，使用粘贴功能！", "确认");
            }
            else
            {
                EditorUtility.DisplayDialog("小提示", "复制信息失败，未成功解析到非引擎自带的Monobehaiver信息！", "确认");
            }
        }
        [MenuItem(Menu_CopyBinding, true)]
        private static bool JudgeCopyBinding()
        {
            return Selection.activeTransform != null;
        }
        [MenuItem(Menu_PastBinding)]
        private static void PastBinding()
        {
            InstallInfomation(bindingInfo, Selection.activeTransform);
        }
        [MenuItem(Menu_PastBinding, true)]
        private static bool JudgePastBinding()
        {
            return bindingInfo != null && Selection.activeTransform != null;
        }
        [MenuItem(Menu_PastInstantiateBinding)]
        private static void PastInstantiateBinding()
        {
            CreateBindingTemplate(bindingInfo);
        }
        [MenuItem(Menu_PastInstantiateBinding,true)]
        private static bool JudgePastInstantiateBinding()
        {
            return bindingInfo != null;
        }
        #endregion

        /// <summary>
        /// 解析关联信息
        /// </summary>
        /// <param name="scripts"></param>
        /// <param name="objHolds"></param>
        public static PrefabBindingInfo DecompressionBindingInfo(Transform root)
        {
            var info = new PrefabBindingInfo(root.name);
            DecompressionBinding(root, root, info.scriptItems);
            return info;
        }

        /// <summary>
        /// 创建临时绑定对象
        /// </summary>
        /// <param name="bindingInfo"></param>
        /// <returns></returns>
        public static GameObject CreateBindingTemplate(PrefabBindingInfo bindingInfo)
        {
            var pathDic = new Dictionary<string, Transform>();
            var root = new GameObject(bindingInfo.name);
            for (int i = 0; i < bindingInfo.scriptItems.Count; i++)
            {
                var scriptItem = bindingInfo.scriptItems[i];
                var transform = CompleteTransform(root.transform, scriptItem.path, pathDic);

                var behaiver = MustMonobeahvier(transform.gameObject, scriptItem.type);
                if (behaiver != null && scriptItem.resources != null)
                {
                    using (var enumerator = scriptItem.resources.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            SetValueToMemeberRuntime(behaiver, current.Key, current.Value, root.transform, pathDic);
                        }
                    }
                }
                else
                {
                    Debug.LogErrorFormat("【装置失败】路径：{0},类型：{1},资源：{2}", scriptItem.path, scriptItem.type, scriptItem.resources);
                }
            }
            return root;
        }

        ///// <summary>
        ///// 加载记录的信息
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="scripts"></param>
        public static void InstallInfomation(PrefabBindingInfo bindingInfo, Transform root)
        {
            var pathDic = new Dictionary<string, Transform>();
            for (int i = 0; i < root.childCount; i++)
            {
                MakeTransfomDicDeep(root.GetChild(i), "", pathDic);
            }

            for (int i = 0; i < bindingInfo.scriptItems.Count; i++)
            {
                var scriptItem = bindingInfo.scriptItems[i];
                Transform currentTransform = null;
                if (string.IsNullOrEmpty(scriptItem.path))
                {
                    currentTransform = root;
                }
                else
                {
                    if (pathDic.ContainsKey(scriptItem.path))
                    {
                        currentTransform = pathDic[scriptItem.path];
                    }
                }

                if (currentTransform != null)
                {
                    var behaiver = MustMonobeahvier(currentTransform.gameObject, scriptItem.type);
                    if (behaiver != null && scriptItem.resources != null)
                    {
                        using (var enumerator = scriptItem.resources.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var current = enumerator.Current;
                                SetValueToMemeber(behaiver, current.Key, current.Value, pathDic);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("【装置失败】路径：{0},类型：{1},资源：{2}", scriptItem.path, scriptItem.type, scriptItem.resources);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("【装置失败】路径：{0}", scriptItem.path);
                }
            }
        }


        /// <summary>
        /// 完善层级信息
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        /// <param name="pathDic"></param>
        private static Transform CompleteTransform(Transform root, string path, Dictionary<string, Transform> pathDic)
        {
            if (string.IsNullOrEmpty(path)) return root;

            var names = path.Split('/');
            string currentPath = null;
            Transform transform = root;
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                if (i == 0)
                {
                    currentPath = name;
                }
                else
                {
                    currentPath = currentPath + "/" + name;
                }

                if (pathDic.ContainsKey(currentPath) && pathDic[currentPath] != null)
                {
                    transform = pathDic[currentPath];
                }
                else
                {
                    var go = new GameObject(name);
                    go.transform.SetParent(transform);
                    transform = go.GetComponent<Transform>();
                    pathDic.Add(currentPath, transform);
                }
            }
            return transform;
        }

        /// <summary>
        /// 建立路径
        /// 索引字典
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="currentPath"></param>
        /// <param name="dic"></param>
        private static void MakeTransfomDicDeep(Transform transform, string currentPath, Dictionary<string, Transform> dic)
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                currentPath = currentPath + "/" + transform.name;
                if (dic.ContainsKey(currentPath))
                {
                    Debug.LogError("路径重复：" + currentPath);
                }
                else
                {
                    Debug.Log(currentPath, transform);
                    dic.Add(currentPath, transform);
                }
            }
            else
            {
                currentPath = transform.name;
                dic.Add(currentPath, transform);
            }


            for (int i = 0; i < transform.childCount; i++)
            {
                var childTransform = transform.GetChild(i);
                MakeTransfomDicDeep(childTransform, currentPath, dic);
            }
        }

        /// <summary>
        /// 设置元素的值
        /// </summary>
        /// <param name="component"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        private static void SetValueToMemeber(MonoBehaviour component, string fieldName, string value, Dictionary<string, Transform> pathDic)
        {
            var mainType = component.GetType();
            var fieldInfo = mainType.GetField(fieldName, set_flags);
            if (fieldInfo != null)
            {
                var type = fieldInfo.FieldType;
                var obj = StringToObject(type, value, pathDic);
                if (obj != null)
                {
                    mainType.InvokeMember(fieldName, set_flags, null, component, new object[] { obj }, null, null, null);
                }
            }
            else
            {
                Debug.LogError(component + " 找不到名称为" + fieldName + "的参数");
            }
        }


        /// <summary>
        /// 动态创建并设置元素的值
        /// </summary>
        /// <param name="component"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="pathDic"></param>
        private static void SetValueToMemeberRuntime(MonoBehaviour component, string fieldName, string value, Transform root, Dictionary<string, Transform> pathDic)
        {
            var mainType = component.GetType();
            var fieldInfo = mainType.GetField(fieldName, set_flags);
            if (fieldInfo != null)
            {
                var type = fieldInfo.FieldType;
                var obj = StringToObjectRuntime(type, value, root, pathDic);
                if (obj != null)
                {
                    mainType.InvokeMember(fieldName, set_flags, null, component, new object[] { obj }, null, null, null);
                }
            }
            else
            {
                Debug.LogError(component + " 找不到名称为" + fieldName + "的参数");
            }
        }

        /// <summary>
        /// 按类型解析值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object StringToObject(Type type, string value, Dictionary<string, Transform> dic)
        {
            //资源型引用类型,记录guid
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                UnityEngine.Object obj = null;
                if (TypeUtil.IsAssetsType(type))//资源类型
                {
                    if (TryLoadAssetFromGUID(type, value, out obj))
                    {
                        return obj;
                    }
                    else
                    {
                        Debug.LogWarningFormat("参数{0}解析失败:guid找不到", value);
                    }
                }
                else
                {
                    if (type == typeof(GameObject))
                    {
                        if (TryLoadAssetFromGUID(type, value, out obj))
                        {
                            return obj;
                        }
                        else if (dic.ContainsKey(value))
                        {
                            return dic[value].gameObject;
                        }
                        else
                        {
                            Debug.LogWarningFormat("参数{0}解析失败,未找到GameOject:{1}", value, type);
                        }
                    }
                    else if (type.IsSubclassOf(typeof(Component)))
                    {
                        if (TryLoadAssetFromGUID(type, value, out obj))
                        {
                            return obj;
                        }
                        else if (TryLoadComponentFromPath(type, value, dic, out obj))
                        {
                            return obj;
                        }
                        else
                        {
                            Debug.LogWarningFormat("参数{0}解析失败未找到组件:{1}", value, type);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("参数{0}解析失败类型未判断:{1}", value, type);
                    }
                }
            }
            else if (TypeUtil.IsInnerStructure(type))
            {
                var objValue = ParamAnalysisTool.InnerStructFromString(type, value);
                if (objValue == null)
                {
                    Debug.LogWarningFormat("参数{0}解析失败,类型:{1}", value, type);
                }
                return objValue;
            }
            else if (typeof(IConvertible).IsAssignableFrom(type))
            {
                return ParamAnalysisTool.IconventibleFromString(type, value);
            }

            Debug.LogWarningFormat("参数{0}解析失败,类型:{1}", value, type);
            return null;
        }


        /// <summary>
        /// 按类型解析值，没有Transform创建之
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="root"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        private static object StringToObjectRuntime(Type type, string value, Transform root, Dictionary<string, Transform> dic)
        {
            //资源型引用类型,记录guid
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                UnityEngine.Object obj = null;
                if (TypeUtil.IsAssetsType(type))//资源类型
                {
                    if (TryLoadAssetFromGUID(type, value, out obj))
                    {
                        return obj;
                    }
                    else
                    {
                        Debug.LogWarningFormat("参数{0}解析失败:guid找不到", value);
                    }
                }
                else
                {
                    if (type == typeof(GameObject))
                    {
                        if (TryLoadAssetFromGUID(type, value, out obj))
                        {
                            return obj;
                        }
                        else
                        {
                            var trans = CompleteTransform(root, value, dic);
                            if (trans != null)
                            {
                                return trans.gameObject;
                            }
                        }
                    }
                    else if (type.IsSubclassOf(typeof(Component)))
                    {
                        if (TryLoadAssetFromGUID(type, value, out obj))
                        {
                            return obj;
                        }
                        else
                        {
                            var trans = CompleteTransform(root, value, dic);
                            if (trans != null)
                            {
                                return MustComponent(trans.gameObject, type);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("参数{0}解析失败类型未判断:{1}", value, type);
                    }
                }
            }
            else if (TypeUtil.IsInnerStructure(type))
            {
                var objValue = ParamAnalysisTool.InnerStructFromString(type, value);
                if (objValue == null)
                {
                    Debug.LogWarningFormat("参数{0}解析失败,类型:{1}", value, type);
                }
                return objValue;
            }
            else if (typeof(IConvertible).IsAssignableFrom(type))
            {
                return ParamAnalysisTool.IconventibleFromString(type, value);
            }

            Debug.LogWarningFormat("参数{0}解析失败,类型:{1}", value, type);
            return null;
        }

        /// <summary>
        /// 尝试从路径加载资源
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <param name="pathDic"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private static bool TryLoadComponentFromPath(Type type, string path, Dictionary<string, Transform> pathDic, out UnityEngine.Object asset)
        {
            if (pathDic.ContainsKey(path))
            {
                var transform = pathDic[path];
                asset = MustComponent(transform.gameObject, type);
                return asset != null;
            }
            asset = null;
            return false;
        }

        /// <summary>
        /// 尝试从guid加载资源
        /// </summary>
        /// <param name="type"></param>
        /// <param name="guid"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        private static bool TryLoadAssetFromGUID(Type type, string guid, out UnityEngine.Object asset)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(assetPath))//资源路径
            {
                asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                return asset != null;
            }
            asset = null;
            return false;
        }

        /// <summary>
        /// 遍历解析每个物体上用户自己添加的脚本
        /// </summary>
        /// <param name="root"></param>
        /// <param name="transform"></param>
        /// <param name="scriptList"></param>
        private static void DecompressionBinding(Transform root, Transform transform, List<PrefabBindingInfo.ScriptItem> scriptList)
        {
            if (transform != null)
            {
                var behaivers = GetCustomMonoBehaivers(transform);
                for (int i = 0; i < behaivers.Length; i++)
                {
                    var behaiver = behaivers[i];
                    var scriptItem = new PrefabBindingInfo.ScriptItem();

                    var fields = GetAllSupportedFieldInfos(behaivers[i]);
                    for (int j = 0; j < fields.Length; j++)
                    {
                        var field = fields[j];
                        var fieldValue = field.GetValue(behaiver);
                        if (fieldValue != null)
                        {
                            var value = GetStringValue(field.FieldType, root, fieldValue);
                            if (!string.IsNullOrEmpty(value))
                            {
                                scriptItem.resources.Add(field.Name, value);
                            }
                        }
                    }

                    if (scriptItem.resources.Count > 0)
                    {
                        scriptItem.path = GetChildPath(root, transform);
                        scriptItem.type = behaiver.GetType();
                        scriptList.Add(scriptItem);
                    }
                }
            }

            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var childTransform = transform.GetChild(i);
                    DecompressionBinding(root, childTransform, scriptList);
                }
            }
        }

        /// <summary>
        /// 按类型得到字符串值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="root"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetStringValue(Type type, Transform root, object value)
        {
            if (typeof(IConvertible).IsAssignableFrom(type))
            {
                return value.ToString();
            }
            //资源型引用类型,记录guid
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                UnityEngine.Object uObj = (UnityEngine.Object)value;
                string guid;
                string rPath;

                if (TypeUtil.IsAssetsType(type))//资源类型
                {
                    if (TryGetGUID(uObj, out guid))
                    {
                        return guid;
                    }
                }
                else if (type == typeof(GameObject))//相对引用
                {
                    if (TryGetGUID(uObj, out guid))
                    {
                        return guid;
                    }
                    else if (TryGetChildPath(root, ((GameObject)uObj).transform, out rPath))
                    {
                        return rPath;
                    }
                }
                else if (type.IsSubclassOf(typeof(Component)))//组件类型
                {
                    if (TryGetGUID(uObj, out guid))
                    {
                        return guid;
                    }
                    else if (TryGetChildPath(root, ((Component)uObj).transform, out rPath))
                    {
                        return rPath;
                    }
                }

            }
            else if (TypeUtil.IsInnerStructure(type))
            {
                var strvalue = ParamAnalysisTool.InnerStructObjectToString(type, value);
                if (!string.IsNullOrEmpty(strvalue))
                {
                    return strvalue;
                }
            }

            Debug.LogWarning("未能成功解析属性信息：" + type.Name);
            return null;
        }

        /// <summary>
        /// 试图获取物体的guid
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static bool TryGetGUID(UnityEngine.Object obj, out string guid)
        {
            var go = (UnityEngine.Object)obj;
            var path = AssetDatabase.GetAssetPath(go);

            if (!string.IsNullOrEmpty(path))
            {
                guid = AssetDatabase.AssetPathToGUID(path);
                return !string.IsNullOrEmpty(guid);
            }
            guid = null;
            return false;
        }

        /// <summary>
        /// 找到Behaivers所有支持FieldInfo
        /// </summary>
        /// <param name="behaiver"></param>
        /// <returns></returns>
        private static FieldInfo[] GetAllSupportedFieldInfos(MonoBehaviour behaiver)
        {
            var type = behaiver.GetType();
            var fields = type.GetFields(get_flags);
            var supportedFields = new List<FieldInfo>();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!field.IsPublic)
                {
                    var attributes = field.GetCustomAttributes(true);
                    var supported = false;
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if (attributes[j] is SerializeField)
                        {
                            supported = true;
                        }
                    }

                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                        continue;
                    if (typeof(UnityEngine.Events.UnityEvent).IsAssignableFrom(field.FieldType))
                        continue;
                    if (!supported)
                        continue;
                }
                supportedFields.Add(field);
            }
            return supportedFields.ToArray();
        }

        /// <summary>
        /// 所有需要控制的脚本
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static MonoBehaviour[] GetCustomMonoBehaivers(Transform transform)
        {
            var comonents = transform.GetComponents<MonoBehaviour>();
            return comonents;
        }

        /// <summary>
        /// 查找物体的路径(确定)
        /// </summary>
        /// <param name="root"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string GetChildPath(Transform root, Transform item)
        {
            var path = "";
            TryGetChildPath(root, item, out path);
            return path;
        }
        /// <summary>
        /// 不确定
        /// </summary>
        /// <param name="root"></param>
        /// <param name="item"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool TryGetChildPath(Transform root, Transform item, out string path)
        {
            if (root == item)
            {
                path = "";
                return true;
            }
            else
            {
                path = item.name;
                var transform = item;

                while (transform && transform != root)
                {
                    transform = transform.parent;

                    if (transform)
                    {
                        if (transform == root)
                        {
                            return true;
                        }
                        else
                        {
                            path = transform.name + "/" + path;
                        }
                    }
                }
            }
            Debug.LogError(item.name);
            return false;
        }


        private static MonoBehaviour MustMonobeahvier(GameObject obj, Type scriptType)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(scriptType))
            {
                return null;
            }

            var comp = obj.GetComponent(scriptType);
            if (comp == null)
            {
                comp = obj.AddComponent(scriptType);
            }
            return comp as MonoBehaviour;
        }

        private static Component MustComponent(GameObject obj, Type scriptType)
        {
            if (!typeof(Component).IsAssignableFrom(scriptType))
            {
                return null;
            }
            var comp = obj.GetComponent(scriptType);
            if (comp == null)
            {
                comp = obj.AddComponent(scriptType);
            }
            return comp;
        }
    }
}