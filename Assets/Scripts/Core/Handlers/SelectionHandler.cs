using UnityEngine;
using Core.Selection;
using Nrealus.Extensions;
using Core.Units;

namespace Core.Handlers
{
    public class SelectionHandler : MonoBehaviour
    {

        /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

        /// <summary>
        /// Singleton used to oversee selectors and other general selection related things the scale of the game.
        /// For now, its only use is to return an appropriate selector instance on request.
        /// Criteria for a selector to return could include a faction or a player, for example.
        /// </summary>    
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

    }
}