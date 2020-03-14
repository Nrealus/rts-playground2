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

public class TestField : MonoBehaviour
{

    public List<ReferenceWrapper<Unit>> unitsList;

    private Camera cam;

    public Vector3 pointedPosition;

    private OrderFactory orderFactory = new OrderFactory();

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
        cam = FindObjectOfType<Camera>();
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


    private RaycastHit hit;
    private void MousePointUpdate()
    {
        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
        {
            pointedPosition = hit.point;
        }
    }

}
