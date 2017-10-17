using System.Linq;
using System.Threading.Tasks;
using Todo.Models;
using Microsoft.AspNetCore.Identity;

namespace Todo.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataSeeder(ApplicationDbContext ctx, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _ctx = ctx;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync(string username, string password)
        {
            _ctx.Database.EnsureCreated();

            if (!_ctx.Users.Any())
            {
                var adminId = await EnsureUser(username, password);
                await EnsureRole(adminId, Authorization.Constants.ContactAdministratorsRole);

                var managerId = await EnsureUser("james@dotnetcore.com", password);
                await EnsureRole(managerId, Authorization.Constants.ContactManagersRole);

                SeedDB(adminId);
            }
        }

        private async Task<string> EnsureUser(string UserName, string password)
        {
            var user = await _userManager.FindByNameAsync(UserName);

            if (user == null)
            {
                user = new ApplicationUser { UserName = UserName, Email = UserName };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
            }

            return user.Id;
        }

        private async Task<IdentityResult> EnsureRole(string uid, string role)
        {
            IdentityResult result = null;

            if (!await _roleManager.RoleExistsAsync(role))
            {
                result = await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var user = await _userManager.FindByIdAsync(uid);

            result = await _userManager.AddToRoleAsync(user, role);

            return result;
        }

        public void SeedDB(string userId)
        {
            if (_ctx.Todo.Any())
            {
                return;  // DB has been seeded
            }

            _ctx.Todo.AddRange(
                new Models.Todo
                {
                    Title = "Gym Time",
                    Description = "Hit the Gym",
                    Status = TodoStatus.New,
                    OwnerID = userId
                },
             new Models.Todo
             {
                 Title = "Read a Book",
                 Description = "Read a Book Description",
                 Status = TodoStatus.New,
                 OwnerID = userId
             },
             new Models.Todo
             {
                 Title = "Organize Office",
                 Description = "Organize Office Description",
                 Status = TodoStatus.New,
                 OwnerID = userId
             });

            _ctx.SaveChanges();
        }
    }
}
