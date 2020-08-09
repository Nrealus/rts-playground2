using System.Collections;
using System.Collections.Generic;
using Core.MapMarkers;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.ReferenceWrapper;
using Core.Helpers;
using Core.Tasks;

namespace Core.Units
{
    public interface IActorGroup : IDestroyable
    {

        List<IActor> GetActors();
        
        TaskPlan2 CreateAndRegisterNewOwnedPlan();

        void UnregisterOwnedPlan(TaskPlan2 taskPlan);

        List<TaskPlan2> GetThisGroupPlans();

        List<TaskPlan2> GetActorsGroupsPlans();

        List<TaskPlan2> GetSubGroupsPlans(bool includeThisGroup, bool recursive);

        List<TaskPlan2> GetActorsGroupsAndSubGroupsPlans(bool includeThisGroup, bool recursive);

        IActorGroup GetParentGroup();

        List<IActorGroup> GetSubGroups();

        void ChangeParentTo(IActorGroup actorGroup);

        /*void Internal_SetParentGroup(IActorGroup actorGroup);

        void Internal_AddSubGroup(IActorGroup actorGroup);

        void Internal_RemoveSubGroup(IActorGroup actorGroup);*/

    }

}