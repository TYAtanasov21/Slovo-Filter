using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Slovo_Filter_BLL.Services;

namespace Slovo_Filter.ViewModel;

public class MainAppViewModel
{
    private readonly SocketService _socketService;
    private string _currentMessage;
    public ObservableCollection<string> Messages { get; set; } = new();
    public string RecieverId { get; set; } = string.Empty;
    public string UserId { get; set; }

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

        // Subscribe to incoming messages
        _socketService.OnMessageReceived += (sender, message) =>
        {
            Console.WriteLine($"Message received from {sender}: {message}");
            Messages.Add($"{sender}: {message}");
        };
        
        SendMessageCommand = new Command(SendMessage);
    }

    private void SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(CurrentMessage) && !string.IsNullOrWhiteSpace(RecieverId))
        {
            _socketService.SendMessage(UserId.ToString(), RecieverId, CurrentMessage);
            Messages.Add($"You: {CurrentMessage}");
            CurrentMessage = string.Empty; // Clear message after sending
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}