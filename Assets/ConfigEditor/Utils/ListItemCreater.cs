using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace UGUIAssembler.Config
{
    public class ListItemCreater<T> where T : MonoBehaviour
    {
        public List<T> CreatedItems { get { return createdItems; } }
        private Transform parent { get; set; }
        private T pfb { get; set; }
        private List<T> createdItems = new List<T>();
        private Stack<GameObject> objectPool = new Stack<GameObject>();

        public ListItemCreater(Transform parent, T pfb)
        {
            this.parent = parent;
            this.pfb = pfb;
            pfb.gameObject.SetActive(false);
        }

        public T[] CreateItems(int length)
        {
            ClearOldItems();
            if (length <= 0) return new T[0];

            GameObject go;
            for (int i = 0; i < length; i++)
            {
                go = GetPoolObject(pfb.gameObject, parent);
                T scr = go.GetComponent<T>();
                createdItems.Add(scr);
            }
            return createdItems.ToArray();
        }

        public T AddItem()
        {
            if (pfb == null) return null;
            GameObject go;
            go = GetPoolObject(pfb.gameObject, parent);
            T scr = go.GetComponent<T>();
            createdItems.Add(scr);
            return scr;
        }

        public void RemoveItem(T item)
        {
            createdItems.Remove(item);
            SavePoolObject(item.gameObject);
        }

        public void ClearOldItems()
        {
            foreach (var item in createdItems)
            {
                SavePoolObject(item.gameObject);
            }

            createdItems.Clear();
        }


        private void SavePoolObject(GameObject gameObject)
        {
            gameObject.SetActive(false);
            objectPool.Push(gameObject);
        }

        private GameObject GetPoolObject(GameObject prefab, Transform parent)
        {
            GameObject instence = null;
           if(objectPool.Count > 0)
            {
                instence = objectPool.Pop();
            }

           if(instence == null)
            {
                instence = GameObject.Instantiate(prefab, parent,false);
            }

            instence.gameObject.SetActive(true);
            instence.transform.SetAsLastSibling();
            return instence;
        }

    }
}