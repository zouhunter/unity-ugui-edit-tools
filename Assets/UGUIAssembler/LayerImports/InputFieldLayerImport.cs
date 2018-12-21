using System;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler
{
    public class InputFieldLayerImport : ILayerImport, IParamsSetter
    {
        public AssemblerStateMechine mechine { get; set; }
        public const string key_textComponent = "textComponent";
        public const string key_placeholder = "placeholder";
        public UINode DrawLayer(LayerInfo layer)
        {
            var node = mechine.CreateRootNode(layer);
            var image = node.AddComponent<Image>();
            var inputField = node.AddComponent<InputField>();
            inputField.targetGraphic = image;
            CompleteInputField(inputField);
            return node;
        }

        public void SetUIParams(UINode node)
        {
            var inputField = node.GetComponent<InputField>();
            
            ResourceDic textComponentResourceDic = null;
            ResourceDic placeHolderResourceDic = null;

            ///加载子项资源
            if (node.layerInfo.subResourceDic != null)
            {
                using (var enumerator = node.layerInfo.subResourceDic.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current.Key == key_placeholder)
                        {
                            mechine.ChargeInfo(inputField.placeholder, current.Value);
                            placeHolderResourceDic = current.Value;
                        }
                        else if (current.Key == key_textComponent) {
                            mechine.ChargeInfo(inputField.textComponent, current.Value);
                            textComponentResourceDic = current.Value;
                        }
                        else
                        {
                            mechine.ChargeSubInfo(inputField,current.Key, current.Value);
                        }
                    }
                }
            }

            //加载属性信息
            if (node.layerInfo.resourceDic != null)
            {
                mechine.ChargeInfo(inputField, node.layerInfo.resourceDic);
            }

            inputField.placeholder.rectTransform.SetRectFromResource(node.layerInfo.rect, placeHolderResourceDic);
            inputField.textComponent.rectTransform.SetRectFromResource(node.layerInfo.rect, textComponentResourceDic);
        }


        private void CompleteInputField(InputField inputfield)
        {
            var holder = new GameObject("Placeholder", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            var text = new GameObject("Text", typeof(Text)).GetComponent<Text>();

            inputfield.targetGraphic = inputfield.GetComponent<Image>();

            holder.transform.SetParent(inputfield.transform, false);
            text.transform.SetParent(inputfield.transform, false);

            inputfield.GetComponent<InputField>().placeholder = holder;
            holder.alignment = TextAnchor.MiddleLeft;
            holder.supportRichText = false;
            holder.raycastTarget = false;
            if (PreferHelper.defultFont) holder.font = PreferHelper.defultFont;
            Color color;
            if (ColorUtility.TryParseHtmlString("#32323280", out color))
            {
                holder.color = color;
            }

            inputfield.GetComponent<InputField>().textComponent = text;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.raycastTarget = false;

            if (PreferHelper.defultFont) text.font = PreferHelper.defultFont;

            if (ColorUtility.TryParseHtmlString("#32323280", out color))
            {
                text.color = color;
            }
        }
    }
}