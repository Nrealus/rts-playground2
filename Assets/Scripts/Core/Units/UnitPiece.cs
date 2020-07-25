using Core.Tasks;
using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Nrealus.Extensions;
using Core.Selection;
using Core.Faction;
using Core.Handlers;
using Core.Helpers;
using Nrealus.Extensions.Observer;
using System;
using Nrealus.Extensions.ReferenceWrapper;

namespace Core.Units
{

    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    public class UnitPieceWrapper : RefWrapper<UnitPiece>
    {
        public UnitPieceWrapper(UnitPiece obj) : base(obj)
        { 
            obj.SubscribeOnDestructionAtEnd("destroywrapper", DestroyRef, true);
        }
    }

    ///<summary>
    /// The base class/component for in-game "concrete unit pieces" Tanks, infantry squads...
    /// NOT USED ACTIVELY ANYWHERE YET.
    ///</summary>
    public class UnitPiece : MonoBehaviour, IDestroyable
    {

        #region IDestroyable implementation
        
        private EasyObserver<string> onDestroyed = new EasyObserver<string>();

        public void SubscribeOnDestruction(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action);
        }

        public void SubscribeOnDestructionAtEnd(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action);
        }

        public void SubscribeOnDestruction(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void SubscribeOnDestructionAtEnd(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void UnsubscribeOnDestruction(string key)
        {
            onDestroyed.UnsubscribeEventHandlerMethod(key);
        }

        public void DestroyThis()
        {
            Destroy(gameObject);

            onDestroyed.Invoke();

            //GetOnSelectionStateChangeObserver().UnsubscribeAllEventHandlerMethods();
        }

        #endregion


        private SpriteRenderer mySprRenderer;

        [HideInInspector] public UnitMover myMover;

        private void Init()
        {
            myMover = GetComponent<UnitMover>();
            mySprRenderer = GetComponent<SpriteRenderer>();            
        }

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
        }

    }
}