using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using StackOverflow.Data.Entities;
using StackOverflow.Data.Repositories;
using StackOverflowService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StackOverflowService.Controllers
{
    public class QuestionsController : Controller
    {
        private QuestionRepository questionRepo = new QuestionRepository();
        private AnswerRepository answerRepo = new AnswerRepository();
        private UserRepository userRepo = new UserRepository();
        private VoteRepository voteRepo = new VoteRepository();

        // GET: Questions
        // Prikazuje listu svih pitanja
        public ActionResult Index(string sortBy = "datum", string searchString = "")
        {
            ViewBag.CurrentSearch = searchString;
            List<QuestionEntity> questions;

            if (!string.IsNullOrEmpty(searchString))
            {
                questions = questionRepo.SearchByTitle(searchString);
            }
            else
            {
                questions = questionRepo.GetAllQuestions();
            }

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
            answers = answers.OrderByDescending(a => a.JeNajboljiOdgovor)
                 .ThenByDescending(a => a.BrojGlasova)
                 .ToList();

            var authorEmails = new List<string> { question.AutorEmail };
            authorEmails.AddRange(answers.Select(a => a.AutorEmail));

            var authors = userRepo.GetUsersByEmails(authorEmails.Distinct().ToList());
            ViewBag.Authors = authors;

            ViewBag.Answers = answers;

            var votedAnswerIds = new HashSet<string>();
            if (Session["user_email"] != null)
            {
                string userEmail = Session["user_email"].ToString();

                var allUserVotes = voteRepo.GetAllVotesByUser(userEmail);

                votedAnswerIds = new HashSet<string>(allUserVotes.Select(v => v.PartitionKey));

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
        [ValidateInput(false)]
        public ActionResult Create(string naslov, string opisProblema, HttpPostedFileBase slikaGreske)
        {
            if (Session["user_email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string autorEmail = Session["user_email"].ToString();
            string slikaUrl = "";

            if (slikaGreske != null && slikaGreske.ContentLength > 0)
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobStorage.GetContainerReference("questionimages");

                string jedinstvenoIme = $"{Guid.NewGuid()}-{slikaGreske.FileName}";
                CloudBlockBlob blob = container.GetBlockBlobReference(jedinstvenoIme);

                blob.UploadFromStream(slikaGreske.InputStream);
                slikaUrl = blob.Uri.ToString();
            }

            var novoPitanje = new QuestionEntity(autorEmail)
            {
                Naslov = naslov,
                OpisProblema = opisProblema,
                SlikaGreskeUrl = slikaUrl
            };

            questionRepo.AddQuestion(novoPitanje);

            return RedirectToAction("Index");
        }

        // GET: Questions/Edit/5
        public ActionResult Edit(string id)
        {
            var question = questionRepo.GetQuestion(id);
            if (question == null) return HttpNotFound();

            if (Session["user_email"] == null || Session["user_email"].ToString() != question.AutorEmail)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View(question);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(QuestionEntity questionData, HttpPostedFileBase novaSlika)
        {
            var question = questionRepo.GetQuestion(questionData.RowKey);
            if (question == null) return HttpNotFound();

            if (Session["user_email"] == null || Session["user_email"].ToString() != question.AutorEmail)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            if (novaSlika != null && novaSlika.ContentLength > 0)
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobStorage.GetContainerReference("questionimages");

                if (!string.IsNullOrEmpty(question.SlikaGreskeUrl))
                {
                    string stariBlobNaziv = Path.GetFileName(new Uri(question.SlikaGreskeUrl).AbsolutePath);
                    CloudBlockBlob stariBlob = container.GetBlockBlobReference(stariBlobNaziv);
                    stariBlob.DeleteIfExists();
                }

                string jedinstvenoIme = $"{Guid.NewGuid()}-{novaSlika.FileName}";
                CloudBlockBlob noviBlob = container.GetBlockBlobReference(jedinstvenoIme);
                noviBlob.UploadFromStream(novaSlika.InputStream);
                question.SlikaGreskeUrl = noviBlob.Uri.ToString();
            }

            question.Naslov = questionData.Naslov;
            question.OpisProblema = questionData.OpisProblema;

            questionRepo.UpdateQuestion(question);
            return RedirectToAction("Details", new { id = question.RowKey });
        }

        // GET: Questions/Delete/5
        public ActionResult Delete(string id)
        {
            var question = questionRepo.GetQuestion(id);
            if (question == null) return HttpNotFound();

            if (Session["user_email"] == null || Session["user_email"].ToString() != question.AutorEmail)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var question = questionRepo.GetQuestion(id);
            if (question == null) return HttpNotFound();

            if (Session["user_email"] == null || Session["user_email"].ToString() != question.AutorEmail)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            var answersToDelete = answerRepo.GetAnswersForQuestion(id);

            if (answersToDelete.Any())
            {
                var answerIds = answersToDelete.Select(a => a.RowKey).ToList();

                voteRepo.DeleteVotesForAnswers(answerIds);

                answerRepo.DeleteAnswersForQuestion(answersToDelete);
            }

            questionRepo.DeleteQuestion(question);
            return RedirectToAction("Index");
        }

        
    }
}