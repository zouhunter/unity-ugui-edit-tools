using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace UGUIAssembler
{
    public class SliderLayerImport : ILayerImport,IParamsSetter
    {
        public AssemblerStateMechine mechine { get; set; }
        public const string runtime_key_background = "background";
        public const string runtime_key_fill = "fill";
        public const string runtime_key_handle = "handle";
        public const string params_size = "size";

        public UINode DrawLayer(LayerInfo layer)
        {
            var node = mechine.CreateRootNode(layer);
            var slider = node.AddComponent<Slider>();
            slider.value = 1;

            var background = new GameObject("Background", typeof(Image));
            background.transform.SetParent(slider.transform, false);
            slider.targetGraphic = background.GetComponent<Image>();
            node.RecordSubTransfrom(runtime_key_background, background.GetComponent<RectTransform>());

            var fillRect = new GameObject("Fill Area", typeof(RectTransform));
            var fill = new GameObject("Fill", typeof(Image));
            fillRect.transform.SetParent(slider.transform, false);
            fill.transform.SetParent(fillRect.transform, false);
            slider.fillRect = fill.GetComponent<RectTransform>();
            node.RecordSubTransfrom(runtime_key_fill, fillRect.GetComponent<RectTransform>());
            return node;
        }

        public void SetUIParams(UINode node)
        {
            var slider = node.GetComponent<Slider>();
            var backgroundImage = node.GetSubComponent<Image>(runtime_key_background);
            ///加载Slider属性
            if (node.layerInfo.resourceDic != null){
                mechine.ChargeInfo(slider, node.layerInfo.resourceDic);
            }

            var fillRect = node.GetSubComponent<RectTransform>(runtime_key_fill);
            ResourceDic fillResourceDic = null;

            ///加载子项资源
            if (node.layerInfo.subResourceDic != null)
            {
                using (var enumerator = node.layerInfo.subResourceDic.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current.Key == runtime_key_background)//动态背景
                        {
                            backgroundImage.rectTransform.SetRectFromResource(node.layerInfo.rect, current.Value);
                            mechine.ChargeInfo(backgroundImage, current.Value);
                        }
                        else if(current.Key == runtime_key_fill)
                        {
                            var key = current.Key;
                            mechine.ChargeInfo(slider.fillRect.GetComponent<Image>(), current.Value);

                            fillResourceDic = current.Value;
                        }
                        else if(current.Key == runtime_key_handle)
                        {
                            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
                            handleArea.transform.SetParent(slider.transform, false);

                            var handle = new GameObject("Handle", typeof(Image)).GetComponent<RectTransform>();
                            handle.transform.SetParent(handleArea.transform, false);

                            slider.handleRect = handle.GetComponent<RectTransform>();
                            mechine.ChargeInfo(slider.handleRect.GetComponent<Image>(), current.Value);

                            Vector2 handleSize = new Vector2(node.layerInfo.rect.height, node.layerInfo.rect.height);
                            LayerImportUtil.UpdateSizeFromResourceDic(current.Value, ref handleSize);

                            if (slider.direction == Slider.Direction.BottomToTop || slider.direction == Slider.Direction.TopToBottom)
                            {
                                var handleRectX = (node.layerInfo.rect.width - handleSize.x) * 0.5f;
                                var handleAreaRect = new Rect(handleRectX, handleSize.y * 0.5f, handleSize.x, node.layerInfo.rect.height - handleSize.y);
                                handleAreaRect = ParamAnalysisTool.PsRectToUnityRect(node.layerInfo.rect.size, handleAreaRect);
                                LayerImportUtil.SetRectTransform(handleAreaRect, handleArea);
                                LayerImportUtil.SetCustomAnchor(node.layerInfo.rect.size, handleArea);

                                handle.anchorMin = Vector2.zero;
                                handle.anchorMax = new Vector2(1, 0);
                                handle.sizeDelta = new Vector2(0, handleSize.y);
                            }
                            else
                            {
                                var handleRectY = (node.layerInfo.rect.height - handleSize.y) * 0.5f;
                                var handleAreaRect = new Rect(handleSize.x * 0.5f, handleRectY, node.layerInfo.rect.width - handleSize.x, handleSize.y);
                                handleAreaRect = ParamAnalysisTool.PsRectToUnityRect(node.layerInfo.rect.size, handleAreaRect);
                                LayerImportUtil.SetRectTransform(handleAreaRect, handleArea);
                                LayerImportUtil.SetCustomAnchor(node.layerInfo.rect.size, handleArea);

                                handle.anchorMin = Vector2.zero;
                                handle.anchorMax = new Vector2(0, 1);
                                handle.sizeDelta = new Vector2(handleSize.x, 0);
                            }
                        }
                        else
                        {
                            mechine.ChargeSubInfo(slider, current.Key, current.Value);
                        }
                    }
                }
            }

            fillRect.SetRectFromResource(node.layerInfo.rect, fillResourceDic);
            slider.fillRect.anchorMin = Vector2.zero;
            slider.fillRect.anchorMax = Vector2.one;
            slider.fillRect.sizeDelta = Vector2.zero;

            LayerImportUtil.LoadCommonResources(mechine, node);
            slider.value = slider.minValue;
        }
    }
}