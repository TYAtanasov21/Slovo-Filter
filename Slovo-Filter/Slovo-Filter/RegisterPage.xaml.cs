using Slovo_Filter.ViewModel;
using System;

namespace Slovo_Filter
{
    public partial class RegisterPage : ContentPage
    {
        private readonly RegisterViewModel _viewModel;

        public RegisterPage()
        {
            InitializeComponent();
            _viewModel = new RegisterViewModel();
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            var firstName = FirstNameEntry.Text;
            var lastName = LastNameEntry.Text;
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;

            var isRegistered = await _viewModel.RegisterUserAsync(firstName, lastName, email, password);

            if (isRegistered)
            {
                Console.WriteLine("successfully registered");
                await DisplayAlert("Success", "User registered successfully!", "OK");
                
                await Navigation.PushAsync(new MainApp());
            }
            else
            {
                Console.WriteLine("Failed to register user");
                await DisplayAlert("Error", "Registration failed. Please try again.", "OK");
            }
        }
    }
}