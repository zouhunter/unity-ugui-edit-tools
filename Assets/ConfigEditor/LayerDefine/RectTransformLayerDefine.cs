using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public class RectTransformLayerDefine : ILayerDefine
    {
        public virtual Type type
        {
            get
            {
                return typeof(RectTransform);
            }
        }
        public ICollection<string> supportMembers { get {return memberTypeDic.Keys; } }
        public List<string> integrantSubControls { get; protected set; }
        public List<string> runtimeSubControls { get; protected set; }
        protected Dictionary<string, Type> memberTypeDic;
        public Type GetMemberType(string key)
        {
            if (!string.IsNullOrEmpty(key) && memberTypeDic != null&& memberTypeDic.ContainsKey(key))
            {
                return memberTypeDic[key];
            }
            else
            {
                return TypeUtil.GetSubTypeDeepth(type, key);
            }
        }
        public virtual string GetSubControlType(string key) { return null; }
        public virtual string GetSubControlName(string key) { return key; }
        public RectTransformLayerDefine()
        {
            memberTypeDic = LayerImportUtil.GetSupportedMembers(type);
        }
    }
}