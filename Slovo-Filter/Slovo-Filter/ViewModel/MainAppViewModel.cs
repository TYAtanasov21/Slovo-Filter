using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Slovo_Filter_BLL.Services;
using Slovo_Filter_DAL.Models;
using Slovo_Filter_DAL.Repositories;

namespace Slovo_Filter.ViewModel
{
    public class MainAppViewModel : INotifyPropertyChanged
    {
        private readonly SocketService _socketService;
        private readonly UserRepository _userRepository;
        private string _currentMessage;
        private string _searchQuery;
        public ObservableCollection<string> Messages { get; set; } = new();
        public ObservableCollection<User> Users { get; set; } = new();
        
        public ObservableCollection<User> SearchResults { get; set; } = new();
        public ObservableCollection<User> PreviousChats { get; set; } = new();

        public string RecieverId { get; set; } = string.Empty;
        public string UserId { get; set; }
        
        private readonly MessageService _messageService;
        public ObservableCollection<Message> MessageHistory { get; set; } = new();
        
        private bool _isLoading = false;

        public bool isLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        
        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(LastName));
            }
        }

        public string Email => CurrentUser?.Email ?? string.Empty;
        public string FirstName => CurrentUser?.FirstName ?? string.Empty;
        public string LastName => CurrentUser?.LastName ?? string.Empty;

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    RecieverId = _selectedUser?.Id.ToString() ?? string.Empty;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RecieverId));

                    if (_selectedUser != null)
                    {
                        LoadMessageHistoryAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        MessageHistory.Clear();
                    }
                }
            }
        }

        public string CurrentMessage
        {
            get => _currentMessage;
            set 
            {
                _currentMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendMessageCommand { get; }
        public ICommand LoadHistoryCommand { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public MainAppViewModel(string userId)
        {
            UserId = userId;
            _socketService = new SocketService(userId);
            _userRepository = new UserRepository();
            _messageService = new MessageService();

            _socketService.OnMessageReceived += async (sender, message) =>
            {
                Console.WriteLine($"Message received from {sender}: {message}");
                await CheckAndAddMessage(message, false);
            };

            _socketService.OnMessageHistoryReceived += (messages) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (!MessageHistory.Any()) 
                    {
                        MessageHistory.Clear();
                    }

                    foreach (var message in messages.OrderBy(m => m.Date))
                    {
                        var existing = MessageHistory.FirstOrDefault(m => m.Id == message.Id);
            
                        if (existing == null)
                        {
                            message.IsFromCurrentUser = message.SenderId.ToString() == UserId;
                            MessageHistory.Add(message);
                        }
                        else
                        {
                            // Preserve existing AI score
                            Console.WriteLine($"Preserved score {existing.AiScore} for message ID {existing.Id}");
                        }
                    }

                    isLoading = false;
                });
            };
            LoadUsers();

            SendMessageCommand = new Command(SendMessage);
            LoadHistoryCommand = new Command(async () => await LoadMessageHistoryAsync());
        }

        private async Task LoadMessageHistoryAsync()
        {
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(RecieverId))
                return;
            try
            {
                isLoading = true;
                MessageHistory.Clear();
                _socketService.RequestMessageHistory(UserId, RecieverId, 50);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading message history: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        private async void LoadUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
                if (user.Id.ToString() == UserId)
                {
                    CurrentUser = user;
                    Console.WriteLine($"Current User Loaded: {CurrentUser.FirstName} {CurrentUser.LastName}");
                }
            }
        }

        private async Task CheckAndAddMessage(string messageContent, bool isFromCurrentUser)
        {
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                Console.WriteLine("Warning: Empty message received.");
                return;
            }

            var filter = new AIFilter { content = messageContent };
            var jsonResponse = await filter.AnalyzeContentAsync();
    
            Console.WriteLine(jsonResponse + " JSON RESPONSE");

            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                Console.WriteLine("Error: Empty or invalid AI response.");
                return;
            }

            AiScore aiResult;
            try
            {
                aiResult = JsonSerializer.Deserialize<AiScore>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing AI response: {ex.Message}");
                return;
            }

            if (aiResult == null)
            {
                Console.WriteLine("Error: AI result is null.");
                return;
            }

            Console.WriteLine(aiResult.Score.ToString() + " - Score");

            // Ensure UserId and ReceiverId are valid integers
            if (!int.TryParse(UserId, out int senderId))
            {
                Console.WriteLine($"Error: Invalid SenderId '{UserId}'.");
                return;
            }

            if (!int.TryParse(RecieverId, out int receiverId))
            {
                Console.WriteLine($"Error: Invalid ReceiverId '{RecieverId}'.");
                return;
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = messageContent,
                Date = DateTime.Now,
                IsFromCurrentUser = isFromCurrentUser,
                AiScore = aiResult.Score,
            };
            Console.WriteLine($"Message created with AiScore: {message.AiScore}");
            MessageHistory.Add(message);

            try
            {
                await _messageService.StoreMessageAsync(message.SenderId, message.ReceiverId, message.Content, message.AiScore);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing message: {ex.Message}");
            }
        }


        private async void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(CurrentMessage) && SelectedUser != null)
            {
                await CheckAndAddMessage(CurrentMessage, true);
                _socketService.SendMessage(UserId, RecieverId, CurrentMessage);
                Messages.Add($"You to {SelectedUser.FirstName}: {CurrentMessage}");
                CurrentMessage = string.Empty;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
