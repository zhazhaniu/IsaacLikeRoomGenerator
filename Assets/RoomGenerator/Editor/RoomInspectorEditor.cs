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