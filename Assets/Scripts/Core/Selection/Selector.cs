using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Faction;
using Core.Units;
using VariousUtilsExtensions;

namespace Core.Selection
{

    public class Selector : MonoBehaviour
    {

        public bool isUsed;

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

        public bool IsSelected(ISelectable selectable)
        {
            return selectedEntities.Contains(selectable);
        }

        public bool IsHighlighted(ISelectable selectable)
        {
            return false;
            //return selectedEntities.Contains(selectable);
        }

        public List<ISelectable> GetCurrentlySelectedEntities()
        {
            return new List<ISelectable>(selectedEntities);
        }

        public void SelectEntity(ISelectable selectable)
        {
            if (!selectedEntities.Contains(selectable))
            {
                selectedEntities.Add(selectable);
                selectable.GetMyReferenceWrapperNonGeneric().SubscribeOnClearance(() => DeselectEntity(selectable));
            }
        }

        public void DeselectEntity(ISelectable selectable)
        {
            if (selectedEntities.Contains(selectable))
            {
                selectable.GetMyReferenceWrapperNonGeneric().UnsubscribeOnClearance(() => DeselectEntity(selectable));
                selectedEntities.Remove(selectable);
            }
        }


        // example (generics shenanigans)
        public void DeselectUnit(ISelectable<Unit> selectable)
        {
            if (selectedEntities.Contains(selectable))
            {
                selectable.GetMyReferenceWrapperGeneric().UnsubscribeOnClearance(() => DeselectEntity(selectable));
                selectedEntities.Remove(selectable);
            }
        }

    }
}