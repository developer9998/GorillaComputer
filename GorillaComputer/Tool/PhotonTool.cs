using Photon.Pun;

namespace GorillaComputer.Tool
{
    internal static class PhotonTool
    {
        public static int GetPing()
        {
            var networkingClient = PhotonNetwork.NetworkingClient;

            if (networkingClient == null)
            {
                return -1;
            }

            var loadBalancingPeer = networkingClient.LoadBalancingPeer;

            if (loadBalancingPeer == null)
            {
                return -1;
            }

            return loadBalancingPeer.RoundTripTime;
        }
    }
}
