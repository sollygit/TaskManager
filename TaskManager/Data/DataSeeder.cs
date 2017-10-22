using System.Linq;
using System.Threading.Tasks;
using TaskManager.Models;
using TaskManager.Authorization;
using Microsoft.AspNetCore.Identity;
using System;

namespace TaskManager.Data
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
                await EnsureRole(adminId, Constants.ContactAdministratorsRole);

                var managerId = await EnsureUser("james@dotnetcore.com", password);
                await EnsureRole(managerId, Constants.ContactManagersRole);

                SeedTodo(managerId, adminId);
                SeedContacts(managerId);
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

        public void SeedTodo(string managerId, string adminId)
        {
            if (_ctx.Todo.Any())
            {
                return;  // Todo has been seeded
            }

            _ctx.Tag.AddRange(
                new Tag { Name = "Personal" },
                new Tag { Name = "Business" },
                new Tag { Name = "Social" },
                new Tag { Name = "Fun" },
                new Tag { Name = "Code" }
            );

            _ctx.Todo.AddRange(
                new Models.Todo
                {
                    Title = "Gym Time!",
                    Description = "Hit the Gym at least twice a week",
                    Status = TodoStatus.New,
                    OwnerID = managerId,
                    StartDate = DateTime.Now,
                    TagId = 1,
                });

            _ctx.Todo.AddRange(
                new Models.Todo
                {
                    Title = "Gym Time!",
                    Description = "Hit the Gym at least twice a week",
                    Status = TodoStatus.New,
                    OwnerID = adminId,
                    StartDate = DateTime.Now,
                    TagId = 1,
                },
             new Models.Todo
             {
                 Title = "Get new business cards",
                 Description = "By January get your new business cards",
                 Status = TodoStatus.New,
                 OwnerID = adminId,
                 StartDate = DateTime.Now,
                 TagId = 2,
             },
             new Models.Todo
             {
                 Title = "Copyright information",
                 Description = "Add copyright information to footer",
                 Status = TodoStatus.New,
                 OwnerID = adminId,
                 StartDate = DateTime.Now,
                 TagId = 2,
             },
             new Models.Todo
             {
                 Title = "Instagram",
                 Description = "Create your Instagram landing page",
                 Status = TodoStatus.New,
                 OwnerID = adminId,
                 StartDate = DateTime.Now,
                 TagId = 3,
             },
            new Models.Todo
            {
                Title = "Go to the Park",
                Description = "Go to the Park. You can take your dog or a pink Armadillo",
                Status = TodoStatus.New,
                OwnerID = adminId,
                StartDate = DateTime.Now,
                TagId = 4,
            });

            _ctx.SaveChanges();
        }

        public void SeedContacts(string userId)
        {
            if (_ctx.Contact.Any())
            {
                return;  // Contacts has been seeded
            }

            _ctx.Contact.AddRange(
                new Contact
                {
                    Name = "Debra Garcia",
                    Address = "1234 Main St",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "debra@example.com",
                    Status = ContactStatus.Approved,
                    OwnerID = userId
                },
             new Contact
             {
                 Name = "Thorsten Weinrich",
                 Address = "5678 1st Ave W",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "thorsten@example.com"
             },
             new Contact
             {
                 Name = "Yuhong Li",
                 Address = "9012 State st",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "yuhong@example.com"
             },
             new Contact
             {
                 Name = "Jon Orton",
                 Address = "3456 Maple St",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "jon@example.com"
             },
             new Contact
             {
                 Name = "Diliana Alexieva-Bosseva",
                 Address = "7890 2nd Ave E",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "diliana@example.com"
             });

            _ctx.SaveChanges();
        }
    }
}
