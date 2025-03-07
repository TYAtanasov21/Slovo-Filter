using System.Threading.Tasks;
using Slovo_Filter_BLL.Services;
using Slovo_Filter_DAL.Repositories;
using System.Text.RegularExpressions;

namespace Slovo_Filter.ViewModel
{
    public class RegisterViewModel
    {
        public User User { get; set; }
        private readonly UserRepository _userRepository;

        public RegisterViewModel()
        {
            _userRepository = new UserRepository();
        }

        public async Task<bool> RegisterUserAsync(string firstName, string lastName, string email, string password)
        {
            var validationMessage = ValidateRegistration(email, password);
            if (validationMessage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Failed", validationMessage, "OK");
                return false;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            
            var isRegistered = await _userRepository.RegisterUserAsync(firstName, lastName, email, passwordHash);
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                User = user;
            }

            return isRegistered > 0;
        }

        private string ValidateRegistration(string email, string password)
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
    }
}
