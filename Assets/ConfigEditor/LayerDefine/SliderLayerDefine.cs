using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    internal class SliderLayerDefine : RectTransformLayerDefine
    {
        private const string key_background = "background";
        private const string key_fill = "fill";
        private const string key_handle = "handle";

        public override Type type
        {
            get
            {
                return typeof(Slider);
            }
        }

        public override string GetSubControlName(string key)
        {
            switch (key)
            {
                case key_background:
                    return "滑动背景";
                case key_fill:
                    return "滑动填充";
                case key_handle:
                    return "滑动手柄";
                default:
                    return key;
            }
        }

        public override string GetSubControlType(string key)
        {
            switch (key)
            {
                case key_background:
                case key_fill:
                case key_handle:
                    return "Image";
                default:
                    return null;
            }
        }

        public SliderLayerDefine()
        {
            runtimeSubControls = new List<string>() { key_handle };
            integrantSubControls = new List<string>() { key_background, key_fill };
            memberTypeDic.Add("size", typeof(Vector2));
        }
    }
}