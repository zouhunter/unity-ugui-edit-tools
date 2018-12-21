using System;
using UnityEngine;
using UnityEngine.UI;
namespace UGUIAssembler.Config
{
    public class SpriteStateField : FieldItem
    {
        [SerializeField] private InputField m_highlight_icon;
        [SerializeField] private Button m_highlight_btn;

        [SerializeField] private InputField m_pressed_icon;
        [SerializeField] private Button m_pressed_btn;

        [SerializeField] private InputField m_disabled_icon;
        [SerializeField] private Button m_disabled_btn;

        protected override void Awake()
        {
            base.Awake();
            m_highlight_btn.onClick.AddListener(() => OpenChoiseSprite(m_highlight_icon));
            m_pressed_btn.onClick.AddListener(() => OpenChoiseSprite(m_pressed_icon));
            m_disabled_btn.onClick.AddListener(() => OpenChoiseSprite(m_disabled_icon));
        }
        
        public override string GetStringValue()
        {
            var highlightedPath = m_highlight_icon.text;
            var pressedPath = m_pressed_icon.text;
            var disabledPath = m_disabled_icon.text;
            var group = ParamAnalysisTool.ArrayToGroup(highlightedPath, pressedPath, disabledPath);
            return group;
        }
        public override void RegistOnValueChanged()
        {
            m_highlight_icon.onEndEdit.AddListener((x)=> OnValueChanged());
            m_pressed_icon.onEndEdit.AddListener((x) => OnValueChanged());
            m_disabled_icon.onEndEdit.AddListener((x) => OnValueChanged());
        }
        public override void SetValue(string value)
        {
            var array = ParamAnalysisTool.GroupToArray(value);
            if(array != null)
            {
                if (array.Length > 0)
                {
                    m_highlight_icon.text = array[0];
                }
                if (array.Length > 1)
                {
                    m_pressed_icon.text = array[1];
                }
                if (array.Length > 2)
                {
                    m_disabled_icon.text = array[2];
                }
            }
        }

        private void OpenChoiseSprite(InputField inputField)
        {
            var folder = PreferHelper.defultSpriteFolder;
            var filePath = DialogHelper.OpenPictureFileDialog("选择图片", folder);
            if (!string.IsNullOrEmpty(filePath))
            {
                var fullFolder = System.IO.Path.GetFullPath(folder);
                var r_filePath = filePath.Replace(fullFolder + "\\", "");
                inputField.text = r_filePath;
                OnValueChanged();
            }
        }

    }
}