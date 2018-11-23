using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using UnityEditorInternal;
namespace UGUIAssembler
{
    public class PrefabBindingWindow : EditorWindow
    {
        public class ObjHolder : IComparable<ObjHolder>
        {
            public string name;
            public int index;
            public GameObject obj;
            public static int id;
            public ObjHolder() { index = id++; }

            public int CompareTo(ObjHolder obj)
            {
                if (obj.index > index)
                {
                    return -1;
                }
                else if (obj.index < index)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        PrefabBindingObj obj;
        string[] switchOption = new string[] { "信息记录", "信息加载" };
        int selected = 0;
        Vector2 viewScrop;
        public List<ObjHolder> objectList = new List<ObjHolder>();
        SerializedObject serializedObj;
        SerializedProperty scriptProp;
        private ReorderableList reorderList;
        private void OnEnable()
        {
            serializedObj = new SerializedObject(this);
            scriptProp = serializedObj.FindProperty("m_Script");
            ObjHolder.id = 0;
            if (obj)
            {
                LoadObjectList(obj);
            }
            InitReorderList();
        }

        private void InitReorderList()
        {
            reorderList = new ReorderableList(objectList, typeof(ObjHolder));
            reorderList.drawHeaderCallback = DrawListHead;
            reorderList.drawElementCallback = DrawElement;
        }

        private void DrawElement(Rect position, int index, bool isActive, bool isFocused)
        {
            if (objectList.Count <= index || index < 0) return;
            var item = objectList[index];
            if (item == null)
            {
                item = new ObjHolder();
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                var rect = new Rect(position.x, position.y, position.width * 0.3f, EditorGUIUtility.singleLineHeight);
                item.index = EditorGUI.IntField(rect, item.index);
                rect.x += 0.32f * position.width;
                item.name = EditorGUI.TextField(rect, item.name);
                rect.x += 0.32f * position.width;
                item.obj = EditorGUI.ObjectField(rect, item.obj, typeof(GameObject), true) as GameObject;

                if (string.IsNullOrEmpty(item.name) && item.obj != null)
                {
                    item.name = item.obj.name;
                }
            }
            objectList[index] = item;
        }

        private void DrawListHead(Rect rect)
        {
            EditorGUI.LabelField(rect, "相关对象列表");
        }

        public void OnGUI()
        {
            EditorGUILayout.PropertyField(scriptProp);

            serializedObj.Update();
            selected = GUILayout.Toolbar(selected, switchOption);
            var newObj = EditorGUILayout.ObjectField(obj, typeof(PrefabBindingObj), false) as PrefabBindingObj;

            if (newObj != obj && newObj != null)
            {
                obj = newObj;
                LoadObjectList(obj);
            }

            if (obj != null)
            {
                using (var ver = new EditorGUILayout.ScrollViewScope(viewScrop, GUILayout.Height(300)))
                {
                    viewScrop = ver.scrollPosition;
                    DrawListView();
                }
                DrawOptionButtons();
            }
            else
            {
                EditorGUI.HelpBox(GUILayoutUtility.GetRect(0, 40), "请先放置信息保存对象", MessageType.Error);
                if (GUILayout.Button("创建"))
                {
                    obj = PrefabBindingUtility.CreatePreabBindingObj();
                }
            }
            serializedObj.ApplyModifiedProperties();
        }
        private void DrawListView()
        {
            if (objectList.Count == 0)
            {
                ObjHolder.id = 0;
            }
            reorderList.DoLayoutList();
        }

        private void DrawOptionButtons()
        {
            var rect = new Rect(0, position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            if (selected == 0)
            {
                if (GUI.Button(rect, "解析对象信息"))
                {
                    bool ok = EditorUtility.DisplayDialog("保存到数据源", "点击确定完成预制体信息记录", "确定");
                    if (ok)
                    {
                        obj.bindingItems.Clear();
                        PrefabBindingUtility.ExargeRentItems(obj.bindingItems, objectList);
                        EditorUtility.SetDirty(obj);
                    }
                }
            }
            else
            {
                if (GUI.Button(rect, "加载对象信息"))
                {
                    bool ok = EditorUtility.DisplayDialog("从数据中加载", "点击确定完成预制体信息加载", "确定");
                    if (ok) PrefabBindingUtility.InstallInfomation(objectList, obj.bindingItems);
                }
            }
        }
        private void LoadObjectList(PrefabBindingObj obj)
        {
            objectList.Clear();
            foreach (var item in obj.bindingItems)
            {
                if (objectList.Find(x => x.index == item.index) == null)
                {
                    var objholder = new ObjHolder();
                    objholder.name = item.name;
                    objholder.index = item.index;
                    objectList.Add(objholder);
                }
            }
            objectList.Sort();
        }
    }
}