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

        public Unit GetParentUnit() { return _parent.Value; }

        public void Internal_SetParentUnit(Unit newParent)
        {
            _parent.Value = newParent;
        }

        public bool IsLeaf()
        {
            return GetSubUnits().Count == 0;
        }

        public bool IsRoot()
        {
            return GetParentUnit() == null;
        }

        private List<Unit> _childNodes = new List<Unit>();

        public List<Unit> GetSubUnits()
        {
            return _childNodes;
        }

        public void SetChildNodes(List<Unit> childNodes)
        {
            _childNodes = childNodes;
        }

        public Unit GetRootUnit(ref int height)
        {
            if (GetParentUnit() == null)
                return this;

            height++;
            return GetParentUnit().GetRootUnit(ref height);
        }

        public Unit GetRootUnit()
        {
            int _useless = 0;
            return GetRootUnit(ref _useless);
        }

        public List<Unit> GetAllAncestors()
        {
            var res = new List<Unit>();
            var p = GetParentUnit();
            while (p!=null)
            {
               res.Add(p);
               p = p.GetParentUnit();
            }
            return res;
        }

        private void AddSubUnit(Unit child)
        {
            if (child.GetParentUnit() != null)
                child.GetParentUnit().RemoveSubUnit(child);
            child.Internal_SetParentUnit(this);
            GetSubUnits().Add(child);
        }

        private void RemoveSubUnit(Unit child)
        {
            child.Internal_SetParentUnit(null);
            GetSubUnits().Remove(child);
        }

        private void RemoveSubUnits(IEnumerable<Unit> children)
        {
            foreach (Unit child in children)
                RemoveSubUnit(child);
        }

        public void ChangeParentTo(Unit newParent)
        {
            if (newParent != null)
            {
                newParent.AddSubUnit(this);
            }
            else
            {
                if (GetParentUnit() != null)
                    GetParentUnit().RemoveSubUnit(this);
            }
        }

        public List<Unit> GetAllSubUnitsBFS()
        {
            List<Unit> result = new List<Unit>();

            Queue<Unit> bfsqueue = new Queue<Unit>();
            var ch = GetSubUnits();
            foreach (var v in ch)
                bfsqueue.Enqueue(v);

            while (bfsqueue.Count > 0)
            {
                result.Add(bfsqueue.Peek());

                ch = bfsqueue.Dequeue().GetSubUnits();
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

            Unit res = units[0].GetParentUnit();
            
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
                    res = u.GetParentUnit();
                }

                var ch = u.GetSubUnits();
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

            var p = GetParentUnit();
            if (p != null)
            {
                var siblings = p.GetSubUnits();
                if (units.Contains2(siblings))
                {
                    units.Remove2(siblings);
                    units.Add(p);
                    p.Flatten_Aux(units);
                }
            }
        }

        #endregion

        #region Basic public functions 

        public bool IsSelected(Selector selector)
        {
            return selector.IsSelected(this);
        }

        public bool IsPreselected(Selector selector)
        {
            return selector.IsPreselected(this);
        }

        private Formation _nominalFormation;
        public Formation GetFormation()
        {
            return _nominalFormation;
        }

        private List<TaskPlan2> _plans = new List<TaskPlan2>();
        public List<TaskPlan2> GetPlans()
        {
            return _plans;
        }

        /*public UnitTeam GetTeamBySubTeamsUnits(List<Unit> units)
        {
            UnitTeam res = null;
            foreach (var v in GetPlans())
            {
                Debug.Log("hhhhhhhhhh1");
                var team = v.GetSubject() as UnitTeam;
                if (team.GetSubTeams().Select((_) => _.GetUnit()).ToList().EqualContentUnordered(units))
                {
                    Debug.Log("hhhhhhhhhh2");
                    res = team;
                    break;
                }
            }
            return res;
        }*/

        //public UnitTeam AddOrGetTeamBySubTeamsUnits(IEnumerable<Unit> units)
        public UnitTeam CreateTeamBySubTeamsUnits(IEnumerable<Unit> units)
        {
            /*UnitTeam res = GetTeamBySubTeamsUnits(units);

            if (res == null)
            {
                res = new UnitTeam(this);
                foreach (var v in units)
                {
                    var cht = v.GenerateTeamFromStructure();
                    cht.ChangeParentTo(res);
                }
            }*/
            UnitTeam res = new UnitTeam(this);
            foreach (var v in units)
            {
                var cht = v.GenerateTeamFromStructure();
                cht.ChangeParentTo(res);
            }
            return res;
        }

        public UnitTeam GenerateTeamFromStructure()
        {
            UnitTeam res;
            res = new UnitTeam(this);
            foreach (var v in GetSubUnits())
            {
                var cht = v.GenerateTeamFromStructure();
                cht.ChangeParentTo(res);
            }
            return res;
        }

        public Vector3 GetPosition()
        {
            return myMover.transform.position;
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
            GetFormation().facingAngle = 0f;
            GetFormation().depthLength = 10f;
            GetFormation().frontLength = 5f;

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

            SubscribeOnDestruction("clearnominalformation",
                () => { _nominalFormation = null; });
            
            /*SubscribeOnDestruction("clearlocalauxiliaryformations",
                () => { _localAuxiliaryFormations.Clear(); });*/

            SubscribeOnDestruction("removefromunittree",
                () =>
                { 
                    ChangeParentTo(null);
                    RemoveSubUnits(GetSubUnits());
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

            var list = GetSubUnits();
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
                foreach (Unit u in GetSubUnits())
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