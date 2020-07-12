using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nrealus.Extensions
{
    //https://answers.unity.com/questions/526058/addcomponent-passing-variable-before-startawake.html
    // /!\ This approach involves Deactivating and Reactivating (if the object was active before) the gameObject.
    // /!\ This means that any coroutine from this object will break and that OnEnable and OnDisable callbacks will be invoked ..!
    public class GameObjectDeactivateSection : IDisposable
    {
        GameObject go;
        bool oldState;
        public GameObjectDeactivateSection(GameObject aGo)
        {
            go = aGo;
            oldState = go.activeSelf;
            go.SetActive(false);
        }
        public void Dispose()
        {
            go.SetActive(oldState);
        }
    }

    public static class GameObjectExtension
    {

        public static GameObject FindInactiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject;
                    }
                }
            }
            return null;
        }

        public static void SetActiveRecursivelyExt(this GameObject obj, bool state)
        {
            obj.SetActive(state);
            foreach (Transform child in obj.transform)
            {
                SetActiveRecursivelyExt(child.gameObject, state);
            }
        }

        public static IDisposable Deactivate(this GameObject obj)
        {
            return new GameObjectDeactivateSection(obj);
        }

        public static T AddComponentWithParams<T>(this GameObject gameObject, Action<T> action) where T : Component
        {
            using (gameObject.Deactivate())
            {
                T component = gameObject.AddComponent<T>();
                if (action != null) action(component);
                return component;
            }
        }

        public static T FindObjectOfTypeAndLayer<T>(int layer) where T : Component
        {
            Debug.Log(typeof(T));
            T[] os = GameObject.FindObjectsOfType(typeof(T)) as T[];
            foreach(var o in os)
            {
                if (o.gameObject.layer == layer)
                    return o;                
            }
            return default(T);
        }

        public static T[] FindObjectsOfTypeAndLayer<T>(int layer) where T : Component
        {
            //Debug.Log(typeof(T));
            T[] os = GameObject.FindObjectsOfType(typeof(T)) as T[];
            List<T> res = new List<T>();
            foreach(var o in os)
            {
                if (o.gameObject.layer == layer)
                    res.Add(o);
            }
            return res.ToArray();
        }

    }
}

