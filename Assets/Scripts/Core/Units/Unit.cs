using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VariousUtilsExtensions;
using UnityEngine;
using Core.Helpers;
using System;
using Core.Faction;
using Core.Selection;
using Core.Handlers;

namespace Core.Units
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// This class represents a unit.
    /// There can be real and virtual units. Virtual units are just a way to group/bundle other units into one group.
    ///</summary>
    public class Unit : MonoBehaviour, IHasRefWrapper<UnitWrapper>
    {
    
        private SpriteRenderer mySprRenderer;
        
        [HideInInspector] public FactionAffiliation factionAffiliation;

        public enum UnitLevel { Army, Corps, Brigade, Division, Regiment, Battalion, Company, Platoon/*, Section*/ };
        public enum UnitType { Order, Infantry, Recon, Armored, Artillery, AntiAir };

        private UnitLevel _level;
        private UnitType _type;

        public UnitLevel unitLevel { get { return _level; } /*private*/ set { _level = value; } }
        public UnitType unitType { get { return _type; } private set { _type = value; } }
        
        private List<UnitPieceWrapper> unitPiecesWrappersList = new List<UnitPieceWrapper>();
        private Dictionary<UnitPieceWrapper, Action> onUnitPiecesRemovalHandlersDict = new Dictionary<UnitPieceWrapper, Action>();
		
        public static List<UnitPieceWrapper> GetUnitPieceWrappersInUnit(UnitWrapper unitWrapper)
        {
            return unitWrapper.GetWrappedReference().unitPiecesWrappersList;
        }
        
        public static List<UnitWrapper> GetMyselfAndSubUnitsWrappers(UnitWrapper unitWrapper)
        {
            // very clunky, to be changed
            var res = new List<UnitWrapper>();
            res.Add(unitWrapper);
            res.AddRange(unitWrapper.GetChildNodes());
            return res;
        }

        public static List<UnitWrapper> GetSubUnits(UnitWrapper unitWrapper)
        {
            return unitWrapper.GetChildNodes();
        }
        
        public static void AddSubUnitToUnit(UnitWrapper unitToAdd, UnitWrapper destinationParentUnit)
        {    
            destinationParentUnit.AddChild(unitToAdd);            
        }
        
        public static void DettachSubUnitFromItsParent(UnitWrapper unitToDettachFromItsParent)
        {
            if (unitToDettachFromItsParent.GetParentNode() != null)
                unitToDettachFromItsParent.GetParentNode().RemoveChild(unitToDettachFromItsParent);
            else
                Debug.LogWarning("already dettached");
        }

        public static void AddUnitPieceToUnit(UnitPieceWrapper unitPieceWrapper, UnitWrapper unitWrapper, Action actionOnRemovalFromGroup)
        {    
            /// Keep an eye
            if (unitPieceWrapper.unitWrapper.GetWrappedReference().isVirtualUnit)
            {
                if (unitPieceWrapper.unitWrapper != null)
                {
                    RemoveUnitPieceFromUnit(unitPieceWrapper, unitPieceWrapper.unitWrapper);
                }
            }
    
            unitWrapper.GetWrappedReference().unitPiecesWrappersList.Add(unitPieceWrapper);

            unitWrapper.GetWrappedReference().SubscribeOnUnitPieceRemovalFromUnit(unitPieceWrapper, actionOnRemovalFromGroup);

            unitPieceWrapper.SubscribeOnClearance("removeunitpiece",() => RemoveUnitPieceFromUnit(unitPieceWrapper, unitWrapper));
            
        }

        public static void RemoveUnitPieceFromUnit(UnitPieceWrapper unitPieceWrapper, UnitWrapper unitWrapper)
        {
            if (unitWrapper.GetWrappedReference() != null)
            {
                unitPieceWrapper.UnsubscribeOnClearance("removeunitpiece");
                if(unitWrapper.GetWrappedReference().onUnitPiecesRemovalHandlersDict[unitPieceWrapper] != null)
                    unitWrapper.GetWrappedReference().onUnitPiecesRemovalHandlersDict[unitPieceWrapper].Invoke();
                else
                    Debug.LogWarning("is this normal ?");
                unitWrapper.GetWrappedReference().onUnitPiecesRemovalHandlersDict[unitPieceWrapper] = null;
                unitWrapper.GetWrappedReference().onUnitPiecesRemovalHandlersDict.Remove(unitPieceWrapper);
            }
        }

        public static bool IsSelected(UnitWrapper unitWrapper, Selector selector)
        {
            return selector.IsSelected(unitWrapper);
        }

        public static bool IsHighlighted(UnitWrapper unitWrapper, Selector selector)
        {
            return selector.IsHighlighted(unitWrapper);
        }

        public static Unit CreateUnit(bool isVirtualUnit, bool realOrVirtualGroup)
        {
            throw new NotImplementedException();
        }

        public static void DestroyUnit(UnitWrapper unitWrapper)
        {
            Destroy(unitWrapper.GetWrappedReference().gameObject);
            unitWrapper.DestroyWrappedReference();
        }

        public static Vector3 GetPosition(UnitWrapper uw)
        {
            return uw.GetWrappedReference().transform.position;
            /*var l = uw.WrappedObject.unitPiecesWrappersList;
            int c = l.Count;
            Vector3 res = Vector3.zero;

            if (c == 0)
                throw new SystemException("what");

            foreach(var v in l)
            {
                res += v.WrappedObject.transform.position;
            }
            res /= c;
            return res;*/
        }

        public void SubscribeOnUnitPieceRemovalFromUnit(UnitPieceWrapper unitPieceWrapper, Action actionOnUnitPieceRemovalFromUnit)
        {
            Action a;
            if(!onUnitPiecesRemovalHandlersDict.TryGetValue(unitPieceWrapper, out a))
                onUnitPiecesRemovalHandlersDict.Add(unitPieceWrapper, actionOnUnitPieceRemovalFromUnit);
            else
                a += actionOnUnitPieceRemovalFromUnit;
        }
       
        private Selector GetUsedSelector()
        {
            return SelectionHandler.GetUsedSelector();
        }
        
        [SerializeField] private bool _isVirtualUnit;
        public bool isVirtualUnit { get { return _isVirtualUnit; } private set { _isVirtualUnit = value;} }

        [HideInInspector] public UnitMover myMover;
        protected UnitWrapper _myWrapper;
        public UnitWrapper GetRefWrapper() { return _myWrapper; }

        private void Init(bool isVirtualUnit)
        {
            factionAffiliation = GetComponent<FactionAffiliation>();
            
            this.isVirtualUnit = isVirtualUnit;
            
            _myWrapper = new UnitWrapper(this, () => 
                {
                    foreach (var v in unitPiecesWrappersList) 
                        RemoveUnitPieceFromUnit(v, GetRefWrapper());
                    _myWrapper = null;
                });
            myMover = GetComponent<UnitMover>();
            mySprRenderer = GetComponent<SpriteRenderer>();            
        }

        private void Awake()
        {
            Init(isVirtualUnit);
        }
        
        private void Update()
        {

            if (GetUsedSelector().IsSelected(GetRefWrapper())
                && Input.GetKeyDown(KeyCode.K))
            {
                DestroyUnit(GetRefWrapper());
            }

            DrawUpdate();

        }

        private void DrawUpdate()
        {
            mySprRenderer.color = Color.white;
            
            if (IsSelected(GetRefWrapper(), GetUsedSelector()))
            {
                mySprRenderer.color = factionAffiliation.MyFaction.baseColor;
                foreach (UnitWrapper uw in GetSubUnits(GetRefWrapper()))
                {
                    var p = Unit.GetPosition(uw); // in the future, take the highest ranked unit in the group
                    Debug.DrawLine(transform.position, p, Color.white);
                }
            }
            else if (IsHighlighted(GetRefWrapper(), GetUsedSelector()))
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