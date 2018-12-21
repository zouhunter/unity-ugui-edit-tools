using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public class RectField : FieldItem
    {
        [SerializeField] private InputField input_x;
        [SerializeField] private InputField input_y;
        [SerializeField] private InputField input_w;
        [SerializeField] private InputField input_h;

        public override string GetStringValue()
        {
            return ParamAnalysisTool.ArrayToRange(input_x.text, input_y.text, input_w.text, input_h.text);
        }

        public override void RegistOnValueChanged()
        {
            input_x.onEndEdit.AddListener((x) => { OnValueChanged(); });
            input_y.onEndEdit.AddListener((x) => { OnValueChanged(); });
            input_w.onEndEdit.AddListener((x) => { OnValueChanged(); });
            input_h.onEndEdit.AddListener((x) => { OnValueChanged(); });
        }

        public override void SetValue(string value)
        {
            var rect = ParamAnalysisTool.StringToRect(value);
            input_x.text = rect.x.ToString();
            input_y.text = rect.y.ToString();
            input_w.text = rect.width.ToString();
            input_h.text = rect.height.ToString();
        }
    }
}