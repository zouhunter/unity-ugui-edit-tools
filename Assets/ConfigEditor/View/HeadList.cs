using UnityEngine;
using UnityEngine.UI;
using UGUIAssembler;
using System.Collections.Generic;
using System;

namespace UGUIAssembler.Config
{
    public class HeadList : MonoBehaviour
    {
        [SerializeField]
        private HeadList_Item itemPrefab;
        [SerializeField]
        private Transform itemParent;
        [SerializeField]
        private ReorderableList reorderList;
        [SerializeField]
        private Button m_add;

        private List<string[]> arrayList = new List<string[]>();
        private ListItemCreater<HeadList_Item> listCreater;
        public Action<int, bool> onSelectChanged { get; set; }
        public Action<int> onInsert { get; set; }
        public Action<int, string> onPathChanged { get; set; }
        public Action<List<int>> onSortChanged { get; set; }
        private List<HeadList_Item> itemDic;

        private void Awake()
        {
            itemDic = new List<HeadList_Item>();
            listCreater = new ListItemCreater<HeadList_Item>(itemParent, itemPrefab);
            reorderList.OnElementDropped.AddListener(OnPositionsUpdated);
            m_add.onClick.AddListener(() => { OnInsetItem(listCreater.CreatedItems.Count - 1); });
        }

        public void SetInfo(List<string[]> uiInfo)
        {
            this.arrayList.Clear();
            if (uiInfo != null)
            {
                this.arrayList.AddRange(uiInfo);
            }
            UpdateLists();
        }

        private void OnSelectHeadItem(int index, bool active)
        {
            if (onSelectChanged != null)
                onSelectChanged.Invoke(index, active);
        }

        private void UpdateLists()
        {
            itemDic.Clear();
            listCreater.ClearOldItems();

            if (arrayList != null && arrayList != null && arrayList.Count > 0)
            {
                var createdCount = arrayList.Count;
                var created = listCreater.CreateItems(createdCount);
                for (int i = 0; i < createdCount; i++)
                {
                    var item = created[i];
                    InitItem(item, i);
                }
            }

        }

        private void OnInsetItem(int index)
        {
            if (onInsert != null)
            {
                onInsert.Invoke(index);
            }
        }

        private void OnPositionsUpdated(ReorderableList.ReorderableListEventStruct arg0)
        {
            var idList = new List<int>();


            var sortedList = new List<HeadList_Item>();

            for (int i = 0; i < itemParent.childCount; i++)
            {
                var child = itemParent.GetChild(i);
                var item = child.gameObject.GetComponent<HeadList_Item>();
                if (child.gameObject.activeSelf && item != null)
                {
                    sortedList.Add(item);
                }
            }

            for (int i = 0; i < sortedList.Count; i++)
            {
                idList.Add(sortedList[i].ID);
            }


            for (int i = 0; i < sortedList.Count; i++)
            {
                var item = sortedList[i];
                item.SetIndex(i);
                itemDic[i] = item;
            }

            var tempList = new List<string[]>(arrayList);
            arrayList.Clear();
            for (int i = 0; i < tempList.Count; i++)
            {
                arrayList.Add(tempList[idList[i]]);
            }

            if (onSortChanged != null)
                onSortChanged.Invoke(idList);
        }

        internal void UpdateItem(int index, string name)
        {
            if (itemDic.Count > (index))
            {
                itemDic[index].UpdateName(name);
            }
        }

        internal void SetActiveItem(int index, bool active)
        {
            if (itemDic.Count > (index))
            {
                itemDic[index].SetSelect(active, false);
            }
        }

        internal void DeleteAtID(int id)
        {
            arrayList.RemoveAt(id);
            UpdateLists();
        }

        public void InsetItem(int index, string name, string path)
        {
            var count = listCreater.CreatedItems.Count;
            for (int i = index; i < count; i++)
            {
                var oitem = itemDic[i];
                oitem.SetIndex(i + 1);
            }

            arrayList.Insert(index, new string[] { name, path });
            var item = listCreater.AddItem();
            itemDic.Insert(index, item);

            InitItem(item, index);
            item.transform.SetSiblingIndex(index + 1);//包括预制体
        }

        private void InitItem(HeadList_Item item, int index)
        {
            item.onSelect = OnSelectHeadItem;
            item.onInsert = OnInsetItem;
            item.SetIndex(index);
            item.SetInformation(arrayList[index]);
            item.onPathChanged = OnPathChanged;
            if (itemDic.Count > index)
            {
                itemDic[index] = item;
            }
            else
            {
                itemDic.Add(item);
            }
        }

        private void OnPathChanged(int arg1, string arg2)
        {
            if (onPathChanged != null)
                onPathChanged.Invoke(arg1, arg2);
        }
    }
}