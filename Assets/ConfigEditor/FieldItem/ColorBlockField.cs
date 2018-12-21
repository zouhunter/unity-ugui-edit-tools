using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UGUIAssembler.Config
{
    public class ColorBlockField : FieldItem
    {
        [SerializeField] private Image m_normal_icon;
        [SerializeField] private Button m_normal_btn;

        [SerializeField] private Image m_highlight_icon;
        [SerializeField] private Button m_highlight_btn;

        [SerializeField] private Image m_pressed_icon;
        [SerializeField] private Button m_pressed_btn;

        [SerializeField] private Image m_disabled_icon;
        [SerializeField] private Button m_disabled_btn;
        private UnityEngine.Events.UnityEvent onChanged = new UnityEngine.Events.UnityEvent();
        protected override void Awake()
        {
            base.Awake();
            m_normal_btn.onClick.AddListener(()=>OpenChoiseColor(m_normal_icon));
            m_highlight_btn.onClick.AddListener(()=>OpenChoiseColor(m_highlight_icon));
            m_pressed_btn.onClick.AddListener(()=>OpenChoiseColor(m_pressed_icon));
            m_disabled_btn.onClick.AddListener(()=>OpenChoiseColor(m_disabled_icon));
        }
   
        public override string GetStringValue()
        {
            var normalColor = ColorUtility.ToHtmlStringRGBA(m_normal_icon.color);
            var highlightedColor = ColorUtility.ToHtmlStringRGBA(m_highlight_icon.color);
            var pressedColor = ColorUtility.ToHtmlStringRGBA(m_pressed_icon.color);
            var disabledColor = ColorUtility.ToHtmlStringRGBA(m_disabled_icon.color);
            var group = ParamAnalysisTool.ArrayToGroup(normalColor, highlightedColor, pressedColor, disabledColor);
            Debug.Log(group);
            return group;
        }
        public override void RegistOnValueChanged()
        {
            onChanged.AddListener(OnValueChanged);
        }
        public override void SetValue(string value)
        {
            var colorblock = ParamAnalysisTool.StringToColorBlock(value);
            m_normal_icon.color = colorblock.normalColor;
            m_highlight_icon.color = colorblock.highlightedColor;
            m_pressed_icon.color = colorblock.pressedColor;
            m_disabled_icon.color = colorblock.disabledColor;
        }
        private void OpenChoiseColor(Image icon)
        {
            ColorPicker.instence.OpenSelectColor(icon.color, (color) =>
            {
                icon.color = color;
                onChanged.Invoke();
            });
        }

    }
}