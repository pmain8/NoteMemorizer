using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NoteMemorizer
{

    class Program
    {

        public enum keyCommand
        {

            FORWARD_ONE = 0,
            BACKWARD_ONE,
            NEXT,
            ESCAPE,
            RESTART_QUESTION
        }

        static void Main(string[] args)
        {

            bool playAgain;
            int numQuestions;
            do
            {
                printTitle();
                TestTaker t;
                bool foundFile;
                do
                {
                    string fileName = askFileName();
                    t = loadNotes($"{fileName}", out foundFile);
                    if (!foundFile)
                    {
                        Console.WriteLine("\nCould not find file specified.");
                        Console.WriteLine("Please choose a file listed or copy desired text file to 'noteFiles' directory");
                        Console.WriteLine("Press enter to continue...");
                        Console.ReadLine();
                    }
                } while (!foundFile);

                numQuestions = askHowManyQuestions(t);

                printInstructions();

                keyCommand command;
                while ((t.exam.asked < numQuestions && t.exam.GetNewQuestion() != false) || t.exam.NumberQuestionsForReview() <= 0)
                {
                    do
                    {
                        // Ask the question
                        command = askQuestion(t);

                        // Pressed Left Arrow:
                        if (command == keyCommand.BACKWARD_ONE)
                            t.Back();

                        // Pressed Right Arrow:
                        else if (command == keyCommand.FORWARD_ONE)
                            t.Forward();

                        // Pressed Enter, but is reviewing question in past
                        else if (command == keyCommand.NEXT && t.HasForwardQuestions())
                        {
                            t.Forward();
                            command = keyCommand.FORWARD_ONE;
                        }

                        // Pressed Left or Up arrow while looking at answer (restarts question)
                        else if (command == keyCommand.RESTART_QUESTION) {  /* do nothing */ }


                    } while (command != keyCommand.ESCAPE && command != keyCommand.NEXT);

                    // End prematurely
                    if (command == keyCommand.ESCAPE)
                        break;
                }
                playAgain = testComplete(t);

            } while (playAgain);
        }


        public static TestTaker loadNotes(string fileName, out bool foundFile)
        {
            StringBuilder file = new StringBuilder(fileName);
            TestTaker t = new TestTaker();
            askType(t);
            Console.WriteLine("Loading file...");
            if (!fileName.Contains('.'))
            {
                file.Append(".txt");
            }
            bool success = t.parseFile($"{file}");
            if (!success)
            {
                foundFile = false;
                Console.WriteLine("...failure :(");
            }
            else
            {
                foundFile = true;
                Console.WriteLine("...success!");
            }
            return t;
        }

        public static void printTitle()
        {
            Console.WriteLine("*------------------------------------------------------------------*");
            Console.WriteLine("|  Note Memorizer | An Educational Project by James Cameron Abreu  |");
            Console.WriteLine("*------------------------------------------------------------------*");
            Console.WriteLine();
        }

        public static void printInstructions()
        {
            Console.WriteLine();
            Console.WriteLine("Let the learning begin!");
            Console.WriteLine("[Press enter to continue]");
            Console.ReadLine();
        }

        public static keyCommand askQuestion(TestTaker t)
        {

            // TITLE
            Console.Clear();
            printTitle();

            // REVIEW QUESTION INDICATOR
            if (t.exam.currentQuestion.IsReviewQuestion)
            {
                Console.WriteLine("                 *---------------------*");
                Console.WriteLine("                 | ~[REVIEW QUESTION]~ |");
                Console.WriteLine("                 *---------------------*");
                Console.WriteLine();
            }

            // HEADER
            string trimmedSectionName = (t.exam.currentSection.topic).Replace(TestTaker.TOPIC_SYMBOL, "");
            int sectionNum = t.exam.currentSection.howManyTotal() - t.exam.currentSection.howManyLeft();
            Console.WriteLine($"SECTION: {trimmedSectionName} [Question {sectionNum} out of {t.exam.currentSection.howManyTotal()}]");

            // Question
            int questionNum = t.exam.currentQuestion.QuestionNumber;
            string reviewIndicator = t.exam.currentQuestion.IsReviewQuestion ? "(review)" : "";
            if (t.HasPreviousQuestions())
                Console.Write("<<--- [previous]    ");
            else
                Console.Write("                      ");
            Console.Write($"Question {questionNum} {reviewIndicator} out of {t.GetNumQuestionsSession()}: ");
            if (t.HasForwardQuestions())
                Console.Write("    [forward] --->>\n");
            else
                Console.Write("\n");

            // Questions for Review:
            Console.Write("                      ");
            Console.Write($"Questions for Review: [{t.exam.NumberQuestionsForReview()}]\n");

            // QUESTION
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            string trimmedQuestion = (t.exam.currentQuestion.processedQuestion).Replace(TestTaker.KEYWORD_SYMBOL, "");
            Console.WriteLine(trimmedQuestion);
            Console.WriteLine("[Press enter to continue]");

            // REVEAL ANSWER?
            Console.WriteLine();
            var key = Console.ReadKey(false).Key;

            if (key == ConsoleKey.Escape)
                return keyCommand.ESCAPE;


            if (key != ConsoleKey.LeftArrow && key != ConsoleKey.RightArrow)
            {
                Console.WriteLine();
                Console.WriteLine("[ANSWER]:");
                Console.WriteLine();
                string trimmedAnswer = (t.exam.currentQuestion.answer).Replace(TestTaker.KEYWORD_SYMBOL, "");
                Console.WriteLine(trimmedAnswer);
                Console.WriteLine();
                Console.WriteLine("Press [Backspace] to review later");
                Console.WriteLine("Press [Enter] to complete this question");
                Console.WriteLine();
                key = Console.ReadKey(false).Key;
            }

            // Add to review questions:
            if (key == ConsoleKey.Backspace)
            {
                t.exam.AddReviewQuestion(t.exam.currentQuestion);
            }
            if (key == ConsoleKey.Enter && !t.HasForwardQuestions())
            {
                t.exam.CompleteQuestion(t.exam.currentQuestion);
            }

            // Restart question:
            if (key == ConsoleKey.UpArrow)
                return (keyCommand.RESTART_QUESTION);

            // KEYBOARD LOGIC
            if (key == ConsoleKey.LeftArrow)
                return keyCommand.BACKWARD_ONE;

            else if (key == ConsoleKey.RightArrow)
                return keyCommand.FORWARD_ONE;

            else if (key == ConsoleKey.Escape)
                return keyCommand.ESCAPE;

            else
                return keyCommand.NEXT;
        }

        public static bool testComplete(TestTaker t)
        {
            Console.Clear();
            printTitle();
            Console.WriteLine("Thanks for playing!");
            string answer;
            do
            {
                Console.WriteLine();
                Console.WriteLine("Would you like to play again? (yes/no)");
                Console.Write("Your response: ");
                answer = Console.ReadLine();
            } while (!answer.ToLower().Contains('y') && !answer.ToLower().Contains('n'));
            if (answer.ToLower().Contains('y')) return true;
            else return false;
        }


        public static List<string> getFileNames()
        {
            List<string> fileNames = Directory
                .GetFiles(@"noteFiles\", "*.txt", SearchOption.AllDirectories)
                .Select(Path.GetFileName)
                .ToList();
            return fileNames;
        }

        public static void listFileNames(List<string> fileNames, int maxListings)
        {
            Console.WriteLine("Files loaded in noteFiles directory:");
            int it = 1;
            foreach (var name in fileNames)
            {
                Console.WriteLine($"\t{name}");
                it++;
                if (it > maxListings) break;
            }
            if (it > maxListings)
            {
                Console.WriteLine("...");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public static string askFileName()
        {
            Console.Clear();
            printTitle();
            List<string> fileNames = getFileNames();
            listFileNames(fileNames, 8);
            string userInput = null;
            do
            {
                Console.WriteLine("Please enter the name of the file you would like to use for input");
                Console.Write("File: ");
                userInput = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(userInput));
            return userInput;
        }

        public static int askHowManyQuestions(TestTaker t)
        {
            Console.Clear();
            printTitle();
            Console.WriteLine();
            Console.WriteLine($"These notes contain {t.exam.totalQuestions} questions.");
            Console.WriteLine();
            Console.WriteLine($"How many would you like to practice?");
            Console.WriteLine($"\t* Press [enter] for all");
            Console.WriteLine($"\t* Otherwise [enter a number] and press [enter]");
            Console.WriteLine();
            int num = t.exam.totalQuestions; // default
            string userInput = null;
            bool parseSuccess = true;
            do
            {
                Console.WriteLine();
                Console.Write("Your choice: ");
                userInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(userInput))
                    parseSuccess = int.TryParse(userInput, out num);
            } while (!parseSuccess || (num > t.exam.totalQuestions) || (num < 1));

            if (num <= 0 || string.IsNullOrWhiteSpace(userInput))
                num = t.exam.totalQuestions;

            t.SetNumQuestionsSession(num);

            return num;
        }

        public static void askType(TestTaker t)
        {
            bool goodKey;
            string key;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Which type of test will you take?");
            Console.WriteLine("\t[1] Keywords Partial (words indicated in txt file will be partially hidden)");
            Console.WriteLine("\t[2] Keywords Full Blank (words indicated in txt file will be fully hidden)");
            Console.WriteLine("\t[3] Keywords First Letters (only first few letters revealed, the rest hidden)");
            Console.WriteLine("\t[4] Full Random (all words in answer will be randomly hidden)");
            Console.WriteLine();
            do
            {
                Console.Write("Your choice: ");
                key = Console.ReadLine();
                goodKey = (key == "1" || key == "2" || key == "3" || key == "4");
                if (!goodKey)
                {
                    Console.WriteLine();
                    Console.WriteLine("Please provide your choice by entering 1, 2, 3, or 4 on your keyboard");
                }
            } while (!goodKey);
            int keyNum = int.Parse(key);
            t.setExamType((TestTaker.testType)keyNum);
            return;
        }


    } // end main class


}


