using System.Threading.Tasks;
using Slovo_Filter_BLL.Services;
using Slovo_Filter_DAL.Repositories;

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
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            
            var isRegistered = await _userRepository.RegisterUserAsync(firstName, lastName, email, passwordHash);
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                User = user;
            }

            return isRegistered > 0;
        }
    }
}