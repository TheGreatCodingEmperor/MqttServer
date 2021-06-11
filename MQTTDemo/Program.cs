

using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttServerTest
{
    class Program
    {
        private static MqttServer mqttServer = null;
        static void Main(string[] args)
        {
            new Thread(StartMqttServer).Start();
            while (true)
            {
                string inputString = Console.ReadLine()?.ToLower().Trim();
                if (inputString == "exist")
                {
                    mqttServer?.StopAsync();
                    Console.WriteLine("Server Down!");
                    break;
                }
                else if (inputString == "clients")
                {
                    foreach (var item in mqttServer.GetClientStatusAsync().Result)
                    {
                        Console.WriteLine($"客戶: {item.ClientId},Protocal: {item.ProtocolVersion}");
                    }
                }
                else
                {
                    Console.WriteLine("cmd not exist!");
                }
            }
        }

        private static void StartMqttServer()
        {
            if (mqttServer == null)
            {
                try
                {
                    //設定客戶訊息(驗證用)
                    var options = new MqttServerOptionsBuilder()
                    .WithConnectionValidator((context) =>
                    {
                        if (string.IsNullOrEmpty(context.ClientId))
                        {
                            if (context.ClientId == "c001")
                            {
                                if (context.Username != "u001" || context.Password != "p002")
                                {
                                    context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                                    Console.WriteLine("Error");
                                }
                            }
                            //context.AssignedClientIdentifier = "test123";
                            //context.ReasonCode = MqttConnectReasonCode.Success;
                        }
                    });

                    //創建 mqtt server
                    mqttServer = new MqttFactory().CreateMqttServer() as MqttServer;

                    //接收後動作
                    mqttServer.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(async c =>
                    {
                        await mqttServer.SubscribeAsync(c.ClientId, "topic1");
                        Console.WriteLine($"action {c.ClientId} {Encoding.UTF8.GetString(c.ApplicationMessage.Payload)}");
                    });

                    //連線後動作
                    mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(c =>
                    {
                        Console.WriteLine($"connected {c.ClientId}");
                    });

                    //斷線後動作
                    mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(c =>
                    {
                        Console.WriteLine($"disconnected {c.ClientId}");
                    });
                    mqttServer.StartAsync(options.Build());
                    Console.WriteLine("MQTT服務啟動成功！");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        }
    }
}