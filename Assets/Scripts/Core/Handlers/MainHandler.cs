using Core.Helpers;
using System;
using UnityEngine;

namespace Core.Handlers
{

    //[RequireComponent(typeof(ParticularHandler1), typeof(ParticularHandler2), typeof(ParticularHandler3))]
    public class MainHandler : MonoBehaviour
    {

        public UnitsRoot unitsRoot { get; private set; }

        public SelectionHandler selectionHandler { get; private set; }
        public OrderHandler orderHandler { get; private set; }
        public TimeHandler timeHandler { get; private set; }
        ///<summary>
        /// Called from the monobehaviour awake of the GameManager singleton.
        /// There needs to be a stable order in initializations, which is why we use this instead of Start and Awake to initialize stuff.
        /// </summary>
        public void Init()
        {
            selectionHandler = GetComponent<SelectionHandler>();
            orderHandler = GetComponent<OrderHandler>();
            timeHandler = GetComponent<TimeHandler>();

            unitsRoot = FindObjectOfType<UnitsRoot>();
        }

        private void Update()
        {

        }

    }
}