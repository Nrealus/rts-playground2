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
            obj.SubscribeOnDestructionLate("destroywrapper", DestroyRef, true);
        }
    }
    
    ///<summary>
    /// Main Unity component for units.
    ///</summary>
    public class Unit : MonoBehaviour, ISelectable
    {

        private Selector GetUsedSelector()
        {
            return SelectionHandler.GetUsedSelector();
        }
        
        #region ISelectable implementation

        private EasyObserver<string, (Selector,bool,int)> onSelectionStateChange = new EasyObserver<string, (Selector, bool, int)>();

        public EasyObserver<string, (Selector,bool,int)> GetOnSelectionStateChangeObserver()
        {
            return onSelectionStateChange;
        }
        
        void ISelectable.InvokeOnSelectionStateChange(Selector selector, bool newSelectionState, int channel)
        {
            onSelectionStateChange.Invoke((selector, newSelectionState, channel));
        }

        #endregion
        
        #region IDestroyable implementation
        
        private EasyObserver<string> onDestroyed = new EasyObserver<string>();

        public void SubscribeOnDestruction(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action);
        }

        public void SubscribeOnDestructionLate(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action);
        }

        public void SubscribeOnDestruction(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void SubscribeOnDestructionLate(string key, Action action, bool combineActionsIfKeyAlreadyExists)
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

            GetOnSelectionStateChangeObserver().UnsubscribeAllEventHandlerMethods();
        }

        #endregion

        #region Tree structure

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

        private List<Unit> _childNodes = new List<Unit>();

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

        public Unit GetRootNode(ref int height)
        {
            if (GetParentNode() == null)
                return GetThisNode();

            height++;
            return GetParentNode().GetRootNode(ref height);
        }

        public Unit GetRootNode()
        {
            int _useless = 0;
            return GetRootNode(ref _useless);
        }

        public List<Unit> GetAllAncestors()
        {
            var res = new List<Unit>();
            var p = GetParentNode();
            while (p!=null)
            {
               res.Add(p);
               p = p.GetParentNode();
            }
            return res;
        }

        private/*public*/ void AddChild(Unit child)
        {
            if (child.GetParentNode() != null)
                child.GetParentNode().RemoveChild(child);
            child.Internal_SetParentNode(GetThisNode());
            GetChildNodes().Add(child);
        }

        private/*public*/ void AddChildren(IEnumerable<Unit> children)
        {
            foreach (Unit child in children)
                AddChild(child);
        }

        private/*public*/ void RemoveChild(Unit child)
        {
            child.Internal_SetParentNode(null);
            GetChildNodes().Remove(child);
        }

        private/*public*/ void RemoveChildren(IEnumerable<Unit> children)
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

        public List<Unit> GetAllDescendantsBFS()
        {
            List<Unit> result = new List<Unit>();

            Queue<Unit> bfsqueue = new Queue<Unit>();
            var ch = GetChildNodes();
            foreach (var v in ch)
                bfsqueue.Enqueue(v);

            while (bfsqueue.Count > 0)
            {
                result.Add(bfsqueue.Peek().GetThisNode());

                ch = bfsqueue.Dequeue().GetChildNodes();
                foreach (var v in ch)
                    bfsqueue.Enqueue(v);
            }

            return result;
        }

        public Unit GetLowestCommonAncestor(Unit unit)
        {
            return GetLowestCommonAncestor(new List<Unit>(){unit}, false);
        }

        public static Unit GetLowestCommonAncestor(List<Unit> units, bool flattenCopiedInputsToParent)
        {   
            if (units.Count == 1)
                return units[0];

            int currentLowestKnownHeight = 0;
            Unit root = units[0].GetRootNode(ref currentLowestKnownHeight);

            Unit res = units[0].GetParentNode();
            
            if (res == null)
                return null;

            List<Unit> unitsCopy = new List<Unit>(units);
            if (flattenCopiedInputsToParent)
                FlattenUnitsToParentIfAllSiblingsContained(unitsCopy);

            Queue<Unit> bfsqueue = new Queue<Unit>();
            bfsqueue.Enqueue(root);

            Queue<int> heightsqueue = new Queue<int>();
            heightsqueue.Enqueue(0);

            while(bfsqueue.Count > 0)
            {
                var h = heightsqueue.Dequeue();
                var u = bfsqueue.Dequeue();
                if (h < currentLowestKnownHeight && h > 0)
                {
                    currentLowestKnownHeight = h;
                    res = u.GetParentNode();
                }

                var ch = u.GetChildNodes();
                unitsCopy.Remove(u); // implied : if contains u

                if (unitsCopy.Count == 0)
                    return res;

                foreach (var v in ch)
                {
                    bfsqueue.Enqueue(v);
                    heightsqueue.Enqueue(h+1);
                }
            }
            
            return null;
        }

        public static void FlattenUnitsToParentIfAllSiblingsContained(List<Unit> units)
        {
            foreach (var u in new List<Unit>(units))
                Flatten_Aux(units, u);
        }

        private static void Flatten_Aux(List<Unit> units, Unit u)
        {
            if (!units.Contains(u))
                return;

            var p = u.GetParentNode();
            if (p != null)
            {
                var siblings = p.GetChildNodes();
                if (units.Contains2(siblings))
                {
                    units.Remove2(siblings);
                    units.Add(p);
                    Flatten_Aux(units, p);
                }
            }
        }

        #endregion

        #region Public functions 

        public bool IsSelected(Selector selector)
        {
            return selector.IsSelected(this);
        }

        public bool IsPreselected(Selector selector)
        {
            return selector.IsPreselected(this);
        }

        private Formation _nominalFormation;
        public Formation GetNominalFormation()
        {
            return _nominalFormation;
        }

        private List<Formation> _localAuxiliaryFormations = new List<Formation>();
        public List<Formation> GetAllLocalAuxiliaryFormations()
        {
            return _localAuxiliaryFormations;
        }
        public Formation GetLocalAuxiliaryFormationByChildren(IEnumerable<Unit> containedDirectChildrenUnits)
        {
            Formation res = null;
            foreach (var v in _localAuxiliaryFormations)
            {
                Debug.Log("hhhhhhhhhh1");
                if (new HashSet<Unit>(v.GetChildFormations().Select((_) => _.GetUnit())) == new HashSet<Unit>(containedDirectChildrenUnits))
                {
                    Debug.Log("hhhhhhhhhh2");
                    res = v;
                    break;
                }
            }
            return res;
        }

        public Formation AddOrGetLocalAuxiliaryFormation(IEnumerable<Unit> containedDirectChildrenUnits)
        {
            Formation res = GetLocalAuxiliaryFormationByChildren(containedDirectChildrenUnits);

            if (res == null)
            {
                res = new Formation(this);
                res.SubscribeOnDestruction("removelocalauxiliaryformation", () => _localAuxiliaryFormations.Remove(res));
                // res.ChangeParentTo(null); no need to write it for real because it already happens by default.
                // it's a "local" formation
                // for now, no reason why they should have a parent
                // alternative (but it's weird and kind of useless and complicates everything even more) : res.ChangeParentTo(GetParentNode()?.AddOrGetLocalAuxiliaryFormation(new List<Unit>() {this}));

                _localAuxiliaryFormations.Add(res);

                foreach (var u in containedDirectChildrenUnits)
                {
                    //u.GetFormation().ResetFormationComposition();
                    foreach (var ulaf in u.GetAllLocalAuxiliaryFormations())
                    {
                        if (ulaf.GetParentFormation() != res)
                            ulaf.ChangeParentTo(res);
                    }
                    if (u.GetNominalFormation().GetParentFormation() != res)
                        u.GetNominalFormation().ChangeParentTo(res);
                }
            }
            return res;
        }

        public void ResetNominalFormationStructure()
        {
            foreach (var chu in GetChildNodes())
            {
                chu.GetNominalFormation().ChangeParentTo(chu.GetParentNode().GetNominalFormation());
                chu.ResetNominalFormationStructure();
            }
        }

        public Vector3 GetPosition()
        {
            return myMover.transform.position; //uw.GetWrappedReference().transform.position;
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

        #endregion       
        
        #region Main declarations

        private SpriteRenderer mySprRenderer;
        [HideInInspector] public UnitMover myMover;        
        [HideInInspector] public FactionAffiliation factionAffiliation;

        public enum UnitLevel { Army, Corps, Brigade, Division, Regiment, Battalion, Company, Platoon/*, Section*/ };
        public enum UnitType { Order, Infantry, Recon, Armored, Artillery, AntiAir };

        private UnitLevel _level;
        private UnitType _type;

        public UnitLevel unitLevel { get { return _level; } /*private*/ set { _level = value; } }
        public UnitType unitType { get { return _type; } private set { _type = value; } }
        
        #endregion

        #region Initialisation

        private void Init()
        {

            factionAffiliation = GetComponent<FactionAffiliation>();

            myMover = GetComponent<UnitMover>();
            mySprRenderer = GetComponentInChildren<SpriteRenderer>();
            
            UIHandler.GetUIOrdobMenu().AddUnitToOrdob(this);

            _nominalFormation = new Formation(this, Formation.FormationRole.MainGuard, Formation.FormationType.Column);
            GetNominalFormation().facingAngle = 0f;
            GetNominalFormation().depthLength = 10f;
            GetNominalFormation().frontLength = 5f;

            _parent.Value = null;
            _parent.ForceInvokeOnValueChange();
            //_childNodes = new List<Unit>();

            onSelectionStateChange.SubscribeEventHandlerMethod("unselectparentsorchildren",
                (_) =>
                {
                    Selector selector = _.Item1;
                    bool newSelectionState = _.Item2;
                    int channel = _.Item3;

                    if (channel == 0)
                    {
                        if (newSelectionState)
                        {
                            foreach (var ps in GetAllAncestors())
                            {
                                selector.DeselectEntity(ps);
                                selector.DeselectEntity(ps, 1);
                            }

                            /*bool _b1 = true;
                            var _ps = GetParentNode();
                            while (_b1 && _ps != null)
                            {
                                foreach (var sibl in _ps.GetChildNodes())
                                {
                                    if (!sibl.IsSelected(selector))
                                    {
                                        _b1 = false;
                                        break;
                                    }
                                    _b1 = true;
                                }
                                if (_b1)
                                {
                                    selector.SelectEntity(_ps, 1);
                                    _ps = _ps.GetParentNode();
                                }
                            }*/

                            foreach (var chn in GetAllDescendantsBFS())
                            {
                                selector.DeselectEntity(chn);
                                selector.SelectEntity(chn, 1);
                            }

                            selector.SelectEntity(this, 1);
                        }
                        else
                        {
                            selector.DeselectEntity(this, 1);

                            foreach (var ps in GetAllAncestors())
                                selector.DeselectEntity(ps, 1);

                            foreach (var chn in GetAllDescendantsBFS())
                                selector.DeselectEntity(chn, 1);
                        }
                    }
                });

            SubscribeOnDestruction("clearnominalformation",
                () => { _nominalFormation = null; });
            
            SubscribeOnDestruction("clearlocalauxiliaryformations",
                () => { _localAuxiliaryFormations.Clear(); });

            SubscribeOnDestruction("removefromunittree",
                () =>
                { 
                    ChangeParentTo(null);
                    RemoveChildren(GetChildNodes());
                });
            
            SubscribeOnDestruction("removeunitfromordob",
                () => UIHandler.GetUIOrdobMenu().RemoveUnitFromOrdob(this));

        }

        #endregion

        #region Behaviour methods

        private void Awake()
        {
            Init();
        }
        
        private void Update()
        {

            var list = GetChildNodes();
            float c = list.Count;
            if (c > 0)
            {
                Vector3 targetPos = Vector3.zero;
                foreach(var chu in list)
                {
                    targetPos += chu.GetPosition();
                }
                targetPos /= c;
                
                myMover.MoveToPosition(targetPos, 0.5f);
            }

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
                mySprRenderer.color = factionAffiliation.GetFaction().baseColor;
                foreach (Unit u in GetChildNodes())
                {
                    var p = u.GetPosition(); // in the future, take the highest ranked unit in the group
                    Debug.DrawLine(transform.position, p, Color.white);
                }
            }
            else if (IsPreselected(GetUsedSelector()) || GetUsedSelector().IsSelected(this, 1))
            {
                mySprRenderer.color = 
                    new Color(factionAffiliation.GetFaction().baseColor.r,
                            factionAffiliation.GetFaction().baseColor.g,
                            factionAffiliation.GetFaction().baseColor.b,
                            factionAffiliation.GetFaction().baseColor.a/2);
            }
            
            // Likely to change at least slightly when the notion of the "position" of a formation will be clarified
            // Most interesting idea for now : Weighted sum of positions of child formations...
            DrawFormation(GetNominalFormation(), transform.position);
            foreach (var form in _localAuxiliaryFormations)
            {
                DrawFormation(form, transform.position);
            }
        }

        private void DrawFormation(Formation formation, Vector3 center)
        {
            if (formation != null)
            {

                var frontPoint = center + formation.GetFacingVect() * formation.depthLength/2;
                var rearPoint = center - formation.GetFacingVect() * formation.depthLength/2;

                var lateralDirection = formation.GetFacingVectLeftNormal()/2;

                //Debug.DrawLine(frontPoint, rearPoint);
                Debug.DrawLine(frontPoint-lateralDirection*formation.frontLength, frontPoint+lateralDirection*formation.frontLength);
                Debug.DrawLine(frontPoint-lateralDirection*formation.frontLength, rearPoint-lateralDirection*formation.frontLength);
                Debug.DrawLine(frontPoint+lateralDirection*formation.frontLength, rearPoint+lateralDirection*formation.frontLength);
                //Debug.DrawLine(rearPoint-lateralDirection*formation.frontLength, rearPoint+lateralDirection*formation.frontLength);
            }
        }

        #endregion

    }
}