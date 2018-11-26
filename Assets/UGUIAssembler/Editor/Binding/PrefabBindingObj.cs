using UnityEngine;
using System.Collections.Generic;
using System;
namespace UGUIAssembler
{
    public class PrefabBindingObj : ScriptableObject
    {
        public enum NodeType
        {
            single, multiple, quote
        }
        [System.Serializable]
        public class ValueNode
        {
            public string name;
            public NodeType type;
            public string text;
        }

        [System.Serializable]
        public class ScriptItem
        {
            public string name;
            public int index;
            public string path;
            public string scriptName;
            public string assemble;
            public List<ValueNode> nodes;
        }

        public List<ScriptItem> bindingItems = new List<ScriptItem>();
    }
}