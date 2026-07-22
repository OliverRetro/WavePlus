using Plus.Communication.Packets.Outgoing.Camera;
using Plus.HabboHotel.Camera;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Camera
{
    public class GetCameraPriceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            CameraManager camera = PlusEnvironment.GetGame().GetCameraManager();

            session.SendPacket(new InitCameraComposer(camera.PurchasePrice, camera.PublishPrice));
        }
    }
}