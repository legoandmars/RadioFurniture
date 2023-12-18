using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RadioFurniture.Events
{
    public static class WeatherEvents
    {
        public static event Action OnStormStarted;
        public static event Action OnStormEnded;

        public static void StormStart()
        {
            OnStormStarted?.Invoke();
            Debug.Log("Storm started!");
        }

        public static void StormEnd()
        {
            Debug.Log("Storm ended!");
            OnStormEnded?.Invoke();
        }
    }
}
