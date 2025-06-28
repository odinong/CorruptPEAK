using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CorruptPEAK
{
    internal class Notifications
    {
        private static readonly HashSet<string> sentMessages = new HashSet<string>();
        public static void SendOnce(string message)
        {
            if (sentMessages.Contains(message))
                return;

            if (TrySend(message))
            {
                sentMessages.Add(message);
            }
        }
        public static void Reset(string message)
        {
            sentMessages.Remove(message);
        }
        public static void ResetAll()
        {
            sentMessages.Clear();
        }

        private static bool TrySend(string message)
        {
            var target = GameObject.FindFirstObjectByType<PlayerConnectionLog>();
            if (target == null)
                return false;

            MethodInfo method = typeof(PlayerConnectionLog).GetMethod("AddMessage", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(target, new object[] { message });
                return true;
            }

            Debug.LogWarning("AddMessage method not found on PlayerConnectionLog.");
            return false;
        }
    }
}
