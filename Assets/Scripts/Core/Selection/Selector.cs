using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Faction;
using Core.Units;
using Nrealus.Extensions;
using Core.Helpers;
using Gamelogic.Extensions;
using System;
using Nrealus.Extensions.ReferenceWrapper;

namespace Core.Selection
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/
    
    /// <summary>
    /// An important class which deals with "selection" of instances that implemented ISelectable, which are mainly intended to be game units.
    /// An instance of this class selects, stores, deselects ISelectables (units) in and out of it. Boolean functions that act like queries can be called externally to check the selection state
    /// of an object in this selector. In a way, it is both a "database" and the terminal needed to interact with it.
    /// It also has a bit of gameplay logic surrounding selection (see finite state machine) that can be activated from another class when dispatching input.
    /// </summary>   
    public class Selector : MonoBehaviour, IHasRefToCamera
    {
        
        /*private class DeepCopyConvertionList : List<ISelectable>
        {
            public List<ISelectable> list = new List<ISelectable>();

            private Dictionary<Type,List<ISelectable>> encounteredTypesAndCastListsDict = new Dictionary<Type, List<ISelectable>>();

            public void Add(ISelectable obj)
            {
                list.Add(obj);
            }

            public List<T> SelectByType<T>() where T : ISelectable
            {
                if (!encounteredTypesAndCastListsDict.ContainsKey(typeof(T)))
                {
                    List<T> castedList = new List<T>();
                    foreach(var v in list)
                    {
                        if (v is T)
                            castedList.Add((T)v);
                    }
                    encounteredTypesAndCastListsDict.Add(typeof(T), castedList);
                }

                return encounteredTypesAndCastListsDict[typeof(T)] as List<T>;
            }

        }*/

        public FactionData selectorFaction;

        private Camera _cam;
        public Camera GetMyCamera()
        {
            if(_cam == null)
                _cam = GameObject.Find("Main Camera").GetComponent<Camera>();

            return _cam;
        }

        #region Basic functions and direct interaction methods (usable both here and from somewhere else if needed)

        //public event Action<ISelectable, bool> onEntitySelectionStateChanged;

        private UnitsRoot unitsRoot;
        private Vector3 myPointerPreviousScreenPosition, myPointerCurrentScreenPostion;

        public enum SelectionModes { Default, Additive, Subtractive };
        public SelectionModes selectionMode = SelectionModes.Default;

        private List<ISelectable> selectedEntities = new List<ISelectable>();
        private List<ISelectable> preselectedEntities = new List<ISelectable>();

        public bool IsSelected(ISelectable selectable)
        {
            return selectedEntities.Contains(selectable);
        }

        public bool IsHighlighted(ISelectable selectable)
        {
            return preselectedEntities.Contains(selectable);
        }

        public List<ISelectable> GetCurrentlySelectedEntities()
        {
            return /*new List<ISelectable>(*/selectedEntities;//);
        }

        public List<T> GetCurrentlySelectedEntitiesAs<T>() where T : class
        {
            var res = new List<T>();
            foreach(var v in selectedEntities)
            {
                res.Add(v as T);
            }
            return res;
        }

        public List<ISelectable> GetCurrentlySelectedEntitiesOfType<T>()
        {

            var res = new List<ISelectable>();
            foreach(var v in selectedEntities)
            {
                if (v is T)
                    res.Add(v);
            }
            return res;
        }

        public List<T> GetCurrentlySelectedEntitiesOfTypeAndCast<T>()
        {
            var res = new List<T>();
            foreach(var v in selectedEntities)
            {
                if (v is T)
                    res.Add((T)v);
            }
            return res;
        }

        public void SelectEntity(ISelectable selectable)
        {
            if (!selectedEntities.Contains(selectable))
            {
                selectable.InvokeOnSelectionStateChange(this, true);
                selectedEntities.Add(selectable);
                //onEntitySelectionStateChanged?.Invoke(selectable, true);
                selectable.SubscribeOnDestruction("deselect",() => DeselectEntity(selectable));
            }
        }

        public void DeselectEntity(ISelectable selectable)
        {
            if (selectedEntities.Contains(selectable))
            {
                selectable.InvokeOnSelectionStateChange(this, false);
                selectable.UnsubscribeOnDestruction("deselect");
                //onEntitySelectionStateChanged?.Invoke(selectable, false);
                selectedEntities.Remove(selectable);
            }
        }

        public void PreselectEntity(ISelectable selectable)
        {
            if (!preselectedEntities.Contains(selectable))
            {
                preselectedEntities.Add(selectable);
                selectable.SubscribeOnDestruction("depreselect",() => DepreselectEntity(selectable));
            }
        }

        public void DepreselectEntity(ISelectable selectable)
        {
            if (preselectedEntities.Contains(selectable))
            {
                selectable.UnsubscribeOnDestruction("depreselect");
                preselectedEntities.Remove(selectable);
            }
        }

        private void DeselectAndDepreselectEveryone()
        {
            int i;
            int c = selectedEntities.Count;
            for (i = c-1; i >= 0; i--)
                DeselectEntity(selectedEntities[i]);

            c = preselectedEntities.Count;
            for (i = c-1; i >= 0; i--)
                DepreselectEntity(preselectedEntities[i]);
        }

        // example (generics shenanigans)
        /*public void DeselectUnit(ISelectable<Unit> selectable)
        {
            if (selectedEntities.Contains(selectable))
            {
                selectable.GetSelectableAsReferenceWrapperNonGeneric().UnsubscribeOnClearance(() => DeselectEntity(selectable));
                selectedEntities.Remove(selectable);
            }
        }*/

        #endregion

        #region Finite State Machine definition and methods for interacting with this selector and "commanding" it (input/output)
        
        public bool isUsed;

        public enum HighStates { Dead, Inactive, Pause, Active }
        public enum LowStates { NotSelecting, Selecting, InternalConfirm, InternalCancel }

        private StateMachine<HighStates> stateMachineHigh;
        private StateMachine<LowStates> stateMachineLow;

        public HighStates GetHighState() { return stateMachineHigh.CurrentState; }
        public LowStates GetLowState() { return stateMachineLow.CurrentState; }

        public bool Kill() { stateMachineHigh.CurrentState = HighStates.Dead; return true; }
        public bool Deactivate() { stateMachineHigh.CurrentState = HighStates.Inactive; return true; }
        public bool Pause() { stateMachineHigh.CurrentState = HighStates.Pause; return true; }
        public bool ActivateAndUnpause() { stateMachineHigh.CurrentState = HighStates.Active; return true; }

        public bool StartSelecting()
        {
            if (GetHighState() == HighStates.Active)
            {
                stateMachineLow.CurrentState = LowStates.Selecting;
                return true;
            }
            else
                return false;
        }
        public bool CancelSelecting()
        {
            stateMachineLow.CurrentState = LowStates.InternalCancel;
            return true;
        }
        public bool ConfirmSelecting()
        {
            if (GetHighState() == HighStates.Active)
            {
                stateMachineLow.CurrentState = LowStates.InternalConfirm;
                return true;
            }
            else
                return false;
        }
        public void UpdatePointerCurrentScreenPosition(Vector3 position)
        {
            if(GetHighState() == HighStates.Active)
                myPointerCurrentScreenPostion = position;
        }

        #endregion
        
        #region MonoBehaviour methods

        private void Awake()
        {
            unitsRoot = FindObjectOfType<UnitsRoot>();

            stateMachineLow = new StateMachine<LowStates>();
            stateMachineLow.AddState(LowStates.NotSelecting);
            stateMachineLow.AddState(LowStates.Selecting, null, () => { ShapeSelecting(); });
            stateMachineLow.AddState(LowStates.InternalConfirm, () => { ConfirmShapeSelecting(); stateMachineLow.CurrentState = LowStates.NotSelecting; });
            stateMachineLow.AddState(LowStates.InternalCancel, () => { CancelShapeSelecting(); stateMachineLow.CurrentState = LowStates.NotSelecting; });

            stateMachineHigh = new StateMachine<HighStates>();
            stateMachineHigh.AddState(HighStates.Dead, () => { selectionMode = SelectionModes.Default; CancelSelecting(); DeselectAndDepreselectEveryone(); });
            stateMachineHigh.AddState(HighStates.Inactive, () => { selectionMode = SelectionModes.Default; CancelSelecting(); });
            stateMachineHigh.AddState(HighStates.Active/*, null, () => { stateMachineLow.Update(); }*/);
            stateMachineHigh.AddState(HighStates.Pause);

            stateMachineLow.CurrentState = LowStates.NotSelecting;
            stateMachineHigh.CurrentState = HighStates.Active;

        }

        private void OnDestroy()
        {
            //onEntitySelectionStateChanged = null;
        }

        private void Update()
        {
            stateMachineHigh.Update();
            stateMachineLow.Update();
        }

        private void LateUpdate()
        {
            if(GetLowState() != LowStates.Selecting)
                myPointerPreviousScreenPosition = myPointerCurrentScreenPostion;
        }

        private void OnGUI()
        {
            if (GetLowState() == LowStates.Selecting)
            {
                // Create a rect from both mouse positions
                var rect = DrawUtils.GetScreenRect(myPointerPreviousScreenPosition, myPointerCurrentScreenPostion);
                DrawUtils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                DrawUtils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        #endregion

        #region Actual selection actions behaviours

        private void ShapeSelecting()
        {
            Bounds viewportBounds = UIUtils.GetViewportBounds(GetMyCamera(), myPointerPreviousScreenPosition, myPointerCurrentScreenPostion);
            Vector3 sp;

            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                var u = unitsRoot.transform.GetChild(i).GetComponent<Unit>();
                ISelectable s = u;
                
                sp = GetMyCamera().WorldToScreenPoint(u.transform.position);
                sp.z = 0;

                if (IsPointed(s, sp, myPointerCurrentScreenPostion, 10)
                    || IsInViewportBounds(s, GetMyCamera().WorldToViewportPoint(u.transform.position), viewportBounds))
                {
                    PreselectEntity(s);
                }
                else
                {
                    DepreselectEntity(s);
                }
            }
        }
        
        private void ConfirmShapeSelecting()
        {
            Bounds viewportBounds = UIUtils.GetViewportBounds(GetMyCamera(), myPointerPreviousScreenPosition, myPointerCurrentScreenPostion);
            Vector3 sp;

            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                var u = unitsRoot.transform.GetChild(i).GetComponent<Unit>();
                ISelectable s = u;

                DepreselectEntity(s);

                sp = GetMyCamera().WorldToScreenPoint(u.transform.position);
                sp.z = 0;

                if (IsPointed(s, sp, myPointerCurrentScreenPostion, 10)
                    || IsInViewportBounds(s, GetMyCamera().WorldToViewportPoint(u.transform.position), viewportBounds))
                {
                    if (selectionMode == SelectionModes.Subtractive)
                        DeselectEntity(s);
                    else
                        SelectEntity(s);
                }
                else if (selectionMode == SelectionModes.Default)
                {
                    DeselectEntity(s);
                }
            }
        }

        private void CancelShapeSelecting()
        {
            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                DepreselectEntity(unitsRoot.transform.GetChild(i).GetComponent<Unit>());
            }
        }

        #endregion

        #region Auxiliary functions

        private bool IsInViewportBounds(ISelectable selectable, Vector3 selectablePosition, Bounds viewportBounds)
        {
            // clumsy ?
            return viewportBounds.Contains(selectablePosition);

            /*
            Collider selectionCollider = selectableObject.GetComponent<Collider>();
            Vector3 center = selectionCollider.bounds.center;
            Vector3 extents = selectionCollider.bounds.extents;
            */

            /*
            return IsPointed(selectableObject)
                    || (viewportBounds.Contains(mainCamera.WorldToViewportPoint(center))
                        && (viewportBounds.Contains(mainCamera.WorldToViewportPoint(center + extents))
                            || viewportBounds.Contains(mainCamera.WorldToViewportPoint(center - extents))));
            */
        }

        private bool IsPointed(ISelectable selectable, Vector3 selectablePosition, Vector3 pointedPosition, float thresholdPosDiff)
        {
            bool b = (pointedPosition - selectablePosition).magnitude < thresholdPosDiff;

            return b;
        }
        
        /*
        private bool IsSelectable(Selectable selectableObject)
        {
            // TODO : clumsy
            var viewportBounds = UIUtils.GetViewportBounds(mainCamera, myPreviousMousePosition, myCurrentMousePosition);

            Collider selectionCollider = selectableObject.GetComponent<Collider>();

            Vector3 center = selectionCollider.bounds.center;
            Vector3 extents = selectionCollider.bounds.extents;

            return IsPointed(selectableObject)
                    || (viewportBounds.Contains(mainCamera.WorldToViewportPoint(center))
                        && (viewportBounds.Contains(mainCamera.WorldToViewportPoint(center + extents))
                            || viewportBounds.Contains(mainCamera.WorldToViewportPoint(center - extents))));
        }

        private bool IsPointed(Selectable selectableObject)
        {

            Collider selectionCollider = selectableObject.GetComponent<Collider>();

            RaycastHit hit = new RaycastHit();
            Physics.Raycast(mainCamera.ScreenPointToRay(myCurrentMousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("Selection"));

            bool pointed;
            pointed = (hit.collider == null) ? false : (hit.collider == selectionCollider);

            return pointed;
        }
        */

        #endregion

    }
}