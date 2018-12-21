using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Reflection;
using UnityEditor;

namespace UGUIAssembler
{
    public static class LayerImportUtil
    {
        public const char path_seperator = '/';
        public const string custom_key_rect = "rect";

        /// <summary>
        /// 创建Canvas
        /// </summary>
        /// <param name="uiSize"></param>
        /// <returns></returns>
        public static Canvas CreateCanvas(Vector2 uiSize)
        {
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(RectTransform), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            scaler.referenceResolution = new Vector2(uiSize.x, uiSize.y);
            return canvas;
        }

        /// <summary>
        ///创建Title
        /// </summary>
        /// <param name="titleKey"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Text MakeTitleComponenet(string titleKey, Transform parent)
        {
            var title = new GameObject(titleKey, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            title.transform.SetParent(parent, false);
            title.alignment = TextAnchor.MiddleCenter;
            title.raycastTarget = false;
            if (PreferHelper.defultFont)
                title.font = PreferHelper.defultFont;

            return title;
        }

        /// <summary>
        /// 获取控件支持的类型
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public static Dictionary<string, Type> GetSupportedMembers(Type controlType)
        {
            var members = controlType.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
            var memberDic = new Dictionary<string, Type>();
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                Type type = null;
                if (member is FieldInfo)
                {
                    var fieldInfo = (member as FieldInfo);
                    type = fieldInfo.FieldType;
                }
                else if (member is PropertyInfo)
                {
                    var propertyInfo = (member as PropertyInfo);
                    type = propertyInfo.PropertyType;
                    if (propertyInfo.GetSetMethod() == null)
                    {
                        continue;
                    }
                }

                //空
                if (type == null)
                {
                    continue;
                }

                //数组和集合
                if (type.IsArray || type.IsGenericType)
                {
                    continue;
                }

                //事件
                if (type.IsSubclassOf(typeof(UnityEngine.Events.UnityEvent)) || type.IsSubclassOf(typeof(Delegate)))
                {
                    continue;
                }

                memberDic.Add(member.Name, type);
            }
            return memberDic;
        }

