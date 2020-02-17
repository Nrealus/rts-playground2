using System;
using System.Collections.Generic;
using UnityEngine;

namespace VariousUtilsExtensions
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
    }
}

