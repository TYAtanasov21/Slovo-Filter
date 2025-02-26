using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Slovo_Filter_BLL.Services;
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
    public event PropertyChangedEventHandler PropertyChanged;


    public MainAppViewModel(string userId)
    {
        UserId = userId;
        _socketService = new SocketService(userId);
        _userRepository = new UserRepository();
        
        // Subscribe to incoming messages
        _socketService.OnMessageReceived += (sender, message) =>
        {
            Console.WriteLine($"Message received from {sender}: {message}");
            Messages.Add($"{sender}: {message}");
        };
        
        CurrentUser = new User();
        LoadUsers();
        
        SendMessageCommand = new Command(SendMessage);
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