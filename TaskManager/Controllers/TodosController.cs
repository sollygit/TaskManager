using TaskManager.Data;
using TaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace TaskManager.Controllers
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
        public async Task<IActionResult> Index(int tagId, string searchString)
        {
            var todos = from t in _context.Todos select t;
            var currentUserId = _userManager.GetUserId(User);

            todos = todos.Where(c => c.OwnerID == currentUserId);

            if (!String.IsNullOrEmpty(searchString))
            {
                todos = todos.Where(t => t.Title.Contains(searchString));
            }

            if (tagId != 0)
            {
                todos = todos.Where(x => x.TagId == tagId);
            }

            var tagList = _context.Tags.ToList();

            var todoTagVM = new TodoTagViewModel
            {
                TagList = tagList,
                TodoList = await todos.ToListAsync()
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

            var todo = await _context.Todos.SingleOrDefaultAsync(m => m.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // GET: Todos/Create
        public IActionResult Create()
        {
            var todoEditVM = new TodoEditViewModel
            {
                TagList = _context.Tags
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

            var todo = ViewModel_to_model(new Todo(), editModel);

            todo.OwnerID = _userManager.GetUserId(User);

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

            var todo = await _context.Todos.SingleOrDefaultAsync(m => m.Id == id);
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
            var todo = await _context.Todos.SingleOrDefaultAsync(m => m.Id == id);
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

            var todo = await _context.Todos.SingleOrDefaultAsync(m => m.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todo = await _context.Todos.SingleOrDefaultAsync(m => m.Id == id);

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, TodoStatus status)
        {
            var todo = await _context.Todos.SingleOrDefaultAsync(m => m.Id == id);

            todo.Status = status;
            _context.Todos.Update(todo);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool TodoExists(int id)
        {
            return _context.Todos.Any(e => e.Id == id);
        }

        private Todo ViewModel_to_model(Todo todo, TodoEditViewModel editModel)
        {
            todo.Id = editModel.Id;
            todo.Title = editModel.Title;
            todo.TagId = editModel.TagId;
            todo.Description = editModel.Description;
            todo.StartDate = editModel.Date == DateTime.MinValue ? DateTime.Now : editModel.Date;

            return todo;
        }

        private TodoEditViewModel Model_to_viewModelAsync(Todo todo)
        {
            var editModel = new TodoEditViewModel
            {
                Id = todo.Id,
                Title = todo.Title,
                TagId = todo.TagId,
                Description = todo.Description,
                Date = todo.StartDate,
                TagList = _context.Tags
            };

            return editModel;
        }

        public string GetTagName(int id)
        {
            return _context.Tags.Where(t => t.Id == id).FirstOrDefault().Name;
        }
    }
}