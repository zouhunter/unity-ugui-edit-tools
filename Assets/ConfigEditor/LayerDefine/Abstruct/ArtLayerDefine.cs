using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public abstract class ArtLayerDefine : RectTransformLayerDefine
    {
        public ArtLayerDefine()
        {
            memberTypeDic.Add("rect", typeof(Rect));
            memberTypeDic.Add("padding", typeof(Vector4));
        }
    }
}