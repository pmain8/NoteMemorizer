using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NoteMemorizer
{
    class Program
    {
        static void Main(string[] args) {

            bool playAgain;
            do {
                printTitle();

                string fileName = askFileName();
                Tester t = loadNotes($"{fileName}");

                printInstructions();

                while (t.exam.getNextQuestion() != false) {
                    askQuestion(t);
                }

                playAgain = testComplete(t);
            } while (playAgain);
        }


        public static Tester loadNotes(string fileName) {
            StringBuilder file = new StringBuilder(fileName);
            Tester t = new Tester();
            Console.WriteLine("Loading file...");
            if (!fileName.Contains('.')) {
                file.Append(".txt");
            }
            bool success = t.parseFile($"{file}");
            if (!success) { System.Environment.Exit(1); }
            Console.WriteLine("...success!");
            return t;
        }

        public static void printTitle() {
            Console.WriteLine("*---------------------------------------------------------*");
            Console.WriteLine("|   Note Memorizer, a James Cameron Abreu project         |");
            Console.WriteLine("|   Copyright © 2019 - James Cameron Abreu Software Inc.  |");
            Console.WriteLine("*---------------------------------------------------------*");
            Console.WriteLine();
            Console.WriteLine();
        }

        public static void printInstructions() {
            Console.WriteLine("Let the learning begin!");
            Console.WriteLine("---------------------------------------------------------------------");
            Console.WriteLine("[Press enter to continue]");
            Console.ReadLine();
        }

        public static void askQuestion(Tester t) {
            Console.Clear();
            printTitle();
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

        public static bool testComplete(Tester t) {
            Console.Clear();
            printTitle();
            Console.WriteLine("Thanks for playing!");
            string answer;
            do {
                Console.WriteLine();
                Console.WriteLine("Would you like to play again? (yes/no)");
                Console.Write("Your response: ");
                answer = Console.ReadLine();
            } while (!answer.ToLower().Contains('y') && !answer.ToLower().Contains('n'));
            if (answer.ToLower().Contains('y')) return true;
            else return false;
        }


        public static List<string> getFileNames() {
            List<string> fileNames = Directory
                .GetFiles(@"noteFiles\", "*.txt", SearchOption.AllDirectories)
                .Select(Path.GetFileName)
                .ToList();
            return fileNames;
        }

        public static void listFileNames(List<string> fileNames, int maxListings) {
            Console.WriteLine("Files loaded in noteFiles directory:");
            int it = 1;
            foreach (var name in fileNames) {
                Console.WriteLine($"\t{name}");
                it++;
                if (it > maxListings) break;
            }
            if (it > maxListings) {
                Console.WriteLine("...");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public static string askFileName() {
            Console.Clear();
            printTitle();
            List<string> fileNames = getFileNames();
            listFileNames(fileNames, 8);
            string userInput = null;
            do {
                Console.WriteLine("Please enter the name of the file you would like to use for input");
                Console.Write("File: ");
                userInput = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(userInput));
            return userInput;
        }

    } // end main class



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
            const char REPLACE_CHAR = '-';

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
                    int amount = words.Length / 2;
                    const int MAX_TRIES = 100;
                    int tries = 0;
                    int hidden = 0;
                    int index;
                    while (tries < MAX_TRIES && hidden < amount)
                    {
                        index = r.Next(words.Length);
                        string curWord = words[index];
                        if (!curWord.Contains(REPLACE_CHAR) && !curWord.Contains("\n") && curWord.Length > 1)
                        {
                            output.Replace(curWord, parseWord(curWord));
                            hidden++;
                        }
                        tries++;
                    }
                    return output.ToString();
                } // end else
            } // end Parse method

            public string parseWord(string input) {
                StringBuilder output = new StringBuilder(input);
                int amount = input.Length / 3;
                int tries = 0;
                int hidden = 0;
                int maxTries = input.Length * 2;
                int index;
                while (tries < maxTries && hidden < amount) {
                    index = r.Next(input.Length);
                    char curChar = input[index];
                    if (curChar != REPLACE_CHAR) {
                        output[index] = REPLACE_CHAR;
                        hidden++;
                    }
                    tries++;
                }
                return output.ToString();
            }

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
