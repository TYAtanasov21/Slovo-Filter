    using System;
    using Microsoft.Win32;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Slovo_Filter_DAL.Models;
    using SocketIOClient;

    namespace Slovo_Filter_BLL.Services
    {
        public class MessageData
        {
            public string Sender { get; set; }
            public string Reciever { get; set; }
            public string Message { get; set; }
            public string Date { get; set; }
            public Int32 Id { get; set; }
        }
        public class SocketService
        {
            private SocketIOClient.SocketIO _client;
            public event Action<string, string> OnMessageReceived;
            public event Action<List<Message>> OnMessageHistoryReceived;


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
                    Console.WriteLine($"Connecting to the server...");

                    _client = new SocketIOClient.SocketIO("http://localhost:3000/");
                    
                    
                    _client.OnConnected += (sender, e) =>
                    {
                        Console.WriteLine("✅ Connected to server");
                        _client.EmitAsync("register", _userId);
                        Console.WriteLine($"Registered user with {_userId}");
                        RegisterMessageListener();
                        RegisterHistoryListener();
                    };

                    _client.OnDisconnected += (sender, e) =>
                    {
                        Console.WriteLine("Disconnected from server: " + e);
                    };

                    _client.OnError += (sender, e) =>
                    {
                        Console.WriteLine("Connection error: " + e);
                    };
                    _client.ConnectAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Socket connection error: " + e.Message);
                }
            }
            
            private void RegisterMessageListener()
            {
                _client.On("private_message", (response) =>
                {
                    try
                    {
                        Console.WriteLine($"📨 Raw data received: {response}");

                        string socketData = response.ToString();
                        Console.WriteLine($"🔍 Raw string data: {socketData}");

                        if (!string.IsNullOrEmpty(socketData))
                        {
                            var messageList = JsonConvert.DeserializeObject<List<MessageData>>(socketData);

                            if (messageList != null && messageList.Count > 0)
                            {
                                var firstMessage = messageList[0];
                                var sender = firstMessage?.Sender;
                                var message = firstMessage?.Message;

                                if (!string.IsNullOrEmpty(sender) && !string.IsNullOrEmpty(message))
                                {
                                    OnMessageReceived?.Invoke(sender, message);
                                    Console.WriteLine($"Message received from {sender}: {message}");
                                }
                                else
                                {
                                    Console.WriteLine("Missing sender or message.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No messages in the list.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Received empty data.");
                        }
                    }
                    catch (JsonReaderException jex)
                    {
                        Console.WriteLine($"JSON Parse Error: {jex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected Error: {ex.Message}");
                    }
                });
            }

           private void RegisterHistoryListener()
            {
                _client.On("message_history", (response) =>
                {
                    try
                    {
                        Console.WriteLine($"📚 Raw history data received: {response}");

                        string socketData = response.ToString();
                        var outerArray = JsonConvert.DeserializeObject<List<List<JObject>>>(socketData);
                        if (!string.IsNullOrEmpty(socketData))
                        {
                            var jsonHistory = outerArray[0];
                            
                            if (jsonHistory != null && jsonHistory.Count > 0)
                            {
                                var messages = new List<Message>();
                                
                                foreach (var item in jsonHistory)
                                {
                                    try
                                    {
                                        var message = new Message
                                        {
                                            Id = item["id"]?.Value<Int32>() ?? 0,
                                            SenderId = item["senderId"] != null ? item["senderId"].Value<int>() : 0,
                                            ReceiverId = int.TryParse(item["receiverId"]?.ToString(), out int receiverId) ? receiverId : 0,
                                            Content = item["content"]?.ToString(),
                                            Date = item["date"] != null
                                                ? DateTime.Parse(item["date"].ToString())
                                                : DateTime.Now,
                                            IsDelivered = item["delivered"]?.Value<bool>() ?? false,
                                            IsFromCurrentUser = item["senderId"]?.ToString() == _userId.ToString(),
                                            AiScore = item["aiScore"]?.Value<int>() ?? 0,
                                        };
                                        
                                        messages.Add(message);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"⚠️ Error processing message item: {ex.Message}");
                                    }
                                }
                                
                                if (messages.Count > 0)
                                {
                                    OnMessageHistoryReceived?.Invoke(messages);
                                    Console.WriteLine($"📚 History received: {messages.Count} messages");
                                }
                                else
                                {
                                    Console.WriteLine("⚠️ No valid messages in history.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("⚠️ Empty history received.");
                            }
                        }
                    }
                    catch (JsonReaderException jex)
                    {
                        Console.WriteLine($"🚨 JSON Parse Error in history: {jex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Unexpected Error in history: {ex.Message}");
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
            
            
            
            public void RequestMessageHistory(string user1Id, string user2Id, int limit = 50)
            {
                try
                {
                    Console.WriteLine($"📚 Requesting message history between {user1Id} and {user2Id}");
                    _client.EmitAsync("get_history", new
                    {
                        user1Id = user1Id,
                        user2Id = user2Id,
                        limit = limit
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error requesting message history: {ex.Message}");
                }
            }
            
            public void Disconnect()
            {
                _client?.DisconnectAsync();
                Console.WriteLine("Disconnecting from server...");
            }
        }
    }