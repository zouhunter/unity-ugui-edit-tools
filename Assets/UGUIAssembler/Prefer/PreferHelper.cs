using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UGUIAssembler
{
    public class PreferHelper
    {
        #region Prefers
        private static PreferString _defultSpriteFolder = new PreferString("UGUIAssembler.defultSpriteFolder", "Assets/Resources/Sprites");
        private static PreferString _configFolderPath = new PreferString("UGUIAssembler.configFolderPath", "D:/Config");
        private static PreferString _uiSizeStr = new PreferString("UGUIAssembler.uiSize", "{\"x\":1920,\"y\":1080}");
        private static PreferString _fontFolderPath = new PreferString("UGUIAssembler.fontFolderPath", "Assets/Resources/Fonts");
        private static PreferString _defultFontrPath = new PreferString("UGUIAssembler.defultFontPath", "_innerFont");
        private static PreferString _materialFolderPath = new PreferString("UGUIAssembler.materialFolderPath", "Assets/Resources/Materials");
        private static PreferString _textureFolderPath = new PreferString("UGUIAssembler.textureFolderPath", "Assets/Resources/Textures");
        private static PreferString _fliterCsvRegix = new PreferString("UGUIAssembler.fliterCsvRegix", "_uiconfig");
        #endregion

        #region Propertys
        public static string defultFontrPath
        {
            get
            {
                return _defultFontrPath.Value;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _defultFontrPath.ResetDefult();
                }
                else
                {
                    _defultFontrPath.Value = value;
                }
            }
        }

        public static string fliterCsvRegix
        {
            get
            {
                return _fliterCsvRegix.Value;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _fliterCsvRegix.ResetDefult();
                }
                else
                {
                    _fliterCsvRegix.Value = value;
                }
            }
        }

        
        public static string fontFolderPath
        {
            get
            {
                return _fontFolderPath.Value;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _fontFolderPath.Value = value;
                    _defultFont = null;//清空加载的字体
                }
            }
        }
        public static string defultSpriteFolder
        {
            get
            {
                return _defultSpriteFolder.Value;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _defultSpriteFolder.Value = value;
                }
            }
        }
        public static string configFolderPath
        {
            get
            {
                return _configFolderPath.Value;
            }
            set
            {
                _configFolderPath.Value = value;
            }
        }

        public static string materialFolderPath
        {
            get
            {
                return _materialFolderPath.Value;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _materialFolderPath.Value = value;
                }
            }
        }
        public static string textureFolderPath
        {
            get
            {
                return _textureFolderPath.Value;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _textureFolderPath.Value = value;
                }
            }
        }

        private static Vector2Int _usSize;
        public static Vector2Int uiSize
        {
            get
            {
                if (_usSize == Vector2Int.zero)
                {
                    if (!string.IsNullOrEmpty(_uiSizeStr.Value))
                    {
                        var vect = JsonUtility.FromJson<Vector2>(_uiSizeStr.Value);
                        _usSize =new Vector2Int((int)vect.x,(int)vect.y);

                        if (_usSize == Vector2Int.zero)
                        {
                            _uiSizeStr.ResetDefult();
                        }
                    }
                }
                return _usSize;
            }
            set
            {
                if (_usSize != value)
                {
                    _usSize = value;
                    _uiSizeStr.Value = JsonUtility.ToJson(new Vector2(value.x,value.y));
                }
            }
        }

        private static Font _defultFont;
        public static Font defultFont
        {
            get
            {
                if (_defultFont == null && defultFontrPath != "_innerFont")
                {
#if UNITY_EDITOR
                    var fullpath = fontFolderPath + "/" + defultFontrPath;
                    _defultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(fullpath);
#endif
                }
                return _defultFont;
            }
            set
            {
                _defultFont = value;
            }
        }
        #endregion

    }
}