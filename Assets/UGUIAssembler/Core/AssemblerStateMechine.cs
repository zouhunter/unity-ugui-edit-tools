using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler
{
    public class AssemblerStateMechine
    {
        private ILayerImport emptyImporter { get; set; }
        private Dictionary<string, ILayerImport> layerImportDic = new Dictionary<string, ILayerImport>();
        public Vector2 uiSize { get; set; }
        public string spriteFolderPath { get; set; }
        public string fontFolderPath { get; set; }
        public Canvas canvas { get; set; }
        internal string textureFolderPath { get; set; }
        internal string materialFolderPath { get; set; }

        public void RegistEmptyImprot(ILayerImport layerImport)
        {
            emptyImporter = layerImport;
            emptyImporter.mechine = this;
        }

        public void RegistLayerimport(string key, ILayerImport layerImport)
        {
            layerImport.mechine = this;
            layerImportDic[key] = layerImport;
        }

        public GameObject GenerateUI(UIInfo uiInfo)
        {
            if (layerImportDic == null)
            {
                Debug.Log("LayerImportDic 未初始化");
                return null;
            }

            if (uiInfo == null || uiInfo.layers == null)
                return null;

            var canvasNode = new UINode(canvas.transform);

            var rootTransform = new GameObject(uiInfo.name,typeof(RectTransform)).GetComponent<RectTransform>();
            rootTransform.SetParent(canvas.transform, false);
            rootTransform.sizeDelta = uiSize;
            var rootNode = new UINode(rootTransform);
            canvasNode.AddChildNode(rootNode);

            ClearEmptMakeEffectiveyLayers(uiInfo.layers);
            var uiNodes = DrawLayers(uiInfo.layers);//绘制所有层级
            MakeRelation(rootNode, uiNodes);//设置父子关系
            SetCustomResource(rootNode);//设置配制的资源参数
            SetAnchorDeepth(rootNode);//设置默认锚点
            DoAnchorCompleteAction(rootNode);//只有锚点设置好了才能进行的事
            return rootTransform.gameObject;
        }

        private void ClearEmptMakeEffectiveyLayers (List<LayerInfo> layers)
        {
            var template = layers.ToArray();
            var ptahKeys = new List<string>();
            for (int i = 0; i < template.Length; i++)
            {
                var layer = template[i];
                if (string.IsNullOrEmpty(layer.name) || string.IsNullOrEmpty(layer.path))
                {
                    Debug.LogWarning("移除无效层级 ：" + i);
                    layers.Remove(layer);
                }
                else
                {
                    if (ptahKeys.Contains(layer.path))
                    {
                        Debug.LogWarning("移除重复层级 ：" + i);
                        layers.Remove(layer);
                    }
                    else
                    {
                        ptahKeys.Add(layer.path);
                    }
                }

            }
        }


        private UINode[] DrawLayers(IList<LayerInfo> layers)
        {
            List<UINode> nodes = new List<UINode>();
            if (layers != null)
            {
                for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
                {
                    var layer = layers[layerIndex];
                    var node = DrawLayer(layer);
                    if (node != null)
                    {
                        nodes.Add(node);
                    }
                }
            }

            var emptyNodes = CompleteEmptyNodes(nodes.ToArray());

            if (emptyNodes != null)
            {
                nodes.AddRange(emptyNodes);
            }

            return nodes.ToArray();
        }

        private UINode[] CompleteEmptyNodes(UINode[] uiNodes)
        {
            var pathCatchDic = new Dictionary<string, string[]>();
            var nodeTemplateDic = new Dictionary<string, UINode>();
            var neededNodes = new Dictionary<string, List<UINode>>();
            var emptyNodes = new List<UINode>();
            var maxdeepth = 0;

            ///确定深度，记录索引
            for (int i = 0; i < uiNodes.Length; i++)
            {
                var node = uiNodes[i];

                if (string.IsNullOrEmpty(node.path))
                    continue;

                nodeTemplateDic.Add(node.path, node);
                var pathArray = LayerImportUtil.PathToArray(node.path);
                pathCatchDic.Add(node.path, pathArray);

                var deepth = pathArray.Length;
                maxdeepth = maxdeepth > deepth ? maxdeepth : deepth;
            }

            ///查找所有空节点的实体子节点
            for (int i = 0; i < uiNodes.Length; i++)
            {
                var node = uiNodes[i];

                if (string.IsNullOrEmpty(node.path))
                    continue;

                var path = pathCatchDic[node.path];

                for (int j = path.Length; j > 0; j--)
                {
                    var parentPath = LayerImportUtil.ArrayToPath(path, j - 1);
                    if (!string.IsNullOrEmpty(parentPath))
                    {
                        if (!nodeTemplateDic.ContainsKey(parentPath))
                        {
                            if (neededNodes.ContainsKey(parentPath))
                            {
                                if (!neededNodes[parentPath].Contains(node))
                                {
                                    neededNodes[parentPath].Add(node);
                                }
                            }
                            else
                            {
                                neededNodes.Add(parentPath, new List<UINode>() { node });
                            }
                        }
                    }
                }
            }

            ///创建节点
            using (var enumerator = neededNodes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    var rects = new Rect[current.Value.Count];
                    for (int i = 0; i < rects.Length; i++)
                    {
                        rects[i] = current.Value[i].layerInfo.rect;
                    }
                    var info = new LayerInfo();
                    info.path = current.Key;
                    info.rect = LayerImportUtil.GetRectContent(rects);
                    var emptyNode = DrawLayer(info);
                    emptyNodes.Add(emptyNode);
                }
            }

            return emptyNodes.ToArray();
        }

        public UINode DrawLayer(LayerInfo layer)
        {
            if (string.IsNullOrEmpty(layer.name) || string.IsNullOrEmpty(layer.type))
            {
                return emptyImporter.DrawLayer(layer);
            }
            else if (layerImportDic.ContainsKey(layer.type))
            {
                return layerImportDic[layer.type].DrawLayer(layer);
            }
            else
            {
                Debug.LogWarningFormat("未解析层{0},因为找不到类型为{1}的Importer", layer.name, layer.type);
                return null;
            }
        }

        private void MakeRelation(UINode rootNode, UINode[] uiNodes)
        {
            if (rootNode == null || uiNodes == null)
                return;
            var pathCatchDic = new Dictionary<string, string[]>();
            var nodeTemplateDic = new Dictionary<string, UINode>();
            var deepthDic = new Dictionary<int, List<UINode>>();
            var maxdeepth = 0;

            ///建立索引
            for (int i = 0; i < uiNodes.Length; i++)
            {
                var node = uiNodes[i];
                nodeTemplateDic.Add(node.path, node);
                var pathArray = LayerImportUtil.PathToArray(node.path);
                pathCatchDic.Add(node.path, pathArray);

                var deepth = pathArray.Length;

                if (deepthDic.ContainsKey(deepth))
                {
                    deepthDic[deepth].Add(node);
                }
                else
                {
                    deepthDic[deepth] = new List<UINode>() { node };
                }

                maxdeepth = maxdeepth > deepth ? maxdeepth : deepth;
            }

            ///关系对应
            for (int deepth = 1; deepth <= maxdeepth; deepth++)
            {
                var nodes = deepthDic[deepth];
                for (int k = 0; k < nodes.Count; k++)
                {
                    var node = nodes[k];
                    var path = pathCatchDic[node.path];

                    var parentPath = LayerImportUtil.ArrayToPath(path, deepth - 1);

                    if (string.IsNullOrEmpty(parentPath))
                    {
                        rootNode.AddChildNode(node);
                    }
                    else if (nodeTemplateDic.ContainsKey(parentPath))
                    {
                        var nodeTemp = nodeTemplateDic[parentPath];
                        nodeTemp.AddChildNode(node);
                    }
                    else
                    {
                        Debug.LogWarning("未找到层级：" + parentPath);
                    }
                }

            }
            //设置父级
            SetUIParentsDeepth(rootNode);
        }

        private void SortLayerDeepth(UINode node)
        {
            if (node.childNodes != null)
            {
                node.childNodes.Sort();
                for (int i = 0; i < node.childNodes.Count; i++)
                {
                    SortLayerDeepth(node.childNodes[i]);
                }
            }
        }

        private void SetUIParentsDeepth(UINode node)
        {
            if (node.childNodes != null)
            {
                foreach (var item in node.childNodes)
                {
                    item.transform.SetParent(node.transform);
                    item.transform.SetAsLastSibling();
                    SetUIParentsDeepth(item);
                }
            }
        }

        private void SetAnchorDeepth(UINode node)
        {
            if (node != null && node.childNodes != null)
            {
                for (int i = 0; i < node.childNodes.Count; i++)
                {
                    var childNode = node.childNodes[i];
                    SetAnchorDeepth(childNode);
                }
            }

            LayerImportUtil.SetAnchorByNode(node);
        }

        private void DoAnchorCompleteAction(UINode node)
        {
            if (node == null || node.childNodes == null) return;

            foreach (var item in node.childNodes)
            {
                DoAnchorCompleteAction(item);

                if(item.layerInfo != null &&!string.IsNullOrEmpty( item.layerInfo.type ))
                {
                    ILayerImport importer;
                    if(layerImportDic.TryGetValue(item.layerInfo.type,out importer) && importer is IFinalCheckUp){
                        (importer as IFinalCheckUp).FinalCheckUp(item);
                    }
                }
            }
        }

        private void SetCustomResource(UINode rootNode)
        {
            if (rootNode == null || rootNode.childNodes == null) return;

            foreach (var item in rootNode.childNodes)
            {
                if (item.layerInfo != null)
                {
                    if (!string.IsNullOrEmpty(item.layerInfo.type))
                    {
                        if (layerImportDic.ContainsKey(item.layerInfo.type))
                        {
                            var importer = layerImportDic[item.layerInfo.type];
                            if (importer is IParamsSetter)
                            {
                                if (item.layerInfo != null)
                                {
                                    (importer as IParamsSetter).SetUIParams(item);
                                }
                            }
                        }
                    }
                }
                SetCustomResource(item);
            }
        }
    }
}