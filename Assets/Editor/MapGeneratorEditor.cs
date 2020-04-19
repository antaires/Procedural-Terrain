using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (mapGenerator))]
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI(){
        mapGenerator mapGenerator = (mapGenerator)target;

        if (DrawDefaultInspector() ){
            if (mapGenerator.autoUpdate){
                mapGenerator.GenerateMap();

            }
        }
         


        if(GUILayout.Button("Generate Map")){
            mapGenerator.GenerateMap();
        }
    }

}
