using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VariousUtilsExtensions;
using UnityEngine;
using Core.Helpers;
using System;

namespace Core.Units
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// This class represents a group or a formation of units.
    /// A group can be made of only one unit. In fact, by default, each Unit instance has an associated UnitGroup instance, through its UnitWrapper.
    /// Formations can be seen as a sub-category of "generic" groups. The difference is that a unit can only be part of one formation at a time but multiple generic groups.
    ///</summary>
    public class UnitGroup : IHasRefToRefWrapper<UnitGroupWrapper>
    {
    
        private List<UnitWrapper> unitWrappersList = new List<UnitWrapper>();
        private Dictionary<UnitWrapper, Action> onRemovalHandlersDict = new Dictionary<UnitWrapper, Action>();
		
        public static List<UnitWrapper> GetUnitWrappersInGroup(UnitGroupWrapper unitGroupWrapper)
        {
            return unitGroupWrapper.WrappedObject.unitWrappersList;
        }

        public static List<UnitGroupWrapper> GetSubGroups(UnitGroupWrapper unitGroupWrapper)
        {
            return unitGroupWrapper.GetChildNodes();
        }

        public static void AddUnitToGroup(UnitWrapper unitWrapper, UnitGroupWrapper unitGroupWrapper, Action actionOnRemovalFromGroup)
        {    
            /// Keep an eye
            if (unitWrapper.unitsGroupWrapper.WrappedObject.isFormation)
            {
                if (unitWrapper.unitsGroupWrapper != null)
                    RemoveUnitFromGroup(unitWrapper, unitWrapper.unitsGroupWrapper);
            }
    
            unitGroupWrapper.WrappedObject.unitWrappersList.Add(unitWrapper);

            unitGroupWrapper.WrappedObject.SubscribeOnRemovalFromGroup(unitWrapper, actionOnRemovalFromGroup);

            unitWrapper.SubscribeOnClearance(() => RemoveUnitFromGroup(unitWrapper, unitGroupWrapper));
            
        }

        public static void RemoveUnitFromGroup(UnitWrapper unitWrapper, UnitGroupWrapper unitGroupWrapper)
        {
            if (unitGroupWrapper.WrappedObject != null)
            {
                unitWrapper.UnsubscribeOnClearance(() => RemoveUnitFromGroup(unitWrapper, unitGroupWrapper));
                if(unitGroupWrapper.WrappedObject.onRemovalHandlersDict[unitWrapper] != null)
                    unitGroupWrapper.WrappedObject.onRemovalHandlersDict[unitWrapper].Invoke();
                else
                    Debug.LogWarning("is this normal ?");
                unitGroupWrapper.WrappedObject.onRemovalHandlersDict[unitWrapper] = null;
                unitGroupWrapper.WrappedObject.onRemovalHandlersDict.Remove(unitWrapper);
            }
        }

        public void SubscribeOnRemovalFromGroup(UnitWrapper unitWrapper, Action actionOnRemovalFromGroup)
        {
            Action a;
            if(!onRemovalHandlersDict.TryGetValue(unitWrapper, out a))
                onRemovalHandlersDict.Add(unitWrapper, actionOnRemovalFromGroup);
            else
                a += actionOnRemovalFromGroup;
        }
       
        public bool isFormation { get; private set; }

        protected UnitGroupWrapper _myWrapper;
        public UnitGroupWrapper GetMyWrapper() { return _myWrapper; }

        public UnitGroup(List<UnitWrapper> uwl, bool isFormationOrJustGroup)
        {
            this.isFormation = isFormationOrJustGroup;
            _myWrapper = new UnitGroupWrapper(this, () => 
                {
                    foreach (var v in unitWrappersList) 
                        RemoveUnitFromGroup(v, GetMyWrapper());
                    _myWrapper = null;
                });
        }

        public Vector3 GetPosition()
        {
            var l = unitWrappersList;
            int c = l.Count;
            Vector3 res = Vector3.zero;

            if (c == 0)
                throw new SystemException("what");

            foreach(var v in l)
            {
                res += v.WrappedObject.transform.position;
            }
            res /= c;
            return res;

        }

    }

}