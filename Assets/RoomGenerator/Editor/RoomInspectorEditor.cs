using UnityEngine;
using System.Collections;
using UnityEditor;
using RogueLike;

[CustomEditor(typeof(RoomInspector))]    
public class RoomInspectorEditor : Editor
{

    public override void OnInspectorGUI()
    { 
        RoomInspector room = target as RoomInspector; 
        EditorGUILayout.TextField("ResPath", room.roomData.GetPath());
        for(int i = 0; i < room.roomData.gridList.Count; ++i)
        {
            EditorGUILayout.TextField("Grid" + i, "Index:" + room.roomData.gridList[i].index + ", Pos:" + room.roomData.gridList[i].position);
        }
        
        base.DrawDefaultInspector();
    }
}

[CustomEditor(typeof(DoorInspector))]
public class DoorInspectorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DoorInspector door = target as DoorInspector;
        if (door.doorData.associateDoor != null)
        {
            EditorGUILayout.ObjectField("AssociateDoor", door.doorData.associateDoor.transform, typeof(Transform), true);
            EditorGUILayout.Vector3Field("Position", door.doorData.position);
            EditorGUILayout.Vector3Field("TransportPos", door.doorData.transportPos);
        }
        
        base.DrawDefaultInspector();
    }
}