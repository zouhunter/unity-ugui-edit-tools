using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;

namespace UGUIAssembler.Config
{
    public class Control_Item : MonoBehaviour
    {
        [SerializeField] protected Button m_option;
        [SerializeField] protected Button m_add;
        [SerializeField] protected Toggle m_check;

        [SerializeField] protected Text m_title;
        [SerializeField] protected Text m_index;
        [SerializeField] protected PropertyItem propertyItemPrefab;
        [SerializeField] protected Transform propertyItemParent;
        [SerializeField] protected Button m_delete;

        protected ListItemCreater<PropertyItem> propertyItemCreater;
        protected ResourceDic propertyDic;
        protected ResourceDic propertyCatchedDic;
        protected List<string> keys;

        public string title { get { return m_title.text; } }
        public int dicHash { get; private set; }
        public int ID { get; private set; }
        public Action<Control_Item> onDelete { get; set; }
        public Action<Control_Item> onMainControl { get; set; }
        public Action<Control_Item> onGenInstenceControl { get; set; }
        public ILayerDefine layerDefine { get; private set; }
        private string[] options = new string[] { "复制", "粘贴", "清空", "恢复", "置为主控件", "独立主控件" };
        private bool[] optionState = new bool[6];
        private static Type typeTemp;
        private static ResourceDic propertyDicTemp = new ResourceDic();

        protected void Awake()
        {
            m_add.onClick.AddListener(AddOnePropertyItem);
            m_delete.onClick.AddListener(OnDeleteThis);
            m_option.onClick.AddListener(OnOptionClicked);
            m_check.onValueChanged.AddListener(OnSwitchActive);
            propertyCatchedDic = new ResourceDic();
            propertyItemCreater = new ListItemCreater<PropertyItem>(propertyItemParent, propertyItemPrefab);
        }

        private void OnSwitchActive(bool arg0)
        {
            if (propertyDic != null)
            {
                propertyDic.active = arg0;
                UpdateView();
            }
        }

        private void OnDeleteThis()
        {
            if (onDelete != null)
            {
                onDelete.Invoke(this);
            }
        }

        public void SetTitle(string title)
        {
            m_title.text = title;
        }

        public void SetIndex(int id)
        {
            this.ID = id;
            m_index.text = ((char)(65 + id)).ToString();
        }

        public void CopyToDic(ResourceDic target)
        {
            target.Clear();
            CopyDictionary(propertyDic, target);
        }

        public void SetPropertys(ResourceDic propertyDic, ILayerDefine layerDic)
        {
            this.layerDefine = layerDic;
            this.propertyDic = propertyDic;
            CopyDictionary(propertyDic, propertyCatchedDic);
            this.dicHash = propertyDic.GetHashCode();
            UpdateView();
        }

        private void UpdateView()
        {
            keys = new List<string>(propertyDic.Keys);
            var created = propertyItemCreater.CreateItems(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (layerDefine != null)
                {
                    var ctrlItem = created[i];
                    ChargeItem(ctrlItem, key, propertyDic[key]);
                }
            }
            m_check.isOn = propertyDic.active;
        }

        private void ChargeItem(PropertyItem ctrlItem, string key, string value)
        {
            ctrlItem.onKeyChanged = OnKeyChanged;
            ctrlItem.onDelete = OnDeleteItem;
            ctrlItem.onValueChanged = OnValueChanged;
            ctrlItem.SetLayerDefine(layerDefine);
            ctrlItem.UpdateInfo(key, value);
        }

        private void OnDeleteItem(PropertyItem item)
        {
            propertyItemCreater.RemoveItem(item);
            if (!string.IsNullOrEmpty(item.key))
            {
                propertyDic.Remove(item.key);
            }
        }

        private void OnValueChanged(PropertyItem arg1, string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                propertyDic[key] = value;

                if (!string.IsNullOrEmpty(value))
                {
                    propertyCatchedDic[key] = value;
                }
            }
        }

        private void OnKeyChanged(PropertyItem item, string lastKey, string key)
        {
            if (!string.IsNullOrEmpty(lastKey) && propertyDic.ContainsKey(lastKey))
            {
                if (!string.IsNullOrEmpty(propertyDic[lastKey]))
                {
                    propertyCatchedDic[lastKey] = propertyDic[lastKey];
                }
                propertyDic.Remove(lastKey);
            }

            if (!string.IsNullOrEmpty(key))
            {
                if (propertyDic.ContainsKey(key))//不支持重复
                {
                    OnDeleteItem(item);
                }
                else
                {
                    if (propertyCatchedDic.ContainsKey(key))//从缓存加载
                    {
                        propertyDic[key] = propertyCatchedDic[key];
                    }
                    else
                    {
                        propertyDic.Add(key, "");
                    }

                    item.UpdateInfo(key, propertyDic[key]);
                }
                SetAcitveKey(propertyDic,key);
            }
        }

        protected void AddOnePropertyItem()
        {
            var item = propertyItemCreater.AddItem();
            ChargeItem(item, "", "");
        }

        public void SetDeleteable(bool deleteable)
        {
            m_delete.gameObject.SetActive(deleteable);
        }

        private void OnOptionClicked()
        {
            optionState[0] = true;
            optionState[1] = propertyDicTemp != null && propertyDicTemp.Count > 0;
            optionState[2] = propertyDic != null && propertyDic.Count > 0;
            optionState[3] = propertyCatchedDic != null && propertyCatchedDic.Count > 0;
            optionState[4] = ID != 0;
            optionState[5] = ID != 0;

            PopOption.Instence.ShowPop(options, optionState, (index) =>
             {
                 if (index == 0)
                 {
                     typeTemp = layerDefine.type;
                     propertyDicTemp.Clear();
                     CopyDictionary(propertyDic, propertyDicTemp);
                 }
                 else if (index == 1)
                 {
                     if (typeTemp != layerDefine.type)
                     {
                         var @continue = DialogHelper.ShowDialog("提示", "类型不一致，是否强制粘贴？", "粘贴", "取消");
                         if (!@continue) return;
                     }

                     CopyDictionary(propertyDicTemp, propertyDic);
                     UpdateView();
                 }
                 else if (index == 2)
                 {
                     propertyDic.Clear();
                     UpdateView();
                 }
                 else if (index == 3)
                 {
                     CopyDictionary(propertyCatchedDic, propertyDic);
                     UpdateView();
                 }
                 else if (index == 4)
                 {
                     if (onMainControl != null)
                         onMainControl.Invoke(this);
                 }
                 else if (index == 5)
                 {
                     if (onGenInstenceControl != null)
                         onGenInstenceControl.Invoke(this);
                 }
             });
        }

        public void CopyFromDic(ResourceDic tempDic)
        {
            propertyDic.Clear();
            CopyDictionary(tempDic, propertyDic);
            UpdateView();
        }
        private void CopyDictionary(ResourceDic source, ResourceDic target)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    target[enumerator.Current.Key] = enumerator.Current.Value;
                    SetAcitveKey(target,enumerator.Current.Key);
                }
            }
        }

        public void SetAcitveKey(ResourceDic dic,string key)
        {
            if (key == "rect")
            {
                dic.Remove("padding");
            }
            else if (key == "padding")
            {
                dic.Remove("rect");
            }
        }

    }
}