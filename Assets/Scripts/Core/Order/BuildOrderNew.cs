using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using NPBehave;
using Core.MapMarkers;
using Core.Handlers;
using Core.BuiltStructures;

namespace Core.Orders
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// This "active" order allows units to build structures. As most orders, it isn't really finished but the necessary outline is there.
    /// It needs a BuildingMarker (or rather its MapMarkerWrapper)
    /// The BuildOrder is actually the first test of "internalized OrderPlans". It can create and run a MoveOrder to get closer to the building marker "inside of itself".
    /// See OrderPlan summary for more details.
    /// </summary>
    public class BuildOrderNew : Order2
    {

        private MapMarkerWrapper<BuildingMarker> _buildingMarkerWrapper;
        private MapMarkerWrapper<BuildingMarker> buildingMarkerWrapper
        {
            get
            {
                if(builtStructureWrapper != null && builtStructureWrapper.WrappedObject != null)
                    return builtStructureWrapper.buildingMarkerWrapper;
                else
                    return _buildingMarkerWrapper;
            }

            set
            {
                _buildingMarkerWrapper = value;
            }
        }

        private BuiltStructureWrapper builtStructureWrapper;
        
        private OrderParams myParameters = OrderParams.DefaultParam();

        private OrderWrapper<MoveOrderNew> predecessorMoveOrderWrapper = null;

        private List<MapMarkerWrapper<WaypointMarker>> waypointMarkerWrappersList = new List<MapMarkerWrapper<WaypointMarker>>();

        private UnitGroupWrapper unitsGroupWrapper;
        private class UnitWrapperExecutionState
        {
            public float materials = 150f;

            public int currentWaypointIndex;
            public bool endedPath;

            public UnitGroupWrapper thisUnitsSubUGW;

            public OrderPlan internalOrdersPlan;

            public UnitWrapperExecutionState(UnitWrapper unitWrapper)
            {
                thisUnitsSubUGW = new UnitGroup(new List<UnitWrapper>() { unitWrapper }, false).GetMyWrapper();
                UnitGroup.AddUnitToGroup(unitWrapper, thisUnitsSubUGW, () => { thisUnitsSubUGW = null; });

                internalOrdersPlan = thisUnitsSubUGW.GetOrdersPlan();

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

        protected override IOrderable InstanceGetOrderReceiver()
        {
            return unitsGroupWrapper;
        }
        
        protected override void InstanceSetOrderReceiver(IOrderable orderable, OrderWrapper predecessor, OrderWrapper successor)
        {
           
            /*unitsGroupWrapper = orderable as UnitGroupWrapper;
            unitsGroupWrapper.GetOrdersPlan().QueueActiveOrderToPlan(GetMyWrapper(), predecessor, successor);

            foreach (var uw in UnitGroup.GetUnitWrappersInFormation(unitsGroupWrapper))
            {
                currentExecutionStatePerUnit.Add(uw, new UnitWrapperExecutionState(uw));
                uw.unitsGroupWrapper.WrappedObject.SubscribeOnRemovalFromFormation(uw, () => currentExecutionStatePerUnit.Remove(uw));
                uw.SubscribeOnClearance(() => currentExecutionStatePerUnit.Remove(uw));
                GetMyWrapper().SubscribeOnClearance(() => currentExecutionStatePerUnit.Remove(uw));
            }

            SetPhase(GetMyWrapper(), OrderPhase.Staging);*/
            
            unitsGroupWrapper = orderable as UnitGroupWrapper;
            unitsGroupWrapper.GetOrdersPlan().QueueActiveOrderToPlan(GetMyWrapper(), predecessor, successor);

            foreach (var uw in UnitGroup.GetUnitWrappersInGroup(unitsGroupWrapper))
            {
                currentExecutionStatePerUnit.Add(uw, new UnitWrapperExecutionState(uw));
                uw.unitsGroupWrapper.WrappedObject.SubscribeOnRemovalFromGroup(uw, () =>
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });

                uw.SubscribeOnClearance(() =>
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });
                GetMyWrapper().SubscribeOnClearance(() => 
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });
            }

            SetPhase(GetMyWrapper(), OrderPhase.Staging);

        }

        protected override OrderParams InstanceGetOrderParams()
        {
            return myParameters;
        }

        public BuildOrderNew()
        {
            _myWrapper = new OrderWrapper<BuildOrderNew>(this, () => {_myWrapper = null;});

            CreateAndInitFSM();
        } 

        #region Specific behaviour logic

        public void SetBuildingToBuild(MapMarkerWrapper<BuildingMarker> buildingMarkerWrapperArg)
        {
            GetMyWrapper().SubscribeOnClearance(
                () => { 
                    if (buildingMarkerWrapperArg != null && ((buildingMarkerWrapperArg.GetWrappedAs<BuildingMarker>().builtStructureWrapper != null
                    && BuiltStructure.GetHP(buildingMarkerWrapperArg.GetWrappedAs<BuildingMarker>().builtStructureWrapper) == 0)
                        || buildingMarkerWrapperArg.GetWrappedAs<BuildingMarker>().builtStructureWrapper == null))
                        buildingMarkerWrapperArg.DestroyWrappedReference(); 
                });
            this.buildingMarkerWrapper = buildingMarkerWrapperArg;
            this.buildingMarkerWrapper.SubscribeOnClearance(() => this.buildingMarkerWrapper = null);
        }

        
        private bool _endedPathForAll;
        protected override void UpdateExecution()
        {
            _endedPathForAll = true;
            foreach (var uw in UnitGroup.GetUnitWrappersInGroup(unitsGroupWrapper))
            {
                if (IsInPhase(GetMyWrapper(), OrderPhase.Execution))
                {
                    if (ReceiverExists(GetMyWrapper()))
                    {
                        if (!CloseEnoughToBuildingMarker(uw))
                        {
                            if (currentExecutionStatePerUnit[uw].internalOrdersPlan.GetFirstInlineActiveOrderInPlan() == null)
                            {
                                //var intermediateMove = OrderFactory.CreateOrderWrapperWithoutReceiver<MoveOrderNew>();
                                var intermediateMove = OrderFactory.CreateOrderWrapperAndSetReceiver<MoveOrderNew>(currentExecutionStatePerUnit[uw].thisUnitsSubUGW);
                                
                                //OrderHandler.AddToGlobalOrderWrapperList(intermediateMove);
                                //SetReceiver(intermediateMove, null, null, uw);

                                //intermediateMove.SubscribeOnClearance(() => intermediateMove = null);
                    
                                intermediateMove.GetWrappedAs<MoveOrderNew>().AddWaypoint(
                                    (WaypointMarker.CreateInstance(
                                        buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().transform.position
                                        + 2*(UnityEngine.Random.Range(0.5f,1)*Vector3.right+UnityEngine.Random.Range(0.5f,1)*Vector3.forward).normalized)
                                    ).GetMyWrapper<WaypointMarker>());

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
                EndExecution(GetMyWrapper());

        }

        private bool CloseEnoughToBuildingMarker(UnitWrapper unitWrapper)
        {
            if (buildingMarkerWrapper != null
                && Vector3.Distance(buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().transform.position, unitWrapper.WrappedObject.myMover.GetPosition()) <= 2.5f)
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
            if (currentExecutionStatePerUnit[unitWrapper].materials < 0 || BuiltStructure.GetHP(builtStructureWrapper) >= 100)
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
            if (builtStructureWrapper == null && buildingMarkerWrapper != null)
            {
                builtStructureWrapper = buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().CreateAndSetBuiltStructure<BuiltStructure>();
                GetMyWrapper().SubscribeOnClearance(() => builtStructureWrapper = null);
            }

            //_previousmaterials = _materials;
            currentExecutionStatePerUnit[unitWrapper].materials = Mathf.Max(-1, currentExecutionStatePerUnit[unitWrapper].materials - buildingRate);

            if (currentExecutionStatePerUnit[unitWrapper].materials > 0)
            {
                if (Mathf.Abs(currentExecutionStatePerUnit[unitWrapper].materials % 30) < 1)
                    Debug.Log(unitWrapper + " : Building structure...");
                BuiltStructure.AddHP(builtStructureWrapper, buildingRate);
            }

        }

        #endregion

    }
}