using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UGUIAssembler;
using UnityEngine;
using UnityEngine.UI;
namespace UGUIAssembler.Config
{
    public class DetailList_Item : MonoBehaviour
    {
        [SerializeField] private Toggle m_select;
        [SerializeField] private InputField m_name;
        [SerializeField] private Dropdown m_controlTypes;
        [SerializeField] private InputField m_x;
        [SerializeField] private InputField m_y;
        [SerializeField] private InputField m_w;
        [SerializeField] private InputField m_h;
        [SerializeField] private Toggle m_expland;
        [SerializeField] private Text m_title;
        [SerializeField] private Button m_preview;
        [SerializeField] private Button m_refesh;
        [SerializeField] private Button m_delete;
        [SerializeField] private Control_Item ctrlItemPrefab;
        [SerializeField] private Transform contrlItemParent;

        [SerializeField] private Toggle m_main_ctrl;
        [SerializeField] private Toggle m_sub_ctrl;
        [SerializeField] private Toggle m_text_ctrl;
        [SerializeField] private Toggle m_sprite_ctrl;
        [SerializeField] private Toggle m_texture_ctrl;

        [SerializeField] private Button m_add_optional;
        [SerializeField] private Button m_add_text;
        [SerializeField] private Button m_add_image;
        [SerializeField] private Button m_add_rawimage;

        [SerializeField] private ScrollRect m_scrollRect;

        private List<string> controlTypes { get; set; }
        private Rect rect
        {
            set
            {
                m_x.text = value.x.ToString();
                m_y.text = value.y.ToString();
                m_w.text = value.width.ToString();
                m_h.text = value.height.ToString();
            }
        }
        private string layerName
        {
            get
            {
                return m_name.text;
            }
            set
            {
                m_name.text = value;
            }
        }

        public int ID { get; private set; }
        public Action<int> onPreview { get; set; }
        public Action<DetailList_Item> onDelete { get; set; }
        public Action<int, string> onNameChanged { get; set; }
        public Action<int, bool> onSelectChanged { get; set; }
        public Action<int, LayerInfo> onCreateNewLayerInfo { get; set; }
        public LayerInfo layerInfo { get; private set; }

        private ListItemCreater<Control_Item> controlItemCreater;
        private string keyMainCtrlName = "控件参数";
        private string key_Text_Name = "文字控件";
        private string key_Image_Name = "图片控件";
        private string key_RawImage_Name = "大图控件";
        private ListFliter listFliter;
        private Dictionary<string, ILayerDefine> controlDic;
        private bool isSettingName;

        private void Awake()
        {
            m_name.onValueChanged.AddListener(OnNameChanged);
            m_controlTypes.onValueChanged.AddListener(OnControlTypeChanged); 
            m_select.onValueChanged.AddListener(OnSelectChanged);
            m_x.onValueChanged.AddListener(On_X_Changed);
            m_y.onValueChanged.AddListener(On_Y_Changed);
            m_w.onValueChanged.AddListener(On_W_Changed);
            m_h.onValueChanged.AddListener(On_H_Changed);
            m_delete.onClick.AddListener(OnDelete);
            m_preview.onClick.AddListener(OnPreview);
            m_refesh.onClick.AddListener(OnRefesh);
            controlItemCreater = new ListItemCreater<Control_Item>(contrlItemParent, ctrlItemPrefab);
            listFliter = new ListFliter()
            {
                main = true,
                sub = true,
                text = true,
                texture = true,
                sprite = true
            };

            m_main_ctrl.onValueChanged.AddListener(OnControlOptionChnaged);
            m_sub_ctrl.onValueChanged.AddListener(OnControlOptionChnaged);
            m_text_ctrl.onValueChanged.AddListener(OnControlOptionChnaged);
            m_sprite_ctrl.onValueChanged.AddListener(OnControlOptionChnaged);
            m_texture_ctrl.onValueChanged.AddListener(OnControlOptionChnaged);

            m_add_optional.onClick.AddListener(OnAddOptional);
            m_add_image.onClick.AddListener(OnAddImage);
            m_add_text.onClick.AddListener(OnAddText);
            m_add_rawimage.onClick.AddListener(OnAddRawImage);
        }


        internal void SetIndex(int index)
        {
            this.ID = index;
            UpdateTitle();
        }

        private void OnDelete()
        {
            if (onDelete != null)
            {
                onDelete.Invoke(this);
            }
        }

