using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace NoteMemorizer
{

    public class Exam
    {
        public Dictionary<string, Section> sections = new Dictionary<string, Section>();
        public Random r = new Random();
        public Question currentQuestion { get; set; }
        public Section currentSection { get; set; }
        public int totalQuestions { get; set; }

        Stack<Question> prevQuestions = new Stack<Question>();
        Stack<Question> forwardQuestions = new Stack<Question>();

        ReviewBag reviewQuestions = new ReviewBag();
        int QuestionsCompleted { get; set; }
        double chanceIncreaser;

        public int NumberQuestionsThisSession { get; set; }

        public int NumberQuestionsForReview() {
            return reviewQuestions.Count();
        }


        public void CompleteQuestion(Question q) {
            // Remove all instances of q in review bag
            reviewQuestions.Purge(q);

            // increment counter
            QuestionsCompleted++;
        }

        public void GrabReviewQuestion() {
            if (reviewQuestions.Count() > 0) {
                prevQuestions.Push(currentQuestion);
                currentQuestion = reviewQuestions.Grab();
            }
            else {
                Forward();
            }
        }

        public void AddReviewQuestion(Question q) {
            reviewQuestions.Add(q);
        }

        public bool HasForwardQuestions()
        {
            return forwardQuestions.Count > 0;
        }

        public bool HasPreviousQuestions()
        {
            return prevQuestions.Count > 0;
        }

        private Question _getPreviousQuestion()
        {
            if (prevQuestions.Count > 0)
                return prevQuestions.Pop();
            else
                return null;
        }

        private Question _getForwardOrCurrentQuestion()
        {
            if (forwardQuestions.Count > 0)
                return forwardQuestions.Pop();
            else
                return null;
        }

        public bool Back()
        {
            Question pq = _getPreviousQuestion();
            if (pq != null)
            {
                forwardQuestions.Push(currentQuestion);
                currentQuestion = pq;
                return true;
            }

            // We went all the way back
            else
                return false;
        }

        public bool Forward()
        {

            Question fq = _getForwardOrCurrentQuestion();
            if (fq != null)
            {
                prevQuestions.Push(currentQuestion);
                currentQuestion = fq;
                return true;
            }

            // No more questions
            else
                return false;
        }

        public int NumForwardQuestions()
        {
            return forwardQuestions.Count();
        }

        public int asked { get; set; }
        public TestTaker.testType examType { get; set; }

        // CONSTRUCTOR:
        public Exam()
        {
            asked = 0;
            totalQuestions = 0;
            QuestionsCompleted = 0;
            chanceIncreaser = 0.0;
            NumberQuestionsThisSession = 1;
        }

        public void addSection(string section)
        {
            if (!sections.ContainsKey(section))
            {
                Section s = new Section(section);
                sections.Add(s.topic, s);
            }
        }


        public void addQuestion(string section, string question, string answer)
        {
            if (!sections.ContainsKey(section)) { addSection(section); }
            Question q = new Question(question, answer, this.examType);
            sections[section].add(q);
            totalQuestions++;
        }

        public Question _getNewQuestion()
        {
            if (sections.Count <= 0) { return null; }
            // pick random section (that has questions still)
            Question cur = null;
            while (cur == null && sections.Count > 0) {
                var i = r.Next(sections.Count);
                string t = sections.ElementAt(i).Key;
                // Remove section if it doesn't have any questions:
                if (!sections[t].hasQuestions()) { sections.Remove(t); }
                else {
                    cur = sections[t].next();
                    currentSection = sections[t];
                }
            }

            asked++;
            return cur;
        }

        public bool GetNewQuestion()
        {
            // store previous question, if existed:
            if (currentQuestion != null)
                prevQuestions.Push(currentQuestion); // add to previous questions


            // Check for review questions:
            Question q;
            double chancePerc = 100.00 * ((double)QuestionsCompleted / (double)NumberQuestionsThisSession) + chanceIncreaser;
            int random = r.Next(0, 100);

            /*
            Console.WriteLine($"chanceIncreaser: {chanceIncreaser}");
            Console.WriteLine($"QuestionsCompleted: {QuestionsCompleted}, totalQuestions: {NumberQuestionsThisSession}");
            Console.WriteLine($"chancePerc: {chancePerc}, random rolled: {random}");
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
            */

            if (reviewQuestions.Count() > 0 && random < chancePerc) {
                q = reviewQuestions.Grab();
                q.SetAsReviewQuestion();
                chanceIncreaser = 0.0; // reset
            }
            else {
               q = _getNewQuestion();

                // Increase chance of getting review question each time NOT shown
                if (reviewQuestions.Count() > 0) {
                    chanceIncreaser += 15.0;
                }
            }

            if (q == null) { return false; }
            currentQuestion = q;
            return true;
        }

    }




}
