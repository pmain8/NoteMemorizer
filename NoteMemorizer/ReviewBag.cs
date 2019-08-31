using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NoteMemorizer
{
    public class ReviewBag
    {
        List<Question> questions = new List<Question>();

        Random ran;

        public void Add(Question q) {
          questions.Add(q);
        }

        public ReviewBag() {
          ran = new Random();
        }

        public Question Grab() {
          if (questions.Count() > 0) {
            int r = ran.Next(0, questions.Count() - 1);
            Question grabbed = questions[r];
            //questions.Remove(grabbed);
            return grabbed;
          }
          else
            return null;
        }

        public int Count() {
            return questions.Count;
        }

        public void Purge(Question q) {
            questions.Remove(q);
        }

    }
}
