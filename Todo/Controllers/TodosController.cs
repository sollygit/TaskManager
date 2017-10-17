using Todo.Authorization;
using Todo.Data;
using Todo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Todo.Controllers
{
    public class TodosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodosController(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Todos
        public async Task<IActionResult> Index()
        {
            var todos = from c in _context.Todo select c;
            var currentUserId = _userManager.GetUserId(User);

            todos = todos.Where(c => c.OwnerID == currentUserId);

            return View(await todos.ToListAsync());
        }

        // GET: Todos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo.SingleOrDefaultAsync(m => m.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            var isAuthorizedRead = await _authorizationService.AuthorizeAsync(User, todo, TodoOperations.Read);
            var isAuthorizedApprove = await _authorizationService.AuthorizeAsync(User, todo, TodoOperations.Start);

            if (todo.Status != TodoStatus.InProgress &&   // Not InProgress.
                !isAuthorizedRead.Succeeded && // Don't own it.
                !isAuthorizedApprove.Succeeded) // Not a manager.
            {
                return new ChallengeResult();
            }

            return View(todo);
        }

        // GET: Todos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Todos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoEditViewModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }

            var todo = ViewModel_to_model(new Models.Todo(), editModel);

            todo.OwnerID = _userManager.GetUserId(User);

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, todo, TodoOperations.Create);

            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            _context.Add(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Todos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo.SingleOrDefaultAsync(m => m.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, todo, TodoOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            var editModel = Model_to_viewModel(todo);

            return View(editModel);
        }

        // POST: Todos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TodoEditViewModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }

            // Fetch Contact from DB to get OwnerID.
            var todo = await _context.Todo.SingleOrDefaultAsync(m => m.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, todo, TodoOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            todo = ViewModel_to_model(todo, editModel);

            if (todo.Status == TodoStatus.InProgress)
            {
                // If the todo task is updated after approval and the user cannot approve set the status back to submitted
                var canApprove = await _authorizationService.AuthorizeAsync(User, todo, TodoOperations.Start);

                if (!canApprove.Succeeded) todo.Status = TodoStatus.New;
            }

            _context.Update(todo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Todos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo.SingleOrDefaultAsync(m => m.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, todo, TodoOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            return View(todo);
        }

        // POST: Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todo = await _context.Todo.SingleOrDefaultAsync(m => m.Id == id);

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, todo, TodoOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            _context.Todo.Remove(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, TodoStatus status)
        {
            var todo = await _context.Todo.SingleOrDefaultAsync(m => m.Id == id);

            var contactOperation = (status == TodoStatus.InProgress) ? TodoOperations.Start : TodoOperations.Finish;

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, todo, contactOperation);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            todo.Status = status;
            _context.Todo.Update(todo);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool TodoExists(int id)
        {
            return _context.Todo.Any(e => e.Id == id);
        }

        private Models.Todo ViewModel_to_model(Models.Todo todo, TodoEditViewModel editModel)
        {
            todo.Id = editModel.Id;
            todo.Title = editModel.Title;
            todo.Description = editModel.Description;

            return todo;
        }

        private TodoEditViewModel Model_to_viewModel(Models.Todo todo)
        {
            var editModel = new TodoEditViewModel
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description
            };

            return editModel;
        }
    }
}