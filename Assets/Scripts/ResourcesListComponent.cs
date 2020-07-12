using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.MapMarkers;
using Core.UI;
using Michsky.UI.ModernUIPack;

public class ResourcesListComponent : MonoBehaviour
{
    
    public WaypointMarker waypointMarkerPrefab;
    //public TaskMarker orderMarkerPrefab;
    public DeployableMarker buildingMarkerPrefab;
    public FirePositionMarker firePositionMarkerPrefab;

    public Component taskMarkerPrefab;
    //public MoveOrderMarker moveOrderMarkerButtonPrefab;

    public UIOrderMenu orderMenuPrefab;

    public UIOrdobTreeViewElement uiOrdobTreeViewElementPrefab;

}
