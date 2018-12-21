using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UGUIAssembler.Config
{
    public class SingleField : FieldItem
    {
        [SerializeField] private InputField m_input;

        public override void SetMemberType(Type memberType)
        {
            base.SetMemberType(memberType);
            var placeHolder = m_input.placeholder as Text;
            if (memberType == typeof(int))
            {
                m_input.contentType = InputField.ContentType.IntegerNumber;
                placeHolder.text = "请输入整数...";
            }
            else if(memberType ==typeof(float))
            {
                m_input.contentType = InputField.ContentType.DecimalNumber;
                placeHolder.text = "请输入符点数...";
            }
            else
            {
                m_input.contentType = InputField.ContentType.Standard;
                placeHolder.text = "请输入字符串...";
            }
        }

        public override string GetStringValue()
        {
            return m_input.text;
        }
        public override void RegistOnValueChanged()
        {
            m_input.onValueChanged.AddListener((x) => { OnValueChanged(); });
        }

        public override void SetValue(string value)
        {
            m_input.text = value;
        }
    }
}