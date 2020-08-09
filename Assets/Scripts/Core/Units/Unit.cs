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
    public class Unit : MonoBehaviour, ISelectable, IActor 
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
            UIHandler.GetUIOrdobMenu().UnregisterUnitFromOrdob(this);

            onDestroyed.Invoke();
            
            foreach (var partgrp in new List<UnitGroup>(GetParticipatedGroupsAsUG()))
                partgrp.RemoveUnitFromGroup(this);

            ChangeParentTo(null);
            foreach (var suba in new List<Unit>(GetSubActorsAsUnits()))
                suba.ChangeParentTo(null);

            _nominalFormation = null;
            
            Destroy(gameObject);

            GetOnSelectionStateChangeObserver().UnsubscribeAllEventHandlerMethods();
        }

        #endregion

        #region IActor implementation + related functions

        #region Parent actor

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

        public Unit GetParentActorAsUnit() { return _parent.Value; }

        public IActor GetParentActor() { return _parent.Value; }

        public void Internal_SetParentActor(Unit newParent)
        {
            _parent.Value = newParent;
        }

        public void ChangeParentTo(IActor newParent)
        {
            if (newParent != null)
            {
                (newParent as Unit).Internal_AddSubActor(this);
            }
            else
            {
                if (GetParentActorAsUnit() != null)
                    GetParentActorAsUnit().Internal_RemoveSubActor(this);
            }
        }

        #endregion

        #region Sub actors

        private List<Unit> _subActors = new List<Unit>();

        public List<Unit> GetSubActorsAsUnits()
        {
            return _subActors;
        }

        public List<IActor> GetSubActors()
        {
            return _subActors.ConvertAll(_ => _ as IActor);
        }

        public void Internal_AddSubActor(Unit child)
        {
            if (child.GetParentActorAsUnit() != null)
                child.GetParentActorAsUnit().Internal_RemoveSubActor(child);
            child.Internal_SetParentActor(this);
            GetSubActorsAsUnits().Add(child);
        }

        public void Internal_RemoveSubActor(Unit child)
        {
            child.Internal_SetParentActor(null);
            GetSubActorsAsUnits().Remove(child);
        }

        public void Internal_RemoveSubActors(IEnumerable<Unit> children)
        {
            foreach (Unit child in children)
                Internal_RemoveSubActor(child);
        }
        
        #endregion 

        #region Groups and Plans

        private List<UnitGroup> _groups = new List<UnitGroup>();
        public List<UnitGroup> GetParticipatedGroupsAsUG()
        {
            return _groups;
        }

        public List<IActorGroup> GetParticipatedGroups()
        {
            return _groups.ConvertAll(_ => _ as IActorGroup);
        }

        public List<TaskPlan2> GetParticipatedGroupsPlans()
        {
            List<TaskPlan2> res = new List<TaskPlan2>();
            foreach (var v in GetParticipatedGroups())
            {
                res.AddRange(v.GetThisGroupPlans());
            }
            return res;
        }

        public UnitGroup CreateGroupBySubGroupsUnits(IEnumerable<Unit> units)
        {
            /*UnitGroup res = GetGroupBySubGroupsUnits(units);

            if (res == null)
            {
                res = new UnitGroup(this);
                foreach (var v in units)
                {
                    var cht = v.GenerateGroupFromStructure();
                    cht.ChangeParentTo(res);
                }
            }*/
            UnitGroup res = new UnitGroup();
            res.AddUnitToGroup(this);

            foreach (var v in units)
            {
                var cht = v.GenerateGroupFromStructure();
                cht.ChangeParentTo(res);
            }
            return res;
        }

        public UnitGroup GenerateGroupFromStructure()
        {
            UnitGroup res;
            res = new UnitGroup();
            res.AddUnitToGroup(this);

            foreach (var v in GetSubActorsAsUnits())
            {
                var cht = v.GenerateGroupFromStructure();
                cht.ChangeParentTo(res);
            }
            return res;
        }

        public UnitGroup GenerateGroupFromStructure2()
        {
            UnitGroup res;
            res = new UnitGroup();
            res.AddUnitToGroup(this);

            GenerateGroupFromStructure2_Aux(res, GetSubActorsAsUnits());

            return res;
        }

        private void GenerateGroupFromStructure2_Aux(UnitGroup ug, List<Unit> us)
        {
            if (us.Count > 0)
            {
                UnitGroup subg = new UnitGroup();
                subg.ChangeParentTo(ug);

                foreach (var u in us)
                {
                    subg.AddUnitToGroup(u);
                    GenerateGroupFromStructure2_Aux(subg, u.GetSubActorsAsUnits());
                }
            }
        }

        #endregion       

        #region Additional tree structure functinos

        public bool IsRoot()
        {
            return GetParentActorAsUnit() == null;
        }

        public bool IsLeaf()
        {
            return GetSubActorsAsUnits().Count == 0;
        }

        public Unit GetRootUnit(ref int height)
        {
            if (GetParentActorAsUnit() == null)
                return this;

            height++;
            return GetParentActorAsUnit().GetRootUnit(ref height);
        }

        public Unit GetRootUnit()
        {
            int _useless = 0;
            return GetRootUnit(ref _useless);
        }

        public List<Unit> GetAllAncestors()
        {
            var res = new List<Unit>();
            var p = GetParentActorAsUnit();
            while (p!=null)
            {
               res.Add(p);
               p = p.GetParentActorAsUnit();
            }
            return res;
        }

        public List<Unit> GetAllSubUnitsBFS()
        {
            List<Unit> result = new List<Unit>();

            Queue<Unit> bfsqueue = new Queue<Unit>();
            var ch = GetSubActorsAsUnits();
            foreach (var v in ch)
                bfsqueue.Enqueue(v);

            while (bfsqueue.Count > 0)
            {
                result.Add(bfsqueue.Peek());

                ch = bfsqueue.Dequeue().GetSubActorsAsUnits();
                foreach (var v in ch)
                    bfsqueue.Enqueue(v);
            }

            return result;
        }

        public static Unit GetLowestCommonAncestorUnit(List<Unit> units, bool flattenCopiedInputsToParent)
        {   
            if (units.Count == 1)
                return units[0];

            int currentLowestKnownHeight = 0;
            Unit root = units[0].GetRootUnit(ref currentLowestKnownHeight);

            Unit res = units[0].GetParentActorAsUnit();
            
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
                    res = u.GetParentActorAsUnit();
                }

                var ch = u.GetSubActorsAsUnits();
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
                u.Flatten_Aux(units);
        }

        private void Flatten_Aux(List<Unit> units)
        {
            if (!units.Contains(this))
                return;

            var p = GetParentActorAsUnit();
            if (p != null)
            {
                var siblings = p.GetSubActorsAsUnits();
                if (units.Contains2(siblings))
                {
                    units.Remove2(siblings);
                    units.Add(p);
                    p.Flatten_Aux(units);
                }
            }
        }

        #endregion

        #endregion

        #region Selection related 

        public bool IsSelected(Selector selector)
        {
            return selector.IsSelected(this);
        }

        public bool IsPreselected(Selector selector)
        {
            return selector.IsPreselected(this);
        }

        #endregion

        private Formation _nominalFormation;
        public Formation GetFormation()
        {
            return _nominalFormation;
        }

        public Vector3 GetPosition()
        {
            return myMover.transform.position;
        }
        
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
            
            _nominalFormation = new Formation(this, Formation.FormationRole.MainGuard, Formation.FormationType.Column);
            GetFormation().facingAngle = 0f;
            GetFormation().depthLength = 10f;
            GetFormation().frontLength = 5f;

            UIHandler.GetUIOrdobMenu().RegisterUnitToOrdob(this);

            _parent.Value = null;
            _parent.ForceInvokeOnValueChange();
            //_childNodes = new List<Unit>();

            onSelectionStateChange.SubscribeEventHandlerMethod("unselectparentsorchildren",
                _ =>
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
                            
                            foreach (var chn in GetAllSubUnitsBFS())
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

                            foreach (var chn in GetAllSubUnitsBFS())
                                selector.DeselectEntity(chn, 1);
                        }
                    }
                });

            /*SubscribeOnDestruction("clearnominalformation",
                () => { _nominalFormation = null; });*/
            
            /*SubscribeOnDestruction("clearlocalauxiliaryformations",
                () => { _localAuxiliaryFormations.Clear(); });*/

            /*SubscribeOnDestruction("removefromunittree",
                () =>
                { 
                    ChangeParentTo(null);
                    Internal_RemoveSubActors(GetSubActorsAsUnits());
                });*/
            
            /*SubscribeOnDestruction("removeunitfromordob",
                () => UIHandler.GetUIOrdobMenu().UnregisterUnitFromOrdob(this));*/

        }

        #endregion

        #region Behaviour methods

        private void Awake()
        {
            Init();
        }
        
        private void Update()
        {

            var list = GetSubActorsAsUnits();
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

            if (IsSelected(GetUsedSelector()))
            {
                if (Input.GetKeyDown(KeyCode.K))
                {
                    DestroyThis();
                }
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    foreach (var gr in GetParticipatedGroups())
                    {
                        (gr as UnitGroup).RemoveUnitFromGroup(this);
                    }
                }
                if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    foreach (var pl in GetParticipatedGroupsPlans())
                    {
                        pl.EndPlanExecution();
                    }
                }
            }

            DrawUpdate();

        }

        private void DrawUpdate()
        {
            mySprRenderer.color = Color.white;
            
            if (IsSelected(GetUsedSelector()))
            {
                mySprRenderer.color = factionAffiliation.GetFaction().baseColor;
                foreach (Unit u in GetSubActorsAsUnits())
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
            DrawFormation(GetFormation(), transform.position);
            //foreach (var form in _localAuxiliaryFormations)
            //    DrawFormation(form, transform.position);
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