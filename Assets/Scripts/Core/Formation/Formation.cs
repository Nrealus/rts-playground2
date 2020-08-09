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

    public class Formation
    {
        
        #region Tree structure

        private Dictionary<string, Action> _actDict = new Dictionary<string,Action>();        
        private ObservedValue<Formation> _parent = new ObservedValue<Formation>(null);

        public Formation GetParentFormation()
        {
            return _parent.Value;
        }

        public void Internal_SetParentFormation(Formation newParent)
        {
            _parent.Value = newParent;
        }

        public bool IsRoot()
        {
            return GetParentFormation() == null;
        }

        public bool IsLeaf()
        {
            return GetSubFormations().Count == 0;
        }

        private List<Formation> _childNodes = new List<Formation>();
        public List<Formation> GetSubFormations()
        {
            return _childNodes;
        }

        private void AddSubFormation(Formation child)
        {
            if (child.GetParentFormation() != null)
                child.GetParentFormation().RemoveSubFormation(child);
            child.Internal_SetParentFormation(this);
            GetSubFormations().Add(child);
        }

        private void RemoveSubFormation(Formation child)
        {
            child.Internal_SetParentFormation(null);
            GetSubFormations().Remove(child);
        }

        public void ChangeParentTo(Formation newParent)
        {
            if (newParent != null)
            {
                newParent.AddSubFormation(this);
            }
            else
            {
                if (GetParentFormation() != null)
                    GetParentFormation().RemoveSubFormation(this);
            }
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
            _instcount++;
            reattachFormationToNewParentKey = new StringBuilder("reattachformationtonewparent").Append(_instcount).ToString();

            _unitWrapper = new UnitWrapper(unit);
            GetUnit().SubscribeOnParentChange(reattachFormationToNewParentKey,
            () =>
            {
                ChangeParentTo(GetUnit().GetParentActorAsUnit()?.GetFormation());
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

        public void FormTest()
        {
            var chn = GetSubFormations();
            
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
        
    }

}