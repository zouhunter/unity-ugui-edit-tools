using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace UGUIAssembler
{
    public class ParamAnalysisTool
    {
        public const char ele_seperator = ',';
        public const char path_seperator = '/';
        public const char dic_seperator = ':';
        public const char line_seperator = '\n';
        public const string group_left = "(";
        public const string group_right = ")";
        public const string group_pattern = @"\((.*)\)";
        public const string range_left = "[";
        public const string range_right = "]";
        public const string range_pattern = @"\[(.*)\]";


        public const string subresource_pattern1 = @"\w+\s*:\s*\{[^\}]+\}";
        public const string subresource_pattern2 = @"(\w+)\s*:\s*\{([^\}]+)\}";
        public const string resouceListItem_pattern = @"\{[^\}]+\}";
        public const string image_art_key = "Image";
        public const string text_art_key = "Text";
        public const string rawImage_art_key = "RawImage";

        #region String To Object
        public static object InnerStructFromString(Type type, string value)
        {
            if (type == typeof(ColorBlock))
            {
                return ParamAnalysisTool.StringToColorBlock(value);
            }
            if (type == typeof(Color))
            {
                return ParamAnalysisTool.StringToColor(value);
            }
            else if (type == typeof(Vector2))
            {
                return ParamAnalysisTool.StringToVector2(value);
            }
            else if (type == typeof(Vector2Int))
            {
                return ParamAnalysisTool.StringToVector2Int(value);
            }
            else if (type == typeof(Vector3))
            {
                return ParamAnalysisTool.StringToVector3(value);
            }
            else if (type == typeof(Vector3Int))
            {
                return ParamAnalysisTool.StringToVector3Int(value);
            }
            else if (type == typeof(Vector4))
            {
                return ParamAnalysisTool.StringToVector4(value);
            }
            else if (type == typeof(Rect))
            {
                return ParamAnalysisTool.StringToRect(value);
            }
            Debug.LogWarningFormat("未成功解析类型为{0}的信息", type.FullName);
            return null;
        }

        public static SpriteState StringToSpriteState(string value, string spriteFolderPath)
        {
            var array = GroupToArray(value);
            if (array == null) return default(SpriteState);
            SpriteState spriteState = new SpriteState();
            for (int i = 0; i < array.Length; i++)
            {
                var sprite = StringToObject<Sprite>(array[i], spriteFolderPath);
                if (sprite == null) continue;
                if (i == 0)
                {
                    spriteState.highlightedSprite = sprite;
                }
                else if (i == 1)
                {
                    spriteState.pressedSprite = sprite;
                }
                else if (i == 2)
                {
                    spriteState.disabledSprite = sprite;
                }
            }
            return spriteState;
        }

        public static ColorBlock StringToColorBlock(string value)
        {
            var array = GroupToArray(value);
            if (array == null)
                return default(ColorBlock);
            ColorBlock colorBlock = new ColorBlock();
            colorBlock.colorMultiplier = 1;
            for (int i = 0; i < 4; i++)
            {
                Color color = Color.white;
                if (array.Length > i)
                {
                    color = StringToColor(array[i]);
                }
                if (i == 0)
                {
                    colorBlock.normalColor = color;
                }
                else if (i == 1)
                {
                    colorBlock.highlightedColor = color;
                }
                else if (i == 2)
                {
                    colorBlock.pressedColor = color;
                }
                else if (i == 3)
                {
                    colorBlock.disabledColor = color;
                }
            }
            return colorBlock;
        }

        public static Color StringToColor(string value)
        {
            if (!value.StartsWith("#"))
            {
                value = "#" + value;
            }
            var color = Color.white;
            ColorUtility.TryParseHtmlString(value, out color);
            return color;
        }

        public static T StringToObject<T>(string path, string folderPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path)) return null;
#if !RUNTIME_UI
            var fullPath = folderPath + "/" + path;
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (obj == null)
            {
                Debug.LogWarning("[资源加载失败]：" + fullPath);
            }
            return obj;
#else
            var fullPath = folderPath + "/" + path;
            fullPath = System.IO.Path.GetFullPath(fullPath);
            if (System.IO.File.Exists(fullPath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(fullPath);
                if (typeof(T) == typeof(Texture))
                {
                    var texture = new Texture2D(2048, 2048);
                    texture.LoadImage(fileBytes, false);
                    return texture as T;
                }
                else if (typeof(T) == typeof(Sprite))
                {
                    var texture = new Texture2D(2048, 2048);
                    texture.LoadImage(fileBytes, false);
                    var rect = new Rect(0, 0, texture.width, texture.height);
                    return Sprite.Create(texture, rect, rect.size * 0.5f) as T;
                }
                else
                {
                    Debug.LogWarning("暂时无法运行时加载：" + typeof(T).FullName);
                }
            }
            else
            {
                Debug.LogWarning("未找到文件：" + fullPath);
            }
            return null;
#endif
        }

        public static Rect StringToRect(string rectString)
        {
            var array = RangeToArray(rectString);
            if (array == null)
            {
                return Rect.zero;
            }
            var rect = new Rect();
            for (int i = 0; i < array.Length; i++)
            {
                float value = 0;
                float.TryParse(array[i], out value);
                if (i == 0)
                {
                    rect.x = value;
                }
                else if (i == 1)
                {
                    rect.y = value;
                }
                else if (i == 2)
                {
                    rect.width = value;
                }
                else if (i == 3)
                {
                    rect.height = value;
                }
            }
            return rect;
        }


        public static Vector2 StringToVector2(string param)
        {
            var array = GroupToFloatArray(param);
            var vector = Vector2.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i < 2)
                    {
                        vector[i] = array[i];
                    }
                }
            }
            return vector;
        }
        public static Vector2Int StringToVector2Int(string param)
        {
            var array = GroupToFloatArray(param);
            var vector = Vector2Int.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i < 2)
                    {
                        vector[i] = (int)array[i];
                    }
                }
            }
            return vector;
        }
        public static Vector3 StringToVector3(string param)
        {
            var array = GroupToFloatArray(param);
            var vector = Vector3.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i < 3)
                    {
                        vector[i] = array[i];
                    }
                }
            }
            return vector;
        }
        public static Vector3Int StringToVector3Int(string param)
        {
            var array = GroupToFloatArray(param);
            var vector = Vector3Int.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i < 3)
                    {
                        vector[i] = (int)array[i];
                    }
                }
            }
            return vector;
        }
        public static Vector4 StringToVector4(string param)
        {
            var array = GroupToFloatArray(param);
            var vector = Vector4.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i < 4)
                    {
                        vector[i] = array[i];
                    }
                }
            }
            return vector;
        }
        public static float[] GroupToFloatArray(string groupStr)
        {
            var array = GroupToArray(groupStr);
            if (array == null)
            {
                return null;
            }
            var fArray = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                float.TryParse(array[i], out fArray[i]);
            }
            return fArray;
        }

        public static string[] RangeToArray(string rangeStr)
        {
            if (string.IsNullOrEmpty(rangeStr)) return null;

            var match = Regex.Match(rangeStr, range_pattern);
            if (match.Success)
            {
                var text = match.Groups[1];
                var array = text.Value.Split(ele_seperator);
                return array;
            }
            return null;
        }

        public static string[] GroupToArray(string groupStr)
        {
            var match = Regex.Match(groupStr, group_pattern);
            if (match.Success)
            {
                var text = match.Groups[1];
                var array = text.Value.Split(ele_seperator);
                return array;
            }
            return null;
        }

        public static object IconventibleFromString(Type type, string value)
        {
            if (typeof(IConvertible).IsAssignableFrom(type))
            {
                if (type.IsSubclassOf(typeof(System.Enum)))
                {
                    return Enum.Parse(type, value);
                }
                else
                {
                    try
                    {
                        var objValue = Convert.ChangeType(value, type);
                        return objValue;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message + ":" + value);
                    }
                }
            }
            return null;
        }
        #endregion

        #region Object To String

        public static string FromDictionary(ResourceDic resourceDic)
        {
            StringBuilder text = new StringBuilder();
            using (var enumerator = resourceDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    text.Append(string.Format("{0}:{1}\n", pair.Key, pair.Value));
                }
            }
            return text.ToString();
        }

        internal static string FromDictionarySub(Dictionary<string, ResourceDic> subResourceDic, List<ResourceDic> sub_images, List<ResourceDic> sub_texts, List<ResourceDic> sub_rawImages)
        {
            StringBuilder text = new StringBuilder();

            if (subResourceDic != null)
            {
                using (var enumerator = subResourceDic.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var pair = enumerator.Current;
                        AppendSubDic(text, pair.Key, pair.Value);
                    }
                }
            }

            if (sub_images != null)
            {
                using (var enumerator = sub_images.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var dic = enumerator.Current;
                        AppendSubDic(text, image_art_key, dic);
                    }
                }
            }

            if (sub_texts != null)
            {
                using (var enumerator = sub_texts.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var dic = enumerator.Current;
                        AppendSubDic(text, text_art_key, dic);
                    }
                }
            }

            if (sub_rawImages != null)
            {
                using (var enumerator = sub_rawImages.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var dic = enumerator.Current;
                        AppendSubDic(text, rawImage_art_key, dic);
                    }
                }
            }

            return text.ToString();
        }

        private static void AppendSubDic(StringBuilder stringBuilder, string subKey, ResourceDic subDic)
        {
            var subText = FromDictionary(subDic);
            stringBuilder.Append(subKey);
            stringBuilder.Append(":{\n");
            stringBuilder.Append(subText);
            stringBuilder.Append("}\n");
        }

        public static string InnerStructObjectToString(Type type, object value)
        {
            if(type == typeof(Rect))
            {
                return RectToString((Rect)value);
            }
            else if(type == typeof(RectInt))
            {
                return RectIntToString((RectInt)value);
            }
            else if(type == typeof(ColorBlock))
            {
                return ColorBlockToString((ColorBlock)value);
            }
            else if (type == typeof(Vector2))
            {
                return ParamAnalysisTool.Vector2ToString((Vector2)value);
            }
            else if (type == typeof(Vector2Int))
            {
                return ParamAnalysisTool.Vector2IntToString((Vector2Int)value);
            }
            else if (type == typeof(Vector3))
            {
                return ParamAnalysisTool.Vector3ToString((Vector3)value);
            }
            else if (type == typeof(Vector2Int))
            {
                return ParamAnalysisTool.Vector3IntToString((Vector3Int)value);
            }
            else if (type == typeof(Vector4))
            {
                return ParamAnalysisTool.Vector4ToString((Vector4)value);
            }
            return null;
        }

        public static string RectToString(Rect rect)
        {
            var strArray = new string[]
            {
                rect.x.ToString("f2"),
                rect.y.ToString("f2"),
                rect.width.ToString("f2"),
                rect.height.ToString("f2")
            };
            return ArrayToRange(strArray);
        }

        public static string RectIntToString(RectInt rect)
        {
            var strArray = new string[]
            {
                rect.x.ToString("f2"),
                rect.y.ToString("f2"),
                rect.width.ToString("f2"),
                rect.height.ToString("f2")
            };
            return ArrayToRange(strArray);
        }

        public static string ColorBlockToString(ColorBlock colorBlock)
        {
            var strArray = new string[]
            {
               ColorUtility.ToHtmlStringRGBA( colorBlock.normalColor),
               ColorUtility.ToHtmlStringRGBA( colorBlock.highlightedColor),
               ColorUtility.ToHtmlStringRGBA( colorBlock.pressedColor),
               ColorUtility.ToHtmlStringRGBA( colorBlock.disabledColor)
            };
            return ArrayToRange(strArray);
        }

        public static string ColorToString(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        public static string Vector2ToString(Vector2 value)
        {
            var array = new string[2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }

        public static string Vector2IntToString(Vector2Int value)
        {
            var array = new string[2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }
        public static string Vector3ToString(Vector3 value)
        {
            var array = new string[3];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }
        public static string Vector3IntToString(Vector3Int value)
        {
            var array = new string[3];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }
        public static string Vector4ToString(Vector4 value)
        {
            var array = new string[4];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }
        public static string IntToString(int value)
        {
            return value.ToString();
        }

        public static int ToInt(string text)
        {
            var intValue = 0;
            int.TryParse(text, out intValue);
            return intValue;
        }
        public static string StructToString(object value)
        {
            return JsonUtility.ToJson(value);
        }

        public static string ArrayToGroup(params string[] array)
        {
            var group = group_left;
            group += string.Join(ele_seperator.ToString(), array);
            group += group_right;
            return group;
        }
        public static string ArrayToRange(params string[] array)
        {
            var range = range_left;
            range += string.Join(ele_seperator.ToString(), array);
            range += range_right;
            return range;
        }
        #endregion


        /// <summary>
        /// 按父级尺寸及相对坐标
        /// 得到Unity下的相对于父级的坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rect PsRectToUnityRect(Vector2 parentSize, Rect rect)
        {
            var pos = rect.position;
            pos.x = (rect.position.x + rect.width * 0.5f) - parentSize.x * 0.5f;
            pos.y = parentSize.y * 0.5f - rect.height * 0.5f - rect.position.y;
            rect.position = pos;
            return rect;
        }

        public static ResourceDic ToDictionary(string resourceStr)
        {
            if (string.IsNullOrEmpty(resourceStr)) return null;

            var array = resourceStr.Split(line_seperator);
            var dic = new ResourceDic();
            for (int i = 0; i < array.Length; i++)
            {
                var keyValue = array[i];
                if (keyValue.Contains(dic_seperator.ToString()))
                {
                    var index = keyValue.IndexOf(dic_seperator);
                    if (index < 0 || index == keyValue.Length - 1) continue;
                    var key = keyValue.Substring(0, index);
                    var value = keyValue.Substring(index + 1);
                    dic[key] = value;
                }
            }
            return dic;
        }

        public static Dictionary<string, ResourceDic> ToDictionary_Sub(string subResourceStr, out List<ResourceDic> images, out List<ResourceDic> texts, out List<ResourceDic> rawImages)
        {
            images = new List<ResourceDic>();
            texts = new List<ResourceDic>();
            rawImages = new List<ResourceDic>();

            if (string.IsNullOrEmpty(subResourceStr)) return null;

            System.Text.RegularExpressions.Regex reg = new Regex(subresource_pattern1);
            var matchs = reg.Matches(subResourceStr);
            Dictionary<string, ResourceDic> subResourceDic = null;
            if (matchs.Count > 0)
            {
                subResourceDic = new Dictionary<string, ResourceDic>();
                for (int i = 0; i < matchs.Count; i++)
                {
                    var match = matchs[i];
                    if (match.Success)
                    {
                        var oneResource = match.Groups[0].Value;
                        reg = new Regex(subresource_pattern2);
                        match = reg.Match(oneResource);
                        if (match.Success && match.Groups.Count == 3)
                        {
                            var key = match.Groups[1].Value;
                            var value = match.Groups[2].Value;
                            var valueDic = ToDictionary(value);
                            if (valueDic != null)
                            {
                                if (key == image_art_key)
                                {
                                    images.Add(valueDic);
                                }
                                else if (key == text_art_key)
                                {
                                    texts.Add(valueDic);
                                }
                                else if (key == rawImage_art_key)
                                {
                                    rawImages.Add(valueDic);
                                }
                                else
                                {
                                    if (!subResourceDic.ContainsKey(key))
                                    {
                                        subResourceDic.Add(key, valueDic);
                                    }
                                    else
                                    {
                                        Debug.LogWarning("重复关键字:" + key);
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return subResourceDic;
        }

        public static List<ResourceDic> ToDictionaryArray(string resourceStr)
        {
            if (string.IsNullOrEmpty(resourceStr)) return null;

            System.Text.RegularExpressions.Regex reg = new Regex(resouceListItem_pattern);
            var matchs = reg.Matches(resourceStr);
            List<ResourceDic> subResourceList = null;
            if (matchs.Count > 0)
            {
                subResourceList = new List<ResourceDic>();
                for (int i = 0; i < matchs.Count; i++)
                {
                    var match = matchs[i];
                    if (match.Success)
                    {
                        var value = match.Groups[0].Value;
                        var valueDic = ToDictionary(value);
                        subResourceList.Add(valueDic);
                    }
                }
            }
            return subResourceList;

        }
    }
}
