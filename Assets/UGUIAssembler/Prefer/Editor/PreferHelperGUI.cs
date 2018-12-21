using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace UGUIAssembler
{

    public class PreferHelperGUI
    {
        private static string defultFontrPath
        {
            get
            {
                return PreferHelper.defultFontrPath;
            }
            set
            {
                PreferHelper.defultFontrPath = value;
            }
        }
        private static string fontFolderPath
        {
            get
            {
                return PreferHelper.fontFolderPath;
            }
            set
            {
                PreferHelper.fontFolderPath = value;
            }
        }
        private static string defultSpriteFolder
        {
            get
            {
                return PreferHelper.defultSpriteFolder;
            }
            set
            {
                PreferHelper.defultSpriteFolder = value;
            }
        }
        private static string materialFolderPath
        {
            get
            {
                return PreferHelper.materialFolderPath;
            }
            set
            {
                PreferHelper.materialFolderPath = value;
            }
        }
        private static string textureFolderPath
        {
            get
            {
                return PreferHelper.textureFolderPath;
            }
            set
            {
                PreferHelper.textureFolderPath = value;
            }
        }
        private static string configFolderPath
        {
            get
            {
                return PreferHelper.configFolderPath;
            }
            set
            {
                PreferHelper.configFolderPath = value;
            }
        }
        private static Vector2Int uiSize
        {
            get
            {
                return PreferHelper.uiSize;
            }
            set
            {
                PreferHelper.uiSize = value;
            }
        }

        private static string fliterCsvRegix
        {
            get
            {
                return PreferHelper.fliterCsvRegix;
            }
            set
            {
                PreferHelper.fliterCsvRegix = value;
            }
        }
        #region GUI
        [PreferenceItem("UI Assembler")]
        private static void UIAssemblerPerfer()
        {
            DrawDefultSpriteFolder();
            DrawTextureFolder();
            DrawMaterialFolder();
            DrawConfigFolder();
            DrawConfigFliter();
            DrawUISize();
            DrawFontFoder();
            DrawDefultFontrPath();
        }

        private static void DrawConfigFliter()
        {
            using (var line = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("配制文件名过滤", GUILayout.Width(100));
                fliterCsvRegix = EditorGUILayout.TextField("", fliterCsvRegix);
            }
        }
        private static void DrawDefultFontrPath()
        {
            using (var line = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("默认字体", GUILayout.Width(100));
                defultFontrPath = EditorGUILayout.TextField(defultFontrPath);
                if (GUILayout.Button("选择", EditorStyles.miniButtonLeft))
                {
                    var path = EditorUtility.OpenFilePanel("选择字体", fontFolderPath, "");

                    if (!string.IsNullOrEmpty(path) && (path.EndsWith("font") || path.EndsWith("ttf") || path.EndsWith("otf")))
                    {
                        path = path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                        if (path.StartsWith(fontFolderPath))
                        {
                            defultFontrPath = path.Replace(fontFolderPath + "/", "");
                            //gui不会自动更新
                            EditorGUILayout.TextField(defultSpriteFolder);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("提示", "请选择字体文件夹中的字体", "确定");
                        }
                    }
                }
                if (GUILayout.Button("清空", EditorStyles.miniButtonRight))
                {
                    defultFontrPath = null;
                }
            }
        }

        private static void DrawDefultSpriteFolder()
        {
            defultSpriteFolder = DrawAssetFolderPath("图片路径（默认）", defultSpriteFolder);
        }

        private static void DrawMaterialFolder()
        {
            materialFolderPath = DrawAssetFolderPath("材质路径", materialFolderPath);
        }

        private static void DrawTextureFolder()
        {
            textureFolderPath = DrawAssetFolderPath("贴图路径", textureFolderPath);
        }

        private static void DrawConfigFolder()
        {
            configFolderPath = DrawNormalFolderPath("配制路径", configFolderPath);
        }
        private static void DrawFontFoder()
        {
            fontFolderPath = DrawAssetFolderPath("字体路径", fontFolderPath);
        }

        private static void DrawUISize()
        {
            using (var line = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("画布尺寸", GUILayout.Width(100));
                uiSize = EditorGUILayout.Vector2IntField("", uiSize);
            }
        }


        #region GUICore

        private static string DrawAssetFolderPath(string title, string value)
        {
            using (var line = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(title, GUILayout.Width(100));
                value = EditorGUILayout.TextField(value);
                if (GUILayout.Button("选择", EditorStyles.miniButtonRight))
                {
                    var path = ChoiseAssetFolderPath(value);
                    if (!string.IsNullOrEmpty(path))
                    {
                        value = path;
                        //gui不会自动更新
                        EditorGUILayout.TextField(value);
                    }
                }
            }
            return value;
        }

        private static string DrawNormalFolderPath(string title, string value)
        {
            using (var line = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(title, GUILayout.Width(100));
                value = EditorGUILayout.TextField(value);
                if (GUILayout.Button("选择", EditorStyles.miniButtonRight))
                {
                    var path = EditorUtility.OpenFolderPanel("选择路径", value, "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        value = path;
                        //gui不会自动更新
                        EditorGUILayout.TextField(value);
                    }
                }
            }
            return value;
        }
        #endregion

        /// <summary>
        /// 选择工程目录文件
        /// </summary>
        /// <returns></returns>
        public static string ChoiseAssetFolderPath(string defultPath)
        {
            var choisedFolder = EditorUtility.OpenFolderPanel("选择工程路径", defultPath, "");
            choisedFolder = choisedFolder.Replace("\\", "/").Replace(Application.dataPath, "Assets");
            if (!string.IsNullOrEmpty(choisedFolder) && choisedFolder.StartsWith("Assets"))
            {
                return choisedFolder;
            }
            return null;
        }

        #endregion

    }
}