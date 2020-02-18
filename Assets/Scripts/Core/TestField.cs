using Core.Units;
using GlobalManagers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Selection;
using Core.Orders;

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

    /*
    public void AddWrapperToList(ReferenceWrapper<Unit> wrapper)
    {
        if (!unitsList.Contains(wrapper))
        {
            unitsList.Add(wrapper);
            wrapper.SubscribeOnClearance(() => RemoveWrapperFromList(wrapper)); // separate method + caching ? (of the lambda expression)
        }
    }

    // situation where would need to remove the wrapper from a list without "destroying" the object.
    public void RemoveWrapperFromList(ReferenceWrapper<Unit> wrapper)
    {
        wrapper.UnsubscribeOnClearance(() => RemoveWrapperFromList(wrapper));
        unitsList.Remove(wrapper);
    }
    */
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //Debug.Log(unitsList.Count);
            //Debug.Log(GameManager.Instance.currentMainHandler.orderHandler.ordersList.Count);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            unitsList[0].DestroyWrappedReference();
        }

        foreach (Transform u in GameManager.Instance.currentMainHandler.unitsRoot.transform)
            UnitTesting(u.GetComponent<Unit>());

        MousePointUpdate();

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            //u.AddOrderToQueue(GameManager.Instance.currentMainHandler.orderHandler.Cr)

            /*   
            movOrd = new MoveOrder();
            movOrdWrp = movOrd.GetMyWrapper();

            movOrdWrp.WrappedObject.SetOrderReceiver(u.GetMyWrapper());
            movOrdWrp.WrappedObject.AddWaypoints(wps);
            */
            /*
            */
            //movOrdWrp = orderFactory.CreateOrderAndSetReceiver<MoveOrder>(u.GetMyWrapper());
            List<IOrderable<Unit>> converted = new List<IOrderable<Unit>>();
            var l = testSelector.GetCurrentlySelectedEntities();
            foreach (var v in l)
                converted.Add((IOrderable<Unit>)v);
            movOrdWrp = orderFactory.CreateOrderWrapperAndSetReceiver<MoveOrder>(converted[0]);

            movOrdGrpWrp = orderFactory.CreateOrderGroup();
            Debug.Log(movOrdGrpWrp);
            movOrdGrpWrp.AddChildOrderWrapper(movOrdWrp);
        }

    }

    private OrderWrapper movOrdWrp;
    private OrderGroupWrapper movOrdGrpWrp;
    private List<Vector3> wps = new List<Vector3>();
    private void UnitTesting(Unit u)
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector().SelectEntity(Selection.activeGameObject.GetComponent<Unit>().GetMyWrapper());
            /*foreach (UnitWrapper ch in Selection.activeGameObject.GetComponent<Unit>().GetSubordinateUnitsWrappersList())
            {
                GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector().SelectUnit(ch);
            }*/
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector().DeselectEntity(Selection.activeGameObject.GetComponent<Unit>().GetMyWrapper());
            /*foreach (UnitWrapper ch in Selection.activeGameObject.GetComponent<Unit>().GetSubordinateUnitsWrappersList())
            {
                GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector().DeselectUnit(ch);
            }*/
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (Unit v in GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector().GetCurrentlySelectedEntities())
                v.Dismantle();
        }

        if (u.subTesting && Input.GetKeyDown(KeyCode.S))
        {
            u.SetAsSubordinateOf(Selection.activeGameObject.GetComponent<Unit>());
        }

        if (u.subTesting && Input.GetKeyDown(KeyCode.Q))
        {
            foreach (var v in u.GetAllSubordinateUnitsList())
                Debug.Log(v.gameObject.name);
        }
        /*
        if (u.subTesting && Input.GetKeyDown(KeyCode.Backspace))
        {
            //u.AddOrderToQueue(GameManager.Instance.currentMainHandler.orderHandler.Cr)

            movOrdWrp = orderFactory.CreateOrder<MoveOrder>();
            movOrdWrp.SetOrderReceiver(u.GetMyWrapper());
        }
        */
        if (u.subTesting && Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            //wps.Add(pointedPosition);
            ((OrderWrapper<MoveOrder>)movOrdWrp).ConvertWrappedToT().AddWaypoint(pointedPosition);
        }

        if (u.subTesting && Input.GetKeyDown(KeyCode.Space) && movOrdWrp.WrappedObject != null)
        {
            movOrdWrp.GetConfirmationFromReceiver();
            Debug.Log(movOrdWrp.WrappedObject.IsInPhase(Order.OrderPhase.AllGoodToStartExecution));
        }

        if (u.subTesting && Input.GetKeyDown(KeyCode.KeypadEnter) && movOrdWrp.WrappedObject != null)
        {
            movOrdWrp.TryStartExecution();
            movOrdWrp = null;
            wps.Clear();
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
