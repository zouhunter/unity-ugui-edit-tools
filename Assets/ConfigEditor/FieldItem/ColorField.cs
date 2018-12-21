using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UGUIAssembler.Config
{
    public class ColorField : FieldItem
    {
        [SerializeField] private InputField m_input;
        [SerializeField] private Image m_icon;
        [SerializeField] private Button m_choise;
        protected override void Awake()
        {
            base.Awake();
            m_choise.onClick.AddListener(OpenChoiseColor);
        }

   
        public override string GetStringValue()
        {
            return m_input.text;
        }
        public override void RegistOnValueChanged()
        {
            m_input.onValueChanged.AddListener((x) => { OnValueChanged(); });
        }

        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            m_icon.color = ParamAnalysisTool.StringToColor(m_input.text);
        }

        public override void SetValue(string value)
        {
            if (m_input.text != value)
            {
                m_input.text = value;
            }
        }
        private void OpenChoiseColor()
        {
      
            ColorPicker.instence.OpenSelectColor(m_icon.color, (color) =>
            {
                m_icon.color = color;
                m_input.text = ColorUtility.ToHtmlStringRGBA(m_icon.color);
            });
        }

    }
}