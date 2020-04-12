using Core.Units;
using GlobalManagers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Selection;
using Core.Orders;
using Core.MapMarkers;
using Core.Helpers;

public class TestField : MonoBehaviour, IHasCameraRef
{

    public List<ReferenceWrapper<Unit>> unitsList;

    [SerializeField] private Camera _cam;
    public Camera GetMyCamera()
    {
        return _cam;
    }

    //private OrderFactory orderFactory = new OrderFactory();

    private Selector testSelector;

    private void TestUnitHierarchySetup()
    {
        GameObject.Find("Unit").GetComponent<Unit>().unitLevel = Unit.UnitLevel.Division;
        GameObject.Find("Unit (2)").GetComponent<Unit>().unitLevel = Unit.UnitLevel.Battalion;
        GameObject.Find("Unit (1)").GetComponent<Unit>().unitLevel = Unit.UnitLevel.Battalion;
        GameObject.Find("Unit (3)").GetComponent<Unit>().unitLevel = Unit.UnitLevel.Company;
        GameObject.Find("Unit (4)").GetComponent<Unit>().unitLevel = Unit.UnitLevel.Company;

        GameObject.Find("Unit (3)").GetComponent<Unit>().SetAsSubordinateOf(GameObject.Find("Unit (2)").GetComponent<Unit>());
        GameObject.Find("Unit (4)").GetComponent<Unit>().SetAsSubordinateOf(GameObject.Find("Unit (2)").GetComponent<Unit>());
        GameObject.Find("Unit (2)").GetComponent<Unit>().SetAsSubordinateOf(GameObject.Find("Unit").GetComponent<Unit>());
        GameObject.Find("Unit (1)").GetComponent<Unit>().SetAsSubordinateOf(GameObject.Find("Unit").GetComponent<Unit>());
    }

    private void Awake()
    {
        testSelector = GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector();
        unitsList = new List<ReferenceWrapper<Unit>>();
    }

    private void Start()
    {
        TestUnitHierarchySetup();
    }

    int orderChoice = 0;
    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (UnitWrapper uw in GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector().GetCurrentlySelectedEntities())
            {
                uw.WrappedObject.Dismantle();
            }
        }

    }

}
