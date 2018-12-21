using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public class BoolField : FieldItem
    {
        [SerializeField] private Toggle m_Check;
        [SerializeField] private Text m_label;
        public override string GetStringValue()
        {
            return m_Check.isOn.ToString();
        }

        public override void RegistOnValueChanged()
        {
            m_Check.onValueChanged.AddListener((x) => {
                OnValueChanged();
            });
        }
        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            m_label.text = m_Check.isOn.ToString();
        }

        public override void SetValue(string value)
        {
            if(!string.IsNullOrEmpty(value) && value.ToLower() ==  "true")
            {
                m_Check.isOn = true;
            }
            else
            {
                m_Check.isOn = false;
            }

            m_label.text = m_Check.isOn.ToString();
        }
    }
}