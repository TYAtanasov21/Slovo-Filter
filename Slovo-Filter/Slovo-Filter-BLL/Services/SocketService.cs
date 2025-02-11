using System;
using SocketIOClient;

namespace Slovo_Filter_BLL.Services
{
    public class SocketService
    {
        private SocketIOClient.SocketIO _client; // Use SocketIO instead of SocketIoClient
        public event Action<string> OnMessageReceived;

        public SocketService()
        {
            Console.WriteLine("SocketService is being constructed...");
            Connect();
        }

        public void Connect()
        {
            try
            {
                Console.WriteLine($"🔌 Connecting to the server...");

                // Initialize the Socket.IO client with the server URL
                _client = new SocketIOClient.SocketIO("http://172.20.10.4:3000");
                
                
                _client.OnConnected += (sender, e) =>
                {
                    Console.WriteLine("✅ Connected to server");
                };

                // Event: Message Received
                _client.On("receive_message", (data) =>
                {
                    string message = data.GetValue<string>();
                    Console.WriteLine("📩 Received message: " + message);
                    OnMessageReceived?.Invoke(message);
                });

                // Event: Disconnected
                _client.OnDisconnected += (sender, e) =>
                {
                    Console.WriteLine("❌ Disconnected from server: " + e);
                };

                // Event: Connection Error
                _client.OnError += (sender, e) =>
                {
                    Console.WriteLine("⚠️ Connection error: " + e);
                };

                // Connect to the server
                _client.ConnectAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("🚨 Socket connection error: " + e.Message);
            }
        }

        public void SendMessage(string message)
        {
            _client?.EmitAsync("send_message", message);
        }

        public void Disconnect()
        {
            _client?.DisconnectAsync();
        }
    }
}