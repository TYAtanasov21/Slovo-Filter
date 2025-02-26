using System;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;

namespace Slovo_Filter_BLL.Services
{
    public class MessageData
    {
        public string Sender { get; set; }
        public string Message { get; set; }
    }
    public class SocketService
    {
        private SocketIOClient.SocketIO _client; // Use SocketIO instead of SocketIoClient
        public event Action<string, string> OnMessageReceived;

        private readonly string _userId;
        
        public SocketService(string userId)
        {
            _userId = userId;
            Console.WriteLine("SocketService is being constructed...");
            Connect();
        }

        public void Connect()
        {
            try
            {
                Console.WriteLine($"🔌 Connecting to the server...");

                _client = new SocketIOClient.SocketIO("https://chat-server-fsdzgyekfke8arc2.northeurope-01.azurewebsites.net/");
                
                
                _client.OnConnected += (sender, e) =>
                {
                    Console.WriteLine("✅ Connected to server");
                    _client.EmitAsync("register", _userId);
                    Console.WriteLine($"Registered user with {_userId}");
                    RegisterMessageListener();
                };

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
                _client.ConnectAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("🚨 Socket connection error: " + e.Message);
            }
        }
        
        private void RegisterMessageListener()
        {
            _client.On("private_message", (response) =>
            {
                try
                {
                    Console.WriteLine($"📨 Raw data received: {response}");

                    // Convert the response to string (assuming it's a JSON array)
                    string socketData = response.ToString();
                    Console.WriteLine($"🔍 Raw string data: {socketData}");

                    if (!string.IsNullOrEmpty(socketData))
                    {
                        // Deserialize the response into a list of MessageData objects
                        var messageList = JsonConvert.DeserializeObject<List<MessageData>>(socketData);

                        if (messageList != null && messageList.Count > 0)
                        {
                            // Get the first message in the list
                            var firstMessage = messageList[0];
                            var sender = firstMessage?.Sender;
                            var message = firstMessage?.Message;

                            if (!string.IsNullOrEmpty(sender) && !string.IsNullOrEmpty(message))
                            {
                                OnMessageReceived?.Invoke(sender, message);
                                Console.WriteLine($"📩 Message received from {sender}: {message}");
                            }
                            else
                            {
                                Console.WriteLine("⚠️ Missing sender or message.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("⚠️ No messages in the list.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Received empty data.");
                    }
                }
                catch (JsonReaderException jex)
                {
                    Console.WriteLine($"🚨 JSON Parse Error: {jex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
                }
            });
        }




        public void SendMessage(string sender, string receiver, string message)
        {
            _client.EmitAsync("private_message", new
            {
                sender = sender,
                receiver = receiver,
                message = message
            });
            Console.WriteLine("Message sent to server");
        }
        
        public void Disconnect()
        {
            _client?.DisconnectAsync();
            Console.WriteLine("Disconnecting from server...");
        }
    }
}