using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nrealus.Extensions;
using UnityEngine;
using Core.Helpers;
using System;
using Core.Faction;
using Core.Selection;
using Core.Handlers;
using Core.Formations;
using Nrealus.Extensions.Observer;
using Core.Tasks;
using Nrealus.Extensions.ReferenceWrapper;
using Nrealus.Extensions.Tree;
using Gamelogic.Extensions;

namespace Core.Units
{

    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    public class UnitWrapper : RefWrapper<Unit>
    {
        public UnitWrapper(Unit obj) : base(obj)
        { 
            obj.SubscribeOnDestructionAtEnd("destroywrapper", DestroyRef, true);
        }
    }
    
    ///<summary>
    /// Main Unity component for units.
    ///</summary>
    public class Unit : MonoBehaviour, ISelectable, ITaskSubject//, ITreeNodeBase<Unit>
    {

        #region ITaskSubject implementation
        
        #endregion
        
        #region ISelectable implementation

        private EasyObserver<string, (Selector,bool)> onSelectionStateChange = new EasyObserver<string, (Selector, bool)>();

        public EasyObserver<string, (Selector,bool)> GetOnSelectionStateChangeObserver()
        {
            return onSelectionStateChange;
        }
        
        void ISelectable.InvokeOnSelectionStateChange(Selector selector, bool b)
        {
            onSelectionStateChange.Invoke((selector, b));
        }

        #endregion
        
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

            //foreach (var v in unitPiecesWrappersList) 
            //    RemoveUnitPieceFromUnit(v, this);

            onDestroyed.Invoke();

            GetOnSelectionStateChangeObserver().UnsubscribeAllEventHandlerMethods();
        }

        #endregion

        #region Tree Structure

        private Dictionary<string,Action> _actDict = new Dictionary<string,Action>();        
        private ObservedValue<Unit> _parent = new ObservedValue<Unit>(null);

        public void SubscribeOnParentChange(string key, Action action)
        {
            if (!_actDict.ContainsKey(key))
            {
                _actDict.Add(key, action);
                _parent.OnValueChange += action;
            }
        }

        public void UnsubscribeOnParentChange(string key)
        {
            _parent.OnValueChange -= _actDict[key];
        }

        public Unit GetThisNode() { return this; }

        public Unit GetParentNode() { return _parent.Value; }

        public void Internal_SetParentNode(Unit newParent)
        {
            _parent.Value = newParent;
        }

        public bool IsLeaf()
        {
            return GetChildNodes().Count == 0;
        }

        public bool IsRoot()
        {
            return GetParentNode() == null;
        }

        private List<Unit> _childNodes;

        public List<Unit> GetChildNodes()
        {
            return _childNodes;
        }
        
        public void SetChildNodes(List<Unit> childNodes)
        {
            _childNodes = childNodes;
        }

        public List<Unit> GetLeafChildren()
        {
            return GetChildNodes().Where(x => x.IsLeaf()).ToList();
        }

        public List<Unit> GetNonLeafChildren()
        {
            return GetChildNodes().Where(x => !x.IsLeaf()).ToList();
        }

        public Unit GetRootNode()
        {
            if (GetParentNode() == null)
                return GetThisNode();

            return GetParentNode().GetRootNode();
        }

        public void AddChild(Unit child)
        {
            if (child.GetParentNode() != null)
                child.GetParentNode().RemoveChild(child);
            child.Internal_SetParentNode(GetThisNode());
            GetChildNodes().Add(child);
        }

        public void AddChildren(IEnumerable<Unit> children)
        {
            foreach (Unit child in children)
                AddChild(child);
        }

        public void RemoveChild(Unit child)
        {
            child.Internal_SetParentNode(null);
            GetChildNodes().Remove(child);
        }

        public void RemoveChildren(IEnumerable<Unit> children)
        {
            foreach (Unit child in children)
                RemoveChild(child);
        }

