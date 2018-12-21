using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UGUIAssembler.Config
{
    public class EnumField : FieldItem
    {
        [SerializeField] private Dropdown m_dropDown;
        private string[] enumNames;
        public override void SetMemberType(Type memberType)
        {
            base.SetMemberType(memberType);
            enumNames = System.Enum.GetNames(memberType);
            m_dropDown.options.Clear();
            m_dropDown.AddOptions(new List<string>(enumNames));
        }
        public override void RegistOnValueChanged()
        {
            m_dropDown.onValueChanged.AddListener((x) => { OnValueChanged(); });
        }
        public override string GetStringValue()
        {
            if (enumNames == null) return null;
            if (m_dropDown.value >= 0 && enumNames.Length > m_dropDown.value)
            {
                return enumNames[m_dropDown.value];
            }
            return null;
        }

        public override void SetValue(string value)
        {
            var index = Array.IndexOf(enumNames, value);
            if(index >=0)
            {
                m_dropDown.value = index;
            }
        }
    }
}