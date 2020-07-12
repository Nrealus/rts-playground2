using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using NPBehave;
using Core.MapMarkers;
using Core.Handlers;
using Core.Deployables;
using Core.Helpers;

namespace Core.Tasks
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// OUTDATED.
    /// This "active" order allows units to build structures. As most orders, it isn't really finished but the necessary outline is there.
    /// It needs a BuildingMarker (or rather its MapMarkerWrapper)
    /// The BuildOrder is actually the first test of "internalized OrderPlans". It can create and run a MoveTask to get closer to the building marker "inside of itself".
    /// See OrderPlan summary for more details.
    /// </summary>
    public class BuildTask : Task2, IHasRefWrapper<TaskWrapper<BuildTask>>
    {

        public new TaskWrapper<BuildTask> GetRefWrapper()
        {
            return _myWrapper as TaskWrapper<BuildTask>;
        }

        private MapMarkerWrapper<DeployableMarker> _buildingMarkerWrapper;
        private MapMarkerWrapper<DeployableMarker> buildingMarkerWrapper
        {
            get
            {
                if(DeployableStructureWrapper != null && DeployableStructureWrapper.GetWrappedReference() != null)
                    return DeployableStructureWrapper.buildingMarkerWrapper;
                else
                    return _buildingMarkerWrapper;
            }

            set
            {
                _buildingMarkerWrapper = value;
            }
        }

        private DeployableStructureWrapper DeployableStructureWrapper;
        
        private TaskParams myParameters = TaskParams.DefaultParam();

        private TaskWrapper<MoveTask> predecessorMoveOrderWrapper = null;

        private List<MapMarkerWrapper<WaypointMarker>> waypointMarkerWrappersList = new List<MapMarkerWrapper<WaypointMarker>>();

        private UnitWrapper unitWrapper;
        private class UnitWrapperExecutionState
        {
            public float materials = 150f;

            public int currentWaypointIndex;
            public bool endedPath;

            public UnitWrapper thisUnitsSubUGW;

            public TaskPlan internalOrdersPlan;

            public UnitWrapperExecutionState(UnitWrapper unitWrapper)
            {
                thisUnitsSubUGW = Unit.CreateUnit(false, false).GetRefWrapper();
                //Unit.AddUnitPieceToUnit(unitWrapper, thisUnitsSubUGW/*, () => { thisUnitsSubUGW = null; }*/);
                Unit.AddSubUnitToUnit(unitWrapper, thisUnitsSubUGW/*, () => { thisUnitsSubUGW = null; }*/);

                internalOrdersPlan = thisUnitsSubUGW.GetTaskPlan();

            }

            public void Destroy()
            {
                if (internalOrdersPlan != null)
                {
                    thisUnitsSubUGW.DestroyWrappedReference();
                    internalOrdersPlan.Clear();
                    internalOrdersPlan = null;
                }
            }

        }
        private Dictionary<UnitWrapper, UnitWrapperExecutionState> currentExecutionStatePerUnit = new Dictionary<UnitWrapper, UnitWrapperExecutionState>();

        protected override ITaskSubject InstanceGetSubject()
        {
            return unitWrapper;
        }
        
        protected override void InstanceSetSubject(ITaskSubject subject, TaskWrapper predecessor, TaskWrapper successor)
        {
           
            /*unitsGroupWrapper = orderable as UnitGroupWrapper;
            unitsGroupWrapper.GetTaskPlan().QueueActiveOrderToPlan(GetMyWrapper(), predecessor, successor);

            foreach (var uw in UnitGroup.GetUnitWrappersInFormation(unitsGroupWrapper))
            {
                currentExecutionStatePerUnit.Add(uw, new UnitWrapperExecutionState(uw));
                uw.unitsGroupWrapper.WrappedObject.SubscribeOnRemovalFromFormation(uw, () => currentExecutionStatePerUnit.Remove(uw));
                uw.SubscribeOnClearance(() => currentExecutionStatePerUnit.Remove(uw));
                GetMyWrapper().SubscribeOnClearance(() => currentExecutionStatePerUnit.Remove(uw));
            }

            SetPhase(GetMyWrapper(), OrderPhase.Staging);*/
            
            unitWrapper = subject as UnitWrapper;
            unitWrapper.GetTaskPlan().QueueActiveOrderToPlan(GetRefWrapper(), predecessor, successor);

            //foreach (var uw in Unit.GetUnitPieceWrappersInUnit(unitWrapper))
            foreach (var uw in Unit.GetMyselfAndSubUnitsWrappers(unitWrapper))
            {
                currentExecutionStatePerUnit.Add(uw, new UnitWrapperExecutionState(uw));
                /*uw.unitWrapper.WrappedObject.SubscribeOnRemovalFromGroup(uw, () =>
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });*/

                uw.SubscribeOnClearance(() =>
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });
                GetRefWrapper().SubscribeOnClearance(() => 
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });
            }

            SetPhase(GetRefWrapper(), OrderPhase.Staging);

        }

        protected override TaskParams InstanceGetParameters()
        {
            return myParameters;
        }

        public BuildTask()
        {
            _myWrapper = new TaskWrapper<BuildTask>(this, () => {_myWrapper = null;});

            CreateAndInitFSM();
        } 

        #region Specific behaviour logic

        public void SetBuildingToBuild(MapMarkerWrapper<DeployableMarker> buildingMarkerWrapperArg)
        {
            GetRefWrapper().SubscribeOnClearance(
                () => { 
                    if (buildingMarkerWrapperArg != null && ((buildingMarkerWrapperArg.GetWrappedReference().DeployableStructureWrapper != null
                    && DeployableStructure.GetHP(buildingMarkerWrapperArg.GetWrappedReference().DeployableStructureWrapper) == 0)
                        || buildingMarkerWrapperArg.GetWrappedReference().DeployableStructureWrapper == null))
                        buildingMarkerWrapperArg.DestroyWrappedReference(); 
                });
            this.buildingMarkerWrapper = buildingMarkerWrapperArg;
            this.buildingMarkerWrapper.SubscribeOnClearance(() => this.buildingMarkerWrapper = null);
        }

        
        private bool _endedPathForAll;
        protected override void UpdateExecution()
        {
            _endedPathForAll = true;
            //foreach (var uw in Unit.GetUnitPieceWrappersInUnit(unitWrapper))
            foreach (var uw in Unit.GetMyselfAndSubUnitsWrappers(unitWrapper))
            {
                if (IsInPhase(GetRefWrapper(), OrderPhase.Execution))
                {
                    if (SubjectExists(GetRefWrapper()))
                    {
                        if (!CloseEnoughToBuildingMarker(uw))
                        {
                            if (currentExecutionStatePerUnit[uw].internalOrdersPlan.GetFirstInlineActiveTaskInPlan() == null)
                            {
                                //var intermediateMove = OrderFactory.CreateOrderWrapperWithoutReceiver<MoveOrderNew>();
                                var intermediateMove = Task.CreateTaskWrapperAndSetReceiver<MoveTask>(currentExecutionStatePerUnit[uw].thisUnitsSubUGW);
                                
                                //OrderHandler.AddToGlobalOrderWrapperList(intermediateMove);
                                //SetReceiver(intermediateMove, null, null, uw);

                                //intermediateMove.SubscribeOnClearance(() => intermediateMove = null);
                    
                                intermediateMove.GetWrappedReference().AddWaypoint(
                                    (WaypointMarker.CreateInstance(
                                        buildingMarkerWrapper.GetWrappedReference().transform.position
                                        + 2*(UnityEngine.Random.Range(0.5f,1)*Vector3.right+UnityEngine.Random.Range(0.5f,1)*Vector3.forward).normalized)
                                    ).GetRefWrapper());

                                //currentExecutionStatePerUnit[uw].internalOrdersPlan.QueueActiveOrderToPlan(intermediateMove, null, null);

                                TryStartExecution(intermediateMove);
                            }
                            else
                            {

                            }
                            _endedPathForAll = false;
                        }
                        else if (CloseEnoughToBuildingMarker(uw) && CanActuallyBuild(uw))
                        {
                            ActuallyBuild(uw);
                            _endedPathForAll = false;
                        }
                    }
                }
            }

            if (_endedPathForAll)
                EndExecution(GetRefWrapper());

        }

        private bool CloseEnoughToBuildingMarker(UnitWrapper unitWrapper)
        {
            if (buildingMarkerWrapper != null
                && Vector3.Distance(buildingMarkerWrapper.GetWrappedReference().transform.position, unitWrapper.GetWrappedReference().myMover.GetPosition()) <= 2.5f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CanActuallyBuild(UnitWrapper unitWrapper)
        {
            if (buildingMarkerWrapper != null
                && currentExecutionStatePerUnit[unitWrapper].materials > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool FinishedBuilding(UnitWrapper unitWrapper)
        {
            if (currentExecutionStatePerUnit[unitWrapper].materials < 0 || DeployableStructure.GetHP(DeployableStructureWrapper) >= 100)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private float buildingRate = 0.15f;
        private void ActuallyBuild(UnitWrapper unitWrapper)
        {
            if (DeployableStructureWrapper == null && buildingMarkerWrapper != null)
            {
                DeployableStructureWrapper = buildingMarkerWrapper.GetWrappedReference().CreateAndSetDeployableStructure<DeployableStructure>();
                GetRefWrapper().SubscribeOnClearance(() => DeployableStructureWrapper = null);
            }

            //_previousmaterials = _materials;
            currentExecutionStatePerUnit[unitWrapper].materials = Mathf.Max(-1, currentExecutionStatePerUnit[unitWrapper].materials - buildingRate);

            if (currentExecutionStatePerUnit[unitWrapper].materials > 0)
            {
                if (Mathf.Abs(currentExecutionStatePerUnit[unitWrapper].materials % 30) < 1)
                    Debug.Log(unitWrapper + " : Building structure...");
                DeployableStructure.AddHP(DeployableStructureWrapper, buildingRate);
            }

        }

        #endregion

    }
}