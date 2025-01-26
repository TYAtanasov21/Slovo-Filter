using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Input;

namespace Slovo_Filter.ViewModel;

public class LoginViewModel : BaseViewModel
{
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand NavigateToRegisterCommand { get; }
        private string _email;
        private string _password;

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ICommand SignInCommand { get; }
        
        public LoginViewModel()
        {
            SignInCommand = new Command(async () => await SignInAsync());
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
        }

        private async void OnNavigateToRegister()
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
            }
            
        }
        
        private async Task SignInAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please fill in all fields", "OK");
                return;
            }

            if (Email == "tya" && Password == "123")
            {
                await App.Current.MainPage.DisplayAlert("Success", "You are signed in!", "OK");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
}
