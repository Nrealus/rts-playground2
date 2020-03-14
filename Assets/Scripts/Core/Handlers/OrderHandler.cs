using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Orders;

namespace Core.Handlers
{
    public class OrderHandler : MonoBehaviour
    {

        public GameObject OrderGOPrefab;

        //public NPBehave.Clock ordersBTClock;

        //plebean implementation, this is actually a forest
        private List<OrderWrapper> orderWrappersList = new List<OrderWrapper>();
        //public List<Order> disposeOrdersList;

        private void Awake()
        {
            //ordersBTClock = new NPBehave.Clock();
            //ordersList = new List<Order>();
            //disposeOrdersList = new List<Order>();
        }


        private void Update()
        {
            //ordersBTClock.Update(Time.deltaTime);
            for (int i = orderWrappersList.Count - 1; i >= 0; i--)
            {
                orderWrappersList[i].UpdateOrderPhaseFSM();
            }

            /*if (Input.GetKeyDown(KeyCode.K))
            {
                foreach (OrderWrapper ow in orderWrappersList)
                {
                    if (ow.WrappedObject.IsInPhase(Order.OrderPhase.Pause))
                        ow.UnpauseExecution();
                    else
                        ow.PauseExecution();
                }
            }*/

            /*
            if (Input.GetKeyDown(KeyCode.U))
            {
                foreach (Order o in ordersList)
                {
                    Debug.Log(((IndividualMoveOrder)o).behaviourTree.CurrentState);
                }
            }
            */
        }

        public bool OrderWrapperRegistered(OrderWrapper wrapper)
        {
            return orderWrappersList.Contains(wrapper);                
        }

        /*
        public bool OrderWrapperFirstInQueue(OrderWrapper wrapper)
        {
            return CorrectPositionsOfOrderWrapper(wrapper)[1] == 0;
        }

        private int[] CorrectPositionsOfOrderWrapper(OrderWrapper wrapper)
        {
            for (int i = 0; i < orderWrappersList.Count; i++)
            {
                for (int j = 0; j < orderWrappersList[i].Count; j++)
                {
                    if(orderWrappersList[i][j] == wrapper)
                    {
                        return new int[] {i,j};                        
                    }
                }
            }
            return new int[] {-1,-1};
        }*/

        public bool AddToOrderWrapperList(OrderWrapper wrapper)
        {
            if(!OrderWrapperRegistered(wrapper))
            {
                orderWrappersList.Add(wrapper);
                return true;
            }
            else
                return false;
        }

        public bool RemoveFromOrderWrapperList(OrderWrapper wrapper)
        {
            if(OrderWrapperRegistered(wrapper))
            {
                orderWrappersList.Remove(wrapper);
                return true;
            }
            else
            {
                return false;
            }
        }

        

        /*private void Update()
        {
            if (state == 0 && Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                selectedUnitsList.Clear();
                int c = unitsRoot.childCount;
                for (int i = 0; i < c; i++)
                {
                    selectedUnitsList.Add(unitsRoot.GetChild(i).GetComponent<Unit>().myWrapper);
                }
            }

            if (state == 0 && Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (selectedUnitsList.Count > 0)
                    selectedUnitsList.RemoveAt(0);
            }

            if (state == 0 && Input.GetKeyDown(KeyCode.C))
            {
                if (selectedUnitsList.Count > 0)
                    state = 1;
            }

            if (state == 1)
            {
                if (Input.GetKeyDown(KeyCode.Keypad1))
                {
                    state = 2;
                    whatOrder = "Move";
                }
            }

            if (state == 2)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    state = 3;
                    if (whatOrder == "Move")
                    {

                    }
                }
            }

            if (state == 3)
            {
                if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    state = 0;
                    if (whatOrder == "Move")
                    {
                        CreateMoveOrder(selectedUnitsList, new List<Vector3>() { new Vector3(5, 0, 0), new Vector3(0, 5, 0) });
                    }
                    whatOrder = "";
                }
            }

        }
        */

        /*
        uiState.AddState(State.Choose,
            () => {
                UtilsAndExtensions.UIUtils.SetUIActive(gameObject, true);
                DisplayActions();
                InputContext.ChangeContext(InputContext.Context.MenuSelectionCombat);
            },
            () => {
                SelectButton(currentButton);
                if (InputContext.commands[InputContext.Command.MoveUp])
                    SelectButton(currentButton-1);
                if (InputContext.commands[InputContext.Command.MoveDown])
                    SelectButton(currentButton+1);
                if (InputContext.commands[InputContext.Command.StartOrConfirmSelection])
                    buttonList[currentButton].Press();
            },
            null);

        uiState.AddState(State.Select,
            () => {
                UtilsAndExtensions.UIUtils.SetUIActive(gameObject, false);
                NextSelection(initialSelection: true);
            },
            () => {
                UpdateSelections();
                DrawSelections();
            },
            () => {
                // The action may have been cancelled
                if (currentAction.name != "")
                    actionsChosen.Add(currentAction);

                currentAction.name = "";
            });

        uiState.CurrentState = State.Choose;

        private void CreateIndividualMoveOrder(IOrderable<Unit> orderedUnitWrapper, List<Vector3> waypointsList)
        {
            IndividualMoveOrder mvComm = new IndividualMoveOrder();
            mvComm.SetOrderReceiver(orderedUnitWrapper);
            mvComm.AddWaypoints(waypointsList);
        }
        */

    }
}