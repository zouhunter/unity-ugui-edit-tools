using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace UGUIAssembler
{
    /// <summary>
    /// 1.从UI导出布局配制表
    /// 2.通过布局配制表生成UI
    /// 3.指导指定图片导入格式及信息
    /// 4.优选设置功能
    /// </summary>
    public class UGUIAssemblerEditor : Editor
    {
        /// <summary>
        ///层级解析
        /// </summary>
        private static Dictionary<string, Type> layerImportTypes;
        private static Type emptyImporter;
        static UGUIAssemblerEditor()
        {
            layerImportTypes = new Dictionary<string, Type>();
            layerImportTypes.Add("Text", typeof(TextLayerImport));
            layerImportTypes.Add("Image", typeof(ImageLayerImport));
            layerImportTypes.Add("Button", typeof(ButtonLayerImport));
            layerImportTypes.Add("Toggle", typeof(ToggleLayerImport));
            layerImportTypes.Add("InputField", typeof(InputFieldLayerImport));
            layerImportTypes.Add("Slider", typeof(SliderLayerImport));
            layerImportTypes.Add("RectTransform", typeof(RectTranformLayerImport));
        }

        public static void RegistCustomImporter(string key, Type type)
        {
            if (string.IsNullOrEmpty(key) || type == null) return;

            if (typeof(ILayerImport).IsAssignableFrom(type))
            {
                layerImportTypes[key] = type;
            }
        }
        public static void SetEmptyImporter(Type emptyImporter)
        {
            UGUIAssemblerEditor.emptyImporter = emptyImporter;
        }
        
        [MenuItem("Assets/Create/Custom-UI", true)]
        private static bool JudgeCreateUIState()
        {
            var activeItem = Selection.activeObject;

            if (activeItem != null)
            {
                var path = AssetDatabase.GetAssetPath(activeItem);
                if (!string.IsNullOrEmpty(path))
                {
                    return path.EndsWith("csv");
                }
                return false;
            }
            else
            {
                return false;
            }
        }
        [MenuItem("Assets/Create/Custom-UI")]
        private static void QuickCreateUI()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var canvas = FindOrCreateCanvas();
            DelyGenerateUI(path, PreferHelper.defultSpriteFolder, canvas);
        }

        [MenuItem("GameObject/UI/Custom")]
        private static void GenerateUGUI()
        {
            var spriteFolderPath = GetSpriteFolderPath();
            if (string.IsNullOrEmpty(spriteFolderPath) && !Directory.Exists(spriteFolderPath))
            {
                EditorUtility.DisplayDialog("错误提示", "请先在Edit/Preferences/UI Assembler中配制图片文件夹路径，或选中文件夹后重试", "确认");
            }
            else
            {
                if (spriteFolderPath != PreferHelper.defultSpriteFolder)
                {
                    var ok = EditorUtility.DisplayDialog("装配UI", "是否从路径 <" + spriteFolderPath + "> 装载图片", "确认", "取消");
                    if (!ok)
                    {
                        return;
                    }
                }

                var configPath = GetConfigPath();
                var canvas = FindOrCreateCanvas();
                DelyGenerateUI(configPath, PreferHelper.defultSpriteFolder, canvas);
            }
        }
        
        private static void DelyGenerateUI(string configPath, string spriteFolderPath, Canvas canvas)
        {
            var counter = 5;
            EditorApplication.update = () =>
            {
                counter--;
                if (counter == 0)
                {
                    GenerateUI(configPath, PreferHelper.defultSpriteFolder, canvas);
                    EditorApplication.update = null;
                }
            };
        }

        private static void GenerateUI(string configPath, string spriteFolderPath, Canvas canvas)
        {
            CsvTable table = null;
        pos_readdoc: if (File.Exists(configPath))
            {
                try
                {
                    table = CsvHelper.ReadCSV(configPath, System.Text.Encoding.GetEncoding("GB2312"));
                }
                catch (Exception e)
                {
                    var reopen = EditorUtility.DisplayDialog("提示", e.Message, "重试", "取消");
                    if (reopen)
                    {
                        goto pos_readdoc;
                    }
                }

                if (table != null)
                {
                    var canLoad = table.IsUIInfoTable(false);
                    if (canLoad)
                    {
                        var isTitleMatch = table.IsUIInfoTable(true);
                        if (!isTitleMatch)
                        {
                            var forceLoad = Notice("文档标题不匹配:" + string.Join(",", UIInfo_TableExtend.uiInfoHead) + "\n继续请按确认！", true);
                            if (!forceLoad)
                            {
                                return;
                            }
                        }

                        var uiInfo = table.LoadUIInfo();
                        if (uiInfo != null)
                        {
                            if (emptyImporter != null)
                            {
                                Assembler.emptyImporter = emptyImporter;
                            }
                            Assembler.GenerateUI(spriteFolderPath, canvas, layerImportTypes, uiInfo);
                        }
                    }
                    else
                    {
                        Notice("配制文档不可用，请核对后重试！");
                    }
                }


            }
        }

        /// <summary>
        /// 初始化Canvas
        /// </summary>
        /// <returns></returns>
        public static Canvas FindOrCreateCanvas()
        {
            Canvas canvas = null;
            if (Selection.activeTransform != null)
            {
                canvas = Selection.activeTransform.GetComponentInChildren<Canvas>();
            }
            if (canvas == null)
            {
                canvas = GameObject.FindObjectOfType<Canvas>();
            }
            if (canvas == null)
            {
                canvas = LayerImportUtil.CreateCanvas(PreferHelper.uiSize);
            }
            return canvas;
        }

        private static string GetConfigPath()
        {
            var path = EditorUtility.OpenFilePanel("选择配制规则文件（csv）", PreferHelper.configFolderPath, "csv");
            return path;
        }

        private static string GetSpriteFolderPath()
        {
            if (Selection.activeInstanceID != 0)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
                if (ProjectWindowUtil.IsFolder(Selection.activeInstanceID))
                {
                    return path;
                }
                else
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        return System.IO.Path.GetDirectoryName(path);
                    }
                }
            }
            return PreferHelper.defultSpriteFolder;
        }

        private static bool Notice(string info, bool canSaleAble = false)
        {
            return EditorUtility.DisplayDialog("提示", info, "确认", canSaleAble ? "取消" : null);
        }
        public static void DelyExecute(System.Action action, int count)
        {
            if (action == null) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update = () =>
            {
                count--;
                if (count <= 0)
                {
                    UnityEditor.EditorApplication.update = null;
                    action.Invoke();
                }
            };
#else
            action.Invoke();
#endif

        }

    }
}