        public void ChangeParentTo(Unit newParent)
        {
            if (newParent != null)
            {
                newParent.AddChild(GetThisNode());
                //Unit.WrappedObject.transform.SetParent(newParent.Unit.WrappedObject.transform);
            }
            else
            {
                if (GetParentNode() != null)
                    GetParentNode().RemoveChild(GetThisNode());
                //Unit.WrappedObject.transform.SetParent(null);
            }
        }
        
        private Queue<Unit> _bfsqueue = new Queue<Unit>();
        public List<Unit> BFSList()
        {
            List<Unit> result = new List<Unit>();

            _bfsqueue.Enqueue(GetThisNode());

            while (_bfsqueue.Count > 0)
            {
                result.Add(_bfsqueue.Peek().GetThisNode());

                var a = _bfsqueue.Dequeue().GetChildNodes();
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);

            }

            _bfsqueue.Clear();

            return result;
        }

        #endregion

        private SpriteRenderer mySprRenderer;
        
        [HideInInspector] public FactionAffiliation factionAffiliation;

        public enum UnitLevel { Army, Corps, Brigade, Division, Regiment, Battalion, Company, Platoon/*, Section*/ };
        public enum UnitType { Order, Infantry, Recon, Armored, Artillery, AntiAir };

        private UnitLevel _level;
        private UnitType _type;

        public UnitLevel unitLevel { get { return _level; } /*private*/ set { _level = value; } }
        public UnitType unitType { get { return _type; } private set { _type = value; } }
        
        private Formation formation;        
        public Formation GetFormation()
        {
            return formation;
        }

        /*
        private List<UnitPieceWrapper> unitPiecesWrappersList = new List<UnitPieceWrapper>();
        private Dictionary<UnitPieceWrapper, Action> onUnitPiecesRemovalHandlersDict = new Dictionary<UnitPieceWrapper, Action>();
        
        public static List<UnitPieceWrapper> GetUnitPieceWrappersInUnit(Unit unit)
        {
            return unit.unitPiecesWrappersList;
        }

        public static void AddUnitPieceToUnit(UnitPieceWrapper unitPieceWrapper, Unit unit, Action actionOnRemovalFromGroup)
        {    
            /// Keep an eye
            if (false//unitPieceWrapper.Unit.GetWrappedReference().isVirtualUnit)
            {
                if (unitPieceWrapper.Unit != null)
                {
                    RemoveUnitPieceFromUnit(unitPieceWrapper, unit);
                }
            }
    
            unit.unitPiecesWrappersList.Add(unitPieceWrapper);

            unit.SubscribeOnUnitPieceRemovalFromUnit(unitPieceWrapper, actionOnRemovalFromGroup);

            unitPieceWrapper.SubscribeOnClearance("removeunitpiece",() => RemoveUnitPieceFromUnit(unitPieceWrapper, unit));
            
        }

        public static void RemoveUnitPieceFromUnit(UnitPieceWrapper unitPieceWrapper, Unit unit)
        {
            if (unit != null)
            {
                unitPieceWrapper.UnsubscribeOnClearance("removeunitpiece");
                if(unit.onUnitPiecesRemovalHandlersDict[unitPieceWrapper] != null)
                    unit.onUnitPiecesRemovalHandlersDict[unitPieceWrapper].Invoke();
                else
                    Debug.LogWarning("is this normal ?");
                unit.onUnitPiecesRemovalHandlersDict[unitPieceWrapper] = null;
                unit.onUnitPiecesRemovalHandlersDict.Remove(unitPieceWrapper);
            }
        }*/

        public bool IsSelected(Selector selector)
        {
            return selector.IsSelected(this);
        }

        public bool IsHighlighted(Selector selector)
        {
            return selector.IsHighlighted(this);
        }

        public static Unit CreateUnit(bool isVirtualUnit, bool realOrVirtualGroup)
        {
            throw new NotImplementedException();
        }

        public Vector3 GetPosition()
        {
            return transform.position; //uw.GetWrappedReference().transform.position;
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

        /*public void SubscribeOnUnitPieceRemovalFromUnit(UnitPieceWrapper unitPieceWrapper, Action actionOnUnitPieceRemovalFromUnit)
        {
            Action a;
            if(!onUnitPiecesRemovalHandlersDict.TryGetValue(unitPieceWrapper, out a))
                onUnitPiecesRemovalHandlersDict.Add(unitPieceWrapper, actionOnUnitPieceRemovalFromUnit);
            else
                a += actionOnUnitPieceRemovalFromUnit;
        }*/
    
        private Selector GetUsedSelector()
        {
            return SelectionHandler.GetUsedSelector();
        }
        
        //[SerializeField] private bool _isVirtualUnit;
        //public bool isVirtualUnit { get { return _isVirtualUnit; } private set { _isVirtualUnit = value;} }

        [HideInInspector] public UnitMover myMover;
        //protected Unit _myWrapper;
        //public Unit GetRefWrapper() { return _myWrapper; }

        private void Init(/*bool isVirtualUnit*/)
        {
            factionAffiliation = GetComponent<FactionAffiliation>();
            
            UIHandler.GetUIOrdobMenu().AddUnitToOrdob(this);
            _parent.Value = null;
            
            SubscribeOnDestruction("removefromunittree",
                () =>
                { 
                    ChangeParentTo(null);
                    RemoveChildren(GetChildNodes());
                } );
            
            SubscribeOnDestruction("removeunitfromordob",
                () => UIHandler.GetUIOrdobMenu().RemoveUnitFromOrdob(this));

            _parent.ForceInvokeOnValueChange();
            _childNodes = new List<Unit>();

            formation = new Formation(this, Formation.FormationRole.MainGuard, Formation.FormationType.Column);

            SubscribeOnDestruction("clearformation",
                () => { formation = null; });

            myMover = GetComponent<UnitMover>();
            mySprRenderer = GetComponent<SpriteRenderer>();

            GetFormation().facingAngle = 0f;
            GetFormation().depthLength = 1f;
            GetFormation().frontLength = 2f;

        }

        private void Awake()
        {
            Init(/*isVirtualUnit*/);
        }
        
        private void Update()
        {

            if (IsSelected(GetUsedSelector())
                && Input.GetKeyDown(KeyCode.K))
            {
                DestroyThis();
            }

            DrawUpdate();

        }

        private void DrawUpdate()
        {
            mySprRenderer.color = Color.white;
            
            if (IsSelected(GetUsedSelector()))
            {
                mySprRenderer.color = factionAffiliation.MyFaction.baseColor;
                foreach (Unit u in GetChildNodes())
                {
                    var p = u.GetPosition(); // in the future, take the highest ranked unit in the group
                    Debug.DrawLine(transform.position, p, Color.white);
                }
            }
            else if (IsHighlighted(GetUsedSelector()))
            {
                mySprRenderer.color = 
                    new Color(factionAffiliation.MyFaction.baseColor.r,
                            factionAffiliation.MyFaction.baseColor.g,
                            factionAffiliation.MyFaction.baseColor.b,
                            factionAffiliation.MyFaction.baseColor.a/2);
            }
            
            if (GetFormation() != null)
            {
                Vector3 center = transform.position;

                var frontPoint = center + GetFormation().GetNormalizedFacingVector() * GetFormation().depthLength;
                var rearPoint = center - GetFormation().GetNormalizedFacingVector() * GetFormation().depthLength;

                var lateralDirection = new Vector3(
                    -GetFormation().GetNormalizedFacingVector().z,
                    GetFormation().GetNormalizedFacingVector().y,
                    GetFormation().GetNormalizedFacingVector().x);

                //Debug.DrawLine(frontPoint, rearPoint);
                Debug.DrawLine(frontPoint-lateralDirection*GetFormation().frontLength, frontPoint+lateralDirection*GetFormation().frontLength);
                Debug.DrawLine(frontPoint-lateralDirection*GetFormation().frontLength, rearPoint-lateralDirection*GetFormation().frontLength);
                Debug.DrawLine(frontPoint+lateralDirection*GetFormation().frontLength, rearPoint+lateralDirection*GetFormation().frontLength);
                //Debug.DrawLine(rearPoint-lateralDirection*formation.frontLength, rearPoint+lateralDirection*formation.frontLength);
            }
        }

    }
}