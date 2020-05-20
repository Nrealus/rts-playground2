using System.Collections;
using System.Collections.Generic;
using Core.Handlers;
using UnityEngine;

namespace Core.Orders
{
    public abstract class Order2 : Order
    {

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(GetMyWrapper(), OrderPhase.Staging))
            {
                if(GetReceiver(GetMyWrapper()).GetOrdersPlan().IsFirstInlineActiveOrderInPlan(GetMyWrapper()))
                {
                    SetPhase(GetMyWrapper(), OrderPhase.ExecutionWaitingTimeToStart);
                    return true;
                }
                else
                {
                    if (InstanceGetOrderParams().ContainsExecutionMode(OrderParams.OrderExecutionMode.InstantOverrideAll))
                    {
                        // "delete" orders that start after the starting time of this order
                        foreach (var ow in GetReceiver(GetMyWrapper()).GetOrdersPlan().GetAllActiveOrdersFromPlan())
                        {
                            if (ow != GetMyWrapper() && GetReceiver(GetMyWrapper()).GetOrdersPlan().IsActiveOrderInPlanAfterOther(GetMyWrapper(), ow))
                            {
                                EndExecution(ow);
                            }
                            else
                            {
                                break;
                            }
                        }
                        
                        if (GetReceiver(GetMyWrapper()).GetOrdersPlan().GetFirstInlineActiveOrderInPlan() != null)
                        {
                            SetPhase(GetMyWrapper(), Order.OrderPhase.ExecutionWaitingTimeToStart);  
                            return true;
                        }
                        
                        return false;
                    }
                }
            }

            return false;
        }


        protected override void OrderPhasesFSMInit()
        {
            
            orderPhasesFSM.AddState(OrderPhase.Initial);

            orderPhasesFSM.AddState(OrderPhase.Staging);

            orderPhasesFSM.AddState(OrderPhase.ExecutionWaitingTimeToStart,
                () =>
                {
                    if(!Order.GetParameters(GetMyWrapper()).plannedStartingTime.isInitialized)
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);
                    }
                    else if (TimeHandler.HasTimeJustPassed(Order.GetParameters(GetMyWrapper()).plannedStartingTime))
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);                
                    }
                },
                () =>
                {
                    if(TimeHandler.HasTimeJustPassed(Order.GetParameters(GetMyWrapper()).plannedStartingTime))
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);
                    }
                });

            orderPhasesFSM.AddState(OrderPhase.Execution,
                () =>
                {
                    GetParameters(GetMyWrapper()).plannedStartingTime = TimeHandler.CurrentTime();
                },
                () =>
                {
                    UpdateExecution();
                });

            orderPhasesFSM.AddState(OrderPhase.Pause);

            orderPhasesFSM.AddState(OrderPhase.Cancelled,
                () =>
                {
                    
                },
                () =>
                {

                });

            orderPhasesFSM.AddState(OrderPhase.End,
               () =>
               {

               },
               () =>
               {
                    SetPhase(GetMyWrapper(), OrderPhase.End2);
               });

            
            bool waitForReactionAtEnd = false;
            OrderWrapper nextActiveOrder = null;
            
            orderPhasesFSM.AddState(OrderPhase.End2,
            () =>
            {
                if(Order.GetParameters(GetMyWrapper()).ContainsExecutionMode(OrderParams.OrderExecutionMode.WaitForReactionAtEnd))
                {
                    waitForReactionAtEnd = true;
                }
                
                if (GetReceiver(GetMyWrapper()).GetOrdersPlan().GetNextInlineActiveOrderInPlan(GetMyWrapper()) != null
                    && GetParameters(GetReceiver(GetMyWrapper()).GetOrdersPlan().GetNextInlineActiveOrderInPlan(GetMyWrapper())).ContainsExecutionMode(OrderParams.OrderExecutionMode.Chain))
                {
                    nextActiveOrder = InstanceGetOrderReceiver().GetOrdersPlan().GetNextInlineActiveOrderInPlan(GetMyWrapper());
                }

            },
            () =>
            {
                if (waitForReactionAtEnd == false)
                    SetPhase(GetMyWrapper(), OrderPhase.Disposed);
            });

            orderPhasesFSM.AddState(OrderPhase.Disposed,
            () =>
            {
            
                GetMyWrapper().DestroyWrappedReference();

                if (nextActiveOrder != null && nextActiveOrder.WrappedObject != null)
                    Order.TryStartExecution(nextActiveOrder);

            });
               
            orderPhasesFSM.CurrentState = OrderPhase.Initial;
        }

        protected abstract void UpdateExecution();

    }
}
