using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nrealus.Extensions.Tree;
using UnityEngine;
using Core.Units;
using System;
using Gamelogic.Extensions;
using Core.Tasks;
using Nrealus.Extensions.Observer;
using System.Text;

namespace Core.Formations
{

    public class Formation : ITaskSubject
    {
        
        #region ITaskSubject implementation

        private List<TaskPlan2> _localPlans = new List<TaskPlan2>();
        private List<TaskPlan2> plans
        {
            get
            {
                if (IsNominalFormation())
                    return _localPlans;
                else
                    return GetUnit().GetNominalFormation()._localPlans;
            }
        }
        
        private static int _instcount2;
        private Dictionary<TaskPlan2,string> removePlanKeyDict = new Dictionary<TaskPlan2, string>();
        public TaskPlan2 AddNewPlan()
        {
            _instcount2++;

            TaskPlan2 res = null;

            res = new TaskPlan2(this);
            plans.Add(res);

            if (!IsNominalFormation())
                _localPlans.Add(res);            

            removePlanKeyDict.Add(res, new StringBuilder("removeplan").Append(_instcount2).ToString());
            SubscribeOnDestruction(removePlanKeyDict[res], () => RemoveAndEndPlan(res));

            return res;
        }

        public void RemoveAndEndPlan(TaskPlan2 taskPlan)
        {
            if (plans.Remove(taskPlan))
            {
                if (!IsNominalFormation())
                    _localPlans.Remove(taskPlan);            
                
                taskPlan.EndPlanExecution();
                UnsubscribeOnDestruction(removePlanKeyDict[taskPlan]);

                if (_localPlans.Count == 0)
                    DestroyThis();
            }
        }

        public IEnumerable<TaskPlan2> GetPlans()
        {
            return plans;
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
            onDestroyed.Invoke();
        }

        #endregion

        #region Tree structure

        private Dictionary<string, Action> _actDict = new Dictionary<string,Action>();        
        //private Stack<ObservedValue<Formation>> _parents = new Stack<ObservedValue<Formation>>();
        private ObservedValue<Formation> _parent = new ObservedValue<Formation>(null);

        /*public void SubscribeOnParentChange(string key, Action action)
        {
            if (!_actDict.ContainsKey(key))
            {
                _actDict.Add(key, action);
                _parents[0].OnValueChange += action;
            }
        }

        public void UnsubscribeOnParentChange(string key)
        {
            _parent[0].OnValueChange -= _actDict[key];
        }*/

        public Formation GetParentFormation()
        {
            //return _parents.Peek().Value;
            return _parent.Value;
            /*if (unit.GetParentNode() != null)
                return unit.GetParentNode().GetFormation();
            return null;*/
        }

        public void Internal_SetParentNode(Formation newParent)
        {
            /*var onp = new ObservedValue<Formation>(newParent);
            _parents.Push(onp);*/
            _parent.Value = newParent;
            /*onp.OnValueChange += 
            _parents.Insert(0, onp);*/
        }

        public bool IsRoot()
        {
            return GetParentFormation() == null;
        }

        public bool IsLeaf()
        {
            return GetChildFormations().Count == 0;
        }

        private List<Formation> _childNodes = new List<Formation>();
        public List<Formation> GetChildFormations()
        {
            return _childNodes;
            //return unit.GetChildNodes().Select((_) => { return _.GetFormation(); } ).ToList();
        }

        private/*public*/ void AddChild(Formation child)
        {
            if (child.GetParentFormation() != null)
                child.GetParentFormation().RemoveChild(child);
            child.Internal_SetParentNode(this);
            GetChildFormations().Add(child);
        }

        private/*public*/ void AddChildren(IEnumerable<Formation> children)
        {
            foreach (Formation child in children)
                AddChild(child);
        }

        private/*public*/ void RemoveChild(Formation child)
        {
            child.Internal_SetParentNode(null);
            GetChildFormations().Remove(child);
        }

