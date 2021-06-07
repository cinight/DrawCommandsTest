using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditmodeUpdate
{
    //Call this in OnGUI()
    public static void Update()
    {
        //Make sure editmode keeps running Update()
        #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
        #endif
    }
}
