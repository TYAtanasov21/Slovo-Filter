using Slovo_Filter.ViewModel;
namespace Slovo_Filter;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}