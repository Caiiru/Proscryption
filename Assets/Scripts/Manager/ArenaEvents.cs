using System;
using UnityEngine;

namespace proscryption
{
    public class ArenaEvents : MonoBehaviour
    {
        public static Action OnArenaStart;
        public static Action OnArenaWaveStart;
        public static Action OnArenaWaveEnded;

        public static void BroadcastArenaStart()
        {
            OnArenaStart?.Invoke();
        }
        public static void BroadcastWaveStart()
        {
            OnArenaWaveStart?.Invoke();
        }
        public static void BroadcastArenaWaveEnded()
        {
            OnArenaWaveEnded?.Invoke();
        }
    }
}
