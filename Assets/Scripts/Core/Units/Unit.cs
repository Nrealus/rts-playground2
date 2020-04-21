using Core.Orders;
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
    public class Unit : MonoBehaviour,
        IHasRefWrapper<UnitWrapper>
        //IHasRefWrapperAndTree<UnitWrapper>
    {

        [HideInInspector] public FactionAffiliation factionAffiliation;

        public enum UnitLevel { Army, Corps, Brigade, Division, Regiment, Battalion, Company, Platoon/*, Section*/ };
        public enum UnitType { Order, Infantry, Recon, Armored, Artillery, AntiAir };

        private UnitLevel _level;
        private UnitType _type;

        public UnitLevel unitLevel { get { return _level; } /*private*/ set { _level = value; } }
        public UnitType unitType { get { return _type; } private set { _type = value; } }

        /*--------*/

        private SpriteRenderer mySprRenderer;

        private UnitTreeNode unitTreeNode;

        /*--------*/

        private UnitWrapper _myWrapper;
        public UnitWrapper GetMyWrapper() { return _myWrapper; }

        /*--------*/

        [HideInInspector] public UnitMover myMover;

        public static bool IsSelected(UnitWrapper unitWrapper, Selector selector)
        {
            return selector.IsSelected(unitWrapper);
        }

        public static bool IsHighlighted(UnitWrapper unitWrapper, Selector selector)
        {
            return selector.IsHighlighted(unitWrapper);
        }

        public static bool IsThereAnyParentHighlightedOrSelected(UnitWrapper unitWrapper, Selector selector)
        {
            if (unitWrapper != null && Unit.GetParentWrapper(unitWrapper) != null)
            {
                bool b = Unit.IsSelected(Unit.GetParentWrapper(unitWrapper), selector)
                    || Unit.IsHighlighted(Unit.GetParentWrapper(unitWrapper), selector);
                return b || Unit.IsThereAnyParentHighlightedOrSelected(Unit.GetParentWrapper(unitWrapper), selector);
            }
            else
            {
                return false;
            }
        }

        public static void Dismantle(UnitWrapper unitWrapper)
        {
            Destroy(unitWrapper.WrappedObject.gameObject);
            unitWrapper.DestroyWrappedReference();
        }

        // Public for testing
        public static void SetAsSubordinateOf(UnitWrapper unitWrapper, UnitWrapper newSuperior)
        {
            unitWrapper.WrappedObject.unitTreeNode.ChangeParentTo(newSuperior.WrappedObject.unitTreeNode);
        }

        /*List<Unit>*/
        public static List<UnitWrapper> GetAllSubordinateUnitsList(UnitWrapper unitWrapper)
        {
            //List<Unit> res = new List<Unit>();
            List<UnitWrapper> tns = unitWrapper.WrappedObject.unitTreeNode.BFSListMeAndAllChildrenWrappers();
            /*float c = tns.Count;
            for (int o = 0; o < c; o++)
                res.Add(tns[o].WrappedObject);
            return res;*/
            return tns;
        }

        public static List<UnitWrapper> GetMeAndAllChildrenWrappersList(UnitWrapper unitWrapper)
        {
            return unitWrapper.WrappedObject.unitTreeNode.BFSListMeAndAllChildrenWrappers();
        }

        public static UnitWrapper GetParentWrapper(UnitWrapper unitWrapper)
        {
            return unitWrapper.WrappedObject.unitTreeNode.GetParentWrapper();
        }

        public static List<UnitWrapper> GetChildrenWrappers(UnitWrapper unitWrapper)
        {
            return unitWrapper.WrappedObject.unitTreeNode.ListChildrenWrappers();
        }


        private Selector GetUsedSelector()
        {
            return SelectionHandler.GetUsedSelector();
        }

        private void Init()
        {
            _myWrapper = new UnitWrapper(this, () => {_myWrapper = null;});
            unitTreeNode = new UnitTreeNode(GetMyWrapper());
            myMover = GetComponent<UnitMover>();
            mySprRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            factionAffiliation = GetComponent<FactionAffiliation>();
            Init();
        }

        public bool subTesting = false;
        private void Update()
        {
            if (IsSelected(GetMyWrapper(), GetUsedSelector())
                && Input.GetKeyDown(KeyCode.K))
            {
                Dismantle(GetMyWrapper());
            }

            DrawUpdate();
        }

        private void DrawUpdate()
        {
            mySprRenderer.color = Color.white;
            
            if (Unit.IsSelected(GetMyWrapper(), GetUsedSelector()))
            {
                mySprRenderer.color = factionAffiliation.MyFaction.baseColor;
            }
            else if (/*Unit.IsSelected(GetMyWrapper(), GetUsedSelector())
                    || */Unit.IsHighlighted(GetMyWrapper(), GetUsedSelector()))
                    // || Unit.IsThereAnyParentHighlightedOrSelected(GetMyWrapper(), GetUsedSelector()))
            {
                mySprRenderer.color = 
                    new Color(factionAffiliation.MyFaction.baseColor.r,
                            factionAffiliation.MyFaction.baseColor.g,
                            factionAffiliation.MyFaction.baseColor.b,
                            factionAffiliation.MyFaction.baseColor.a/2);
            }
        }

    }
}