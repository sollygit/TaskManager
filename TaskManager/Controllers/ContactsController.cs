using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace TaskManager.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactsController(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var contacts = from c in _context.Contacts
                           select c;

            var isAuthorized = User.IsInRole(Constants.ContactManagersRole) ||
                               User.IsInRole(Constants.ContactAdministratorsRole);

            var currentUserId = _userManager.GetUserId(User);

            // Only approved contacts are shown UNLESS you're authorized to see them or you are the owner.
            if (!isAuthorized)
            {
                contacts = contacts.Where(c => c.Status == ContactStatus.Approved || c.OwnerID == currentUserId);
            }

            // Match search string to name or email
            if (!String.IsNullOrEmpty(searchString))
            {
                contacts = contacts
                    .Where(c =>
                        c.Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1 ||
                        c.Email.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1);
            }

            return View(await contacts.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorizedRead = await _authorizationService.AuthorizeAsync(User, contact, ContactOperations.Read);

            var isAuthorizedApprove = await _authorizationService.AuthorizeAsync(User, contact, ContactOperations.Approve);

            if (contact.Status != ContactStatus.Approved &&   // Not approved.
                                  !isAuthorizedRead.Succeeded &&        // Don't own it.
                                  !isAuthorizedApprove.Succeeded)       // Not a manager.
            {
                return new ChallengeResult();
            }

            return View(contact);
        }

        public IActionResult Create()
        {
            //return View();
            // TODO: Remove, this is just for quick testing.
            return View(new ContactEditViewModel
            {
                Address = "60b Parr Terrace",
                City = "Auckland",
                Email = _userManager.GetUserName(User),
                Name = "Solly Fathi",
                State = "NZ",
                Zip = "0620"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactEditViewModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }

            var contact = ViewModel_to_model(new Contact(), editModel);

            contact.OwnerID = _userManager.GetUserId(User);

            var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                        User, contact,
                                                        ContactOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            _context.Add(contact);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts.SingleOrDefaultAsync(
                                                        m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                        User, contact,
                                                        ContactOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            var editModel = Model_to_viewModel(contact);

            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContactEditViewModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }

            // Fetch Contact from DB to get OwnerID.
            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact,
                                                                ContactOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            contact = ViewModel_to_model(contact, editModel);

            if (contact.Status == ContactStatus.Approved)
            {
                // If the contact is updated after approval, 
                // and the user cannot approve set the status back to submitted
                var canApprove = await _authorizationService.AuthorizeAsync(User, contact,
                                        ContactOperations.Approve);

                if (!canApprove.Succeeded) contact.Status = ContactStatus.Submitted;
            }

            _context.Update(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact,
                                        ContactOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            return View(contact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.ContactId == id);

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact,
                                        ContactOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, ContactStatus status)
        {
            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.ContactId == id);

            var contactOperation = (status == ContactStatus.Approved) ? ContactOperations.Approve
                                                                      : ContactOperations.Reject;

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, contact,
                                        contactOperation);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }
            contact.Status = status;
            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.ContactId == id);
        }

        private Contact ViewModel_to_model(Contact contact, ContactEditViewModel editModel)
        {
            contact.Address = editModel.Address;
            contact.City = editModel.City;
            contact.Email = editModel.Email;
            contact.Name = editModel.Name;
            contact.State = editModel.State;
            contact.Zip = editModel.Zip;

            return contact;
        }

        private ContactEditViewModel Model_to_viewModel(Contact contact)
        {
            var editModel = new ContactEditViewModel
            {
                ContactId = contact.ContactId,
                Address = contact.Address,
                City = contact.City,
                Email = contact.Email,
                Name = contact.Name,
                State = contact.State,
                Zip = contact.Zip
            };

            return editModel;
        }
    }
}