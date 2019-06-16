using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NoteMemorizer
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("*---------------------------------------------------------*");
            Console.WriteLine("|   Note Memorizer, a James Cameron Abreu project         |");
            Console.WriteLine("|   Copyright © 2019 - James Cameron Abreu Software Inc.  |");
            Console.WriteLine("*---------------------------------------------------------*");

            Tester t = new Tester();
            Console.WriteLine("Loading file...");
            bool success = t.parseFile("aspnetCert.txt");
            if (!success) { System.Environment.Exit(1); }

            Console.WriteLine("...success!");
            Console.WriteLine("Let the learning begin!");
            Console.WriteLine("---------------------------------------------------------------------");
            Console.WriteLine("[Press enter to continue]");
            Console.ReadLine();

            while (t.exam.getNextQuestion() != false) {
                Console.Clear();

                Console.WriteLine($"Section: {t.exam.currentSection.topic}");
                Console.WriteLine($"\t{t.exam.currentSection.howManyLeft()} out of {t.exam.currentSection.howManyTotal()} left");
                Console.WriteLine();
                Console.WriteLine($"Question {t.exam.asked} out of {t.exam.totalQuestions}: ");
                Console.WriteLine(t.exam.currentQuestion.processedQuestion);
                Console.ReadLine();
                Console.WriteLine();
                Console.WriteLine("Answer:");
                Console.WriteLine(t.exam.currentQuestion.answer);
                Console.WriteLine();
                Console.WriteLine();
            Console.WriteLine("[Press enter to continue]");
                Console.ReadLine();
            }

            Console.WriteLine("Thanks for playing!");

        }

    }


    public class Tester
    {

        const string QUESTION_SYMBOL = "*";
        const string TOPIC_SYMBOL = "~";

        public Exam exam = new Exam();

        public bool isSymbol(string word)
        {
            if (isQuestion(word))
                return true;
            else if (isTopic(word))
                return true;
            else
                return false;
        }

        public bool isQuestion(string word) { return (word == QUESTION_SYMBOL); }
        public bool isTopic(string word) { return (word == TOPIC_SYMBOL); }

        public bool parseFile(string fileName) {

            try
            {
                string[] lines = System.IO.File.ReadAllLines($@"noteFiles\{fileName}");
                string curTopic = "ERR";
                string curQuestion = "";
                foreach (string line in lines)
                {
                    if (!String.IsNullOrWhiteSpace(line)) {

                        string[] words = line.Split(' ');
                        string firstWordLine = "";

                        foreach (string word in words) {
                            if (!String.IsNullOrWhiteSpace(word))
                            {
                                firstWordLine = word;
                                break;
                            }
                        }

                        // TOPIC:
                        if (isTopic(firstWordLine)) {
                            curTopic = line; // the whole line
                            exam.addSection(line); // only adds if didn't exist

                            // wrap up previous question:
                            if (curQuestion.Length > 0) {
                                exam.addQuestion(curTopic, curQuestion);
                                curQuestion = "";
                            }
                        } // end topic

                        // QUESTION:
                        else if (isQuestion(firstWordLine)) {
                            if (curQuestion.Length > 0) {
                                exam.addQuestion(curTopic, curQuestion);
                                curQuestion = $"{line}\n";
                            }
                            else {
                                curQuestion += $"{line}\n";
                            }
                        } // end question

                        // Neither a question or a topic (trail from previous question):
                        else { curQuestion += $"{line}\n"; }

                    } //end if line is not blank
                } // end foreach

                // Finalize (any left over questions?)
                if (curQuestion.Length > 0) {
                    exam.addQuestion(curTopic, curQuestion);
                }

            } // end try
            catch (Exception e)
            {
                Console.WriteLine("Could not complete parse");
                Console.Write(e.Message);
                Console.WriteLine(e.InnerException);
                Console.WriteLine(e.StackTrace);
                return false;
            }

            var sections = exam.sections.Count;
            var totalQuestions = exam.totalQuestions;

            return true;
        }

        public class Question
        {
            public Random r = new Random();
            public string answer { get; set; }
            public string processedQuestion { get; set; }

            public Question(string input)
            {
                answer = input;
                processedQuestion = Parse(input);
            }

            public string Parse(string input)
            {

                if (input.Length < 10) { return input; }
                else
                {
                    char[] splitSymbols = { ' ', '.', '{', '}', '(', ')', '[', ']', '"', '/' };
                    string[] words = input.Split(splitSymbols);
                    if (words.Length < 2) { return input; }
                    StringBuilder output = new StringBuilder(input);
                    int amount = Math.Min(words.Length / 5, 3);
                    const int MAX_TRIES = 100;
                    int tries = 0;
                    int hidden = 0;
                    int index;
                    while (tries < MAX_TRIES && hidden < amount)
                    {
                        index = r.Next(words.Length);
                        string curWord = words[index];
                        if (!curWord.Contains("_") && !curWord.Contains("\n") && curWord.Length > 1)
                        {
                            string underscores = "";
                            foreach (char c in curWord) { underscores += "_"; }
                            output.Replace(curWord, underscores);
                            hidden++;
                        }
                        tries++;
                    }
                    return output.ToString();
                } // end else
            } // end Parse method

        } // end question class

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

        public class Exam
        {
            public Dictionary<string, Section> sections = new Dictionary<string, Section>();
            public Random r = new Random();
            public Question currentQuestion { get; set; }
            public Section currentSection { get; set; }
            public int totalQuestions { get; set; }

            public int asked { get; set; }

            public Exam()
            {
                asked = 0;
                totalQuestions = 0;
            }

            public void addSection(string section)
            {
                if (!sections.ContainsKey(section))
                {
                    Section s = new Section(section);
                    sections.Add(s.topic, s);
                }
            }


            public void addQuestion(string section, string question)
            {
                if (!sections.ContainsKey(section)) { addSection(section); }
                Question q = new Question(question);
                sections[section].add(q);
                totalQuestions++;
            }

            public Question next()
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

            public bool getNextQuestion()
            {
                Question q = next();
                if (q == null) { return false; }
                currentQuestion = q;
                return true;
            }

        }

    }



}
