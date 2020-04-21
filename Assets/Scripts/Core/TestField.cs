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
using UnityEngine.UIElements;


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

        Unit.SetAsSubordinateOf(GameObject.Find("Unit (3)").GetComponent<Unit>().GetMyWrapper(), GameObject.Find("Unit (2)").GetComponent<Unit>().GetMyWrapper());
        Unit.SetAsSubordinateOf(GameObject.Find("Unit (4)").GetComponent<Unit>().GetMyWrapper(), GameObject.Find("Unit (2)").GetComponent<Unit>().GetMyWrapper());
        Unit.SetAsSubordinateOf(GameObject.Find("Unit (2)").GetComponent<Unit>().GetMyWrapper(), GameObject.Find("Unit").GetComponent<Unit>().GetMyWrapper());
        Unit.SetAsSubordinateOf(GameObject.Find("Unit (1)").GetComponent<Unit>().GetMyWrapper(), GameObject.Find("Unit").GetComponent<Unit>().GetMyWrapper());
    }

    private void Awake()
    {
        //testSelector = GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector();
        unitsList = new List<ReferenceWrapper<Unit>>();
    }

    private void Start()
    {
        TestUnitHierarchySetup();
    }

    private void OnGUI() {
        GUI.TextArea(new Rect(10,10,196,32), Core.Handlers.TimeHandler.CurrentTimeToString());    
    }

    int orderChoice = 0;
    private void Update()
    {
        
        //Debug.Log(Core.Handlers.TimeHandler.HasTimeJustPassed(new Core.Handlers.TimeHandler.TimeStruct(9,15,21)));
        /*if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (UnitWrapper uw in GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector().GetCurrentlySelectedEntities())
            {
                uw.WrappedObject.Dismantle();
            }
        }*/

    }

}
