using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler
{
    public class UIInfo
    {
        public string name;
        public List<LayerInfo> layers;

        public UIInfo(string name)
        {
            this.name = name;
            layers = new List<LayerInfo>();
        }
    }
}