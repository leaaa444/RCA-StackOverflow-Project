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
        private QuestionRepository questionRepo = new QuestionRepository(); 

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
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsBest(string idPitanja, string idOdgovora)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var question = questionRepo.GetQuestion(idPitanja);
            if (question == null)
            {
                return HttpNotFound();
            }

            if (Session["user_email"].ToString() != question.AutorEmail)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, "Samo autor pitanja može izabrati najbolji odgovor.");
            }

            var sviOdgovori = answerRepo.GetAnswersForQuestion(idPitanja);

            foreach (var odg in sviOdgovori)
            {
                odg.JeNajboljiOdgovor = false;
            }

            var izabraniOdgovor = sviOdgovori.FirstOrDefault(o => o.RowKey == idOdgovora);
            if (izabraniOdgovor != null)
            {
                izabraniOdgovor.JeNajboljiOdgovor = true;

                var queue = queueHelper.GetQueueReference("acceptedanswersqueue");
                queue.AddMessage(new CloudQueueMessage(idOdgovora));
            }

            answerRepo.BatchUpdateAnswers(sviOdgovori);

            return RedirectToAction("Details", "Questions", new { id = idPitanja });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public ActionResult UnmarkAsBest(string idPitanja, string idOdgovora)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var question = questionRepo.GetQuestion(idPitanja);
            if (question == null)
            {
                return HttpNotFound();
            }

            if (Session["user_email"].ToString() != question.AutorEmail)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, "Samo autor pitanja može izmeniti najbolji odgovor.");
            }

            var answer = answerRepo.GetAnswer(idPitanja, idOdgovora);
            if (answer != null)
            {
                answer.JeNajboljiOdgovor = false;
                answerRepo.UpdateAnswer(answer);
            }
            return RedirectToAction("Details", "Questions", new { id = idPitanja });
        }


    }
}