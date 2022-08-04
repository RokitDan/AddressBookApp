using AddressBookApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AddressBookApp.Models;
using AddressBookApp.Data;

namespace AddressBookApp.Services
{
    public class AddressBookService : IAddressBookService
    {
        private readonly ApplicationDbContext _context;

        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                //check to see if the category has already been added to the list
                if (!await IsContactInCategory(categoryId, contactId))
                {
                    //add the category to the database
                    Contact? contact = await _context.Contact.FindAsync(contactId);
                    Category? category = await _context.Category.FindAsync(categoryId);

                    if (contact != null && category != null)
                    {
                        category.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }
                }

            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetContactCategoriesAsync(int contactId)
        {
            try
            {
                Contact contact = await _context.Contact.Include(contact => contact.Categories).FirstOrDefaultAsync(contact => contact.Id == contactId);

                return contact.Categories;
            }
            catch
            {
                throw;
            }
        }



        public async Task<IEnumerable<int>> GetContactCategoryIdsAsync(int contactId)
        {
            try
            {
                //go to database, select contact, select all associated categories, select 1 ID from the categories list      
                Contact contact = await _context.Contact.Include(contact => contact.Categories)
                                                        .FirstOrDefaultAsync(contact => contact.Id == contactId);

                List<int> categoryIds = contact.Categories.Select(category => category.Id).ToList();
                return categoryIds;
            }
            catch
            {
                throw;
            }
        }



        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string appUserId)
        {
            List<Category> categories = new List<Category>();

            try
            {

                //LINQ statement
                //This gets categories from the Database, but only the ones related to the logged-in user. The categories are converted to a list.
                //_context is the connection to the database
                categories = await _context.Category.Where(c => c.AppUserId == appUserId).OrderBy(c => c.Name).ToListAsync();
            }
            catch
            {
                throw;
            }
            return categories;
        }

        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            try
            {
                Contact? contact = await _context.Contact.FindAsync(contactId);

                return await _context.Category
                                              .Include(c => c.Contacts)
                                              .Where(c => c.Id == categoryId && c.Contacts.Contains(contact))
                                              .AnyAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task RemoveContactFromCategoryAsync(int categoryId, int contactId)
        {
            //find contact, find category, do a null check, remove category from contact
            Contact? contact = await _context.Contact.FindAsync(contactId);
            Category? category = await _context.Category.FindAsync(categoryId);

            if (category != null && contact != null)
            {
                category.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }





        }
    }
}
