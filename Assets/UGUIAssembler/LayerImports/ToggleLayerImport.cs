using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UGUIAssembler
{
    public class ToggleLayerImport : ILayerImport, IParamsSetter
    {
        public AssemblerStateMechine mechine { get; set; }
        public const string titleKey = "label";
        public const string targetGraphicKey = "targetGraphic";
        public const string graphicKey = "graphic";

        public UINode DrawLayer(LayerInfo layer)
        {
            UINode node = mechine.CreateRootNode(layer);
            CompleteToggle<Image>(node.transform.gameObject);
            return node;
        }

        public void SetUIParams(UINode node)
        {
            var toggle = node.GetComponent<Toggle>();

            if (node.layerInfo.resourceDic != null)
            {
                mechine.ChargeInfo(toggle, node.layerInfo.resourceDic);
            }

            var update_targetGraphicRect = false;
            var update_graphicRect = false;

            ///加载子项资源
            if (node.layerInfo.subResourceDic != null)
            {
                using (var enumerator = node.layerInfo.subResourceDic.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if(current.Key == titleKey)
                        {
                            var title = LayerImportUtil.MakeTitleComponenet("Title", toggle.transform);
                            title.rectTransform.SetRectFromResource(node.layerInfo.rect, current.Value);
                            mechine.ChargeInfo(title, current.Value);
                        }
                        else if(current.Key == targetGraphicKey)
                        {
                            toggle.targetGraphic.rectTransform.SetRectFromResource(node.layerInfo.rect, current.Value);
                            update_targetGraphicRect = true;
                            mechine.ChargeInfo(toggle.targetGraphic, current.Value);
                        }
                        else if(current.Key == graphicKey)
                        {
                            toggle.graphic.rectTransform.SetRectFromResource(node.layerInfo.rect, current.Value);
                            update_graphicRect = true;
                            mechine.ChargeInfo(toggle.graphic, current.Value);
                        }
                    }
                }
            }

            if(!update_targetGraphicRect)
            {
                toggle.targetGraphic.rectTransform.SetRectFromResource(node.layerInfo.rect);
            }

            if (!update_graphicRect)
            {
                toggle.graphic.rectTransform.SetRectFromResource(node.layerInfo.rect);
            }
        }

        public Toggle CompleteToggle<T>(GameObject go) where T : Graphic
        {
            var toggle = go.AddComponent<Toggle>();

            var background = new GameObject("Background", typeof(RectTransform), typeof(T)).GetComponent<T>();
            var mask = new GameObject("Mask", typeof(RectTransform), typeof(T)).GetComponent<T>();

            mask.transform.SetParent(background.transform, false);
            background.transform.SetParent(toggle.transform, false);

            toggle.targetGraphic = background;
            toggle.graphic = mask;

            return toggle;
        }
    }
}