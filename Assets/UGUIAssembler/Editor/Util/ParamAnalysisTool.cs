using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler
{
    public class ParamAnalysisTool
    {
        public const char seperator = '·';

        public static Vector3 ToVector3(string param)
        {
            if (string.IsNullOrEmpty(param)) return Vector3.zero;

            string[] split = param.Split(seperator);
            var value = Vector3.zero;
            for (int i = 0; i < split.Length; i++)
            {
                if (i < 3)
                {
                    float fvalue = 0;
                    if (float.TryParse(split[i], out fvalue))
                    {
                        value[i] = fvalue;
                    }
                }
            }
            return value;
        }

        public static string Vector3ToString(Vector3 value)
        {
            var array = new string[3];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("0.00");
            }
            return string.Join(seperator.ToString(), array);
        }

        public static string IntToString(int value)
        {
            return value.ToString();
        }

        public static int ToInit(string text)
        {
            var intValue = 0;
            int.TryParse(text, out intValue);
            return intValue;
        }
    }
}
