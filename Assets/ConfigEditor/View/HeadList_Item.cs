using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public class HeadList_Item : MonoBehaviour
    {
        [SerializeField] private Text m_name;
        [SerializeField] private Toggle m_select;
        [SerializeField] private Button m_inset;
        [SerializeField] private InputField m_path;

        public int ID { get; private set; }
        public Action<int,bool> onSelect { get; set; }
        public Action<int> onInsert { get; set; }
        public Action<int, string> onPathChanged { get; set; }
        public string layerName { get; private set; }
        private bool settingPath;
        private void Awake()
        {
            m_select.onValueChanged.AddListener(OnSelect);
            m_path.onValueChanged.AddListener(OnPathChanged);
            m_inset.onClick.AddListener(OnInsetItem);
        }

        public void SetSelect(bool active,bool trigger)
        {
            if (!trigger) m_select.onValueChanged.RemoveListener(OnSelect);

            if(m_select.isOn != active)
            {
                m_select.isOn = active;
            }
            else{
                OnSelect(active);
            }

            if (!trigger) m_select.onValueChanged.AddListener(OnSelect);
        }

        private void OnInsetItem()
        {
            if(onInsert != null)
            {
                onInsert.Invoke(ID);
            }
        }

        private void OnSelect(bool isOn)
        {
            if (onSelect != null)
            {
                onSelect.Invoke(ID, isOn);
            }
        }

        public void SetIndex(int index)
        {
            this.ID = index;
            UpdateView();
        }
        public void UpdateName(string name)
        {
            Debug.Log("UpdateName:" + name);
            this.layerName = name;
            UpdateView();
        }
        public void SetInformation(string[] infos)
        {
            this.layerName = infos[0];
            settingPath = true;
            m_path.text = infos[1];
            settingPath = false;
            UpdateView();
        }

        private void OnPathChanged(string arg0)
        {
            if (settingPath) return;

            if (onPathChanged != null)
                onPathChanged.Invoke(ID,arg0);
        }

        private void UpdateView()
        {
            if (layerName != null)
            {
                this.m_name.text = string.Format("{0}.{1}", ID.ToString("00"), layerName);
            }
        }
    }
}