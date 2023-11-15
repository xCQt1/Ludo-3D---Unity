using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

class Debug : UnityEngine.Debug {


    new public static void Log(object message) {
        if (GameHandler.Instance.VerboseLogging) UnityEngine.Debug.Log(message);
    }
}