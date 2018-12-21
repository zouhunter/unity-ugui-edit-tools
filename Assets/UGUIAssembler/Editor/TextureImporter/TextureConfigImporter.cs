using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UGUIAssembler
{
    public class TextureConfigImporter
    {
        [MenuItem("Assets/Group-Import-Textures", true)]
        private static bool JudgeGroupImportTextureState()
        {
            var activeItem = Selection.activeObject;

            if (activeItem != null)
            {
                var path = AssetDatabase.GetAssetPath(activeItem);
                if (!string.IsNullOrEmpty(path))
                {
                    return path.EndsWith("_tc.csv");
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        [MenuItem("Assets/Group-Import-Textures")]
        private static void GroupImportTexture()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                var table = CsvHelper.ReadCSV(path, System.Text.Encoding.GetEncoding("gb2312"));
                TextureInfoTable infoTable = new TextureInfoTable();
                infoTable.LoadFromCsvTable(table);

                if (infoTable != null)
                {
                    var spriteFolder = EditorUtility.OpenFolderPanel("请选择需要批量导入信息的图片根目录！", System.IO.Path.GetDirectoryName(path), "");
                    if (!string.IsNullOrEmpty(spriteFolder))
                    {
                        spriteFolder = spriteFolder.Replace("\\", "/");
                        if (spriteFolder.StartsWith(Application.dataPath))
                        {
                            spriteFolder = spriteFolder.Replace(Application.dataPath, "Assets");

                            for (int i = 0; i < infoTable.textureInfos.Count; i++)
                            {
                                var textureInfo = infoTable.textureInfos[i];
                                var fullPath = spriteFolder + "/" + textureInfo.texturePath;

                                var texture = AssetDatabase.LoadAssetAtPath<Texture>(fullPath);

                                if (texture != null && textureInfo != null)
                                {
                                    SetTextureResourceDic(fullPath, textureInfo);
                                }
                            }

                            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// 设置图片资源字典
        /// </summary>
        /// <param name="texturePath"></param>
        /// <param name="info"></param>
        private static void SetTextureResourceDic(string texturePath, TextureInfo info)
        {
            TextureImporter textureImport = TextureImporter.GetAtPath(texturePath) as TextureImporter;
            textureImport.textureType = info.type;
            if (info.spritesheetList != null && info.spritesheetList.Count > 0)
                textureImport.spritesheet = AnalysisSpriteMetaData(info.spritesheetList);
            ChargeInfo(textureImport, info.resourceDic);
            textureImport.SaveAndReimport();
        }
        
        /// <summary>
        /// 解析子图片信息
        /// </summary>
        /// <param name="resourceList"></param>
        /// <returns></returns>
        private static SpriteMetaData[] AnalysisSpriteMetaData(List<ResourceDic> resourceList)
        {
            var metaData = new SpriteMetaData[resourceList.Count];
            for (int i = 0; i < metaData.Length; i++)
            {
                metaData[i] = new SpriteMetaData();
                metaData[i] = (SpriteMetaData)ChargeInfo(metaData[i], resourceList[i]);
            }
            return metaData;
        }

        /// <summary>
        /// 深度获取对象
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static MemberInfo GetFieldOrPropertyMember(object Instance, string memberName)
        {
            Type type = Instance.GetType();
            var members = type.GetMember(memberName, BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (members != null && members.Length > 0)
            {
                for (int i = 0; i < members.Length; i++)
                {
                   var member = members[0];

                    if (member is FieldInfo || member is PropertyInfo)
                    {
                        return member;
                    }
                }
            }
            Debug.LogError(type + "中未找到属性：" + memberName);
            return null;
        }

        /// <summary>
        /// 支持结构体的赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="resourceDic"></param>
        /// <returns></returns>
        public static object ChargeInfo(object target, ResourceDic resourceDic)
        {
            using (var enumerator = resourceDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;

                    var member = GetFieldOrPropertyMember(target, pair.Key);
                    if (member != null)
                    {
                        if (member is FieldInfo)
                        {
                            var info = (member as FieldInfo);
                            var value = GetValueFromTypeString(info.FieldType, pair.Value);
                            if (value != null)
                            {
                                info.SetValue(target, value);
                            }
                        }
                        else if (member is PropertyInfo)
                        {
                            var info = (member as PropertyInfo);
                            var value = GetValueFromTypeString(info.PropertyType, pair.Value);
                            if (value != null)
                            {
                                info.SetValue(target, value, null);
                            }
                        }
                    }
                }
            }
            return target;
        }

        /// <summary>
        /// 按类型和字符串解析值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetValueFromTypeString(Type type, string value)
        {
            if (type == typeof(Rect))
            {
                return ParamAnalysisTool.StringToRect(value);
            }
            else if (type == typeof(Vector2))
            {
                return ParamAnalysisTool.StringToVector2(value);
            }
            else if (type == typeof(Vector3))
            {
                return ParamAnalysisTool.StringToVector2(value);
            }
            else if (type == typeof(Vector4))
            {
                return ParamAnalysisTool.StringToVector4(value);
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
    }
}