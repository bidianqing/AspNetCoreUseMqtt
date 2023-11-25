using MQTTnet.Server;
using MQTTnet;
using MQTTnet.Internal;
using MQTTnet.Packets;
using System.Text;

namespace ConsoleServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                // Attach the event handler.
                mqttServer.ClientAcknowledgedPublishPacketAsync += e =>
                {
                    Console.WriteLine($"Client '{e.ClientId}' acknowledged packet {e.PublishPacket.PacketIdentifier} with topic '{e.PublishPacket.Topic}'");

                    // It is also possible to read additional data from the client response. This requires casting the response packet.
                    var qos1AcknowledgePacket = e.AcknowledgePacket as MqttPubAckPacket;
                    Console.WriteLine($"QoS 1 reason code: {qos1AcknowledgePacket?.ReasonCode}");

                    var qos2AcknowledgePacket = e.AcknowledgePacket as MqttPubCompPacket;
                    Console.WriteLine($"QoS 2 reason code: {qos1AcknowledgePacket?.ReasonCode}");
                    return CompletedTask.Instance;
                };
                mqttServer.InterceptingInboundPacketAsync += async e =>
                {
                    switch (e.Packet)
                    {
                        case MqttConnectPacket _:
                            await Console.Out.WriteLineAsync(e.Packet.GetRfcName());
                            break;

                        case MqttPingReqPacket _:
                            await Console.Out.WriteLineAsync(e.Packet.GetRfcName());
                            break;

                        case MqttPublishPacket _:
                            await Console.Out.WriteLineAsync(e.Packet.GetRfcName());
                            var publishPacket = e.Packet as MqttPublishPacket;
                            await Console.Out.WriteLineAsync(Encoding.UTF8.GetString(publishPacket.PayloadSegment));
                            break;

                        default:

                            break;
                    }
                };

                await mqttServer.StartAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                await mqttServer.StopAsync();
            }
        }
    }
}
