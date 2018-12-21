using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler
{
    public class PreferInt
    {
        public string key { get; private set; }
        public int defultValue { get; private set; }
        public int _value = -1;
        public int Value
        {
            get
            {
                if (_value == -1)
                {
                    if (PlayerPrefs.HasKey(key))
                    {
                        _value = PlayerPrefs.GetInt(key);
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
                    PlayerPrefs.SetInt(key, value);
                    PlayerPrefs.Save();
                }
            }
        }
        public PreferInt(string key, int defultValue)
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