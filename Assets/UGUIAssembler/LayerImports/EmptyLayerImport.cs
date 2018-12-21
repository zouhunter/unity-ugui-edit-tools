using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UGUIAssembler
{
    public class EmptyLayerImport : ILayerImport
    {
        public AssemblerStateMechine mechine { get; set; }

        public UINode DrawLayer(LayerInfo layer)
        {
            return mechine.CreateRootNode(layer);
        }
    }
}