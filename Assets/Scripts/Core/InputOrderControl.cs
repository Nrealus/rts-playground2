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
using Gamelogic.Extensions;

public class InputOrderControl : MonoBehaviour
{

    public List<ReferenceWrapper<Unit>> unitsList;

    private Camera cam;

    public Vector3 pointedPosition;

    private OrderFactory orderFactory = new OrderFactory();

    private OrderWrapper currentlyEditedOrderWrapper;
    private List<OrderWrapper> currentlyEditedOrderWrappersList = new List<OrderWrapper>();
    //private OrderGroupWrapper currentlyEditedOrderGroupWrapper;

    private List<OrderWrapper> GetCurrentlyEditedOrderWrappers()
    {
        return currentlyEditedOrderWrappersList;
    }

    private Selector testSelector;

    public UnityEngine.UI.Button sampleButtonPrefab;

    private void Awake()
    {
        testSelector = GameManager.Instance.currentMainHandler.selectionHandler.GetUsedSelector();
        unitsList = new List<ReferenceWrapper<Unit>>();
        cam = FindObjectOfType<Camera>();
    }


    private enum ChoiceStates { Neutral, OrderChoice, 
        MoveOrderSelection, MoveOrderSelectionUpdate,
        OrderOptions, OrderOptionsDuringExec, OrderOptionsDuringEditing,
        OrderConfirm , OrderCancel, }

    private PushdownAutomaton<ChoiceStates> controlPhasesFSM;

