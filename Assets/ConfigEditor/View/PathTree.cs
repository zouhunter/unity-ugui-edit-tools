using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeView;
using UGUIAssembler;
using System;

namespace UGUIAssembler.Config
{
    public class LayerData : ITreeIMGUIData
    {
        public bool isExpanded { get; set; }
        public string name;//名称
        public Transform transform;//

        public LayerData(string name, Transform transform, bool isExpanded)
        {
            this.name = name;
            this.isExpanded = isExpanded;
            this.transform = transform;
        }
    }

    public class LayerTree
    {
        public TreeNode<LayerData> root;
        public LayerTree()
        {
            var rootData = new LayerData("【运行时层级结构】", null, true);
            this.root = new TreeNode<LayerData>(rootData);
        }

        public void SetTransform(Transform transform)
        {
            root.Clear();
            var contentNode = root.AddChild(new LayerData(transform.name, transform, true));
            AnalysisTree(transform, contentNode);
        }

        private void AnalysisTree(Transform root, TreeNode<LayerData> rootNode)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                var childNode = rootNode.AddChild(new LayerData(child.name, child, true));
                AnalysisTree(child, childNode);
            }
        }
    }

    public class LayerTreeNodeGUI : TreeIMGUI<LayerData>
    {
        public Action<LayerData> onSelect { get; set; }
        public LayerTreeNodeGUI(TreeNode<LayerData> root) : base(root) { }

        protected override float GetRowHeight(TreeNode<LayerData> node)
        {
            return base.GetRowHeight(node);
        }
        protected override void OnSelect(TreeNode<LayerData> node)
        {
            base.OnSelect(node);
            if (onSelect != null)
            {
                onSelect.Invoke(node.Data);
            }
        }
        protected override void OnDrawTreeNode(Rect rect, TreeNode<LayerData> node, bool selected, bool focus)
        {
            Texture icon = null;
            GUIContent labelContent = new GUIContent(node.Data.name, icon);
            
            node.Data.isExpanded = GUI.Toggle(new Rect(rect.x - 12, rect.y, 12, rect.height), node.Data.isExpanded, GUIContent.none);
            if(node.Data.transform != null && node.Data.transform.gameObject.activeSelf != node.Data.isExpanded)
            {
                node.Data.transform.gameObject.SetActive(node.Data.isExpanded);
            }

            GUI.Label(rect, labelContent);
        }
    }
}