using Core.Orders;
using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Selection;
using Core.Faction;
using GlobalManagers;
using Core.Helpers;

namespace Core.Units
{
    public class Unit : MonoBehaviour,
        IHasRefWrapperAndTree<UnitWrapper>,
        IAreaInfluence
    {

        [HideInInspector] public FactionAffiliation factionAffiliation;

        public enum UnitLevel { Army, Corps, Brigade, Division, Regiment, Battalion, Company, Platoon/*, Section*/ };
        public enum UnitType { Order, Infantry, Recon, Armored, Artillery, AntiAir };

        private UnitLevel _level;
        private UnitType _type;

        public UnitLevel unitLevel { get { return _level; } /*private*/ set { _level = value; } }
        public UnitType unitType { get { return _type; } private set { _type = value; } }

        /*--------*/

        private UnitTreeNode unitTreeNode;

        /*--------*/

        private UnitWrapper _myWrapper;
        public UnitWrapper GetMyWrapper() { return _myWrapper; }
        public void ClearWrapper()
        {
            GetMyWrapper().DestroyWrappedReference();
            _myWrapper = null;
        }

        /*--------*/

        public UnitMover myMover;

        /*--------*/

        private Selector GetUsedSelector()
        {
            return GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector();
        }

        public void Init()
        {
            _myWrapper = new UnitWrapper(this);
            unitTreeNode = new UnitTreeNode(GetMyWrapper());
            myMover = GetComponent<UnitMover>();
        }

        private void Awake()
        {
            factionAffiliation = GetComponent<FactionAffiliation>();
            Init();
        }

        public bool subTesting = false;
        private void Update()
        {

        }

        private void OnDrawGizmos()
        {
            Color color = Color.white;

            if(Application.isPlaying && GetUsedSelector() != null)
            {
                if (Application.isPlaying
                    && GetMyWrapper().IsSelected(GetUsedSelector()))
                {
                    color = factionAffiliation.MyFaction.baseColor;
                }
                else if (Application.isPlaying
                        && (GetMyWrapper().IsHighlighted(GetUsedSelector()) 
                            || GetMyWrapper().IsThereAnyParentHighlightedOrSelected(GetUsedSelector())))
                {
                    color = factionAffiliation.MyFaction.baseColor;
                    color.a /= 2;
                }

                Gizmos.color = color;
                Gizmos.DrawSphere(transform.position, 5f/(int)unitLevel);
            }
        }

        public void Dismantle()
        {
            ClearWrapper();
            Destroy(gameObject);
        }

        public Influence ComputeInfluence()
        {
            return null;
        }

        // Public for testing
        public void SetAsSubordinateOf(Unit newSuperior)
        {
            unitTreeNode.ChangeParentTo(newSuperior.unitTreeNode);
        }

        public List<Unit> GetAllSubordinateUnitsList()
        {
            List<Unit> res = new List<Unit>();
            List<UnitWrapper> tns = unitTreeNode.BFSListMeAndAllChildrenWrappers();
            float c = tns.Count;
            for (int o = 0; o < c; o++)
                res.Add(tns[o].WrappedObject);
            return res;
        }

        public List<UnitWrapper> GetMeAndAllChildrenWrappersList()
        {
            return unitTreeNode.BFSListMeAndAllChildrenWrappers();
        }

        public UnitWrapper GetParentWrapper()
        {
            return unitTreeNode.GetParentWrapper();
        }

        public List<UnitWrapper> GetChildrenWrappers()
        {
            return unitTreeNode.ListChildrenWrappers();
        }

    }
}