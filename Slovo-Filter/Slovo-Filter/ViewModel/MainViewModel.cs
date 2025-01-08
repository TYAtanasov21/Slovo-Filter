using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Slovo_Filter.ViewModel;

public partial class MainViewModel: ObservableObject
{
    public MainViewModel()
    {
        Items = new ObservableCollection<string>();
    }
    
    
    [ObservableProperty]
    ObservableCollection<string> items;
    
    
    [ObservableProperty]
    private string text;
    
    [RelayCommand]
    void Add()
    {
        if (string.IsNullOrWhiteSpace(Text)) return;
        
        Items.Add(Text);
        foreach (var txt in Items)
        {
            Console.WriteLine(txt);   
        }
        Text = string.Empty;
    }
    
}