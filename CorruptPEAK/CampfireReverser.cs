using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CorruptPEAK
{
    // this doesnt do anything sorry
    internal class CampfireReverser
    {
        [HarmonyPatch(typeof(Campfire), "Update")]
        private class CampfireReverserPatch
        {
            static void Prefix(Campfire __instance)
            {
                if(__instance.Lit == true)
                {
                    Plugin.litthecampfiresostopcorruption = __instance.Lit;
                }
            }
        }
    }
}
