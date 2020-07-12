using Core.Tasks;
using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Selection;
using Core.Faction;
using Core.Handlers;
using Core.Helpers;

namespace Core.Units
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// The base class/component for in-game "concrete unit pieces" Tanks, infantry squads...
    ///</summary>
    public class UnitPiece : MonoBehaviour,
        IHasRefWrapper<UnitPieceWrapper>
    {

        private SpriteRenderer mySprRenderer;

        private UnitPieceWrapper _myWrapper;
        public UnitPieceWrapper GetRefWrapper() { return _myWrapper; }

        [HideInInspector] public UnitMover myMover;

        private void Init()
        {
            _myWrapper = new UnitPieceWrapper(this, () => {_myWrapper = null;});
            //unitTreeNode = new UnitTreeNode(GetMyWrapper());
            myMover = GetComponent<UnitMover>();
            mySprRenderer = GetComponent<SpriteRenderer>();            
        }

        private void Awake()
        {
            Init();
        }

        public bool subTesting = false;
        private static UnitPieceWrapper _toaddtest = null;
        private void Update()
        {
            /*if (IsSelected(GetMyWrapper(), GetUsedSelector())
                && Input.GetKeyDown(KeyCode.K))
            {
                Dismantle(GetMyWrapper());
            }*/
            if (subTesting)
                _toaddtest = GetRefWrapper();

            if (_toaddtest == GetRefWrapper() && Input.GetKeyDown(KeyCode.K))
            {
                //var l = GetUsedSelector().GetCurrentlySelectedEntities();
                /*var w = (new UnitGroup(false, true)).GetMyWrapper();
                foreach (var v in l)
                {
                    Debug.Log("hhhhhh");
                    v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().ChangeUnitGroup(w);
                }
                UnitGroup.AddSubGroupToGroup(w, GetMyWrapper().unitsGroupWrapper);*/
                //foreach (var v in l)
                {
                    //Unit.AddSubGroupToGroup(v.GetSelectableAsReferenceWrapperSpecific<UnitPieceWrapper>().unitWrapper, GetMyWrapper().unitWrapper);
                }
            }

            //DrawUpdate();
        }

    }
}