        private/*public*/ void RemoveChildren(IEnumerable<Formation> children)
        {
            foreach (Formation child in children)
                RemoveChild(child);
        }

        public void ChangeParentTo(Formation newParent)
        {
            if (newParent != null)
            {
                newParent.AddChild(this);
            }
            else
            {
                if (GetParentFormation() != null)
                    GetParentFormation().RemoveChild(this);
            }
        }

        public List<Formation> GetSiblingFormations()
        {
            var p = GetParentFormation();
            if (p != null)
                return /*p.GetUnit().GetFormation()*/p.GetChildFormations();
            var res = new List<Formation>();
            res.Add(this);
            return res;
        }

        public List<Formation> GetLeafChildren()
        {
            return GetChildFormations().Where(x => x.IsLeaf()).ToList();
        }

        public List<Formation> GetNonLeafChildren()
        {
            return GetChildFormations().Where(x => !x.IsLeaf()).ToList();
        }

        public Formation GetRootNode()
        {
            if (GetParentFormation() == null)
                return this;

            return GetParentFormation().GetRootNode();
        }
        
        private Queue<Formation> _bfsqueue = new Queue<Formation>();
        public List<Formation> BFSList()
        {
            List<Formation> result = new List<Formation>();

            _bfsqueue.Enqueue(this);

            while (_bfsqueue.Count > 0)
            {
                result.Add(_bfsqueue.Peek());

                var a = _bfsqueue.Dequeue().GetChildFormations();
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);
            }

            _bfsqueue.Clear();

            return result;
        }

        #endregion

        #region Main declarations

        public enum FormationType { Undefined, Line, Column, Column2, Wedge /* etc...?*/ } // Column2 = queue
        public enum FormationRole { Undefined, MainGuard, AdvanceGuard, RearGuard, HQ, CSS, CS/*, ManeuverGroup, etc ...? */ }
        // HQ : Head Quarters, CSS : Combat Service Support, CS : Combat Support

        public FormationType formationType;
        public FormationRole formationRole;

        public float lateralOffsetToParentNominalFraction; // (Left) -1 --- 0 --- 1 (Right)
        public float depthOffsetToParentNominalFraction; // (Rear) -1 --- 0 --- 1 (Front)
        public float facingAngle;
        public float frontLength;
        public float depthLength;

        #endregion

        private UnitWrapper _unitWrapper;
        public Unit GetUnit() { return _unitWrapper.Value; }

        public bool IsNominalFormation()
        {
            return GetUnit().GetNominalFormation() != this;
        }

        #region Formation shape functions

        public Vector3 GetFacingVect()
        {
            return new Vector3(Mathf.Cos(facingAngle*Mathf.Deg2Rad), 0, Mathf.Sin(facingAngle*Mathf.Deg2Rad));
        }

        public Vector3 GetFacingVectLeftNormal()
        {
            var v = GetFacingVect();
            return new Vector3(-v.z, 0, v.x);
        }

        public Vector3 GetRotatedOffsetVect(Vector3 facing, float lateral, float depth)
        {
            return new Vector3(
                facing.x * depth - facing.z * lateral,
                0,
                facing.z * depth + facing.x * lateral);
        }

        /*public Vector3 GetRotatedOffsetFractionVect(Vector3 facing)
        {
            return GetRotatedOffsetVect(facing, lateralOffsetToParentNominalFraction, depthOffsetToParentNominalFraction);
        }*/

        public Vector3 GetAcceptableMovementTargetPosition(Vector3 position)
        {
            if (GetParentFormation() != null)
            {
                return position + GetRotatedOffsetVect(
                    GetParentFormation().GetFacingVect(),
                    lateralOffsetToParentNominalFraction * GetParentFormation().frontLength/2,
                    depthOffsetToParentNominalFraction * GetParentFormation().depthLength/2);
            }
            else
            {
                return position;
            }
        }

        #endregion

        #region Initialisation

