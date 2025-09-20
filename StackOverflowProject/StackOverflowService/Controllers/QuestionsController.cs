using StackOverflow.Data.Entities;
using StackOverflow.Data.Repositories;
using StackOverflowService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StackOverflowService.Controllers
{
    public class QuestionsController : Controller
    {
        private QuestionRepository questionRepo = new QuestionRepository();
        private AnswerRepository answerRepo = new AnswerRepository(); // DODATO
        private VoteRepository voteRepo = new VoteRepository();

        // GET: Questions
        // Prikazuje listu svih pitanja
        public ActionResult Index(string sortBy = "datum")
        {
            var questions = questionRepo.GetAllQuestions();
            var model = new List<QuestionWithAnswerCountViewModel>();

            foreach (var q in questions)
            {
                model.Add(new QuestionWithAnswerCountViewModel
                {
                    Question = q,
                    AnswerCount = answerRepo.GetAnswersForQuestion(q.RowKey).Count
                });
            }
            if (sortBy == "odgovori")
            {
                model = model.OrderByDescending(m => m.AnswerCount).ToList();
            }
            else 
            {
                model = model.OrderByDescending(m => m.Question.Timestamp).ToList();
            }

            return View(model);
        }

        // GET: Questions/Details/5
        // Prikazuje jedno pitanje i njegove odgovore
        public ActionResult Details(string id)
        {
            var question = questionRepo.GetQuestion(id);
            if (question == null)
            {
                return HttpNotFound();
            }

            var answers = answerRepo.GetAnswersForQuestion(id);
            ViewBag.Answers = answers;

            var votedAnswerIds = new List<string>();
            if (Session["user_email"] != null)
            {
                string userEmail = Session["user_email"].ToString();
                var answerIds = answers.Select(a => a.RowKey).ToList();

                if (answerIds.Any())
                {
                    var userVotes = voteRepo.GetVotesForQuestionByUser(answerIds, userEmail);
                    votedAnswerIds = userVotes.Select(v => v.PartitionKey).ToList();
                }
            }
            ViewBag.VotedAnswerIds = votedAnswerIds;

            return View(question);
        }

        // GET: Questions/Create
        // Ova metoda samo prikazuje praznu formu za unos novog pitanja
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // Ova metoda prima podatke iz forme i kreira novo pitanje
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string naslov, string opisProblema)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string autorEmail = Session["user_email"].ToString();

            var novoPitanje = new QuestionEntity(autorEmail)
            {
                Naslov = naslov,
                OpisProblema = opisProblema,
                SlikaGreskeUrl = "" 
            };

            questionRepo.AddQuestion(novoPitanje);

            return RedirectToAction("Index");
        }
    }
}