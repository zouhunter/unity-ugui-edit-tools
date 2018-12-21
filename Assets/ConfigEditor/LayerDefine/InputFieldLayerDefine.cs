using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UGUIAssembler.Config
{
    internal class InputFieldLayerDefine : RectTransformLayerDefine
    {
        public const string key_textComponent = "textComponent";
        public const string key_placeholder = "placeholder";

        public override Type type
        {
            get
            {
                return typeof(InputField);
            }
        }
        public InputFieldLayerDefine()
        {
            integrantSubControls = new List<string>() {
                key_textComponent,
                key_placeholder
            };
        }
       
        public override string GetSubControlName(string key)
        {
            switch (key)
            {
                case key_placeholder:
                    return "占位";
                case key_textComponent:
                    return "文字";
                default:
                    return key;
            }
        }
        public override string GetSubControlType(string key)
        {
            switch (key)
            {
                case key_placeholder:
                case key_textComponent:
                    return "Text";
                default:
                    return key;
            }
        }
    }
}