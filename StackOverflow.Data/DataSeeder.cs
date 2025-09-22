using StackOverflow.Data.Entities;
using StackOverflow.Data.Repositories;
using System;
using System.Collections.Generic;

namespace StackOverflow.Data
{
    public static class DataSeeder
    {
        public static void SeedInitialData()
        {
            var userRepo = new UserRepository();
            var questionRepo = new QuestionRepository();
            var answerRepo = new AnswerRepository();
            var voteRepo = new VoteRepository();

            if (userRepo.GetUser("ana.anic@email.com") != null)
            {
                return;
            }

            // === 1. KREIRANJE KORISNIKA ===
            var userAna = new UserEntity("ana.anic@email.com") { Ime = "Ana", Prezime = "Anić", Pol = Pol.Zenski, Lozinka = "ana123" };
            var userBojan = new UserEntity("bojan.b@email.com") { Ime = "Bojan", Prezime = "Bojić", Pol = Pol.Muski, Lozinka = "bojan123" };
            var userCeca = new UserEntity("ceca.c@email.com") { Ime = "Ceca", Prezime = "Cvetković", Pol = Pol.Zenski, Lozinka = "ceca123" };
            var userDarko = new UserEntity("darko.d@email.com") { Ime = "Darko", Prezime = "Darkić", Pol = Pol.Muski, Lozinka = "darko123" };
            var userEma = new UserEntity("ema.e@email.com") { Ime = "Ema", Prezime = "Emić", Pol = Pol.Zenski, Lozinka = "ema123" };
            userRepo.RegisterUser(userAna);
            userRepo.RegisterUser(userBojan);
            userRepo.RegisterUser(userCeca);
            userRepo.RegisterUser(userDarko);
            userRepo.RegisterUser(userEma);

            // Definisemo sadasnje vreme kao osnovu
            var baseTime = DateTime.UtcNow;

            // === 2. KREIRANJE PITANJA SA RAZLIČITIM VREMENOM ===
            var pitanjeSql = new QuestionEntity("darko.d@email.com") { Naslov = "Kako da uradim JOIN preko tri tabele u SQL-u?", OpisProblema = "Imam tabele Orders, Customers i Products...", Timestamp = baseTime.AddDays(-3) }; // Pre 3 dana
            var pitanjeAzure = new QuestionEntity("ana.anic@email.com") { Naslov = "Koja je razlika izmedju Worker Role i Web Role u Azure?", OpisProblema = "Nije mi najjasnija fundamentalna razlika...", Timestamp = baseTime.AddDays(-2) }; // Pre 2 dana
            var pitanjeCss = new QuestionEntity("ana.anic@email.com") { Naslov = "Kako da centriram div u CSS-u?", OpisProblema = "Pokušavam da centriram jedan div...", Timestamp = baseTime.AddHours(-5) }; // Pre 5 sati
            var pitanjeCSharp = new QuestionEntity("bojan.b@email.com") { Naslov = "Problem sa NullReferenceException u C#", OpisProblema = "Konstantno dobijam 'Object reference...' grešku.", Timestamp = baseTime.AddMinutes(-30) }; // Pre 30 minuta
            questionRepo.AddQuestion(pitanjeSql);
            questionRepo.AddQuestion(pitanjeAzure);
            questionRepo.AddQuestion(pitanjeCss);
            questionRepo.AddQuestion(pitanjeCSharp);

            // === 3. KREIRANJE ODGOVORA SA RAZLIČITIM VREMENOM ===
            // Odgovori na CSS pitanje
            var odgCss1 = new AnswerEntity(pitanjeCss.RowKey, "bojan.b@email.com") { TekstOdgovora = "Najlakši način je da koristiš flexbox...", Timestamp = baseTime.AddHours(-4) };
            var odgCss2 = new AnswerEntity(pitanjeCss.RowKey, "ceca.c@email.com") { TekstOdgovora = "Možeš i sa 'position: absolute'...", Timestamp = baseTime.AddHours(-3) };
            var odgCss3 = new AnswerEntity(pitanjeCss.RowKey, "darko.d@email.com") { TekstOdgovora = "Grid je takođe odlična opcija...", Timestamp = baseTime.AddHours(-2) };
            answerRepo.AddAnswer(odgCss1);
            answerRepo.AddAnswer(odgCss2);
            answerRepo.AddAnswer(odgCss3);

            // Odgovori na C# pitanje
            var odgCSharp1 = new AnswerEntity(pitanjeCSharp.RowKey, "ana.anic@email.com") { TekstOdgovora = "Ta greška znači da pokušavaš da koristiš objekat koji je 'null'...", Timestamp = baseTime.AddMinutes(-20) };
            var odgCSharp2 = new AnswerEntity(pitanjeCSharp.RowKey, "ceca.c@email.com") { TekstOdgovora = "Da, 99% je problem u tome što neki objekat nije kreiran sa 'new'...", Timestamp = baseTime.AddMinutes(-10) };
            answerRepo.AddAnswer(odgCSharp1);
            answerRepo.AddAnswer(odgCSharp2);

            // Odgovori na Azure pitanje
            var odgAzure1 = new AnswerEntity(pitanjeAzure.RowKey, "darko.d@email.com") { TekstOdgovora = "Web Role ima IIS i služi za hostovanje web sajtova...", Timestamp = baseTime.AddDays(-1) };
            answerRepo.AddAnswer(odgAzure1);

            // === 4. KREIRANJE GLASOVA ===
            voteRepo.AddVote(new VoteEntity(odgCss1.RowKey, "ana.anic@email.com"));
            voteRepo.AddVote(new VoteEntity(odgCss1.RowKey, "ceca.c@email.com"));
            voteRepo.AddVote(new VoteEntity(odgCss1.RowKey, "darko.d@email.com"));
            voteRepo.AddVote(new VoteEntity(odgCss1.RowKey, "ema.e@email.com"));
            odgCss1.BrojGlasova = 4;

            voteRepo.AddVote(new VoteEntity(odgCSharp2.RowKey, "ana.anic@email.com"));
            odgCSharp2.BrojGlasova = 1;

            voteRepo.AddVote(new VoteEntity(odgCSharp1.RowKey, "ceca.c@email.com"));
            odgCSharp1.BrojGlasova = 1;

            answerRepo.BatchUpdateAnswers(new List<AnswerEntity> { odgCss1, odgCSharp1, odgCSharp2 });

            // === 5. OZNACAVANJE NAJBOLJEG ODGOVORA ===
            odgCSharp1.JeNajboljiOdgovor = true;
            answerRepo.UpdateAnswer(odgCSharp1);
        }
    }
}