using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public class TextLayerDefine : ArtLayerDefine
    {
        public override Type type
        {
            get
            {
                return typeof(Text);
            }
        }
    }
}