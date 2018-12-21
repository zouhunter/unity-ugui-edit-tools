using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIAssembler
{
    public class UINode:System.IComparable<UINode>
    {
        public string nameKey { get { if (layerInfo == null) return null; return layerInfo.name; } }
        public string path { get { if (layerInfo == null) return null; return layerInfo.path; } }
        public Transform transform { get; private set; }
        public LayerInfo layerInfo { get; set; }

        public UINode parentNode { get; set; }
        public List<UINode> childNodes { get; set; }
        public AnchoType anchoType { get; set; }

        private Dictionary<string, RectTransform> subTransforms;

        public UINode(Transform transform, LayerInfo layerInfo = null)
        {
            this.layerInfo = layerInfo;
            this.transform = transform;
            anchoType = AnchoType.Custom;
        }

        public void RecordSubTransfrom(string key, RectTransform transfrom)
        {
            if (string.IsNullOrEmpty(key) || transfrom == null)
                return;

            if (subTransforms == null)
                subTransforms = new Dictionary<string, RectTransform>();

            subTransforms[key] = transfrom;
        }

        public RectTransform GetSubTransfrom(string key)
        {
            if (subTransforms == null || string.IsNullOrEmpty(key)) return null;

            if (subTransforms.ContainsKey(key))
            {
                return subTransforms[key];
            }
            return null;
        }

        public T GetSubComponent<T>(string key) where T : Component
        {
            if (subTransforms == null || string.IsNullOrEmpty(key)) return null;
            if (subTransforms.ContainsKey(key) && subTransforms[key] != null)
            {
                return subTransforms[key].GetComponent<T>();
            }
            return null;
        }

        public T AddComponent<T>() where T : Component
        {
            var t = transform.gameObject.AddComponent<T>();
            return t;
        }

        public T GetComponent<T>() where T : Component
        {
            var t = transform.gameObject.GetComponent<T>();
            return t;
        }
        public T MustComponent<T>() where T : Component
        {
            var t = transform.gameObject.GetComponent<T>();
            if(t == null)
            {
                t = transform.gameObject.AddComponent<T>();
            }
            return t;
        }
        public void AddChildNode(UINode node)
        {
            if (childNodes == null)
                childNodes = new List<UINode>();

            if (!childNodes.Contains(node))
            {
                childNodes.Add(node);
                node.parentNode = this;
            }
        }

        public int CompareTo(UINode other)
        {
            var sizeOther = other.layerInfo.rect.size.magnitude;
            var sizeMe = layerInfo.rect.size.magnitude;
            if(sizeOther > sizeMe)
            {
                return 1;
            }
            else if(sizeOther == sizeMe)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}