using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AddressBookApp.Data;
using AddressBookApp.Models;
using AddressBookApp.Models.ViewModels;
using AddressBookApp.Enums;
using AddressBookApp.Services;
using AddressBookApp.Services.Interfaces;

namespace AddressBookApp.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IABEmailService _emailService;

        public ContactsController(ApplicationDbContext context,
                                  UserManager<AppUser> userManager,
                                  IImageService imageService,
                                  IAddressBookService addressBookService,
                                  IABEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Contacts
        [Authorize]
        public async Task<IActionResult> Index(int categoryId, string swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;

            string appUserId = _userManager.GetUserId(User);

            List<Contact> contactList = new List<Contact>();

            AppUser appUser = await _context.Users.Include(user => user.Contacts)
                                                  .ThenInclude(contact => contact.Categories)
                                                  .FirstOrDefaultAsync(user => user.Id == appUserId);


            if (categoryId == 0)
            {
                contactList = appUser.Contacts.OrderBy(contact => contact.LastName)
                                              .ThenBy(contact => contact.FirstName)
                                              .ToList();
            }
            else
            {
                contactList = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)
                                  .Contacts
                                  .OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                  .ToList();
            }




            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", categoryId);
            return View(contactList);
        }


        [Authorize]
        //the (string searchString) parameter matches the name of the input in the form in the view
        public async Task<IActionResult> Search(string searchInput)
        {

            //UserManager is built into ASP.NET. User is a built-in object
            string appUserId = _userManager.GetUserId(User);
            List<Contact> contacts = new List<Contact>();

            AppUser appUser = await _context.Users.Include(user => user.Contacts)
                                                  .ThenInclude(contact => contact.Categories)
                                                  .FirstOrDefaultAsync(user => user.Id == appUserId);

            if (string.IsNullOrEmpty(searchInput))
            {
                contacts = appUser.Contacts.OrderBy(contact => contact.LastName)
                                           .ThenBy(contact => contact.FirstName)
                                           .ToList();

            }
            else
            {
                contacts = appUser.Contacts.Where(contact => contact.FullName!.ToLower().Contains(searchInput.ToLower()))
                                           .OrderBy(contact => contact.LastName)
                                           .ThenBy(contact => contact.FirstName)
                                           .ToList();
            }




            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name");

            return View(nameof(Index), contacts);
        }



        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            //determines who is logged in user based onunique user ID
            string appUserId = _userManager.GetUserId(User);

            //get list of states - being passed as viewBag. StatesList is not part of model - it is in an enum
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());

            //get list of catgories. MultiSelect allows users to select more than one item. get categories for given user. Users create their own categories
            //a service was written so that this logic can be used in multiple places
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");

            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,AddressOne,AddressTwo,City,State,ZipCode,EmailAddress,PhoneNumber,ImageFile")] Contact contact, List<int> CategoryList)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                contact.CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);

                }

                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                //Save images to database as byte arrays
                _context.Add(contact);
                await _context.SaveChangesAsync();

                //Add contact to categories
                foreach (int categoryId in CategoryList)
                {
                    //save each category to the database for the contact
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                }



            }
            return RedirectToAction(nameof(Index));

        }

        // GET: Contacts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            Contact contact = await _context.Contact.Where(c => c.Id == id && c.AppUserId == appUserId).FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _addressBookService.GetContactCategoryIdsAsync(contact.Id));

            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,AddressOne,AddressTwo,City,State,ZipCode,EmailAddress,PhoneNumber,CreatedDate,ImageData,ImageFile,ImageType")] Contact contact, List<int> CategoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.CreatedDate = DateTime.SpecifyKind(contact.CreatedDate, DateTimeKind.Utc);

                    if (contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }

                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }



                    _context.Update(contact);




                    //1) remove all categories associated with this contact

                    //1.a) get a list of the old categories (categories associated with contact prior to editing)
                    List<Category> oldCategories = (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();

                    foreach (Category category in oldCategories)
                    {
                        //remove a category from the contact during each cycle of the loop
                        await _addressBookService.RemoveContactFromCategoryAsync(category.Id, contact.Id);
                    }


                    //2) then we add back the selected categories to the contact
                    foreach (int categoryId in CategoryList)
                    {
                        //save each category to the database for the contact
                        await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                    }







                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contact == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contact == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contact'  is null.");
            }
            var contact = await _context.Contact.FindAsync(id);
            if (contact != null)
            {
                _context.Contact.Remove(contact);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return (_context.Contact?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize]
        public async Task<IActionResult> EmailContact(int id)
        {
            string appUserId = _userManager.GetUserId(User);
            Contact contact = await _context.Contact.Where(c => c.Id == id && c.AppUserId == appUserId)
                                                    .FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }

            EmailData emailData = new()
            {
                EmailAddress = contact.EmailAddress,
                FirstName = contact.FirstName,
                LastName = contact.LastName,

            };

            EmailContactViewModel model = new EmailContactViewModel()
            {
                Contact = contact,
                EmailData = emailData,

            };

            return View(model);


        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EmailContact(EmailContactViewModel ecvm)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(ecvm.EmailData.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Success: Email Sent!" });
                }
                catch
                {
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Error, Email Not Sent!" });
                    throw;
                }
            }
            return View(ecvm);

        }



    }




}

