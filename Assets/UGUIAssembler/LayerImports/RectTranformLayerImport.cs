using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIAssembler
{
    public class RectTranformLayerImport : ILayerImport,IParamsSetter
    {
        public AssemblerStateMechine mechine { get; set; }

        public UINode DrawLayer(LayerInfo layer)
        {
            UINode node = mechine.CreateRootNode(layer);
            return node;
        }

        public void SetUIParams(UINode node)
        {
            if (node.layerInfo.resourceDic != null)
            {
                mechine.ChargeInfo(node.transform, node.layerInfo.resourceDic);
            }

            LayerImportUtil.LoadCommonResources(mechine, node);
        }
    }
}