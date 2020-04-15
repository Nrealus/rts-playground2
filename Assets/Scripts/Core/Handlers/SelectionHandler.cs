using UnityEngine;
using Core.Selection;
using VariousUtilsExtensions;
using Core.Units;

namespace Core.Handlers
{
    public class SelectionHandler : MonoBehaviour
    {

        private static SelectionHandler _instance;
        private static SelectionHandler MyInstance
        {
            get
            {
                if(_instance == null)
                    _instance = FindObjectOfType<SelectionHandler>(); 
                return _instance;
            }
        }

        [SerializeField]
        private Selector[] selectors;

        public static Selector GetUsedSelector()
        {
            for (int r = 0; r < MyInstance.selectors.Length; r++)
            {
                if (MyInstance.selectors[r].isUsed)
                    return MyInstance.selectors[r];
            }
            Debug.LogError("no selector is in use");
            return null;
        }

        public static Selector GetAppropriateSelectorForUnit(ReferenceWrapper<Unit> unitWrapper)
        {
            for (int r = 0; r < MyInstance.selectors.Length; r++)
            {
                if (MyInstance.selectors[r].selectorFaction == unitWrapper.WrappedObject.factionAffiliation)
                    return MyInstance.selectors[r];
            }
            return null;
        }

    }
}