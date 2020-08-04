using System;
using System.Collections;
using System.Collections.Generic;
using Core.Handlers;
using Core.Selection;
using Core.Units;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using Nrealus.Extensions;
using System.Linq;
using Nrealus.Extensions.Observer;

namespace Core.UI
{
    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// Main UI class for the "Units under command" UI panel, allowing to see the commanded units structure (Order of Battle)
    /// as a tree of buttons (UIOrdobTreeViewElement) that can be used to select or deselect units. The selection state of the buttons is in sync with that of the units.
    /// </summary>   
    public class UIOrdobMenu : MonoBehaviour
    {

        public UIOrdobTreeViewElement treeViewRootElement;

        private List<UIOrdobTreeViewElement> trackedUnitsTVEs = new List<UIOrdobTreeViewElement>();
        //private Dictionary<UnitWrapper, UIOrderOfBattleTreeViewElement> trackedUnitWrappersTreeViewElements = new Dictionary<UnitWrapper, UIOrderOfBattleTreeViewElement>();
        private Dictionary<Unit, MultiEventObserver> trackedUnitsBinders = new Dictionary<Unit, MultiEventObserver>();

        private void Start()
        {
            
        }

        public void AddUnitToOrdob(Unit uToAdd)
        {
            uToAdd.SubscribeOnParentChange("changeobui",
                () =>
                {
                    if (trackedUnitsTVEs.Any((_) => { return _.GetAssociatedUnit() == uToAdd; }))
                        UpdateTreeViewElementParent(uToAdd);
                    else
                        AddUnitToOrdob_Aux(uToAdd);
                }
            );
        }

        private void AddUnitToOrdob_Aux(Unit u)
        {
            var tve = AddTreeViewElement(u);
            
            var binder = new MultiEventObserver();
            
            BindSelectionEvent(binder,
                (sender, args) => {
                    //Debug.Log("Triggered by direct selection status change.");
                    if (sender is Unit)
                        tve.ActualOnPointerDown(null);
                }, u);

            tve.BindPressEvent(binder,
                (sender, args) => {
                    //Debug.Log("Triggered by direct button status change (click)");
                    if (args is SimpleEventArgs)
                    {
                        if ((bool)(args as SimpleEventArgs).args[0])
                            SelectionHandler.GetUsedSelector().SelectEntity(u);
                        else
                            SelectionHandler.GetUsedSelector().DeselectEntity(u);
                    }
                });

            trackedUnitsTVEs.Add(tve);
        }

        public void RemoveUnitFromOrdob(Unit uToRemove)
        {
            uToRemove.UnsubscribeOnParentChange("changeobui");
            var bndr = trackedUnitsBinders[uToRemove];
            UnbindSelectionEvent(bndr, uToRemove);

            var r = trackedUnitsTVEs.Find((_) => { return _.GetAssociatedUnit() == uToRemove; });
            
            trackedUnitsTVEs.Remove(r);       
            
            r.DestroyMe();
        }

        private UIOrdobTreeViewElement AddTreeViewElement(Unit uToAdd)
        {
            UIOrdobTreeViewElement res;
            
            foreach (var v in trackedUnitsTVEs)
            {
                if (v.GetAssociatedUnit() == uToAdd.GetParentUnit())
                {
                    res = Instantiate<UIOrdobTreeViewElement>(
                        GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().uiOrdobTreeViewElementPrefab,
                        v.childrenRoot.transform);
                        
                    // This (VVVV) obviously sucks, will be changed in the future during next massive refactoring.
                    //res.containerOfWholeTreeView = transform.Find("Scroll Area").Find("List").GetComponent<RectTransform>();
                    // Already better :
                    res.Init(uToAdd, v.dragArea, v.containerOfWholeTreeView);

                    return res;
                }
            }

            res = Instantiate<UIOrdobTreeViewElement>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().uiOrdobTreeViewElementPrefab,
                treeViewRootElement.childrenRoot.transform);

            // This (VVVV) obviously sucks, will be changed in the future during next massive refactoring.
            //res.containerOfWholeTreeView = transform.Find("Scroll Area").Find("List").GetComponent<RectTransform>();
            // Already better :
            res.Init(uToAdd, treeViewRootElement.dragArea, treeViewRootElement.containerOfWholeTreeView);
            
            return res;
        }
        
        private void UpdateTreeViewElementParent(Unit u)
        {
            UIOrdobTreeViewElement ugwTve = trackedUnitsTVEs.Find((_) => { return _.GetAssociatedUnit() == u; });
            UIOrdobTreeViewElement ugwParentTve = trackedUnitsTVEs.Find((_) => { return _.GetAssociatedUnit() == u.GetParentUnit(); });
            
            if (ugwParentTve == null)
                ugwParentTve = treeViewRootElement;
            
            if (ugwTve != null && ugwParentTve != null)
                ugwTve.transform.SetParent(ugwParentTve.childrenRoot.transform);
        }

        private void BindSelectionEvent(MultiEventObserver binder, Action<object, EventArgs> action, Unit u)
        {
            if (!trackedUnitsBinders.ContainsKey(u))
            {
                var id = binder.AddNewEventAndSubscribeMethodToIt(action);
                u.GetOnSelectionStateChangeObserver().SubscribeEventHandlerMethod("doactiontreeview",
                    (_) => { if(_.Item3 == 0) binder.InvokeEvent(id, u, null); });
                trackedUnitsBinders.Add(u, binder);
            }
        }

        private void UnbindSelectionEvent(MultiEventObserver binder, Unit u)
        {
            if (trackedUnitsBinders.ContainsKey(u))
            {
                u.GetOnSelectionStateChangeObserver().UnsubscribeEventHandlerMethod("doactiontreeview");
                trackedUnitsBinders.Remove(u);
            }
        }


    }

}