        public static void LoadCommonResources(AssemblerStateMechine mechine, UINode node)
        {
            LoadCommonResources(mechine, node, node.layerInfo.rect);
        }
        /// <summary>
        /// 加载Image/Text/Label
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="layerInfo"></param>
        public static void LoadCommonResources(AssemblerStateMechine mechine, UINode node, Rect baseRect)
        {
            Transform parent = node.transform;
            LayerInfo layerInfo = node.layerInfo;

            if (layerInfo.sub_images != null)
            {
                for (int i = 0; i < layerInfo.sub_images.Count; i++)
                {
                    var current = layerInfo.sub_images[i];
                    var image = new GameObject("Image", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                    image.transform.SetParent(parent, false);
                    mechine.ChargeInfo(image, current);
                    image.rectTransform.SetRectFromResource(layerInfo.rect, baseRect, current);
                }
            }

            if (layerInfo.sub_texts != null)
            {
                for (int i = 0; i < layerInfo.sub_texts.Count; i++)
                {
                    var current = layerInfo.sub_texts[i];
                    var title = LayerImportUtil.MakeTitleComponenet("Title", parent);
                    title.resizeTextForBestFit = true;//缩放时会看不到？
                    mechine.ChargeInfo(title, current);
                    title.rectTransform.SetRectFromResource(layerInfo.rect, baseRect, current);
                }

            }

            if (layerInfo.sub_rawImages != null)
            {
                for (int i = 0; i < layerInfo.sub_rawImages.Count; i++)
                {
                    var current = layerInfo.sub_rawImages[i];
                    var rawImage = new GameObject("RawImage", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
                    rawImage.transform.SetParent(parent, false);
                    mechine.ChargeInfo(rawImage, current);
                    rawImage.rectTransform.SetRectFromResource(layerInfo.rect, baseRect, current);
                }
            }
        }

        /// <summary>
        /// 根UGUI节点
        /// </summary>
        /// <param name="mechine"></param>
        /// <param name="layerInfo"></param>
        /// <returns></returns>
        public static UINode CreateRootNode(this AssemblerStateMechine mechine, LayerInfo layerInfo)
        {
            var path = layerInfo.path;
            var instenceName = NameFromPath(path);
            var go = new GameObject(instenceName, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(mechine.canvas.transform, false);
            var uiRect = ParamAnalysisTool.PsRectToUnityRect(mechine.uiSize, layerInfo.rect);
            SetRectTransform(uiRect, go.transform as RectTransform);
            UINode node = new UINode(go.transform, layerInfo);
            node.layerInfo = layerInfo;
            return node;
        }

        public static UINode CreateRootNode(this AssemblerStateMechine mechine, LayerInfo layerInfo, Rect customRect)
        {
            var path = layerInfo.path;
            var instenceName = NameFromPath(path);
            var go = new GameObject(instenceName, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(mechine.canvas.transform, false);
            var uiRect = ParamAnalysisTool.PsRectToUnityRect(mechine.uiSize, customRect);
            SetRectTransform(uiRect, go.transform as RectTransform);
            UINode node = new UINode(go.transform, layerInfo);
            node.layerInfo = layerInfo;
            return node;
        }

        /// <summary>
        /// 普通UGUI节点
        /// </summary>
        /// <param name="go"></param>
        /// <param name="rect"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static UINode CreateNormalNode(GameObject go, Transform root, Vector2 rootSize, LayerInfo layerInfo)
        {
            go.transform.SetParent(root, false);
            var uiRect = ParamAnalysisTool.PsRectToUnityRect(rootSize, layerInfo.rect);
            SetRectTransform(uiRect, go.transform as RectTransform);
            UINode node = new UINode(go.transform, layerInfo);
            return node;
        }

        /// <summary>
        /// 获取一个大的rect
        /// </summary>
        /// <param name="rects"></param>
        /// <returns></returns>
        public static Rect GetRectContent(params Rect[] rects)
        {
            var xmin = float.MaxValue;
            var ymin = float.MaxValue;
            var xmax = float.MinValue;
            var ymax = float.MinValue;

            for (int i = 0; i < rects.Length; i++)
            {
                var rect = rects[i];
                xmin = rect.x < xmin ? rect.x : xmin;
                ymin = rect.y < ymin ? rect.y : ymin;
                xmax = rect.xMax > xmax ? rect.xMax : xmax;
                ymax = rect.yMax > ymax ? rect.yMax : ymax;
            }


            return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        /// <summary>
        /// 设置目标Anchor(Custom 类型)
        /// </summary>
        /// <param name="parentRectt"></param>
        /// <param name="rectt"></param>
        public static void SetCustomAnchor(Vector2 p_sizeDelta, RectTransform rectt)
        {
            Vector2 sizeDelta = rectt.sizeDelta;
            Vector2 anchoredPosition = rectt.anchoredPosition;
            float xmin = p_sizeDelta.x * 0.5f + anchoredPosition.x - sizeDelta.x * 0.5f;
            float xmax = p_sizeDelta.x * 0.5f + anchoredPosition.x + sizeDelta.x * 0.5f;
            float ymin = p_sizeDelta.y * 0.5f + anchoredPosition.y - sizeDelta.y * 0.5f;
            float ymax = p_sizeDelta.y * 0.5f + anchoredPosition.y + sizeDelta.y * 0.5f;
            float xSize = 0;
            float ySize = 0;
            float xanchored = 0;
            float yanchored = 0;
            rectt.anchorMin = new Vector2(xmin / p_sizeDelta.x, ymin / p_sizeDelta.y);
            rectt.anchorMax = new Vector2(xmax / p_sizeDelta.x, ymax / p_sizeDelta.y);
            rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeDelta.x);
            rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeDelta.y);
            rectt.sizeDelta = new Vector2(xSize, ySize);
            rectt.anchoredPosition = new Vector2(xanchored, yanchored);
        }

        /// <summary>
        /// 设置目标Anchor
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rectTransform"></param>
        public static void SetAnchorByNode(UINode node)
        {
            RectTransform p_rt = node.parentNode.GetComponent<RectTransform>();
            RectTransform c_rt = node.GetComponent<RectTransform>();

            switch (node.anchoType)
            {
                case AnchoType.Custom:
                    SetCustomAnchor(p_rt.sizeDelta, c_rt);
                    break;
                default:
                    SetNormalAnchor(node.anchoType, p_rt.sizeDelta, c_rt);
                    break;
            }
        }

        /// <summary>
        /// 使用的前提是rectt的类型为双局中
        /// </summary>
        /// <param name="anchoType"></param>
        /// <param name="parentRectt"></param>
        /// <param name="rectt"></param>
        public static void SetNormalAnchor(AnchoType anchoType, Vector2 p_sizeDelta, RectTransform rectt)
        {
            Vector2 sizeDelta = rectt.sizeDelta;
            Vector2 anchoredPosition = rectt.anchoredPosition;

            float xmin = 0;
            float xmax = 0;
            float ymin = 0;
            float ymax = 0;
            float xSize = 0;
            float ySize = 0;
            float xanchored = 0;
            float yanchored = 0;

            if ((anchoType & AnchoType.Up) == AnchoType.Up)
            {
                ymin = ymax = 1;
                yanchored = anchoredPosition.y - p_sizeDelta.y * 0.5f;
                ySize = sizeDelta.y;
            }
            if ((anchoType & AnchoType.Down) == AnchoType.Down)
            {
                ymin = ymax = 0;
                yanchored = anchoredPosition.y + p_sizeDelta.y * 0.5f;
                ySize = sizeDelta.y;
            }
            if ((anchoType & AnchoType.Left) == AnchoType.Left)
            {
                xmin = xmax = 0;
                xanchored = anchoredPosition.x + p_sizeDelta.x * 0.5f;
                xSize = sizeDelta.x;
            }
            if ((anchoType & AnchoType.Right) == AnchoType.Right)
            {
                xmin = xmax = 1;
                xanchored = anchoredPosition.x - p_sizeDelta.x * 0.5f;
                xSize = sizeDelta.x;
            }
            if ((anchoType & AnchoType.XStretch) == AnchoType.XStretch)
            {
                xmin = 0; xmax = 1;
                xanchored = anchoredPosition.x;
                xSize = sizeDelta.x - p_sizeDelta.x;
            }
            if ((anchoType & AnchoType.YStretch) == AnchoType.YStretch)
            {
                ymin = 0; ymax = 1;
                yanchored = anchoredPosition.y;
                ySize = sizeDelta.y - p_sizeDelta.y;
            }
            if ((anchoType & AnchoType.XCenter) == AnchoType.XCenter)
            {
                xmin = xmax = 0.5f;
                xanchored = anchoredPosition.x;
                xSize = sizeDelta.x;
            }
            if ((anchoType & AnchoType.YCenter) == AnchoType.YCenter)
            {
                ymin = ymax = 0.5f;
                yanchored = anchoredPosition.y;
                ySize = sizeDelta.y;
            }

            rectt.anchorMin = new Vector2(xmin, ymin);
            rectt.anchorMax = new Vector2(xmax, ymax);

            rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeDelta.x);
            rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeDelta.y);

            rectt.sizeDelta = new Vector2(xSize, ySize);
            rectt.anchoredPosition = new Vector2(xanchored, yanchored);
        }

        /// <summary>
        /// 解析路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string[] PathToArray(string path)
        {
            path = path.Replace("\\", path_seperator.ToString());
            return path.Split(path_seperator);
        }

        /// <summary>
        /// 设置目标对象的尺寸
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="rectTransform"></param>
        public static void SetRectTransform(Rect rect, RectTransform rectTransform)
        {
            rectTransform.pivot = Vector2.one * 0.5f;
            rectTransform.anchorMin = rectTransform.anchorMax = Vector2.one * 0.5f;
            rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
            rectTransform.anchoredPosition = new Vector2(rect.x, rect.y);
        }


        public static string NameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            var id = path.LastIndexOf(path_seperator);
            path = path.Substring(id + 1);
            return path;
        }

