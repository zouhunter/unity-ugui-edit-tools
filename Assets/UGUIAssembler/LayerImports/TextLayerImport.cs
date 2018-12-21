using UnityEngine;

namespace UGUIAssembler
{
    public class TextLayerImport : ILayerImport, IParamsSetter
    {
        public AssemblerStateMechine mechine { get; set; }

        public UINode DrawLayer(LayerInfo layer)
        {
            var newRect = LayerImportUtil.AddSubRectFromResourceDic(layer.rect, layer.resourceDic);
            UINode node = mechine.CreateRootNode(layer,newRect);
            node.AddComponent<UnityEngine.UI.Text>();
            return node;
        }

        public void SetUIParams(UINode node)
        {
            var component = node.GetComponent<UnityEngine.UI.Text>();
            component.resizeTextForBestFit = true;//缩放时会看不到？
            if (PreferHelper.defultFont)
                component.font = PreferHelper.defultFont;

            mechine.ChargeInfo(component, node.layerInfo.resourceDic);

            var newRect = LayerImportUtil.AddSubRectFromResourceDic(node.layerInfo.rect, node.layerInfo.resourceDic);
            LayerImportUtil.LoadCommonResources(mechine, node, newRect);
        }
    }
}