using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CorruptPEAK
{
    public class CustomPropCheck : MonoBehaviourPunCallbacks
    {
        // idk why im doing this but i am 😭
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerListOthers)
            {
                if (player.CustomProperties.ContainsKey("CRPTPEAK") && !PhotonNetwork.IsMasterClient)
                {
                    if(player.IsMasterClient)
                    {
                        Notifications.SendOnce("<color=blue>[CorruptPEAK] : The host is using CorruptPEAK!</color>");
                    }
                    else
                    {
                        Notifications.SendOnce("<color=blue>[CorruptPEAK] : Another player is using CorruptPEAK!</color>");

                    }
                }
                if (player.CustomProperties.ContainsKey("CRPTPEAK") && PhotonNetwork.IsMasterClient)
                {
                    Notifications.SendOnce("<color=blue>[CorruptPEAK] : Another user is using CorruptPEAK!</color>");
                }
            }
        }
    }
}
