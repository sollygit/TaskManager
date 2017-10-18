using Todo.Authorization;
using Todo.Data;
using Todo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Todo.Services;

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
        public async Task<IActionResult> Index(string tag, string searchString)
        {
            // Get a list of tags.
            IQueryable<string> tagQuery = from m in _context.Todo
                                            orderby m.Tag
                                            select m.Tag;

            var todos = from t in _context.Todo select t;
            var currentUserId = _userManager.GetUserId(User);

            todos = todos.Where(c => c.OwnerID == currentUserId);

            if (!String.IsNullOrEmpty(searchString))
            {
                todos = todos.Where(t => t.Title.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(tag))
            {
                todos = todos.Where(x => x.Tag == tag);
            }

            var todoTagVM = new TodoTagViewModel
            {
                tags = new SelectList(await tagQuery.Distinct().ToListAsync()),
                todos = await todos.ToListAsync()
            };

            return View(todoTagVM);
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
            var todoTags = new EnumHelper().MapEnumToDictionary<TodoTags>().Select(o => o.Value);

            var todoEditVM = new TodoEditViewModel
            {
                TagList = new SelectList(todoTags)
            };

            return View(todoEditVM);
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

            var editModel = Model_to_viewModelAsync(todo);

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

            todo = ViewModel_to_model(todo, editModel);

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
            todo.Tag = editModel.Tag;
            todo.Description = editModel.Description;

            return todo;
        }

        private TodoEditViewModel Model_to_viewModelAsync(Models.Todo todo)
        {
            // Get a list of tag names.
            var todoTags = new EnumHelper().MapEnumToDictionary<TodoTags>().Select(o => o.Value);

            var editModel = new TodoEditViewModel
            {
                Id = todo.Id,
                Title = todo.Title,
                Tag = todo.Tag,
                Description = todo.Description,
                TagList = new SelectList(todoTags)
            };

            return editModel;
        }
    }
}