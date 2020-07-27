using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nrealus.Extensions.Tree;
using UnityEngine;
using Core.Units;

namespace Core.Formations
{

    public class Formation// : ITreeNodeBase<Formation>
    {
        
        public enum FormationType { Undefined, Line, Column, Column2, Wedge /* etc...?*/ } // Column2 = queue
        public enum FormationRole { Undefined, MainGuard, AdvanceGuard, RearGuard, HQ, CSS, CS/*, ManeuverGroup, etc ...? */ }
        // HQ : Head Quarters, CSS : Combat Service Support, CS : Combat Support

        public FormationType formationType;
        public FormationRole formationRole;

        public float lateralOffsetToParentNominalFraction ; // (Left) -1 --- 0 --- 1 (Right)
        public float depthOffsetToParentNominalFraction ; // (Front) -1 --- 0 --- 1 (Rear)
        public float facingAngle;        
        public float frontLength;
        public float depthLength;

        public Vector3 GetFacingVector()
        {
            return new Vector3(Mathf.Cos(facingAngle*Mathf.Deg2Rad), 0, Mathf.Sin(facingAngle*Mathf.Deg2Rad));
        }

        public Vector3 GetFacingVectorLeftNormal()
        {
            var v = GetFacingVector();
            return new Vector3(-v.z, 0, v.x);
        }

        public Vector3 GetRotatedOffsetVect(Vector3 facing, float lateral, float depth)
        {
            return new Vector3(
                facing.x * depth - facing.z * lateral,
                0,
                facing.z * depth + facing.x * lateral);
        }

        public Vector3 GetRotatedOffsetFractionVect(Vector3 facing)
        {
            return GetRotatedOffsetVect(facing, lateralOffsetToParentNominalFraction, depthOffsetToParentNominalFraction);
        }

        public Vector3 GetAcceptableMovementTargetPosition(Vector3 position)
        {
            if (GetParentFormation() != null)
            {
                return position + GetRotatedOffsetVect(
                    GetParentFormation().GetFacingVector(),
                    lateralOffsetToParentNominalFraction * GetParentFormation().frontLength,
                    depthOffsetToParentNominalFraction* GetParentFormation().depthLength);
            }
            else
            {
                return position;
            }
        }

        private UnitWrapper _unitWrapper;
        public Unit unit { get { return _unitWrapper.Value; } }
        //public List<UnitWrapper> unitsInThisFormation = new List<UnitWrapper>();

        public Formation(Unit unit)
        {
            _unitWrapper = new UnitWrapper(unit);
        }

        public Formation(Unit unit, FormationRole formationRole)
        {
            _unitWrapper = new UnitWrapper(unit);

            this.formationRole = formationRole;
        }

        public Formation(Unit unit, FormationRole formationRole, FormationType formationType)
        {
            _unitWrapper = new UnitWrapper(unit);

            this.formationRole = formationRole;

            this.formationType = formationType;
        }

        #region Tree Structure

        public Formation GetParentFormation()
        {
            if (unit.GetParentNode() != null)
                return unit.GetParentNode().GetFormation();
            return null;
        }

        public bool IsRoot()
        {
            return GetParentFormation() == null;
        }

        public bool IsLeaf()
        {
            return GetChildFormations().Count == 0;
        }

        public List<Formation> GetChildFormations()
        {
            return unit.GetChildNodes().Select((_) => { return _.GetFormation(); } ).ToList();
        }

        public List<Formation> GetSiblingFormations()
        {
            var p = GetParentFormation();
            if (p != null)
                return p.unit.GetFormation().GetChildFormations();
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

        public void Form()
        {
            if (formationType == FormationType.Column)
                FormColumn();
        }

        public void FormTest()
        {
            var chn = GetChildFormations();
            
            var n = chn.Count;

            if (n > 0)
            {
                for(int i = 0; i<n; i++)
                {
                    chn[i].depthOffsetToParentNominalFraction = ((float)(i+1))/((float)n);
                }
            }
        }

        public void FormColumn()
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
                    ch.Item1.depthOffsetToParentNominalFraction = ch.Item2;
                }
            }
        }
   
    }

    public class ColumnDispatcher
    {
        private List<Formation> formationsList;

        public ColumnDispatcher(List<Formation> formationsList)
        {
            this.formationsList = new List<Formation>(formationsList);
        }

        private float Evaluate(Formation form)
        {
            int n = formationsList.Count; // could be needed for more complex evaluation ? (depending on other formations in the list etc... : "multi-agent" ?)
            
            if (form.formationRole == Formation.FormationRole.AdvanceGuard)
                return -1;

            if (form.formationRole == Formation.FormationRole.MainGuard)
                return 0f;

            if (form.formationRole == Formation.FormationRole.HQ)
                return 0.5f;

            if (form.formationRole == Formation.FormationRole.RearGuard)
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
    }

}