        /// <summary>
        /// 按资源的Rect设置拉伸的坐标
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="baseRect"></param>
        /// <param name="resourceDic"></param>
        public static void SetRectFromResource(this RectTransform rectTransform, Rect baseRect, ResourceDic resourceDic = null)
        {
            var rect = new Rect(Vector2.zero, baseRect.size);
            if (resourceDic != null)
            {
                LayerImportUtil.UpdateRectFromResourceDic(ref rect, resourceDic);
            }
            rect = ParamAnalysisTool.PsRectToUnityRect(baseRect.size, rect);
            LayerImportUtil.SetRectTransform(rect, rectTransform);
            LayerImportUtil.SetCustomAnchor(baseRect.size, rectTransform);
        }

        /// <summary>
        /// 按资源的Rect设置拉伸的坐标
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="rootRect"></param> 
        /// <param name="baseRect"></param> 
        /// <param name="resourceDic"></param>
        public static void SetRectFromResource(this RectTransform rectTransform, Rect rootRect, Rect baseRect, ResourceDic resourceDic = null)
        {
            var rect = new Rect(Vector2.zero, baseRect.size);
            if (resourceDic != null)
            {
                if (LayerImportUtil.UpdateRectFromResourceDic(ref rect, resourceDic))
                {
                    //得到相对于BaseRect的相对值
                    rect = new Rect(rect.x + rootRect.x - baseRect.x, rect.y + rootRect.y - baseRect.y, rect.width, rect.height);
                }
            }
            rect = ParamAnalysisTool.PsRectToUnityRect(baseRect.size, rect);
            LayerImportUtil.SetRectTransform(rect, rectTransform);
            LayerImportUtil.SetCustomAnchor(baseRect.size, rectTransform);
        }


