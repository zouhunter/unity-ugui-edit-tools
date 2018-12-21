using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAssembler.Config
{
    public class PropertyItem : MonoBehaviour
    {
        [SerializeField] private Dropdown title_dropDown;
        [SerializeField] private InputField title_inputField;
        [SerializeField] private Button m_seek;
        [SerializeField] private Button m_delete;
        [SerializeField] private Transform parentTransform;

        private FieldItem activeItem;
        private List<string> supportKeys;
        public Action<PropertyItem, string,string> onKeyChanged { get; set; }
        public Action<PropertyItem, string,string> onValueChanged { get; set; }
        public Action<PropertyItem> onDelete { get; set; }
        public string key
        {
            get
            {
                return title_inputField.text;
            }
            private set {
                title_inputField.text = value;
            }
        }
        private string lastKey;
        private string value { get; set; }
        private ILayerDefine layerDic;
        private GameObjectPool gameObjectPool {
            get
            {
                return GameObjectPool.instence;
            }
        }
        private bool notTriggerKey;

        private void Awake()
        {
            title_dropDown.onValueChanged.AddListener(OnKeyIndexChanged);
            title_inputField.onEndEdit.AddListener(OnKeyChanged);
            m_seek.onClick.AddListener(ShowDropDown);
            m_delete.onClick.AddListener(OnDeleteControl);
            title_dropDown.transform.SetAsFirstSibling();
            supportKeys = new List<string>();
        }

        private void OnDeleteControl()
        {
            if (onDelete != null)
                onDelete.Invoke(this);
        }

        private void ShowDropDown()
        {
            if (title_dropDown.options != null && title_dropDown.options.Count > 0)
            {
                title_dropDown.transform.SetAsLastSibling();

                if (!string.IsNullOrEmpty(key) && supportKeys.Contains(key))
                {
                    title_dropDown.value = supportKeys.IndexOf(key);
                }
                else
                {
                    title_dropDown.value = -1;
                }
            }
        }

        private void OnKeyChanged(string key)
        {
            if (!notTriggerKey)
            {
                if (onKeyChanged != null){
                    onKeyChanged.Invoke(this, lastKey, key);
                }
            }
            UpdateValueType();
            lastKey = key;
        }

        private void OnKeyIndexChanged(int arg0)
        {
            if (supportKeys != null && supportKeys.Count > arg0) {
                this.key = supportKeys[arg0];
            }
            title_dropDown.transform.SetAsFirstSibling();
            OnKeyChanged(key);
        }

        public void SetLayerDefine(ILayerDefine layerDic)
        {
            this.layerDic = layerDic;
        }

        public void UpdateInfo(string key, string value)
        {
            notTriggerKey = true;
           
            supportKeys.Clear();
            this.supportKeys.AddRange(layerDic.supportMembers);

            this.key = key;

            this.lastKey = key;
            this.value = value;

            if (supportKeys != null)
            {
                title_dropDown.options.Clear();
                title_dropDown.AddOptions(supportKeys);

                if(supportKeys.Contains(key)) {
                    title_dropDown.value = supportKeys.IndexOf(key);
                }
            }

            UpdateValueType();
            notTriggerKey = false;
        }

        private void UpdateValueType()
        {
            Type memberType = layerDic.GetMemberType(key);
            UpdateFieldItemFromType(memberType);
            activeItem.SetMemberType(memberType);
            activeItem.SetValue(value);
            activeItem.onValueChanged = OnActiveItemValueChanged;
        }

        private void OnActiveItemValueChanged(string value)
        {
            if (onValueChanged != null)
            {
                this.value = value;
                onValueChanged.Invoke(this,key, value);
            }
        }

        private void UpdateFieldItemFromType(Type memberType)
        {
            if (activeItem && activeItem.gameObject)
            {
                activeItem.onValueChanged = null;
                activeItem.SetValue("");
                gameObjectPool.Release(activeItem.gameObject,false);
            }

            if (memberType == null)
            {
                activeItem = GetInstenceFieldItem<SingleField>();
            }
            else if (memberType.IsSubclassOf(typeof(System.Enum)))
            {
                activeItem = GetInstenceFieldItem<EnumField>();
            }
            else if (memberType == typeof(bool))
            {
                activeItem = GetInstenceFieldItem<BoolField>();
            }
            else if (memberType == typeof(Vector2)|| memberType == typeof(Vector2Int)|| memberType == typeof(Vector3)|| memberType == typeof(Vector3Int)|| memberType == typeof(Vector4))
            {
                activeItem = GetInstenceFieldItem<VectorField>();
            }
            else if(memberType == typeof(Sprite))
            {
                activeItem = GetInstenceFieldItem<SpriteField>();
            }
            else if(memberType == typeof(Rect))
            {
                activeItem = GetInstenceFieldItem<RectField>();
            }
            else if(memberType == typeof(Color))
            {
                activeItem = GetInstenceFieldItem<ColorField>();
            }
            else if(memberType == typeof(ColorBlock))
            {
                activeItem = GetInstenceFieldItem<ColorBlockField>();
            }
            else if(memberType == typeof(SpriteState))
            {
                activeItem = GetInstenceFieldItem<SpriteStateField>();
            }
            else {
                activeItem = GetInstenceFieldItem<SingleField>();
            }
        }

        private T GetInstenceFieldItem<T> () where T:FieldItem
        {
            var name = typeof(T).Name;
            return gameObjectPool.GetInstenceComponent<T>(name, parentTransform, false);
        }
    }
}