using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UGUIAssembler.Config
{
    public class SpriteField : FieldItem
    {
        [SerializeField] private InputField m_input;
        [SerializeField] private Button m_select;
        protected override void Awake()
        {
            base.Awake();
            m_select.onClick.AddListener(OpenSelectPicture);
        }

        private void OpenSelectPicture()
        {
            var folder = PreferHelper.defultSpriteFolder;
            var file = DialogHelper.OpenPictureFileDialog("选择图片", folder);
            if (!string.IsNullOrEmpty(file))
            {
                var fullFolder = System.IO.Path.GetFullPath(folder);
                var r_filePath = file.Replace(fullFolder + "\\", "");
                m_input.text = r_filePath;
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