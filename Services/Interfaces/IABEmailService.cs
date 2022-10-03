using AddressBookApp.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AddressBookApp.Services.Interfaces
{
    public interface IABEmailService : IEmailSender
    {
        Task SendEmailAsync(AppUser appUser, List<Contact> contacts, EmailData emailData);
    }

}