        private void OnControlOptionChnaged(bool isOn)
        {
            listFliter.main = m_main_ctrl.isOn;
            listFliter.sub = m_sub_ctrl.isOn;
            listFliter.text = m_text_ctrl.isOn;
            listFliter.sprite = m_sprite_ctrl.isOn;
            listFliter.texture = m_texture_ctrl.isOn;

            UpdateListView();
        }

        private void On_X_Changed(string arg0)
        {
            float value;
            float.TryParse(arg0, out value);
            layerInfo.rect.x = value;
        }

        private void On_Y_Changed(string arg0)
        {
            float value;
            float.TryParse(arg0, out value);
            layerInfo.rect.y = value;
        }

        private void On_W_Changed(string arg0)
        {
            float value;
            float.TryParse(arg0, out value);
            layerInfo.rect.width = value;
        }

        private void On_H_Changed(string arg0)
        {
            float value;
            float.TryParse(arg0, out value);
            layerInfo.rect.height = value;
        }
        private void OnPreview()
        {
            if (onPreview != null)
            {
                onPreview.Invoke(ID);
            }
        }
        private void OnRefesh()
        {
            UpdateListView();
        }

        private void OnSelectChanged(bool arg0)
        {
            if (onSelectChanged != null)
            {
                onSelectChanged.Invoke(ID, arg0);
            }
        }

        internal void SetSelect(bool isOn)
        {
            if (m_select.isOn != isOn)
            {
                m_select.isOn = isOn;
            }
        }

        private void OnControlTypeChanged(int arg0)
        {
            layerInfo.type = controlTypes[arg0];
            UpdateListView();
        }

        private void OnNameChanged(string arg0)
        {
            UpdateTitle();
            if (!isSettingName)
            {
                if (onNameChanged != null)
                {
                    onNameChanged.Invoke(ID, arg0);
                }
            }

        }

        public void SetInformation(int index, List<string> controlTypes, Dictionary<string, ILayerDefine> controlDic, LayerInfo layerInfo)
        {
            this.layerInfo = layerInfo;
            this.controlDic = controlDic;
            this.ID = index;
            isSettingName = true;
            layerName = layerInfo.name;
            isSettingName = false;
            this.rect = layerInfo.rect;
            this.controlTypes = controlTypes;

            m_controlTypes.options.Clear();
            m_controlTypes.AddOptions(controlTypes);

            if (controlTypes.Contains(layerInfo.type))
            {
                m_controlTypes.value = controlTypes.IndexOf(layerInfo.type);
            }
            UpdateTitle();
            UpdateListView();
        }

        private void UpdateListView()
        {
            if (controlDic == null || layerInfo == null || string.IsNullOrEmpty(layerInfo.type)) return;

            this.m_add_optional.gameObject.SetActive(controlDic[layerInfo.type].runtimeSubControls != null);//设置按扭状态

            if (controlDic.ContainsKey(layerInfo.type))
            {
                var type = controlDic[layerInfo.type];
                CreateListFliter(type);
            }
        }

