using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StackOverflow.Data;
using StackOverflow.Data.Entities;
using StackOverflow.Data.Repositories;

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
        public ActionResult Register(string ime, string prezime, string email, string lozinka)
        {
            if (userRepo.GetUser(email) != null)
            {
                ViewBag.Error = "Korisnik sa datim emailom vec postoji.";
                return View();
            }

            var newUser = new UserEntity(email)
            {
                Ime = ime, 
                Prezime = prezime,
                Lozinka = lozinka, 
                Pol = "Nepoznato",
                DrzavaGradAdresa = "Nije uneto",
                SlikaUrl = "" 
            };

            userRepo.RegisterUser(newUser);

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
    }
}