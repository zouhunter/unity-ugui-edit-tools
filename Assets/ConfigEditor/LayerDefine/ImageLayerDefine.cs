using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    internal class ImageLayerDefine : ArtLayerDefine
    {
        public override Type type
        {
            get
            {
                return typeof(Image);
            }
        }
    }
}