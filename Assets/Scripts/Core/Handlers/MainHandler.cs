using Core.Helpers;
using System;
using UnityEngine;

namespace Core.Handlers
{

    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

    /// <summary>
    /// Main singleton overseeing all the other ones.
    /// Its use should not be too extensive, it is easy to fall to the temptation of abusing the singleton pattern.
    /// For now, it isn't really used.
    /// </summary>    
    //[RequireComponent(typeof(ParticularHandler1), typeof(ParticularHandler2), typeof(ParticularHandler3))]
    public class MainHandler : MonoBehaviour
    {
        
        private static MainHandler _instance;
        private static MainHandler MyInstance
        {
            get
            {
                if(_instance == null)
                    _instance = FindObjectOfType<MainHandler>(); 
                return _instance;
            }
        }

        public UnitsRoot unitsRoot { get; private set; }

        //public SelectionHandler selectionHandler { get; private set; }
        //public OrderHandler orderHandler { get; private set; }
        //public TimeHandler timeHandler { get; private set; }
        
        ///<summary>
        /// Called from the monobehaviour awake of the GameManager singleton.
        /// There needs to be a stable order in initializations, which is why we use this instead of Start and Awake to initialize stuff.
        /// </summary>
        public void Init()
        {
            //selectionHandler = GetComponent<SelectionHandler>();
            //orderHandler = GetComponent<OrderHandler>();
            //timeHandler = GetComponent<TimeHandler>();

            unitsRoot = FindObjectOfType<UnitsRoot>();
        }

        private void Update()
        {

        }

    }
}