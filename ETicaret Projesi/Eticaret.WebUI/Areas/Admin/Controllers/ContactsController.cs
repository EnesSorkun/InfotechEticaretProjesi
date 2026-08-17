using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class ContactsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;

        public ContactsController(
            DatabaseContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // =====================================================
        // LISTELEME
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var contacts = await _context.Contacts
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();

            return View(contacts);
        }


        // =====================================================
        // DETAY
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(x => x.Id == id);

            if (contact is null)
            {
                return NotFound();
            }

            return View(contact);
        }


        // =====================================================
        // DELETE GET
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(x => x.Id == id);

            if (contact is null)
            {
                return NotFound();
            }

            return View(contact);
        }


        // =====================================================
        // DELETE POST
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts
                .FindAsync(id);

            if (contact is null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(contact);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "İletişim kaydı başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // MAIL SAYFASI
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> SendMail(int id)
        {
            var contact = await _context.Contacts
                .FindAsync(id);

            if (contact is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(contact.Email))
            {
                TempData["ErrorMessage"] =
                    "Bu kullanıcıya ait email adresi bulunmuyor.";

                return RedirectToAction(nameof(Index));
            }

            // _SendMail.cshtml Shared klasöründe olduğu için
            // View adını açıkça belirtiyoruz.
            return View("_SendMail", contact);
        }


        // =====================================================
        // MAIL GÖNDER
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMail(
            int id,
            string subject,
            string message)
        {
            var contact = await _context.Contacts
                .FindAsync(id);

            if (contact is null)
            {
                return NotFound();
            }


            // Kullanıcının email adresi var mı?
            if (string.IsNullOrWhiteSpace(contact.Email))
            {
                TempData["ErrorMessage"] =
                    "Bu kullanıcıya ait email adresi bulunmuyor.";

                return RedirectToAction(nameof(Index));
            }


            // Konu kontrolü
            if (string.IsNullOrWhiteSpace(subject))
            {
                ModelState.AddModelError(
                    "subject",
                    "Mail konusu boş bırakılamaz.");
            }


            // Mesaj kontrolü
            if (string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(
                    "message",
                    "Mail mesajı boş bırakılamaz.");
            }


            if (!ModelState.IsValid)
            {
                return View("_SendMail", contact);
            }


            // =================================================
            // SMTP AYARLARI
            // =================================================

            var smtpHost =
                _configuration["MailSettings:Host"];

            var smtpPortValue =
                _configuration["MailSettings:Port"];

            var smtpUser =
                _configuration["MailSettings:UserName"];

            var smtpPassword =
                _configuration["MailSettings:Password"];

            var fromEmail =
                _configuration["MailSettings:FromEmail"];

            var fromName =
                _configuration["MailSettings:FromName"];


            // Ayarlar eksik mi?
            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpPortValue) ||
                string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPassword) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                TempData["ErrorMessage"] =
                    "Mail ayarları eksik veya hatalı.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            if (!int.TryParse(
                    smtpPortValue,
                    out var smtpPort))
            {
                TempData["ErrorMessage"] =
                    "SMTP port bilgisi geçersiz.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // MAIL GÖNDERME
            // =================================================

            try
            {
                using var smtpClient =
                    new SmtpClient(
                        smtpHost,
                        smtpPort);

                smtpClient.EnableSsl = true;

                smtpClient.UseDefaultCredentials = false;

                smtpClient.Credentials =
                    new NetworkCredential(
                        smtpUser,
                        smtpPassword);


                using var mailMessage =
                    new MailMessage();

                mailMessage.From =
                    new MailAddress(
                        fromEmail,
                        string.IsNullOrWhiteSpace(fromName)
                            ? "Eticaret"
                            : fromName);

                mailMessage.To.Add(
                    contact.Email);

                mailMessage.Subject =
                    subject.Trim();

                mailMessage.Body =
                    message.Trim();

                mailMessage.IsBodyHtml =
                    false;


                await smtpClient
                    .SendMailAsync(mailMessage);


                TempData["SuccessMessage"] =
                    $"{contact.Email} adresine mail başarıyla gönderildi.";


                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch (SmtpException)
            {
                TempData["ErrorMessage"] =
                    "Mail gönderilirken SMTP bağlantı hatası oluştu. Mail ayarlarınızı kontrol ediniz.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "Mail gönderilirken beklenmeyen bir hata oluştu.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }
    }
}