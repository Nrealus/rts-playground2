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
    
    public interface IActor : IDestroyable
    {
        
        List<IActorGroup> GetParticipatedGroups();

        List<TaskPlan2> GetParticipatedGroupsPlans();

        IActor GetParentActor();

        List<IActor> GetSubActors();

        void ChangeParentTo(IActor actor);

        /*void Internal_SetParentActor(IActor actor);

        void Internal_AddSubActor(IActor actor);

        void Internal_RemoveSubActor(IActor actor);*/
    }
    
}