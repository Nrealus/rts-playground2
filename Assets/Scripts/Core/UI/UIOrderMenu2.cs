using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.MapMarkers;
using Core.Handlers;
using Core.Selection;
using Core.Tasks;
using Core.Units;
using System.Linq;
using Nrealus.Extensions;
using System;

namespace Core.UI
{
    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// Main UI class for the "Order" UI panel, allowing to "give orders". (i.e. spawn and place TaskMarkers on the map)
    /// </summary>   
    public class UIOrderMenu2 : MonoBehaviour
    {
    
        private static Camera _cam;
        public Camera GetMyCamera()
        {
            if (_cam == null)
                _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            return _cam;
        }

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