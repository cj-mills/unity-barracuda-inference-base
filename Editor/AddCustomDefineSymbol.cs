using UnityEditor;
using UnityEngine;

namespace CJM.BarracudaInference
{
    public class DependencyDefineSymbolAdder
    {
        private const string CustomDefineSymbol = "CJM_BARRACUDA_INFERENCE";

        [InitializeOnLoadMethod]
        public static void AddCustomDefineSymbol()
        {
            // Get the currently selected build target group
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            // Retrieve the current scripting define symbols for the selected build target group
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            // Check if the CustomDefineSymbol is already present in the defines string
            if (!defines.Contains(CustomDefineSymbol))
            {
                // Append the CustomDefineSymbol to the defines string, separated by a semicolon
                defines += $";{CustomDefineSymbol}";
                // Set the updated defines string as the new scripting define symbols for the selected build target group
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
                // Log a message in the Unity console to inform the user that the custom define symbol has been added
                Debug.Log($"Added custom define symbol '{CustomDefineSymbol}' to the project.");
            }
        }
    }
}
