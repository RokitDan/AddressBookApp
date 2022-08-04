using AddressBookApp.Models;
namespace AddressBookApp.Services.Interfaces
{
    public interface IAddressBookService
    {
        Task<IEnumerable<Category>> GetUserCategoriesAsync(string appUserId);
        Task AddContactToCategoryAsync(int categoryId, int contactId);
        Task<IEnumerable<Category>> GetContactCategoriesAsync(int contactId);
        Task RemoveContactFromCategoryAsync(int categoryId, int contactId);
        Task<IEnumerable<int>> GetContactCategoryIdsAsync(int contactId);

    }


}
