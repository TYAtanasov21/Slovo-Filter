using System.ComponentModel;
using System.Text.Json;
using System.Windows.Input;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Slovo_Filter_BLL.Services;
using Slovo_Filter_DAL.Models;
using Slovo_Filter_DAL.Repositories;

namespace Slovo_Filter.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Email))); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password))); }
        }

        public User User { get; private set; }
        private readonly UserRepository _userRepository;
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand GoogleLoginCommand { get; }

        public LoginViewModel()
        {
            _userRepository = new UserRepository();
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
            LoginCommand = new Command(async () => await LoginAsync());
            GoogleLoginCommand = new Command(async () => await AuthenticateWithGoogleAsync());
        }

        private async void OnNavigateToRegister()
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
            }
        }

        private async Task LoginAsync()
        {
            IsLoading = true;

            var email = Email;
            var password = Password;

            var validationMessage = ValidateLogin(email, password);
            if (validationMessage != null)
            {
                IsLoading = false;
                await Application.Current.MainPage.DisplayAlert("Validation Failed", validationMessage, "OK");
                return;
            }

            var (isSuccess, message) = await LoginUserAsync(email, password);

            if (isSuccess)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new MainApp(User));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Login Failed", message, "OK");
            }

            IsLoading = false; 
        }

        private string ValidateLogin(string email, string password)
        {
            if (!IsValidEmail(email))
            {
                return "Please enter a valid email address.";
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                return "Password must be at least 8 characters long.";
            }

            return null;
        }

        private bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        public async Task<(bool, string)> LoginUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Email and/or password are required");
            }

            try
            {
                var isAuthenticated = await _userRepository.LoginUserAsync(email, password);

                if (isAuthenticated)
                {
                    var user = await _userRepository.GetUserByEmailAsync(email);
                    if (user != null)
                    {
                        User = user;
                        return (true, "Login Successful");
                    }
                    else
                    {
                        return (false, "User data not found");
                    }
                }
                else
                {
                    return (false, "Login Failed");
                }
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        private async Task AuthenticateWithGoogleAsync()
{
    try
    {
        IsLoading = true;

        var clientId = "908935968734-34k0psqoiehjhjpdgul8cqa6qkpr9ekb.apps.googleusercontent.com";
        var clientSecret = "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"; 
        var redirectUri = "http://localhost:5000/";
        var scope = "openid email profile";

        var authUrl = new Uri($"https://accounts.google.com/o/oauth2/v2/auth?" +
                                $"client_id={clientId}&" +
                                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                                $"response_type=code&" +
                                $"scope={Uri.EscapeDataString(scope)}");

        var callbackUrl = new Uri(redirectUri);
        var authResult = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl);

        if (authResult.Properties.TryGetValue("code", out string code))
        {
            var tokenResponse = await ExchangeCodeForTokensAsync(code, clientId, clientSecret, redirectUri);

            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                var userInfo = DecodeIdToken(tokenResponse.IdToken);

                var user = await _userRepository.GetUserByEmailAsync(userInfo.Email);
                if (user == null)
                {
                    string firstName = userInfo.Name;
                    string lastName = "";
                    if (userInfo.Name.Contains(" "))
                    {
                        var parts = userInfo.Name.Split(' ');
                        firstName = parts[0];
                        lastName = string.Join(" ", parts, 1, parts.Length - 1);
                    }

                    var randomPassword = Guid.NewGuid().ToString();
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword);

                    int newUserId = await _userRepository.RegisterUserAsync(firstName, lastName, userInfo.Email, passwordHash);

                    user = await _userRepository.GetUserByEmailAsync(userInfo.Email);
                }
                
                User = user;
                await Application.Current.MainPage.Navigation.PushAsync(new MainApp(user));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Google OAuth", "Failed to retrieve tokens.", "OK");
            }
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Google OAuth", "No authorization code returned.", "OK");
        }
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("Google OAuth Error", ex.Message, "OK");
    }
    finally
    {
        IsLoading = false;
    }
}

        
        public async Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(string code, string clientId, string clientSecret, string redirectUri)
        {
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(parameters);
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<GoogleTokenResponse>(responseString);
            }
        }
        public GoogleUserInfo DecodeIdToken(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(idToken);

            // Extract email and name from token claims.
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
    
            return new GoogleUserInfo { Email = email, Name = name };
        }
    }
}
