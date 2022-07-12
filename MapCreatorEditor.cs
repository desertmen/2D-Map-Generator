using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapCreator))]
public class MapCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapCreator mapCreator = (MapCreator)target;
        DrawDefaultInspector();
        EditorGUILayout.LabelField("MapSize(Optional)");
        // show map width and height field and add it to mapSize array
        mapCreator.mapSize[0] = EditorGUILayout.IntField(
            "Map Width",
            mapCreator.mapSize[0]);

        mapCreator.mapSize[1] = EditorGUILayout.IntField(
            "Map Height",
            mapCreator.mapSize[1]);

        // correct inputs
        if (GUI.changed)
        {
            mapCreator.correctInputs();
        }

        // add create map button
        if (GUILayout.Button("Create New Map"))
        {
            mapCreator.generateMap();
        }

        mapCreator.testSize = EditorGUILayout.IntField(
            "Test Size",
            mapCreator.testSize);

        if(GUILayout.Button("Test Room Generator Settings"))
        {
            mapCreator.testMapSize();
        }
        if (GUILayout.Button("Test Map Generator"))
        {
            mapCreator.testMapGeneration();
        }
    }
}
