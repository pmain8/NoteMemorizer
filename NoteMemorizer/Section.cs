using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NoteMemorizer
{

        public class Section
        {
            public string topic { get; set; }
            public HashSet<Question> questions = new HashSet<Question>();
            public HashSet<Question> previous = new HashSet<Question>();
            public Random r = new Random();

            public int howManyLeft()
            {
                return questions.Count;
            }

            public int howManyTotal()
            {
                return questions.Count + previous.Count;
            }

            public Section(string top)
            {
                topic = top;
            }

            public void add(Question q)
            {
                questions.Add(q);
            }

            public bool hasQuestions()
            {
                return (questions.Count > 0);
            }

            public Question next()
            {
                var i = r.Next(questions.Count);
                Question q = questions.ElementAt(i);
                previous.Add(q);
                questions.Remove(q);
                return q;
            }


        }


}
