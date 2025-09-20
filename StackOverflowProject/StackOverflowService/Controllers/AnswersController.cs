using Microsoft.WindowsAzure.Storage.Queue;
using StackOverflow.Data.Entities;
using StackOverflow.Data.Helpers;
using StackOverflow.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StackOverflowService.Controllers
{
    public class AnswersController : Controller
    {
        private AnswerRepository answerRepo = new AnswerRepository();
        private VoteRepository voteRepo = new VoteRepository(); 
        private QueueHelper queueHelper = new QueueHelper();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string idPitanja, string tekstOdgovora)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string autorEmail = Session["user_email"].ToString();

            var noviOdgovor = new AnswerEntity(idPitanja, autorEmail)
            {
                TekstOdgovora = tekstOdgovora
            };

            answerRepo.AddAnswer(noviOdgovor);

            return RedirectToAction("Details", "Questions", new { id = idPitanja });
        }

        // POST: Answers/Upvote
        [HttpPost]
        public ActionResult Upvote(string idPitanja, string idOdgovora)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userEmail = Session["user_email"].ToString();

            // Proveravamo da li je korisnik vec glasao
            if (!voteRepo.HasVoted(idOdgovora, userEmail))
            {
                // Ako nije, dodaj glas
                voteRepo.AddVote(new VoteEntity(idOdgovora, userEmail));

                var answer = answerRepo.GetAnswer(idPitanja, idOdgovora);
                if (answer != null)
                {
                    answer.BrojGlasova++;
                    answerRepo.UpdateAnswer(answer);
                }
            }
            // Ako je vec glasao, ne radimo nista.

            return RedirectToAction("Details", "Questions", new { id = idPitanja });
        }

        // POST: Answers/MarkAsBest
        [HttpPost]
        public ActionResult MarkAsBest(string idPitanja, string idOdgovora)
        {
            // TODO: Dodati proveru da li je trenutni korisnik autor pitanja

            var answer = answerRepo.GetAnswer(idPitanja, idOdgovora);
            if (answer != null)
            {
                answer.JeNajboljiOdgovor = true;
                answerRepo.UpdateAnswer(answer);

                var queue = queueHelper.GetQueueReference("acceptedanswersqueue");
                queue.AddMessage(new CloudQueueMessage(idOdgovora));
            }
            return RedirectToAction("Details", "Questions", new { id = idPitanja });
        }




    }
}