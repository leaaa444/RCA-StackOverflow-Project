using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using StackOverflow.Data;
using StackOverflow.Data.Entities;
using StackOverflow.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StackOverflowService.Controllers
{
    public class AccountController : Controller
    {
        private UserRepository userRepo = new UserRepository();

        // GET: Account/Register
        // Ova metoda samo prikazuje praznu formu za registraciju
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        // Ova metoda prima podatke iz forme koju smo napravili
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string ime, string prezime, string email, string lozinka,
                             Pol pol, string drzava, string grad, string adresa, HttpPostedFileBase slika)
        {
            if (userRepo.GetUser(email) != null)
            {
                ViewBag.Error = "Korisnik sa datim emailom vec postoji.";
                return View();
            }

            string slikaUrl = "";
            if (slika != null && slika.ContentLength > 0)
            {
                // ... (postojeci kod za upload slike)
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobStorage.GetContainerReference("profileimages");
                string jedinstvenoIme = $"{Guid.NewGuid()}-{slika.FileName}";
                CloudBlockBlob blob = container.GetBlockBlobReference(jedinstvenoIme);
                blob.UploadFromStream(slika.InputStream);
                slikaUrl = blob.Uri.ToString();
            }

            var newUser = new UserEntity(email)
            {
                Ime = ime,
                Prezime = prezime,
                Lozinka = lozinka,
                Pol = pol,
                Drzava = drzava, 
                Grad = grad,    
                Adresa = adresa,   
                SlikaUrl = slikaUrl
            };

            userRepo.RegisterUser(newUser);

            Session["user_email"] = newUser.Email;

            return RedirectToAction("Index", "Questions");
        }


        // GET: Account/Login
        // Prikazuje formu za prijavu
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        // Obradjuje podatke iz forme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string lozinka)
        {
            if (userRepo.LoginUser(email, lozinka))
            {
                Session["user_email"] = email;

                return RedirectToAction("Index", "Questions");
            }
            else
            {
                ViewBag.Error = "Pogresan email ili lozinka.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear(); 
            return RedirectToAction("Index", "Questions");
        }

        // GET: Account/EditProfile
        public ActionResult EditProfile()
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login");
            }
            string email = Session["user_email"].ToString();
            var user = userRepo.GetUser(email);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(UserEntity formData)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login");
            }
            string email = Session["user_email"].ToString();
            var user = userRepo.GetUser(email);

            user.Ime = formData.Ime;
            user.Prezime = formData.Prezime;
            user.Pol = formData.Pol;
            user.Drzava = formData.Drzava;
            user.Grad = formData.Grad;
            user.Adresa = formData.Adresa;
            // Ne dozvoljavamo promenu emaila ili sifre na ovoj formi

            userRepo.UpdateUser(user);

            ViewBag.SuccessMessage = "Profil je uspešno ažuriran!";
            return View(user);
        }
    }
}