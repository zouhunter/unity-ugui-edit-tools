using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIAssembler
{
    public class ImageLayerImport : ILayerImport, IParamsSetter
    {
        public AssemblerStateMechine mechine { get; set; }

        public UINode DrawLayer(LayerInfo layer)
        {
            var newRect = LayerImportUtil.AddSubRectFromResourceDic(layer.rect, layer.resourceDic);

            UINode node = mechine.CreateRootNode(layer,newRect);
            node.AddComponent<UnityEngine.UI.Image>();
            return node;
        }

        public void SetUIParams(UINode node)
        {
            var pic = node.GetComponent<UnityEngine.UI.Image>();
            mechine.ChargeInfo(pic, node.layerInfo.resourceDic);

            var newRect = LayerImportUtil.AddSubRectFromResourceDic(node.layerInfo.rect, node.layerInfo.resourceDic);
            LayerImportUtil.LoadCommonResources(mechine, node, newRect);
        }
    }
}