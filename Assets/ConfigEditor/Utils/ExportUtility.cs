#define UNITY_2017
using Ntreev.Library.Psd;
using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using UnityEditor;
using UnityEngine;


namespace UGUIAssembler.Config
{
    /// <summary>
    /// 1.psd文件层级和unity显示方式相反
    /// </summary>
    public static class ExportUtility
    {
        private static string exportPathFolder
        {
            get
            {
                var _exportPath = PreferHelper.defultSpriteFolder;
                if (!Directory.Exists(_exportPath))
                {
                    Directory.CreateDirectory(_exportPath);
                }
                return _exportPath;
            }
        }
        private static string rootName = "[根目标]";
        private static bool analysisError;
        public static UIInfo CreatePictures(PsdDocument document, string fileName)
        {
            var uiInfo = new UIInfo(fileName);
            LayerInfo rootLayerInfo = new LayerInfo();
            rootLayerInfo.name = uiInfo.name;
            rootLayerInfo.path = rootName;
            rootLayerInfo.rect = GetRectFromLayer(document);
            rootLayerInfo.type = "RectTransform";
            uiInfo.layers.Add(rootLayerInfo);
            analysisError = false;
            for (int i = document.Childs.Length - 1; i >= 0; i--)
            {
                var rootLayer = document.Childs[i];
                AnalysisLayer(rootLayer as PsdLayer, rootLayerInfo, uiInfo.layers);
            }
            if(analysisError == true)
            {
                DialogHelper.ShowDialog("解析异常", "层级解析不正常，请查看日志以看详情！","确认");
            }
            return uiInfo;
        }

        private static void AnalysisLayer(PsdLayer layer, LayerInfo layerInfo, List<LayerInfo> nodes)
        {
            if (layer.IsGroup)
            {
                var rect = GetRectFromLayer(layer);
                var path = GetFullPath(layer);
                var childLayerInfo = new LayerInfo()
                {
                    name = layer.Name,
                    rect = rect,
                    path = path,
                    type = "RectTransform",
                };
                nodes.Add(childLayerInfo);

                if (layer.Childs.Length > 0)
                {
                    for (int i = 0; i < layer.Childs.Length; i++)
                    {
                        var child = layer.Childs[i];
                        AnalysisLayer(child, childLayerInfo, nodes);
                    }
                }
            }
            else
            {
                AnalysisLayerInfo(layer, layerInfo);
            }
        }
        public static void AnalysisLayerInfo(PsdLayer layer, LayerInfo layerInfo)
        {
            ResourceDic dic = new ResourceDic();
            Rect parentRect = layerInfo.rect;
            Rect innerRect = GetSubRectFromLayer(layer, parentRect);
            dic.Add("name", layer.Name);
            switch (layer.LayerType)
            {
                case LayerType.Normal:
                case LayerType.Color:
                    layerInfo.sub_images.Add(dic);
                    dic.Add("sprite", layer.Name + ".png");
                    dic.Add("rect", ParamAnalysisTool.RectToString(innerRect));
                    CreatePNGFile(layer);
                    break;
                case LayerType.Text:
                    layerInfo.sub_texts.Add(dic);
                    var textInfo = layer.Records.TextInfo;
                    var color = new Color(textInfo.color[0], textInfo.color[1], textInfo.color[2], textInfo.color[3]);
                    dic.Add("rect", ParamAnalysisTool.RectToString(GetMarginRect(innerRect, 1.2f)));
                    dic.Add("color", ParamAnalysisTool.ColorToString(color));
                    dic.Add("text", textInfo.text);
                    if (!string.IsNullOrEmpty(textInfo.fontName))
                        dic.Add("font", textInfo.fontName);
                    dic.Add("fontSize", textInfo.fontSize.ToString());
                    break;
                case LayerType.Complex:
                    Debug.Log("目标层解析能正常，请修改为智能对象！ layer --> " + layer.Name);
                    layerInfo.sub_images.Add(dic);
                    dic.Add("sprite", layer.Name + ".png");
                    dic.Add("rect", ParamAnalysisTool.RectToString(innerRect));
                    CreatePNGFile(layer);
                    break;
                default:
                    break;
            }
        }

        private static void CreatePNGFile(PsdLayer layer)
        {
            var texture = CreateTexture(layer);

        trySave: try
            {
                var bytes = texture.EncodeToPNG();
                System.IO.File.WriteAllBytes(exportPathFolder + "/" + layer.Name + ".png", bytes);
            }
            catch (Exception e)
            {
                var retry = DialogHelper.ShowDialog("保存文件出错", e.Message, "重试", "取消");
                if (retry)
                {
                    goto trySave;
                }
            }
        }

