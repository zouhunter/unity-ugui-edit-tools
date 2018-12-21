using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler.Config
{
    public abstract class FieldItem : MonoBehaviour
    {
        protected Type memberType;
        public Action<string> onValueChanged { get; set; }
        protected virtual void Awake()
        {
            RegistOnValueChanged();
        }
        public abstract string GetStringValue();
        public abstract void SetValue(string value);
        public abstract void RegistOnValueChanged();
        public virtual void SetMemberType(Type memberType)
        {
            this.memberType = memberType;
        }
        protected virtual void OnValueChanged()
        {
            if (onValueChanged != null)
                onValueChanged.Invoke(GetStringValue());
        }
    }
}