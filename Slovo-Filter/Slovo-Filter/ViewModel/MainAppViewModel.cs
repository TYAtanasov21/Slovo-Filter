using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Slovo_Filter_BLL.Services;
using Slovo_Filter_DAL.Models;
using Slovo_Filter_DAL.Repositories;

namespace Slovo_Filter.ViewModel;

public class MainAppViewModel
{
    private readonly SocketService _socketService;
    private readonly UserRepository _userRepository;
    private string _currentMessage;
    public ObservableCollection<string> Messages { get; set; } = new();
    public ObservableCollection<User> Users { get; set; } = new();

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
    

    
    //Current user declaration
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
    
    public string Email => CurrentUser?.Email;
    public string FirstName => CurrentUser?.FirstName;

    public string LastName => CurrentUser?.LastName;

    //Selected user declaration
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
    
    
    //Message declaration
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
        
        // Subscribe to incoming messages
        _socketService.OnMessageReceived += (sender, message) =>
        {
            Console.WriteLine($"Message received from {sender}: {message}");
            Messages.Add($"{sender}: {message}");
        };
        
        _socketService.OnMessageHistoryReceived += (messages) =>
        {
            Microsoft.Maui.Controls.Device.BeginInvokeOnMainThread(() =>
                {
                MessageHistory.Clear();
                foreach (var message in messages.OrderBy(m => m.Date))
                {
                    // Set IsFromCurrentUser based on the current user ID
                    message.IsFromCurrentUser = message.SenderId.ToString() == UserId;
                    MessageHistory.Add(message);
                }
                isLoading = false;
                });
        };
        
        CurrentUser = new User();
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
        }
    }

    private void SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(CurrentMessage) && SelectedUser != null)
        {
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