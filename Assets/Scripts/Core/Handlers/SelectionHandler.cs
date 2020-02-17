using UnityEngine;
using Core.Selection;
using VariousUtilsExtensions;
using Core.Units;

namespace Core.Handlers
{
    public class SelectionHandler : MonoBehaviour
    {
        public Selector[] selectors;

        public Selector GetUsedSelector()
        {
            for (int r = 0; r < selectors.Length; r++)
            {
                if (selectors[r].isUsed)
                    return selectors[r];
            }
            Debug.LogError("no selector is in use");
            return null;
        }

        public Selector GetAppropriateSelectorForUnit(ReferenceWrapper<Unit> unitWrapper)
        {
            for (int r = 0; r < selectors.Length; r++)
            {
                if (selectors[r].selectorFaction == unitWrapper.WrappedObject.factionAffiliation)
                    return selectors[r];
            }
            return null;
        }

    }
}