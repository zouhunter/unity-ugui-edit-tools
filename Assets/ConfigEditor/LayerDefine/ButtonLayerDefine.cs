using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public class ButtonLayerDefine : RectTransformLayerDefine
    {
        private const string key_label = "label";
        private const string key_image = "image";
        public override Type type
        {
            get
            {
                return typeof(Button);
            }
        }
        public override string GetSubControlName(string key)
        {
            switch (key)
            {
                case key_label:
                    return "文字(二选一)";
                case key_image:
                    return "图片(二选一)";
                default:
                    return key;
            }
        }
        public override string GetSubControlType(string key)
        {
            switch (key)
            {
                case key_label:
                    return "Text";
                case key_image:
                    return "Image";
                default:
                    return null;
            }
        }
        public ButtonLayerDefine()
        {
            runtimeSubControls = new List<string>() { key_image,key_label };
        }
    }
}