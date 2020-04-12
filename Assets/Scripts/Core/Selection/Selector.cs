using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Faction;
using Core.Units;
using VariousUtilsExtensions;
using Core.Helpers;
using Gamelogic.Extensions;

namespace Core.Selection
{

    public class Selector : MonoBehaviour, IHasCameraRef
    {
        public FactionData selectorFaction;

        private Camera _cam;
        public Camera GetMyCamera()
        {
            if(_cam == null)
                _cam = GameObjectExtension.FindObjectOfTypeAndLayer<Camera>(LayerMask.NameToLayer("Default"));

            return _cam;
        }

        private UnitsRoot unitsRoot;
        private Vector3 myPointerPreviousScreenPosition, myPointerCurrentScreenPostion;

        public enum SelectionModes { Default, Additive, Subtractive };
        public SelectionModes selectionMode = SelectionModes.Default;

        /*
        private List<ISelectable<Unit>> selectedUnits = new List<ISelectable<Unit>>();

        public bool IsSelected(ISelectable<Unit> unitWrapper)
        {
            return selectedUnits.Contains(unitWrapper);
        }
        public bool IsHighlighted(ISelectable<Unit> unitWrapper)
        {
            return false;
            //return selectedUnits.Contains(unitWrapper);
        }

        public ISelectable<Unit>[] GetCurrentlySelectedUnits()
        {
            return selectedUnits.ToArray();
        }

        public void SelectUnit(ISelectable<Unit> selectableUnit)
        {
            if (!selectedUnits.Contains(selectableUnit))
            {
                selectedUnits.Add(selectableUnit);
                selectableUnit.GetMyInstanceType().SubscribeOnClearance(() => DeselectUnit(selectableUnit));
            }
        }

        public void DeselectUnit(ISelectable<Unit> selectable)
        {
            if (selectedUnits.Contains(selectable))
            {
                selectable.GetMyInstanceType().UnsubscribeOnClearance(() => DeselectUnit(selectable));
                selectedUnits.Remove(selectable);
            }
        }
        */

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
            return new List<ISelectable>(selectedEntities);
        }

        public List<T> GetCurrentlySelectedEntitiesAs<T>() where T : ReferenceWrapper
        {
            var res = new List<T>();
            foreach(var v in selectedEntities)
            {
                res.Add(v.GetSelectableAsReferenceWrapperSpecific<T>());
            }
            return res;
        }

        public void SelectEntity(ISelectable selectable)
        {
            if (!selectedEntities.Contains(selectable))
            {
                selectedEntities.Add(selectable);
                selectable.GetSelectableAsReferenceWrapperNonGeneric().SubscribeOnClearance(() => DeselectEntity(selectable));
            }
        }

        public void PreselectEntity(ISelectable selectable)
        {
            if (!preselectedEntities.Contains(selectable))
            {
                preselectedEntities.Add(selectable);
                selectable.GetSelectableAsReferenceWrapperNonGeneric().SubscribeOnClearance(() => DepreselectEntity(selectable));
            }
        }

        public void DeselectEntity(ISelectable selectable)
        {
            if (selectedEntities.Contains(selectable))
            {
                selectable.GetSelectableAsReferenceWrapperNonGeneric().UnsubscribeOnClearance(() => DeselectEntity(selectable));
                selectedEntities.Remove(selectable);
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

        public void DepreselectEntity(ISelectable selectable)
        {
            if (preselectedEntities.Contains(selectable))
            {
                selectable.GetSelectableAsReferenceWrapperNonGeneric().UnsubscribeOnClearance(() => DepreselectEntity(selectable));
                preselectedEntities.Remove(selectable);
            }
        }


        // example (generics shenanigans)
        public void DeselectUnit(ISelectable<Unit> selectable)
        {
            if (selectedEntities.Contains(selectable))
            {
                selectable.GetSelectableAsReferenceWrapperNonGeneric().UnsubscribeOnClearance(() => DeselectEntity(selectable));
                selectedEntities.Remove(selectable);
            }
        }

        //-----------------------------------------------------------------------------------------------------//
        
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

        //-----------------------------------------------------------------------------------------------------//

        private void ShapeSelecting()
        {
            Bounds viewportBounds = UIUtils.GetViewportBounds(GetMyCamera(), myPointerPreviousScreenPosition, myPointerCurrentScreenPostion);
            Vector3 sp;

            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                var u = unitsRoot.transform.GetChild(i).GetComponent<Unit>();
                ISelectable s = u.GetMyWrapper();
                
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
                ISelectable s = u.GetMyWrapper();

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
                DepreselectEntity(unitsRoot.transform.GetChild(i).GetComponent<Unit>().GetMyWrapper());
            }
        }

        //-----------------------------------------------------------------------------------------------------//

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

    }
}