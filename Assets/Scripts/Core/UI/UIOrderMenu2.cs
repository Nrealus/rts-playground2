using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.MapMarkers;
using Core.Handlers;
using Core.Selection;
using Core.Tasks;
using Core.Units;
using System.Linq;
using VariousUtilsExtensions;
using System;

namespace Core.UI
{
    public class UIOrderMenu2 : MonoBehaviour
    {
    
        private static Camera _cam;
        public Camera GetMyCamera()
        {
            if (_cam == null)
                _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            return _cam;
        }

        public GameObject editTasksParent;

        private Selector mySelector;

        private void Start()
        {
            mySelector = SelectionHandler.GetUsedSelector();
        }

        private void Update()
        {

        }

    }
}