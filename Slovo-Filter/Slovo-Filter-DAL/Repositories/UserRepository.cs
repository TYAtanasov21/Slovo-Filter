using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Slovo_Filter_BLL.Services;
namespace Slovo_Filter_DAL.Repositories
{
    
    public class UserRepository
    {
        private readonly dbContext _context;

        public UserRepository()
        {
            _context = new dbContext();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var query = "SELECT id, firstname, lastname, email FROM users WHERE email = @Email";
            var parameters = new Dictionary<string, object> { { "@Email", email } };

            var dataTable = await _context.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count > 0)
            {
                return new User
                {
                    Id = Convert.ToInt32(dataTable.Rows[0]["id"]),
                    FirstName = dataTable.Rows[0]["firstname"].ToString(),
                    LastName = dataTable.Rows[0]["lastname"].ToString(),
                    Email = dataTable.Rows[0]["email"].ToString()
                };
            }

            return null;
        }
        public async Task<int> RegisterUserAsync(string firstName, string lastName, string email, string passwordHash)
        {
            var emailCheckQuery = "SELECT id FROM users WHERE email = @Email";
            var emailCheckParameters = new Dictionary<string, object> { { "@Email", email } };
            var emailCheckTable = await _context.ExecuteQueryAsync(emailCheckQuery, emailCheckParameters);

            if (emailCheckTable.Rows.Count > 0)
            {
                Console.WriteLine("Duplicate email found: " + email);
                return -1; // Email already exists
            }

            // Insert the user and return the generated ID
            var query = @"
                INSERT INTO users (firstname, lastname, email, password)
                VALUES (@firstname, @lastname, @email, @password)
                RETURNING id;";

            var parameters = new Dictionary<string, object>
            {
                { "@firstname", firstName },
                { "@lastname", lastName },
                { "@email", email },
                { "@password", passwordHash }
            };

            var dataTable = await _context.ExecuteQueryAsync(query, parameters);
            if (dataTable.Rows.Count > 0)
            {
                return Convert.ToInt32(dataTable.Rows[0]["id"]);
            }

            return -1; // Failed to register user
        }


        public async Task<bool> LoginUserAsync(string email, string password)
        {
            var query = "SELECT id, firstname, lastname, password FROM users WHERE email = @Email";
            var parameters = new Dictionary<string, object> {{"@Email", email}};
    
            // Log the query for debugging purposes
            Console.WriteLine($"Executing query: {query}");
    
            var dataTable = await _context.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
            {
                Console.WriteLine("No user found with this email.");
                return false; // User not found
            }

            var storedPasswordHash = dataTable.Rows[0]["password"].ToString();
    
            // Log the password hash for debugging
            Console.WriteLine($"Stored hash: {storedPasswordHash}");
    
            // Verify the password using BCrypt
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, storedPasswordHash);
            Console.WriteLine($"Password is valid: {isPasswordValid}");

            return isPasswordValid;
        }
        
        public async Task<List<User>> GetAllUsersAsync()
        {
            var query = "SELECT id, firstname, lastname, email FROM users";
            var dataTable = await _context.ExecuteQueryAsync(query, new Dictionary<string, object>());

            var users = new List<User>();

            foreach (System.Data.DataRow row in dataTable.Rows)
            {
                users.Add(new User
                {
                    Id = Convert.ToInt32(row["id"]),
                    FirstName = row["firstname"].ToString(),
                    LastName = row["lastname"].ToString(),
                    Email = row["email"].ToString()
                });
            }

            return users;
        }
        
        
        

        
    }
}