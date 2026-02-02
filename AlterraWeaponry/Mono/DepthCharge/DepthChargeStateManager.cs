// This is not working due to PrefabIdentifier IDs not being consistent across game sessions.
// namespace VELD.AlterraWeaponry.Mono.DepthCharge
// {
//     public static class DepthChargeStateManager
//     {
//         private static DepthChargeState? _state;

//         private static void Initialize()
//         {
//             if (_state == null)
//             {
//                 Main.logger.LogInfo("Initializing DepthChargeStateManager and loading saved state.");
//                 _state = new DepthChargeState();
//                 _state.Load();
//             }
//         }

//         public static bool TryGetValue(PrefabIdentifier prefabIdentifier, out bool isPrimed)
//         {
//             if (_state == null)
//                 Initialize();

//             if (_state!.DepthCharges.TryGetValue(prefabIdentifier.Id, out isPrimed))
//             {
//                 Main.logger.LogInfo($"Loaded saved primed state for Depth Charge {prefabIdentifier.Id}: {isPrimed}");
//                 return true;
//             }

//             Main.logger.LogInfo($"No saved state found for Depth Charge {prefabIdentifier.Id}. Defaulting to unprimed.");
//             return false;
//         }

//         public static void UpdateValue(PrefabIdentifier prefabIdentifier, bool isPrimed)
//         {
//             if (_state == null)
//                 Initialize();

//             Main.logger.LogInfo($"Updating primed state for Depth Charge {prefabIdentifier.Id} to: {isPrimed}");
//             _state!.DepthCharges[prefabIdentifier.Id] = isPrimed;
//         }

//         public static void Save()
//         {
//             if (_state == null)
//                 Initialize();

//             Main.logger.LogInfo("Saving DepthChargeStateManager state.");
//             _state!.Save();
//         }

//         /// <summary>
//         /// Persists vehicle air bladder states across game sessions.
//         /// Maps vehicle instance IDs to their remaining air values.
//         /// </summary>
//         public class DepthChargeState : Nautilus.Json.ConfigFile
//         {
//             public DepthChargeState() : base("depthcharges")
//             {
//             }

//             /// <summary>
//             /// Dictionary mapping depth charge PrefabIdentifier ID to primed state.
//             /// Using PrefabIdentifier.Id ensures unique identification per depth charge across save files.
//             /// </summary>
//             public Dictionary<string, bool> DepthCharges { get; set; } = [];
//         }
//     }
// }
