using System.Threading.Tasks;
using Slovo_Filter_DAL.Repositories;

namespace Slovo_Filter.ViewModel
{
    public class RegisterViewModel
    {
        private readonly UserRepository _userRepository;

        public RegisterViewModel()
        {
            _userRepository = new UserRepository();
        }

        public async Task<bool> RegisterUserAsync(string firstName, string lastName, string email, string password)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password); // Hash the password
            var userId = await _userRepository.RegisterUserAsync(firstName, lastName, email, passwordHash);
            return userId > 0; // Returns true if the user was successfully registered
        }
    }
}