        /// <summary>
        /// psd中解析出的文字区域偏小
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="bigger"></param>
        /// <returns></returns>
        public static Rect GetMarginRect(Rect rect, float bigger)
        {
            var width = bigger * rect.width;
            var height = bigger * rect.height;
            var center = rect.center;
            return new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);
        }
        private static string GetFullPath(IPsdLayer layer)
        {
            var path = new List<string>();
            var current = layer;
            while (!(current is PsdDocument))
            {
                path.Insert(0, current.Name);
                current = current.Parent;
            }
            return string.Join("/", path.ToArray());
        }

        /// <summary>
        /// 从layer解析图片
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static Texture2D CreateTexture(PsdLayer layer)
        {
            Debug.Assert(layer.Width != 0 && layer.Height != 0, layer.Name + ": width = height = 0");

            if (layer.Width == 0 || layer.Height == 0)
                return new Texture2D(layer.Width, layer.Height);

            if (layer.Resources.Contains("lfx2"))
            {
                analysisError = true;
                Debug.LogWarning(layer.Name + " 包含特效，本工具暂不支持解析，可将目标层转换为智能对象后重试！");
            }

            return ChannelToTexture(layer.Width, layer.Height, layer.Channels);
        }

        private static Texture2D ChannelToTexture(int width, int height, Channel[] Channels)
        {
            Texture2D texture = new Texture2D(width, height);
            Color32[] pixels = GetPixelsFromChannels(width, height, Channels);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 从通道解析颜色
        /// </summary>
        /// <param name="Channels"></param>
        /// <returns></returns>
        private static Color32[] GetPixelsFromChannels(int width, int height, Channel[] Channels)
        {
            Color32[] pixels = new Color32[width * height];

            Channel red = Array.Find(Channels, i => i.Type == ChannelType.Red);
            Channel green = Array.Find(Channels, i => i.Type == ChannelType.Green);
            Channel blue = Array.Find(Channels, i => i.Type == ChannelType.Blue);
            Channel alpha = Array.Find(Channels, i => i.Type == ChannelType.Alpha);

            for (int i = 0; i < pixels.Length; i++)
            {
                var redErr = red == null || red.Data == null || red.Data.Length <= i;
                var greenErr = green == null || green.Data == null || green.Data.Length <= i;
                var blueErr = blue == null || blue.Data == null || blue.Data.Length <= i;
                var alphaErr = alpha == null || alpha.Data == null || alpha.Data.Length <= i;

                byte r = redErr ? (byte)0 : red.Data[i];
                byte g = greenErr ? (byte)0 : green.Data[i];
                byte b = blueErr ? (byte)0 : blue.Data[i];
                byte a = alphaErr ? (byte)255 : alpha.Data[i];

                int mod = i % width;
                int n = ((width - mod - 1) + i) - mod;
                pixels[pixels.Length - n - 1] = new Color32(r, g, b, a);
            }
            return pixels;
        }

        /// <summary>
        /// 解析Layer中的尺寸信息
        /// </summary>
        /// <param name="psdLayer"></param>
        /// <returns></returns>
        private static Rect GetRectFromLayer(IPsdLayer psdLayer)
        {
            var left = psdLayer.Left;
            var top = psdLayer.Top;
            var width = psdLayer.Width;
            var height = psdLayer.Height;
            return new Rect(left, top, width, height);
        }

        /// <summary>
        /// 获取相对坐标
        /// </summary>
        /// <param name="psdLayer"></param>
        /// <param name="rootRect"></param>
        /// <returns></returns>
        private static Rect GetSubRectFromLayer(IPsdLayer psdLayer, Rect rootRect)
        {
            var rect = GetRectFromLayer(psdLayer);
            rect.x = rect.x - rootRect.x;
            rect.y = rect.y - rootRect.y;
            return rect;
        }

        /// <summary>
        /// 偏移
        /// </summary>
        /// <param name="psdLayer"></param>
        /// <param name="rootRect"></param>
        /// <returns></returns>
        private static Rect GetPaddingFromLayer(IPsdLayer psdLayer, Rect rootRect)
        {
            var rect = GetRectFromLayer(psdLayer);

            var left = rootRect.x - rect.x;
            var top = rootRect.y - rect.y;
            var right = rootRect.width - rect.width - left;
            var bottom = rootRect.height - rect.height - top;
            var padding = new Rect(left, top, right, bottom);
            return padding;
        }
    }
}