        private void CreateListFliter(ILayerDefine mainLayerDefine)
        {
            List<ResourceDic> createdList = new List<ResourceDic>();
            List<string> names = new List<string>();
            Dictionary<int, ILayerDefine> typeDic = new Dictionary<int, ILayerDefine>();

            if (mainLayerDefine.integrantSubControls != null)
            {//自动补全缺少的字典
                for (int i = 0; i < mainLayerDefine.integrantSubControls.Count; i++)
                {
                    var integrantSubControl = mainLayerDefine.integrantSubControls[i];
                    if (!layerInfo.subResourceDic.ContainsKey(integrantSubControl))
                    {
                        layerInfo.subResourceDic.Add(integrantSubControl, new ResourceDic());
                    }
                }
            }

            if (listFliter.main)
            {
                names.Add(keyMainCtrlName);
                createdList.Add(layerInfo.resourceDic);
                typeDic.Add(layerInfo.resourceDic.GetHashCode(), mainLayerDefine);
            }

            if (listFliter.sub)
            {
                foreach (var item in layerInfo.subResourceDic)
                {
                    var subType = mainLayerDefine.GetSubControlType(item.Key);
                    if (!string.IsNullOrEmpty(subType) && controlDic.ContainsKey(subType))
                    {
                        names.Add(item.Key);
                        createdList.Add(item.Value);
                        typeDic.Add(item.Value.GetHashCode(), controlDic[subType]);
                    }
                }
            }

            if (listFliter.text)
            {
                foreach (var item in layerInfo.sub_texts)
                {
                    names.Add(key_Text_Name);
                    createdList.Add(item);
                    typeDic.Add(item.GetHashCode(), controlDic[ParamAnalysisTool.text_art_key]);
                }
            }

            if (listFliter.sprite)
            {
                foreach (var item in layerInfo.sub_images)
                {
                    names.Add(key_Image_Name);
                    createdList.Add(item);
                    typeDic.Add(item.GetHashCode(), controlDic[ParamAnalysisTool.image_art_key]);
                }
            }


            if (listFliter.sprite)
            {
                foreach (var item in layerInfo.sub_rawImages)
                {
                    names.Add(key_RawImage_Name);
                    createdList.Add(item);
                    typeDic.Add(item.GetHashCode(), controlDic[ParamAnalysisTool.rawImage_art_key]);
                }
            }

            var created = controlItemCreater.CreateItems(createdList.Count);
            for (int i = 0; i < created.Length; i++)
            {
                var ctrlItem = created[i];
                var dic = createdList[i];
                var name = names[i];
                var layerDefine = typeDic[dic.GetHashCode()];
                InitCtrlItem(i, ctrlItem, layerDefine, dic, name);
            }
        }
        /// <summary>
        /// 初始化CtrlItem
        /// </summary>
        /// <param name="ctrlItem"></param>
        /// <param name="layerDefine"></param>
        /// <param name="dic"></param>
        /// <param name="name"></param>
        private void InitCtrlItem(int index, Control_Item ctrlItem, ILayerDefine layerDefine, ResourceDic dic, string name)
        {
            var mainLayerDefine = controlDic[layerInfo.type];
            var controlName = name;
            if (index != 0){
                controlName = mainLayerDefine.GetSubControlName(name);
            }
            ctrlItem.onDelete = OnDeleteCtrlItem;
            ctrlItem.onMainControl = OnMainCtrlItem;
            ctrlItem.onGenInstenceControl = OnGenInstenceControl;
            ctrlItem.SetIndex(index);
            ctrlItem.SetTitle(controlName);
            UpdateCtrlItemState(index, ctrlItem);
            ctrlItem.SetPropertys(dic, layerDefine);
        }

        private void UpdateCtrlItemState(int index,Control_Item ctrlItem)
        {
            var mainLayerDefine = controlDic[layerInfo.type];
            if (index != 0)//第0个默认为主控件
            {
                if (mainLayerDefine.integrantSubControls == null || !mainLayerDefine.integrantSubControls.Contains(name))
                {
                    ctrlItem.SetDeleteable(true);
                }
                else
                {
                    ctrlItem.SetDeleteable(false);
                }
            }
            else
            {
                ctrlItem.SetDeleteable(false);
            }
        }

        /// <summary>
        /// 置为主控件
        /// </summary>
        /// <param name="ctrlItem"></param>
        private void OnMainCtrlItem(Control_Item ctrlItem)
        {
            var mainLayerDefine = controlDic[layerInfo.type];

            if (ctrlItem.layerDefine != mainLayerDefine)
            {
                var @continue = DialogHelper.ShowDialog("小提示", "注意到与目标类型不一致！", "继续", "取消");
                if (!@continue)
                {
                    return;
                }
            }

            var tempDic = new ResourceDic(layerInfo.resourceDic);
            ctrlItem.CopyToDic(layerInfo.resourceDic);

            var typeName = ctrlItem.layerDefine.type.Name;
            layerInfo.type = typeName;
            var id = controlTypes.IndexOf(typeName);

            if (m_controlTypes.value != id)
            {
                layerInfo.type = controlTypes[id];

                m_controlTypes.onValueChanged.RemoveListener(OnControlTypeChanged);
                m_controlTypes.value = id;
                m_controlTypes.onValueChanged.AddListener(OnControlTypeChanged);
            }

            var keep = tempDic.Count > 0 && DialogHelper.ShowDialog("小提示", "原有主控件信息不为空，是否保留信息继续编辑！", "保留", "删除");

            if (keep)
            {
                ctrlItem.CopyFromDic(tempDic);
            }
            else
            {
                OnDeleteCtrlItem(ctrlItem);
            }
            UpdateListView();
        }

        /// <summary>
        /// 创建独立控件
        /// </summary>
        /// <param name="ctrlItem"></param>
        private void OnGenInstenceControl(Control_Item ctrlItem)
        {
            LayerInfo subLayerInfo = new LayerInfo();
            var typeName = ctrlItem.layerDefine.type.Name;
            subLayerInfo.name = "new " + typeName;
            subLayerInfo.path = this.layerInfo.path + "/" + subLayerInfo.name;
            subLayerInfo.type = typeName;
            subLayerInfo.rect = this.layerInfo.rect;
            ctrlItem.CopyToDic(subLayerInfo.resourceDic);
            OnDeleteCtrlItem(ctrlItem);
            if (onCreateNewLayerInfo != null)
                onCreateNewLayerInfo.Invoke(this.ID, subLayerInfo);
        }

