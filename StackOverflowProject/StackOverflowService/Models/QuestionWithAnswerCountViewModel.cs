using StackOverflow.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService.Models
{
    public class QuestionWithAnswerCountViewModel
    {
        public QuestionEntity Question { get; set; }
        public int AnswerCount { get; set; }
    }
}