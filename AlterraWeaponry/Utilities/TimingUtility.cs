using System.Collections;
using UnityEngine;

namespace VELD.AlterraWeaponry.Utilities;

public static class TimingUtility
{
    /// <summary>
    /// Waits until the specified time (from Time.time) is reached
    /// </summary>
    /// <param name="startTime">The target time from Time.time</param>
    /// <param name="coroutineRunner">MonoBehaviour to run the coroutine on</param>
    /// <returns>A coroutine that waits until the specified time</returns>
    public static IEnumerator WaitUntilTime(float startTime, MonoBehaviour coroutineRunner)
    {
        while (Time.time < startTime)
        {
            yield return null;
        }
    }
}
