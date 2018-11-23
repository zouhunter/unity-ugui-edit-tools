using UnityEngine;
using System.Collections.Generic;
using System;
public class PrefabBindingObj : ScriptableObject {

    public enum NodeType
    {
        Single, Object, Vector3
    }
    [System.Serializable]
    public class ValueNode
    {
        public string valueName;
        public NodeType type;
        public string singleValue;
        public Vector3 vector3Value;
        public int index;
    }
    [System.Serializable]
    public class ScriptItem
    {
        public string name;
        public int index;
        public string scriptName;
        public string assemble;
        public List<ValueNode> nodes;
    }

    public List<ScriptItem> bindingItems = new List<ScriptItem>();
}
