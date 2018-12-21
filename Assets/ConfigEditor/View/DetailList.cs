using System;
using System.Collections.Generic;

using UnityEngine;
namespace UGUIAssembler.Config
{
    public class DetailList : MonoBehaviour
    {
        [SerializeField]
        private DetailList_Item itemPrefab;
        [SerializeField]
        private Transform itemParent;
        [SerializeField]
        private UnityEngine.UI.ScrollRect scrollRect;
        private UIInfo _uiInfo;
        private UIInfo uiInfo;
        private ListItemCreater<DetailList_Item> listCreater;
        public List<string> controlTypes { get; set; }
        public Dictionary<string, ILayerDefine> controlDic { get; set; }
        public Action<int> onPreviewItem { get; set; }
        public Action<int, LayerInfo> onInsert { get; set; }
        public System.Action<int, string> onLayerNameChanged { get; set; }
        public System.Action<int> onLayerDeleted { get; set; }
        public System.Action<int, bool> onSelectChanged { get; set; }
        private List<DetailList_Item> itemDic;
        private void Awake()
        {
            listCreater = new ListItemCreater<DetailList_Item>(itemParent, itemPrefab);
            itemDic = new List<DetailList_Item>();
        }

        public void SetUIInfo(UIInfo uiInfo)
        {
            this.uiInfo = uiInfo;
            UpdateLists();
        }

        private void UpdateLists()
        {
            listCreater.ClearOldItems();
            itemDic.Clear();

            if (uiInfo != null && uiInfo.layers != null && uiInfo.layers.Count > 0)
            {
                var createdCount = uiInfo.layers.Count;
                var created = listCreater.CreateItems(createdCount);
                for (int i = 0; i < createdCount; i++)
                {
                    var item = created[i];
                    InitDetailItem(i, item);
                }
            }

        }

        private void OnDeleteItem(DetailList_Item obj)
        {
            if(DialogHelper.ShowDialog("小提示","此操作会删除当前层级所有信息，并且暂时没有回退功能！","删除","取消"))
            {
                uiInfo.layers.Remove(obj.layerInfo);
                UpdateLists();

                if (onLayerDeleted != null)
                {
                    onLayerDeleted.Invoke(obj.ID);
                }
            }
        }

        private void OnPreviewItem(int index)
        {
            if (onPreviewItem != null)
                onPreviewItem.Invoke(index);
        }

        private void OnItemSelected(int obj, bool active)
        {
            if (onSelectChanged != null)
                onSelectChanged.Invoke(obj, active);
        }

        private void OnItemNameChanged(int index, string name)
        {
            var layerInfo = uiInfo.layers[index];
            layerInfo.name = name;

            if (onLayerNameChanged != null)
            {
                onLayerNameChanged.Invoke(index, name);
            }
        }

        public void SetActiveItem(int index, bool active)
        {
            if (itemDic.Count > index)
            {
                itemDic[index].SetSelect(active);
            }

            if (listCreater.CreatedItems.Count >= 1)
            {
                float span = listCreater.CreatedItems.Count - 1f;
                scrollRect.verticalNormalizedPosition = (span - index) / span;
            }
        }

        public void Sort(List<int> idList)
        {
            var layerTemplate = new List<LayerInfo>(uiInfo.layers);
            uiInfo.layers.Clear();

            for (int i = 0; i < idList.Count; i++)
            {
                var id = idList[i];
                uiInfo.layers.Add(layerTemplate[id]);
            }

            UpdateLists();
        }

        public void InsetItem(int index, string name, string path, string type, Rect rect)
        {
            var layerInfo = new LayerInfo();
            layerInfo.name = name;
            layerInfo.path = path;
            layerInfo.type = type;
            layerInfo.rect = rect;
            CreateNewLayerInternal(index, layerInfo);
        }

        private void InitDetailItem(int index, DetailList_Item item)
        {
            item.onNameChanged = OnItemNameChanged;
            item.onSelectChanged = OnItemSelected;
            item.onCreateNewLayerInfo = OnCreateNewLayer;
            item.onPreview = OnPreviewItem;
            item.onDelete = OnDeleteItem;
            item.SetInformation(index, controlTypes, controlDic, uiInfo.layers[index]);
            if (itemDic.Count > index)
            {
                itemDic[index] = item;
            }
            else
            {
                itemDic.Add(item);
            }
        }

        private void OnCreateNewLayer(int id, LayerInfo layerInfo)
        {
            CreateNewLayerInternal(id + 1, layerInfo);
            if (onInsert != null)
            {
                onInsert.Invoke(id + 1, layerInfo);
            }
        }

        private void CreateNewLayerInternal(int index, LayerInfo layerInfo)
        {
            var count = listCreater.CreatedItems.Count;
            for (int i = index; i < count; i++)
            {
                var oitem = itemDic[i];
                oitem.SetIndex(i + 1);
            }
            var item = listCreater.AddItem();
            itemDic.Insert(index, item);
            uiInfo.layers.Insert(index, layerInfo);
            InitDetailItem(index, item);
            item.transform.SetSiblingIndex(index + 1);//包括预制体
        }

        public LayerInfo GetLayerInfoFromIndex(int index)
        {
            if (itemDic.Count > index)
            {
                return itemDic[index].layerInfo;
            }
            return null;
        }
    }
}