        /// <summary>
        /// 查找并从字典中将信息移除
        /// </summary>
        /// <param name="obj"></param>
        private void OnDeleteCtrlItem(Control_Item obj)
        {
            var hashCode = obj.dicHash;

            controlItemCreater.RemoveItem(obj);

            using (var enumerator = layerInfo.subResourceDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current.Value.GetHashCode() == hashCode)
                    {
                        layerInfo.subResourceDic.Remove(current.Key);
                        return;
                    }
                }
            }

            using (var enumerator = layerInfo.sub_images.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current.GetHashCode() == hashCode)
                    {
                        layerInfo.sub_images.Remove(current);
                        return;
                    }
                }
            }
            using (var enumerator = layerInfo.sub_texts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current.GetHashCode() == hashCode)
                    {
                        layerInfo.sub_texts.Remove(current);
                        return;
                    }
                }
            }
            using (var enumerator = layerInfo.sub_rawImages.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current.GetHashCode() == hashCode)
                    {
                        layerInfo.sub_rawImages.Remove(current);
                        return;
                    }
                }
            }
        }

        private void UpdateTitle()
        {
            this.m_title.text = string.Format("{0}.{1}", ID.ToString("00"), layerName);
        }

        /// <summary>
        /// 添加可选控件
        /// </summary>
        private void OnAddOptional()
        {
            var mainLayerDefine = controlDic[layerInfo.type];
            var options = mainLayerDefine.runtimeSubControls.ToArray();
            var optionNames = new string[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                optionNames[i] = mainLayerDefine.GetSubControlName(options[i]);
            }

            var count = controlItemCreater.CreatedItems.Count;

            PopOption.Instence.ShowPop(optionNames, (id) =>
            {
                var key = options[id];
                if (!layerInfo.subResourceDic.ContainsKey(key))
                {
                    var subType = mainLayerDefine.GetSubControlType(key);
                    if (!string.IsNullOrEmpty(subType))
                    {
                        var dic = new ResourceDic();
                        var ctrlItem = controlItemCreater.AddItem();
                        layerInfo.subResourceDic.Add(key, dic);
                        var layerDefine = controlDic[subType];
                        InitCtrlItem(count, ctrlItem, layerDefine, dic, key);
                        SetLastScroll();
                    }
                }
            });
        }

        /// <summary>
        /// 添加图片
        /// </summary>
        private void OnAddImage()
        {
            if (controlDic != null)
            {
                var count = controlItemCreater.CreatedItems.Count;

                var dic = new ResourceDic();
                layerInfo.sub_images.Add(dic);
                var ctrlItem = controlItemCreater.AddItem();
                var layerDefine = controlDic[ParamAnalysisTool.image_art_key];
                InitCtrlItem(count, ctrlItem, layerDefine, dic, key_Image_Name);
                SetLastScroll();
            }
        }
        /// <summary>
        /// 添加文字控件
        /// </summary>
        private void OnAddText()
        {
            if (controlDic != null)
            {
                var count = controlItemCreater.CreatedItems.Count;

                var textDic = new ResourceDic();
                layerInfo.sub_texts.Add(textDic);
                var ctrlItem = controlItemCreater.AddItem();
                var layerDefine = controlDic[ParamAnalysisTool.text_art_key];
                InitCtrlItem(count, ctrlItem, layerDefine, textDic, key_Text_Name);
                SetLastScroll();
            }
        }
        /// <summary>
        /// 添加图片
        /// </summary>
        private void OnAddRawImage()
        {
            if (controlDic != null)
            {
                var count = controlItemCreater.CreatedItems.Count;

                var dic = new ResourceDic();
                layerInfo.sub_rawImages.Add(dic);
                var ctrlItem = controlItemCreater.AddItem();
                var layerDefine = controlDic[ParamAnalysisTool.rawImage_art_key];
                InitCtrlItem(count, ctrlItem, layerDefine, dic, key_RawImage_Name);
                SetLastScroll();
            }
        }
        private void SetLastScroll()
        {
            StartCoroutine(DelySetLastScroll());
        }

        private IEnumerator DelySetLastScroll()
        {
            yield return null;
            m_scrollRect.horizontalNormalizedPosition = 1;
        }
    }
}
