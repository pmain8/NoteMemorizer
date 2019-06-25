﻿using System;
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
            int numQuestions;
            do {
                printTitle();
                Tester t;
                bool foundFile;
                do {
                    string fileName = askFileName();
                    t = loadNotes($"{fileName}", out foundFile);
                    if (!foundFile) {
                        Console.WriteLine("\nCould not find file specified.");
                        Console.WriteLine("Please choose a file listed or copy desired text file to 'noteFiles' directory");
                        Console.WriteLine("Press enter to continue...");
                        Console.ReadLine();
                    }
                } while (!foundFile);

                numQuestions = askHowManyQuestions(t);

                printInstructions();

                while (t.exam.asked < numQuestions && t.exam.getNextQuestion() != false) {
                    askQuestion(t);
                }

                playAgain = testComplete(t);
            } while (playAgain);
        }


        public static Tester loadNotes(string fileName, out bool foundFile) {
            StringBuilder file = new StringBuilder(fileName);
            Tester t = new Tester();
            askType(t);
            Console.WriteLine("Loading file...");
            if (!fileName.Contains('.')) {
                file.Append(".txt");
            }
            bool success = t.parseFile($"{file}");
            if (!success) {
                foundFile = false;
                Console.WriteLine("...failure :(");
            }
            else {
                foundFile = true;
                Console.WriteLine("...success!");
            }
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
            Console.WriteLine();
            Console.WriteLine("Let the learning begin!");
            Console.WriteLine("[Press enter to continue]");
            Console.ReadLine();
        }

        public static void askQuestion(Tester t) {
            Console.Clear();
            printTitle();
            Console.WriteLine($"Section: {t.exam.currentSection.topic}");
            Console.WriteLine($"\t{t.exam.currentSection.howManyLeft()} out of {t.exam.currentSection.howManyTotal()} left");
            Console.WriteLine();
            Console.WriteLine($"Question {t.exam.asked} out of {t.numQuestionsDesired}: ");
            Console.WriteLine();
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

        public static int askHowManyQuestions(Tester t) {
            Console.Clear();
            printTitle();
            Console.WriteLine();
            Console.WriteLine($"This test contains {t.exam.totalQuestions} questions.");
            Console.WriteLine($"How many would you like to learn?");
            Console.WriteLine($"\t* Enter '0' for all");
            Console.WriteLine($"\t* Otherwise provide how many");
            Console.WriteLine();
            int num = t.exam.totalQuestions; // default
            string userInput = null;
            bool parseSuccess = false;
            do {
                Console.WriteLine();
                Console.Write("Your choice: ");
                userInput = Console.ReadLine();
                parseSuccess = int.TryParse(userInput, out num);
            } while (string.IsNullOrWhiteSpace(userInput) || !parseSuccess || (num > t.exam.totalQuestions) || (num < 0) );

            if (num <= 0)
                num = t.exam.totalQuestions;

            t.numQuestionsDesired = num;

            return num;
        }

        public static void askType(Tester t) {
            bool goodKey;
            string key;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Which type of test will you take?");
            Console.WriteLine("\t[1] Keywords Partial (words indicated in txt file will be partially hidden)");
            Console.WriteLine("\t[2] Keywords Full (words indicated in txt file will be fully hidden)");
            Console.WriteLine("\t[3] Full Random (all words in answer will be randomly hidden)");
            Console.WriteLine();
            do {
                Console.Write("Your choice: ");
                key = Console.ReadLine();
                goodKey = (key == "1" || key == "2" || key == "3");
                if (!goodKey) {
                    Console.WriteLine();
                    Console.WriteLine("Please provide your choice by entering 1, 2, or 3 on your keyboard");
                }
            } while (!goodKey);
            int keyNum = int.Parse(key);
            t.setExamType((Tester.testType)keyNum);
            return;
        }


    } // end main class



    public class Tester
    {

        public enum testType { keywordsPartial = 1, kewordsFull = 2, fullRandom = 3 }

        const string QUESTION_SYMBOL = "#";
        const string ANSWER_SYMBOL = "*";
        const string TOPIC_SYMBOL = "~";
        const string KEYWORD_SYMBOL = "^";

        public Exam exam = new Exam();

        public int numQuestionsDesired { get; set; }

        public bool isSymbol(string word)
        {
            if (isAnswer(word))
                return true;
            else if (isTopic(word))
                return true;
            else if (isQuestion(word))
                return true;
            else
                return false;
        }

        public bool isQuestion(string word) { return (word == QUESTION_SYMBOL); }
        public bool isAnswer(string word) { return (word == ANSWER_SYMBOL); }
        public bool isTopic(string word) { return (word == TOPIC_SYMBOL); }

        public void setExamType(testType _type) {
            if (exam != null) {
                exam.examType = _type;
            }
        }

        public bool parseFile(string fileName) {

            try
            {
                string[] lines = System.IO.File.ReadAllLines($@"noteFiles\{fileName}");
                string curSection = "~ Unsorted / Miscellaneous"; // just in case user forgets first section
                string curQuestion = "";
                string curAnswer = "";
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
                            curSection = line; // the whole line
                            exam.addSection(line); // only adds if didn't exist

                            // wrap up previous question:
                            if (curQuestion.Length > 0 && curAnswer.Length > 0) {
                                exam.addQuestion(curSection, curQuestion, curAnswer);
                                curAnswer = "";
                                curQuestion = "";
                            }
                        } // end topic

                        // QUESTION
                        else if (isQuestion(firstWordLine)) {
                            if (curQuestion.Length > 0) {
                                if (curAnswer.Length > 0) {
                                    exam.addQuestion(curSection, curQuestion, curAnswer);
                                    curAnswer = "";
                                    curQuestion = $"{line}\n";
                                }
                                else {
                                    // no answer to previous question,
                                    // ignore previous question
                                    curQuestion = $"{line}\n";
                                }
                            }
                            else {
                                curQuestion = $"{line}\n";
                            }
                        }

                        // ANSWER:
                        else if (isAnswer(firstWordLine)) {
                            // We don't want more than one answer per question,
                            // However, extra lines can be added to answers
                            if (curQuestion.Length > 0) {
                                if (curAnswer.Length == 0) {
                                    curAnswer = $"{line}\n";
                                }
                                else {
                                    curAnswer += $"\n{line}\n";
                                }
                            }
                        } // end answer

                        // OTHER
                        else {

                            // first try to add extra lines to answer
                            if (curAnswer.Length > 0) {
                                curAnswer += $"{line}\n";
                            }

                            // if not, try to add it to the question
                            else if (curQuestion.Length > 0) {
                                curQuestion += $"{line}\n";
                            }
                        }

                    } //end if line is not blank
                } // end foreach

                // Finalize (any left over questions?)
                if (curAnswer.Length > 0) {
                    exam.addQuestion(curSection, curQuestion, curAnswer);
                }

            } // end try
            catch (Exception e)
            {
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

            public Question(string question, string _answer, testType tt)
            {
                answer = _answer;
                switch (tt) {
                    case testType.kewordsFull:
                    case testType.keywordsPartial:
                        processedQuestion = $"{question}\n\n{ParseKeyword(_answer, tt)}";
                        break;

                    case testType.fullRandom:
                    default:
                        processedQuestion = $"{question}\n\n{ParseFullRandom(_answer)}";
                        break;
                }
            }

            public string ParseFullRandom(string input) {
                if (input.Length < 10) { return input; }
                else
                {
                    char[] splitSymbols = { ' ', '.', '{', '}', '(', ')', '[', ']', '"', '/' };
                    string[] words = input.Split(splitSymbols);
                    if (words.Length < 2) { return input; }
                    StringBuilder output = new StringBuilder(input);
                    int amount = (int)(words.Length * 0.75);
                    const int MAX_TRIES = 100;
                    int tries = 0;
                    int hidden = 0;
                    int index;
                    while (tries < MAX_TRIES && hidden < amount)
                    {
                        index = r.Next(words.Length);
                        string curWord = words[index];
                        if (!curWord.Contains(REPLACE_CHAR) && curWord.Length > 1)
                        {
                            output.Replace(curWord, partialReplace(curWord));
                            hidden++;
                        }
                        tries++;
                    }
                    return output.ToString();
                } // end else
            }
            public string ParseKeyword(string input, testType tt) {
                if (input.Length < 10) { return input; }
                else
                {
                    char[] splitSymbols = { ' ', '.', '{', '}', '(', ')', '[', ']', '"', '/' };
                    string[] words = input.Split(splitSymbols);
                    if (words.Length < 2) { return input; }
                    StringBuilder output = new StringBuilder(input);
                    foreach (string curWord in words)
                    {
                        if (!curWord.Contains(REPLACE_CHAR) && curWord.Length > 1 && curWord.Contains(KEYWORD_SYMBOL))
                        {
                            if (tt == testType.kewordsFull)
                                output.Replace(curWord, fullReplace(curWord));
                            else
                                output.Replace(curWord, partialReplace(curWord));
                        }
                    }
                    return output.ToString();
                } // end else
            }


            public string partialReplace(string input) {
                if (input.Length <= 3) return input;
                StringBuilder output = new StringBuilder(input);
                int amount = (int)(input.Length * 0.65);
                int tries = 0;
                int hidden = 0;
                int maxTries = input.Length * 2;
                int index;
                while (tries < maxTries && hidden < amount) {
                    index = r.Next(1, input.Length);
                    char curChar = input[index];
                    if (curChar != REPLACE_CHAR && curChar != '\n') {
                        output[index] = REPLACE_CHAR;
                        hidden++;
                    }
                    tries++;
                }
                return output.ToString();
            }

            public string fullReplace(string input) {
                if (input.Length <= 3) return input;
                StringBuilder output = new StringBuilder(input);
                for (int i = 1; i < input.Length; i++) {
                    if (output[i] != '\n') // don't replace newline chars
                        output[i] = REPLACE_CHAR;
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
            public testType examType { get; set; }

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


            public void addQuestion(string section, string question, string answer)
            {
                if (!sections.ContainsKey(section)) { addSection(section); }
                Question q = new Question(question, answer, this.examType);
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
