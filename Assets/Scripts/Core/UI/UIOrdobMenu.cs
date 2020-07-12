using System;
using System.Collections;
using System.Collections.Generic;
using Core.Handlers;
using Core.Selection;
using Core.Units;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using VariousUtilsExtensions;
using System.Linq;

namespace Core.UI
{
    public class UIOrdobMenu : MonoBehaviour
    {

        public UIOrdobTreeViewElement treeViewRootElement;

        private List<UIOrdobTreeViewElement> trackedUnitWrappersTreeViewElements = new List<UIOrdobTreeViewElement>();
        //private Dictionary<UnitWrapper, UIOrderOfBattleTreeViewElement> trackedUnitWrappersTreeViewElements = new Dictionary<UnitWrapper, UIOrderOfBattleTreeViewElement>();
        private Dictionary<UnitWrapper, MultiEventObserver> trackedUnitWrappersBinders = new Dictionary<UnitWrapper, MultiEventObserver>();

        private void Start()
        {
            
        }

        public void AddUnitToOrdob(UnitWrapper uwToAdd)
        {
            uwToAdd.SubscribeOnParentChange("changeobui",
                () =>
                {
                    if (trackedUnitWrappersTreeViewElements.Any((_) => { return _.associatedUnitWrapper == uwToAdd; }))
                        UpdateTreeViewElementParent(uwToAdd);
                    else
                        AddUnitToOrdob_Aux(uwToAdd);
                }
            );
        }

        private void AddUnitToOrdob_Aux(UnitWrapper uw)
        {
            var tve = AddTreeViewElement(uw);
            
            var binder = new MultiEventObserver();
            
            BindSelectionEvent(binder,
                (sender, args) => {
                    //Debug.Log("Triggered by direct selection status change.");
                    if (sender is UnitWrapper)
                        tve.ActualOnPointerDown(null);
                }, uw);

            tve.BindPressEvent(binder,
                (sender, args) => {
                    //Debug.Log("Triggered by direct button status change (click)");
                    if (args is SimpleEventArgs)
                    {
                        if ((bool)(args as SimpleEventArgs).args[0])
                            SelectionHandler.GetUsedSelector().SelectEntity(uw);
                        else
                            SelectionHandler.GetUsedSelector().DeselectEntity(uw);
                    }
                });

            trackedUnitWrappersTreeViewElements.Add(tve);
        }

        public void RemoveUnitFromOrdob(UnitWrapper uwToRemove)
        {
            uwToRemove.UnsubscribeOnParentChange("changeobui");
            var bndr = trackedUnitWrappersBinders[uwToRemove];
            UnbindSelectionEvent(bndr, uwToRemove);

            var r = trackedUnitWrappersTreeViewElements.Find((_) => { return _.associatedUnitWrapper == uwToRemove; });
            
            r.associatedUnitWrapper = null;
            trackedUnitWrappersTreeViewElements.Remove(r);       
            
            r.DestroyMe();
        }

        private UIOrdobTreeViewElement AddTreeViewElement(UnitWrapper uwToAdd)
        {
            UIOrdobTreeViewElement res;
            
            foreach (var v in trackedUnitWrappersTreeViewElements)
            {
                if (v.associatedUnitWrapper == uwToAdd.GetParentNode())
                {
                    res = Instantiate<UIOrdobTreeViewElement>(
                        GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().uiOrdobTreeViewElementPrefab,
                        v.childrenRoot.transform);
                        
                    // This (VVVV) obviously sucks, will be changed in the future during next massive refactoring.
                    //res.containerOfWholeTreeView = transform.Find("Scroll Area").Find("List").GetComponent<RectTransform>();
                    // Already better :
                    res.dragArea = v.dragArea;
                    res.containerOfWholeTreeView = v.containerOfWholeTreeView;
                    res.associatedUnitWrapper = uwToAdd;

                    return res;
                }
            }

            res = Instantiate<UIOrdobTreeViewElement>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().uiOrdobTreeViewElementPrefab,
                treeViewRootElement.childrenRoot.transform);

            // This (VVVV) obviously sucks, will be changed in the future during next massive refactoring.
            //res.containerOfWholeTreeView = transform.Find("Scroll Area").Find("List").GetComponent<RectTransform>();
            // Already better :
            res.dragArea = treeViewRootElement.dragArea;
            res.containerOfWholeTreeView = treeViewRootElement.containerOfWholeTreeView;
            res.associatedUnitWrapper = uwToAdd;
            
            return res;
        }
        
        private void UpdateTreeViewElementParent(UnitWrapper uw)
        {
            UIOrdobTreeViewElement ugwTve = trackedUnitWrappersTreeViewElements.Find((_) => { return _.associatedUnitWrapper == uw; });
            UIOrdobTreeViewElement ugwParentTve = trackedUnitWrappersTreeViewElements.Find((_) => { return _.associatedUnitWrapper == uw.GetParentNode(); });
            
            if (ugwParentTve == null)
                ugwParentTve = treeViewRootElement;
            
            if (ugwTve != null && ugwParentTve != null)
                ugwTve.transform.SetParent(ugwParentTve.childrenRoot.transform);
        }

        private void BindSelectionEvent(MultiEventObserver binder, Action<object, EventArgs> action, UnitWrapper uw)
        {
            if (!trackedUnitWrappersBinders.ContainsKey(uw))
            {
                var id = binder.AddEventAndSubscribeToIt(action);
                uw.GetOnSelectionStateChangeObserver().SubscribeToEvent("doactiontreeview",
                    (_) => binder.InvokeEvent(id, uw, null));
                trackedUnitWrappersBinders.Add(uw, binder);
            }
        }

        private void UnbindSelectionEvent(MultiEventObserver binder, UnitWrapper uw)
        {
            if (trackedUnitWrappersBinders.ContainsKey(uw))
            {
                uw.GetOnSelectionStateChangeObserver().UnsubscribeFromEvent("doactiontreeview");
                trackedUnitWrappersBinders.Remove(uw);
            }
        }


    }

}