        private static int _instcount;
        private string reattachFormationToNewParentKey;
        private void InitUnit(Unit unit)
        {
            //_parents.Push(new ObservedValue<Formation>(null));

            _instcount++;
            reattachFormationToNewParentKey = new StringBuilder("reattachformationtonewparent").Append(_instcount).ToString();

            _unitWrapper = new UnitWrapper(unit);
            GetUnit().SubscribeOnParentChange(reattachFormationToNewParentKey,
            () =>
            {
                //if (GetUnit().GetParentNode() != null)
                {
                    if (IsNominalFormation())
                        ChangeParentTo(GetUnit().GetParentNode()?.GetNominalFormation());
                    else
                    {
                        ChangeParentTo(GetUnit().GetParentNode()?.GetLocalAuxiliaryFormationByChildren(new List<Unit>() { GetUnit() }));
                    }
                }
            });
            
            //SubscribeOnDestruction("unsubreattachtonewparent", () => GetUnit().UnsubscribeOnDestruction(reattachFormationToNewParentKey));
        }
        
        public Formation(Unit unit)
        {
            InitUnit(unit);
        }

        public Formation(Unit unit, FormationRole formationRole)
        {
            InitUnit(unit);

            this.formationRole = formationRole;
        }

        public Formation(Unit unit, FormationRole formationRole, FormationType formationType)
        {
            InitUnit(unit);

            this.formationRole = formationRole;

            this.formationType = formationType;
        }

        #endregion

        /*public void ResetFormationComposition()
        {
            foreach (var v in new List<Formation>(GetChildFormations()))
            {
                v.ChangeParentTo(v.GetUnit().GetParentNode()?.GetFormation());
            }
            foreach (var v in GetUnit().GetChildNodes())
            {
                v.GetFormation().ChangeParentTo(this/*GetUnit().GetFormation()*);
                v.GetFormation().ResetFormationComposition();
            }
        }*/

        public void FormTest()
        {
            var chn = GetChildFormations();
            
            var n = chn.Count;

            if (n > 1)
            {
                for(int i = 0; i<n; i++)
                {
                    chn[i].lateralOffsetToParentNominalFraction = 0f;
                    chn[i].depthOffsetToParentNominalFraction = 1 - 2f*((float)i)/((float)Mathf.Max(1,n-1));
                }
            }
        }

        /*public void FormColumn()
        {
            var chn = GetChildFormations(); // 

            var n = chn.Count;

            if (n > 0)
            {
                ColumnDispatcher comparer = new ColumnDispatcher(chn);

                var sortedChn = comparer.Output();

                // basically greedy algorithm for optimisation.
                // Possible use of something more elaborate ? (dyn. prog., linear optimisation techniques, bab...)
                // (future ai considerations....)

                for(int i = 0; i<n; i++)
                {
                    (Formation,float) ch = sortedChn[i];
                    ch.Item1.GetFormationData().depthOffsetToParentNominalFraction = ch.Item2;
                }
            }
        }*/
   
    }

    /*public class ColumnDispatcher
    {
        private List<Formation> formationsList;

        public ColumnDispatcher(List<Formation> formationsList)
        {
            this.formationsList = new List<Formation>(formationsList);
        }

        private float Evaluate(Formation form)
        {
            int n = formationsList.Count; // could be needed for more complex evaluation ? (depending on other formations in the list etc... : "multi-agent" ?)
            
            if (form.GetFormationData().formationRole == Formation.GetFormationData().FormationRole.AdvanceGuard)
                return -1;

            if (form.GetFormationData().formationRole == Formation.GetFormationData().FormationRole.MainGuard)
                return 0f;

            if (form.GetFormationData().formationRole == Formation.GetFormationData().FormationRole.HQ)
                return 0.5f;

            if (form.GetFormationData().formationRole == Formation.GetFormationData().FormationRole.RearGuard)
                return 1f;

            return 0;
        }

        public List<(Formation,float)> Output()
        {
            List<(Formation,float)> res = new List<(Formation,float)>();
            
            foreach (var f in formationsList)
            {
                res.Add((f,Evaluate(f)));
            }

            return res;
        }
    }*/

}