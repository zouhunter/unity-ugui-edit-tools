using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler
{
    public class PreferString
    {
        public string key { get; private set; }
        public string defultValue { get; private set; }
        public string _value;
        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(_value))
                {
                    if (PlayerPrefs.HasKey(key))
                    {
                        _value = PlayerPrefs.GetString(key);
                    }
                    else
                    {
                        _value = defultValue;
                    }
                }
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    PlayerPrefs.SetString(key, value);
                    PlayerPrefs.Save();
                }
            }
        }
        public PreferString(string key, string defultValue)
        {
            this.key = key;
            this.defultValue = defultValue;
        }

        public void ResetDefult()
        {
            Value = defultValue;
        }
    }
}