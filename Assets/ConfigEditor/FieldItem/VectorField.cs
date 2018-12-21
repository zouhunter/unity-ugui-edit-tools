using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public class VectorField : FieldItem
    {
        [SerializeField] private InputField x_axis;
        [SerializeField] private InputField y_axis;
        [SerializeField] private InputField z_axis;
        [SerializeField] private InputField w_axis;

        private string[] fieldValues;
        private InputField[] fieldArray;
        protected override void Awake()
        {
            base.Awake();
            fieldArray = new InputField[] {
                x_axis,
                y_axis,
                z_axis,
                w_axis,
            };
        }

        public override void SetMemberType(Type memberType)
        {
            base.SetMemberType(memberType);
            if(memberType == typeof(Vector2))
            {
                SetFieldCount(2,InputField.ContentType.DecimalNumber);
            }
            else if(memberType == typeof(Vector2Int))
            {
                SetFieldCount(2, InputField.ContentType.IntegerNumber);
            }
            else if(memberType == typeof(Vector3))
            {
                SetFieldCount(3, InputField.ContentType.DecimalNumber);
            }
            else if(memberType == typeof(Vector3Int))
            {
                SetFieldCount(3, InputField.ContentType.IntegerNumber);
            }
            else if(memberType == typeof(Vector4))
            {
                SetFieldCount(4, InputField.ContentType.DecimalNumber);
            }
        }

        public override string GetStringValue()
        {
            return ParamAnalysisTool.ArrayToGroup(fieldValues);
        }
        public override void RegistOnValueChanged()
        {
            x_axis.onValueChanged.AddListener((x) => OnInputFieldChanged(0,x));
            y_axis.onValueChanged.AddListener((x) => OnInputFieldChanged(1,x));
            z_axis.onValueChanged.AddListener((x) => OnInputFieldChanged(2,x));
            w_axis.onValueChanged.AddListener((x) => OnInputFieldChanged(3,x));
        }

        public override void SetValue(string value)
        {
            var values = ParamAnalysisTool.GroupToArray(value);
            for (int i = 0; i < values.Length; i++)
            {
                if(fieldValues.Length > i)
                {
                    fieldValues[i] = values[i];
                }
            }
        }

        private void OnInputFieldChanged(int index, string value)
        {
            fieldValues[index] = value;
            OnValueChanged();
        }

        private void SetFieldCount(int count,InputField.ContentType contentType)
        {
            fieldValues = new string[count];
            for (int i = 0; i < fieldArray.Length; i++)
            {
                var fieldItem = fieldArray[i];
                if (i < count)
                {
                    fieldItem.gameObject.SetActive(true);
                    fieldItem.contentType = contentType;
                }
                else
                {
                    fieldItem.gameObject.SetActive(false);
                }
            }
        }
    }
}