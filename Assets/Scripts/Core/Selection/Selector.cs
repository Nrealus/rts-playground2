using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Faction;
using Core.Units;
using VariousUtilsExtensions;
using Core.Helpers;

namespace Core.Selection
{

    public class Selector : MonoBehaviour
    {

        private Camera cam;
        private UnitsRoot unitsRoot;
        private Vector3 myPreviousMousePosition, myCurrentMousePosition;

        [HideInInspector] public enum SelectionModes { Simple, Additive, Complementary };
        private bool isSelecting;

        public bool isUsed;

        private bool _activeSelector = false;
        public bool activeSelector
        {
            get { return _activeSelector; }

            set {
                if(value == false && activeSelector == true)
                {
                    CancelShapeSelecting();
                    isSelecting = false;
                }
                _activeSelector = value;
            }
        }

        public FactionData selectorFaction;

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

        private void Start()
        {
            cam = Camera.main;
            unitsRoot = FindObjectOfType<UnitsRoot>();

        }

        private void Update()
        {

            myCurrentMousePosition = Input.mousePosition;

            if (activeSelector)
            {

                // will be done with the new InputSystem - the global idea will remain the same (as in no overly complicated stuff involved)
                // but the great thing is that these will be done with anonymous methods subscribing to events
                if (Input.GetMouseButtonDown(0) && isSelecting && isUsed)
                {
                    isSelecting = false;
                }
                if (Input.GetMouseButtonDown(0) && !isSelecting && isUsed)
                {
                    isSelecting = true;
                }
                if (Input.GetMouseButtonUp(0) && isSelecting && isUsed)
                {
                    ConfirmShapeSelecting();
                    isSelecting = false;
                }
                if (isSelecting && Input.GetMouseButtonDown(1) && isUsed)
                {
                    CancelShapeSelecting();
                    isSelecting = false;
                }

                if(isSelecting && isUsed)
                {
                    ShapeSelecting();
                }

            }
        }

        private void LateUpdate()
        {
            if (!isSelecting && isUsed)
                myPreviousMousePosition = myCurrentMousePosition;
        }

        private void OnGUI()
        {
            if (isSelecting)
            {
                // Create a rect from both mouse positions
                var rect = DrawUtils.GetScreenRect(myPreviousMousePosition, myCurrentMousePosition);
                DrawUtils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                DrawUtils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        //-----------------------------------------------------------------------------------------------------//

        private void ShapeSelecting()
        {
            Bounds viewportBounds = UIUtils.GetViewportBounds(cam, myPreviousMousePosition, myCurrentMousePosition);
            Vector3 sp;

            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                var u = unitsRoot.transform.GetChild(i).GetComponent<Unit>();
                ISelectable s = u.GetMyWrapper();
                
                sp = cam.WorldToScreenPoint(u.transform.position);
                sp.z = 0;

                if (IsPointed(s, sp, Input.mousePosition, 10)
                    || IsInViewportBounds(s, cam.WorldToViewportPoint(u.transform.position), viewportBounds))
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
            Bounds viewportBounds = UIUtils.GetViewportBounds(cam, myPreviousMousePosition, myCurrentMousePosition);
            Vector3 sp;

            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                var u = unitsRoot.transform.GetChild(i).GetComponent<Unit>();
                ISelectable s = u.GetMyWrapper();

                DepreselectEntity(s);

                sp = cam.WorldToScreenPoint(u.transform.position);
                sp.z = 0;

                if (IsPointed(s, sp, Input.mousePosition, 10)
                    || IsInViewportBounds(s, cam.WorldToViewportPoint(u.transform.position), viewportBounds))
                {
                    SelectEntity(s);
                }
                else
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
            // TODO : clumsy
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