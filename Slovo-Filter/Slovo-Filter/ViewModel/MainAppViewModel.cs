using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Slovo_Filter_BLL.Services;

namespace Slovo_Filter.ViewModel;

public class MainAppViewModel
{
    private readonly SocketService _socketService;
    private string _message;
    private string _chatHistory;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    public string ChatHistory
    {
        get => _chatHistory;
        set { _chatHistory = value; OnPropertyChanged(); }
    }

    public ICommand SendMessageCommand { get; }

    public MainAppViewModel(SocketService socketService)
    {
        Console.WriteLine("Connecting...");
        _socketService = socketService;
        _socketService.OnMessageReceived += OnMessageReceived;
        SendMessageCommand = new Command(SendMessage);
    }

    private void OnMessageReceived(string message)
    {
        ChatHistory += $"\n{message}";
    }

    private void SendMessage()
    {
        if (!string.IsNullOrEmpty(Message))
        {
            Console.WriteLine("Sending message..." + Message);
            _socketService.SendMessage(Message);
            Message = string.Empty;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}