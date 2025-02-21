using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slovo_Filter_BLL.Services;
using Slovo_Filter.ViewModel;

namespace Slovo_Filter;

public partial class MainApp : ContentPage
{
    private readonly MainAppViewModel _viewModel;
    public MainApp(User user)
    {
        InitializeComponent();
        var socketService = new SocketService();
        _viewModel = new MainAppViewModel(socketService);
        BindingContext = _viewModel;
        
        Console.WriteLine($"Logged in as: {user.Email}");
    }
}