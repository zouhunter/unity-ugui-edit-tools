using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UGUIAssembler
{
    public class ButtonLayerImport : ILayerImport, IParamsSetter
    {
        public AssemblerStateMechine mechine { get; set; }
        public const string runtime_key_text = "label";
        public const string runtime_key_image = "image";

        public UINode DrawLayer(LayerInfo layer)
        {
            var node = mechine.CreateRootNode(layer);
            return node;
        }
        
        public void SetUIParams(UINode node)
        {
            var buttonTrans = node.transform;
            ///加载子项资源
            if(node.layerInfo.subResourceDic != null)
            {
                using (var enumerator = node.layerInfo.subResourceDic.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current.Key == runtime_key_text)//文字按扭
                        {
                            var text = buttonTrans.gameObject.GetComponent<Text>();
                            if (text == null && buttonTrans.gameObject.GetComponent<Graphic>()==null)
                            {
                                text = buttonTrans.gameObject.AddComponent<Text>();
                                text.alignment = TextAnchor.MiddleCenter;
                                text.font = PreferHelper.defultFont;
                                var btn = node.MustComponent<Button>();
                                btn.targetGraphic = text;
                                mechine.ChargeInfo(text, current.Value);
                            }
                        }
                        else if(current.Key == runtime_key_image && buttonTrans.gameObject.GetComponent<Graphic>() == null)//图片按扭
                        {
                            var image = buttonTrans.gameObject.GetComponent<Image>();
                            if (image == null)
                            {
                                image = buttonTrans.gameObject.AddComponent<Image>();
                                var btn = node.MustComponent<Button>();
                                btn.targetGraphic = image;
                                mechine.ChargeInfo(image, current.Value);
                            }
                        }
                        else
                        {
                            var key = current.Key;
                            mechine.ChargeSubInfo(buttonTrans, key, current.Value);
                        }
                    }
                }
            }
            var button = node.MustComponent<Button>();

            ///加载Button属性
            if (node.layerInfo.resourceDic != null)
            {
                mechine.ChargeInfo(button, node.layerInfo.resourceDic);
            }

            ///将有图片的Button显示方式设置为SpriteSwap (优先)
            if (button.spriteState.highlightedSprite || button.spriteState.pressedSprite || button.spriteState.disabledSprite)
            {
                button.transition = Selectable.Transition.SpriteSwap;
            }

            ///加载公式资源
            LayerImportUtil.LoadCommonResources(mechine, node);
        }
    }
}