    private void Start()
    {
        controlPhasesFSM = new PushdownAutomaton<ChoiceStates>();
        controlPhasesFSM.AddState(ChoiceStates.Neutral,
            () => {
                testSelector.activeSelector = true;
            },
            () => {
                //foreach (Transform u in GameManager.Instance.currentMainHandler.unitsRoot.transform)
                //    UnitTesting(u.GetComponent<Unit>());

                if (Input.GetMouseButtonDown(1))
                {
                    controlPhasesFSM.CurrentState = ChoiceStates.OrderChoice;
                }
            },
            () => {
                testSelector.activeSelector = false;
            }
        );
        UnityEngine.UI.Button bbu1 = null;
        UnityEngine.UI.Button bbu2 = null;
        controlPhasesFSM.AddState(ChoiceStates.OrderChoice,
            () => {
                bbu1 = Instantiate<UnityEngine.UI.Button>(sampleButtonPrefab, FindObjectOfType<Canvas>().transform);
                bbu1.transform.position = pointedPosition - Vector3.right * 5;
                bbu1.GetComponentInChildren<UnityEngine.UI.Text>().text = "Options";                

                bbu2 = Instantiate<UnityEngine.UI.Button>(sampleButtonPrefab, FindObjectOfType<Canvas>().transform);
                bbu2.transform.position = pointedPosition + Vector3.right * 5;
                bbu2.GetComponentInChildren<UnityEngine.UI.Text>().text = "Move";
                
                bbu1.onClick.AddListener(
                    () => {
                        controlPhasesFSM.CurrentState = ChoiceStates.OrderOptionsDuringEditing;
                        bbu1.onClick.RemoveAllListeners();
                        bbu2.onClick.RemoveAllListeners();
                        Destroy(bbu1.gameObject);
                        Destroy(bbu2.gameObject);
                    }
                );

                bbu2.onClick.AddListener(
                    () => {
                        controlPhasesFSM.CurrentState = ChoiceStates.MoveOrderSelection;
                        bbu1.onClick.RemoveAllListeners();
                        bbu2.onClick.RemoveAllListeners();
                        Destroy(bbu1.gameObject);
                        Destroy(bbu2.gameObject);
                    }
                );
            },
            () => {
                /*if(Input.GetKeyDown(KeyCode.M))
                {
                    controlPhasesFSM.CurrentState = ChoiceStates.MoveOrderSelection;
                }*/
                if(Input.GetMouseButtonDown(1))
                {
                    controlPhasesFSM.CurrentState = ChoiceStates.Neutral;
                    
                    // actually useless
                    bbu1.onClick.RemoveAllListeners();
                    bbu2.onClick.RemoveAllListeners();
                    Destroy(bbu1.gameObject);
                    Destroy(bbu2.gameObject);
                }
            }
        );
        controlPhasesFSM.AddState(ChoiceStates.MoveOrderSelection,
            () => {
                //List<IOrderable<Unit>> orderReceivers = new List<IOrderable<Unit>>();
                //List<OrderWrapper> orderWrappers = new List<OrderWrapper>();
                
                var l = testSelector.GetCurrentlySelectedEntities();
                foreach (var v in l)
                {
                    //orderReceivers.Add((IOrderable<Unit>)v);
                    orderFactory.CreateOrderWrapperAndSetReceiver<MoveOrder>((IOrderable<Unit>)v);
                    GetCurrentlyEditedOrderWrappers().Add(v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().GetLastAddedOrder());
                }

                //currentlyEditedOrderWrapper = orderFactory.CreateOrderWrapperAndSetReceiver<MoveOrder>(converted[0]);

                //currentlyEditedOrderGroupWrapper = orderFactory.CreateOrderGroup();
                //currentlyEditedOrderGroupWrapper.AddChildOrderWrappers(orderWrappers);

                controlPhasesFSM.CurrentState = ChoiceStates.MoveOrderSelectionUpdate;
            }
        );
        controlPhasesFSM.AddState(ChoiceStates.MoveOrderSelectionUpdate,
            () => {
                
            },
            () => {
                
                if (Input.GetMouseButtonDown(0)
                    // cringeee
                    && NoObjectOfTypeUnderPosition<WaypointMarkerTransform>(Input.mousePosition, 10))
                {
                    //wps.Add(pointedPosition);            
                    
                    //var wpmrk = new WaypointMarker(pointedPosition);
                    //currentlyEditedOrderWrapper.GetWrappedAs<MoveOrder>()
                    //    .AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());

                    WaypointMarker wpmrk;
                    int c = GetCurrentlyEditedOrderWrappers().Count;
                    for(int i = 0; i < c; i++)
                    {
                        wpmrk = new WaypointMarker(
                            pointedPosition - Vector3.right * 10 * i / c);
                        GetCurrentlyEditedOrderWrappers()[i]
                            .GetWrappedAs<MoveOrder>()
                            .AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());
                    }

                }
                
                if (Input.GetMouseButtonDown(1))
                {
                    controlPhasesFSM.CurrentState = ChoiceStates.OrderOptionsDuringEditing;
                }
                
                /*if (Input.GetKeyDown(KeyCode.Space) && currentlyEditedOrderGroupWrapper != null)
                {
                    controlPhasesFSM.CurrentState = ChoiceStates.OrderConfirm;
                }*/
                
            }
        );
        UnityEngine.UI.Button bu2opt = null;
        UnityEngine.UI.Button bu3opt = null;
        controlPhasesFSM.AddState(ChoiceStates.OrderOptionsDuringEditing,
            () => {
                /*var bu = Instantiate<UnityEngine.UI.Button>(sampleButtonPrefab, FindObjectOfType<Canvas>().transform);
                bu.transform.position = pointedPosition - Vector3.right * 5;
                bu.GetComponentInChildren<UnityEngine.UI.Text>().text = "Back";                
                */
                bu2opt = Instantiate<UnityEngine.UI.Button>(sampleButtonPrefab, FindObjectOfType<Canvas>().transform);
                bu2opt.transform.position = pointedPosition - Vector3.right * 5;
                bu2opt.GetComponentInChildren<UnityEngine.UI.Text>().text = "Cancel";                
                
                bu3opt = Instantiate<UnityEngine.UI.Button>(sampleButtonPrefab, FindObjectOfType<Canvas>().transform);
                bu3opt.transform.position = pointedPosition + Vector3.right * 5;
                bu3opt.GetComponentInChildren<UnityEngine.UI.Text>().text = "Confirm";
                
                /*bu.onClick.AddListener(
                    () => {
                    
                        controlPhasesFSM.CurrentState = ChoiceStates.Neutral;

                        bu.onClick.RemoveAllListeners();
                        bu2.onClick.RemoveAllListeners();
                        bu3.onClick.RemoveAllListeners();
                        Destroy(bu.gameObject);
                        Destroy(bu2.gameObject);
                        Destroy(bu3.gameObject);
                    }
                );*/

                bu2opt.onClick.AddListener(
                    () => {
                    
                        //controlPhasesFSM.CurrentState = ChoiceStates.OrderChoice;
                        controlPhasesFSM.CurrentState = ChoiceStates.Neutral;
                        int c = GetCurrentlyEditedOrderWrappers().Count;
                        for(int i = c-1; i >= 0; i--)
                        {
                            GetCurrentlyEditedOrderWrappers()[i].DestroyWrappedReference();
                            GetCurrentlyEditedOrderWrappers().RemoveAt(i);
                        }

                        //bu.onClick.RemoveAllListeners();
                        bu2opt.onClick.RemoveAllListeners();
                        bu3opt.onClick.RemoveAllListeners();
                        //Destroy(bu.gameObject);
                        Destroy(bu2opt.gameObject);
                        Destroy(bu3opt.gameObject);
                    }
                );

                bu3opt.onClick.AddListener(
                    () => {
                        if(GetCurrentlyEditedOrderWrappers().Count > 0)
                        {
                            controlPhasesFSM.CurrentState = ChoiceStates.OrderConfirm;
                            //controlPhasesFSM.CurrentState = ChoiceStates.Neutral;
                            //bu.onClick.RemoveAllListeners();
                            bu2opt.onClick.RemoveAllListeners();
                            bu3opt.onClick.RemoveAllListeners();
                            //Destroy(bu.gameObject);
                            Destroy(bu2opt.gameObject);
                            Destroy(bu3opt.gameObject);
                        }
                    }
                );
            },
            () => {
                if(Input.GetMouseButtonDown(1)) 
                {
                    controlPhasesFSM.CurrentState = ChoiceStates.MoveOrderSelectionUpdate;
                
                    //bu.onClick.RemoveAllListeners();
                    bu2opt.onClick.RemoveAllListeners();
                    bu3opt.onClick.RemoveAllListeners();
                    //Destroy(bu.gameObject);
                    Destroy(bu2opt.gameObject);
                    Destroy(bu3opt.gameObject);
                }
                /*if(Input.GetMouseButtonDown(1)) 
                {
                    controlPhasesFSM.CurrentState = ChoiceStates.OrderChoice;
                    //currentlyEditedOrderWrapper.DestroyWrappedReference();
                    //currentlyEditedOrderWrapper = null;
                    currentlyEditedOrderGroupWrapper.DestroyWrappedReference();
                    currentlyEditedOrderGroupWrapper = null;
                }
                if(Input.GetMouseButtonDown(2)) 
                {
                    controlPhasesFSM.CurrentState = ChoiceStates.Neutral;
                }
                */
            }
        );
        controlPhasesFSM.AddState(ChoiceStates.OrderConfirm,
            () => {
                int c = GetCurrentlyEditedOrderWrappers().Count;
                for(int i = c-1; i >= 0; i--)
                {
                    if (GetCurrentlyEditedOrderWrappers()[i].GetConfirmationFromReceiver())
                    {
                        GetCurrentlyEditedOrderWrappers()[i].TryStartExecution();
                        GetCurrentlyEditedOrderWrappers().RemoveAt(i);
                        controlPhasesFSM.CurrentState = ChoiceStates.Neutral;
                    }
                }
            },
            () => {

            }
        );
        controlPhasesFSM.AddState(ChoiceStates.OrderCancel,
            () => {

            },
            () => {
                
            }
        );
        controlPhasesFSM.CurrentState = ChoiceStates.Neutral;
    }

    int orderChoice = 0;
    private void Update()
    {
        MousePointUpdate();
        
        controlPhasesFSM.Update();

    }

    private RaycastHit hit;
    private void MousePointUpdate()
    {
        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
        {
            pointedPosition = hit.point;
        }
    }


    // I'll find a better way at some point lol
    private bool NoObjectOfTypeUnderPosition<T>(Vector3 position, float distance) where T : MonoBehaviour
    {
        
        var objectList = FindObjectsOfType<T>();

        Vector3 sp;
        foreach (var o in objectList)
        {
            sp = cam.WorldToScreenPoint(o.transform.position);
            sp.z = 0;
            if((position - sp).magnitude < distance)
            {
                return false;
            }            
        }
        return true;

    }   


}