        /// <summary>
        /// 装载入信息
        /// </summary>
        /// <param name="component"></param>
        /// <param name="resourceDic"></param>
        public static void ChargeInfo(this AssemblerStateMechine mechine, Component component, ResourceDic resourceDic)
        {
            component.gameObject.SetActive(resourceDic.active);

            if (!resourceDic.active) return;

            using (var enumerator = resourceDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    ChargeInfoInternal(component, mechine, pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// 装载入信息
        /// </summary>
        /// <param name="component"></param>
        /// <param name="resourceDic"></param>
        public static void ChargeSubInfo(this AssemblerStateMechine mechine, Component component, string current, ResourceDic resourceDic)
        {
            component.gameObject.SetActive(resourceDic.active);

            if (!resourceDic.active) return;

            using (var enumerator = resourceDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    var key = current + "." + pair.Key;
                    ChargeInfoInternal(component, mechine, key, pair.Value);
                }
            }
        }

        private static void ChargeInfoInternal(object component, AssemblerStateMechine mechine, string key, string pair_value)
        {
            var instence = component;
            var member = TypeUtil.GetDeepMember(ref instence, key);

            if (member != null)
            {
                if (member is FieldInfo)
                {
                    var info = (member as FieldInfo);
                    var value = GetValueFromTypeString(info.FieldType, pair_value, mechine);
                    if (value != null)
                    {
                        info.SetValue(instence, value);
                    }
                }
                else if (member is PropertyInfo)
                {
                    var info = (member as PropertyInfo);
                    var value = GetValueFromTypeString(info.PropertyType, pair_value, mechine);
                    if (value != null)
                    {
                        info.SetValue(instence, value, null);
                    }
                }
            }
        }

        internal static string ArrayToPath(string[] array, int deepth)
        {
            if (array == null || deepth < 1)
                return null;

            deepth = deepth < array.Length ? deepth : array.Length;
            var path = "";
            for (int i = 0; i < deepth - 1; i++)
            {
                path += array[i] + path_seperator;
            }
            path += array[deepth - 1];
            return path;
        }

        public static object GetValueFromTypeString(Type type, string value, AssemblerStateMechine mechine)
        {
            if (type == typeof(SpriteState))
            {
                return ParamAnalysisTool.StringToSpriteState(value, mechine.spriteFolderPath);
            }
            else if (type == typeof(ColorBlock))
            {
                return ParamAnalysisTool.StringToColorBlock(value);
            }
            else if (type == typeof(Sprite))
            {
                return ParamAnalysisTool.StringToObject<Sprite>(value, mechine.spriteFolderPath);
            }
            else if (type == typeof(Font))
            {
                return ParamAnalysisTool.StringToObject<Font>(value, mechine.fontFolderPath);
            }
            else if (type == typeof(Material))
            {
                return ParamAnalysisTool.StringToObject<Material>(value, mechine.materialFolderPath);
            }
            else if (type == typeof(Texture))
            {
                return ParamAnalysisTool.StringToObject<Texture>(value, mechine.textureFolderPath);
            }
            else if (type == typeof(Texture2D))
            {
                return ParamAnalysisTool.StringToObject<Texture2D>(value, mechine.textureFolderPath);
            }
            else if (type == typeof(Color))
            {
                return ParamAnalysisTool.StringToColor(value);
            }
            else if (type == typeof(Vector2))
            {
                return ParamAnalysisTool.StringToVector2(value);
            }
            else if (type == typeof(Vector3))
            {
                return ParamAnalysisTool.StringToVector3(value);
            }
            else if (type == typeof(Vector4))
            {
                return ParamAnalysisTool.StringToVector4(value);
            }
            else if (type == typeof(Rect))
            {
                return ParamAnalysisTool.StringToRect(value);
            }
            else if (typeof(IConvertible).IsAssignableFrom(type))
            {
                if (type.IsSubclassOf(typeof(System.Enum)))
                {
                    return Enum.Parse(type, value);
                }
                else
                {
                    try
                    {
                        return Convert.ChangeType(value, type);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message + ":" + value);
                    }
                }

            }
            Debug.LogWarningFormat("未成功解析类型为{0}的信息", type.FullName);
            return null;
        }

        /// <summary>
        /// 从字典中解析rect信息
        /// </summary>
        /// <param name="defultRect"></param>
        /// <param name="resourceDic"></param>
        /// <returns></returns>
        public static bool UpdateRectFromResourceDic(ref Rect defultRect, ResourceDic resourceDic)
        {
            if (resourceDic.ContainsKey("rect"))
            {
                defultRect = ParamAnalysisTool.StringToRect(resourceDic["rect"]);
                return true;
            }
            else if (resourceDic.ContainsKey("padding"))
            {
                var paddingRect = ParamAnalysisTool.StringToVector4(resourceDic["padding"]);
                defultRect = new Rect(defultRect.x + paddingRect.x, defultRect.y + paddingRect.y, defultRect.width - paddingRect.z - paddingRect.x, defultRect.height - paddingRect.y - paddingRect.w);
            }
            return false;
        }

        /// <summary>
        /// 从字典中解析rect信息
        /// </summary>
        /// <param name="defultRect"></param>
        /// <param name="resourceDic"></param>
        /// <returns></returns>
        public static Rect AddSubRectFromResourceDic(Rect defultRect, ResourceDic resourceDic)
        {
            if (resourceDic.ContainsKey("rect"))
            {
                var subRect = ParamAnalysisTool.StringToRect(resourceDic["rect"]);
                defultRect = new Rect(defultRect.x + subRect.x, defultRect.y + subRect.y, subRect.width, subRect.height);
            }
            return defultRect;
        }

        public static void UpdateSizeFromResourceDic(ResourceDic resourceDic, ref Vector2 defultSize)
        {
            if (resourceDic.ContainsKey("size"))
            {
                defultSize = ParamAnalysisTool.StringToVector2(resourceDic["size"]);
            }
        }

    }
}