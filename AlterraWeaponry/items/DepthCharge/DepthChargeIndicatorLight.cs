// namespace VELD.AlterraWeaponry.Items.DepthCharge;

// /// <summary>
// /// Handles the blinking indicator light on the depth charge.
// /// Changes behavior based on state: priming, armed, or collision detonation.
// /// </summary>
// public class DepthChargeIndicatorLight : MonoBehaviour
// {
//     private const float armedBlinkCycle = 3f; // Armed state: blink cycle
//     private const float blinkDuration = 0.3f; // How long the light is on per blink

//     private Light? indicatorLight;
//     private MeshRenderer? meshRenderer;
//     private Material? material;
//     private const float onIntensity = 5f;
//     private const float offIntensity = 0f;
//     private Color onEmission = Color.red * 3f; // Bright emission
//     private Color offEmission = Color.black; // No emission
//     private bool lastIsOn = false;

//     private DepthChargeBehavior? behavior;

//     private void Start()
//     {
//         indicatorLight = GetComponentInChildren<Light>();
//         meshRenderer = GetComponentInChildren<MeshRenderer>();
//         behavior = GetComponentInParent<DepthChargeBehavior>(); // Changed from GetComponent to GetComponentInParent

//         if (indicatorLight != null && meshRenderer != null && meshRenderer.material != null)
//         {
//             material = meshRenderer.material;
//             indicatorLight.intensity = offIntensity;
//             material.SetColor("_EmissionColor", offEmission);
//         }
//     }

//     private float lastLogTime = 0f;

//     private void Update()
//     {
//         if (indicatorLight == null || material == null || behavior == null)
//         {
//             if (Time.time - lastLogTime > 5f)
//             {
//                 Main.logger.LogWarning($"DepthChargeIndicatorLight Update - Missing components: Light={indicatorLight != null}, Material={material != null}, Behavior={behavior != null}");
//                 lastLogTime = Time.time;
//             }
//             return;
//         }

//         bool isOn = false;

//         // Use state machine to determine blink pattern
//         switch (behavior.CurrentState)
//         {
//             case DepthChargeState.Priming:
//                 // 3 beeps, 1 second apart
//                 float timeSincePrimingStart = Time.time - behavior.PrimingStartedAt;
//                 int currentBeep = Mathf.FloorToInt(timeSincePrimingStart);

//                 if (currentBeep < behavior.PrimingCountdown)
//                 {
//                     float timeIntoCurrentBeep = timeSincePrimingStart - currentBeep;
//                     isOn = timeIntoCurrentBeep < blinkDuration;
//                 }
//                 break;

//             case DepthChargeState.Armed:
//                 // Check if we're in the initial "just armed" sequence (3 quick beeps)
//                 float timeSinceArmed = Time.time - behavior.ArmedAt;
//                 float primedSequenceDuration = DepthChargeBehavior.PrimedBeepCount * DepthChargeBehavior.PrimedBeepInterval;

//                 if (timeSinceArmed < primedSequenceDuration)
//                 {
//                     // Play the "just armed" sequence (3 quick beeps)
//                     int primedBeep = Mathf.FloorToInt(timeSinceArmed / DepthChargeBehavior.PrimedBeepInterval);
//                     if (primedBeep < DepthChargeBehavior.PrimedBeepCount)
//                     {
//                         float timeIntoPrimedBeep = timeSinceArmed - (primedBeep * DepthChargeBehavior.PrimedBeepInterval);
//                         isOn = timeIntoPrimedBeep < (blinkDuration * 0.5f); // Shorter blinks for primed sequence
//                     }
//                 }
//                 else
//                 {
//                     // After primed sequence, switch to normal armed blink (once every 3 seconds)
//                     float cycleTime = Time.time % armedBlinkCycle;
//                     isOn = cycleTime < blinkDuration;
//                 }
//                 break;

//             case DepthChargeState.CollisionDetonating:
//                 // 5 rapid blinks with clear on/off pattern
//                 const float collisionBlinkDuration = 0.07f; // On for 0.07s, off for 0.03s

//                 float timeSinceCollision = Time.time - behavior.CollisionDetectedAt;
//                 int collisionBeep = Mathf.FloorToInt(timeSinceCollision / DepthChargeBehavior.CollisionBeepInterval);

//                 if (collisionBeep < DepthChargeBehavior.CollisionBeepCount)
//                 {
//                     float timeIntoCurrentBeep = timeSinceCollision - (collisionBeep * DepthChargeBehavior.CollisionBeepInterval);
//                     isOn = timeIntoCurrentBeep < collisionBlinkDuration;
//                 }
//                 break;

//             case DepthChargeState.Detonated:
//                 // Light is off
//                 isOn = false;
//                 break;
//         }

//         // Only update light if state changed
//         if (isOn != lastIsOn)
//         {
//             lastIsOn = isOn;
//             indicatorLight.intensity = isOn ? onIntensity : offIntensity;
//             material.SetColor("_EmissionColor", isOn ? onEmission : offEmission);
//         }
//     }
// }
