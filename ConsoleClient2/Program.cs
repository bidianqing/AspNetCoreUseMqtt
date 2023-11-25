using MQTTnet.Client;
using MQTTnet;
using System.Text;

namespace ConsoleClient2
{
    /// <summary>
    /// Subscribe
    /// </summary>
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    //.WithTcpServer("broker.hivemq.com")
                    .WithTcpServer("127.0.0.1")
                    .Build();

                // Setup message handling before connecting so that queued messages
                // are also handled properly. When there is no event handler attached all
                // received messages get lost.
                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());

                    await Console.Out.WriteLineAsync("Received application message = " + payload);

                    e.AutoAcknowledge = true;
                };

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic("samples/temperature/living_room");
                        })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                Console.WriteLine("MQTT client subscribed to topic.");

                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
