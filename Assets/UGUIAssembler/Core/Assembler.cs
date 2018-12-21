using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIAssembler
{
    public class Assembler
    {
        private static AssemblerStateMechine mechine;
        private static Type _emptyImporter;
        public static Type emptyImporter
        {
            get
            {
                if (_emptyImporter == null)
                {
                    _emptyImporter = typeof(EmptyLayerImport);
                }
                return _emptyImporter;
            }
            set
            {
                _emptyImporter = value;
            }
        }

        public static GameObject GenerateUI(string spriteFolderPath, Canvas canvas, Dictionary<string, Type> layerImportTypes, UIInfo uiInfo)
        {
            if (layerImportTypes == null) return null;
            mechine = new AssemblerStateMechine();
            mechine.spriteFolderPath = spriteFolderPath;
            mechine.uiSize = PreferHelper.uiSize;
            mechine.canvas = canvas;
            mechine.fontFolderPath = PreferHelper.fontFolderPath;
            mechine.textureFolderPath = PreferHelper.textureFolderPath;
            mechine.materialFolderPath = PreferHelper.materialFolderPath;
            mechine.RegistEmptyImprot(Activator.CreateInstance(emptyImporter) as ILayerImport);
            using (var enumerator = layerImportTypes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (typeof(ILayerImport).IsAssignableFrom(current.Value))
                    {
                        var instence = Activator.CreateInstance(current.Value) as ILayerImport;
                        if (instence != null)
                        {
                            mechine.RegistLayerimport(current.Key, instence);
                        }
                    }
                }
            }
            if (mechine.canvas != null)
            {
                Screen.SetResolution(PreferHelper.uiSize.x, PreferHelper.uiSize.y, false);
#if UNITY_EDITOR
                var ok = CheckGameViewSize(PreferHelper.uiSize);
                if (!ok)
                {
                    var @continue = UnityEditor.EditorUtility.DisplayDialog("屏幕比例和配制不一致：" + PreferHelper.uiSize.ToString(), "继续生成会造成坐标不正确", "继续", "取消");
                    if (!@continue)
                    {
                        return null;
                    }
                }
#endif
                return mechine.GenerateUI(uiInfo);
            }
            return null;

        }

#if UNITY_EDITOR
        private static bool CheckGameViewSize(Vector2Int uiSize)
        {
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetMainGameView.Invoke(null, null);
            var gameView = (UnityEditor.EditorWindow)Res;
            var prop = gameView.GetType().GetProperty("currentGameViewSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gvsize = prop.GetValue(gameView, new object[0] { });
            var gvSizeType = gvsize.GetType();
            var height = (int)gvSizeType.GetProperty("height", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(gvsize, new object[0]);
            var width = (int)gvSizeType.GetProperty("width", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(gvsize, new object[0]);

            if ((width / (height + 0f)) != (uiSize.x / (uiSize.y + 0f)))
            {
                return false;
            }
            return true;
        }
#endif

    }
}