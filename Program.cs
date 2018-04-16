// Adam James Rushton - G20700507
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ass1CompleteIntroToProg
{
    class Program
    {
        /* Additional Program Features
         * Copy Files
         * Move Files
         * Write To Files
         * Delete Files
         * Good contrast colour themes
         * Executing files
         * Folder listing
         * Displaying file listing in pages
         * Removing the ability to change the size of the console (Done using the dll imports as commented further down)
         * Displaying one file size (Bytes, KB, MB, GB depending on the file size)
         * Neat, readable file listing throughout
         * Chopping end of names off and replacing with ... when they exceed a certain length
         * Create a new folder
         * Searching for file names    
         * Files accessed in specific month
         * More features may exist that are not listed...  
         * Sort by the create and modified date (currently only sorts for access date)
         */
        // ----------------------------------------------------------------------------
        // Remove the ability to change the size of the console
        // Researched from: 
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/1aa43c6c-71b9-42d4-aa00-60058a85f0eb/c-console-window-disable-resize?forum=csharpgeneral

        private const int MF_BYCMD = 0x00000000;
        public const int SC_MAX = 0xF030;
        public const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        // --------------------------------------------------------------------------------------------------------------
        // Enumerated Types
        enum sortBy // List of sort types
        {
            Default = 1,
            Alphabetical = 2,
            Size = 3,
            CreationDate = 4,
            LastAccessed = 5,
            LastModified = 6,
            Extension = 7
        };

        enum optionChoice // List of option choices
        {
            FullFileListing = 1,
            FilteredFileListing = 2,
            FolderStatistics = 3,
            CustomFileDirectory = 4,
            InputFileLength = 5,
            FilesAccessedInLastMonth = 6,
            SortingOrder = 7,
            ExecuteFiles = 8,
            ModifyFiles = 9,
            ChangeColours = 10,
            CreateFolder = 11,
            FolderListing = 12,
            SearchForFile = 13,
            FilesAccessedInSpecificMonth = 14,
            QuitProgram = 15
        };

        enum fileModifiers // List of file modifiers
        {
            Copy = 1,
            Move = 2,
            Delete = 3,
            WriteTo = 4
        };

        enum colourChoices // List of colour schemes
        {
                WhiteOnBlack = 1,
                YellowOnBlack = 2,
                GreenOnBlack = 3,
                CyanOnBlack = 4,
                DarkGreenOnBlack = 5,
                RedOnBlack = 6,
                WhiteOnBlue = 7,
                WhiteOnDarkBlue = 8,
                WhiteOnDarkGreen = 9,
                DefaultConsoleColours = 10
        };
        // ---------------------------------------------------
        static bool reverse = false;                                         // Reverse ordering of files state

        static string sortingMethod = "File Size";                           // Default sorting method
        static string startingDirectory = @"C:\Windows";                     // Starting directory
        static string newDirectory = null;                                   // New directory path
        static string startDir = null;                                       // Start directory for copy, move, delete, write to
        static string finalDir = null;                                       // Final directory for copy, move
        static string spacing = "\t\t\t";                                    // File spacing 

        static long byteInGB = 1024*1024*1024;                               // Bytes in GB = 1024^3
        static long byteInMB = 1024*1024;                                    // Bytes in MB 1024^2
        static long byteInKB = 1024*1;                                       // Bytes in KB 1024 * 1

        static int posInList = 1;                                            // Starting Position
        static int consoleWindowHeight = 40;                                 // Set console height 
        static int consoleWindowWidth = 88;                                  // Set console width
        static int bufferSize = 3000;                                        // How many lines are shown by the console
        static int modifyFileNumber = 0;                                     // Number to modify

        public static void Main(string[] args)
        {
            // ----------------------------------------------------------------------------
            // Remove the ability to change the size of the console
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_MAX, MF_BYCMD);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCMD);
            }
            // ----------------------------------------------------------------------------
            bool validInput = false;                                        // Checks to see if the program can convert String to Enum                                        
            string optionInput = null;                                      // Receives the user input
            const int optionAmount = 15;                                    // Maximum number of options
            string consoleTitle = "Assignment 1 - Adam Rushton";            // Store Console title text

            string[] optionList = new string[optionAmount]                  // Options on the main menu
            {
                "Full File Listing",
                "Filtered File Listing",
                "Folder Statistics",
                "Custom File Directory",
                "Input File Length",
                "Files accessed within the last month",
                "Sorting order",
                "Execute Files",
                "Copy/Move/Delete/WriteTo Files",
                "Colour Scheme",
                "Create a folder",
                "View Folder Listing",
                "Search for a file",
                "Files accessed in a specific month",
                "Exit"
            };

            Console.Title = consoleTitle;                                   // Set the console title
            Console.SetBufferSize(consoleWindowWidth, bufferSize);          // Setting the buffer size (number of lines displayed)
            Console.SetWindowSize(consoleWindowWidth, consoleWindowHeight); // Set the Console dimensions from variables stored at the top
            Console.BackgroundColor = ConsoleColor.DarkBlue;                // Default Background colour on the Console
            Console.ForegroundColor = ConsoleColor.White;                   // Default Text colour on the Console
            DateTime currentDate = DateTime.Now;                            // Getting the current time
            Console.Clear();                                                // Clear so the background goes over the entire console, not just the text            
                              
            do {
                ContentSeperator();                                                                  
                Console.WriteLine("{0}Introduction to Programming - Assignment 1", spacing);          
                Console.WriteLine("{0}Programmed by Adam Rushton", spacing);                          
                Console.WriteLine("{0}Last refreshed: {1} ", spacing, DateTime.Now.ToString());      
                ContentSeperator();

                FileInformation();                                          // Display working directory and order sorting
                WelcomeMessage();                                           // Display welcome message
                for (int option = 0; option < optionAmount; option++)       // Display all the options on the screen, with a position in list at the start of each option
                {
                    Console.WriteLine("{0:00}. {1}", posInList + option, optionList[option]);
                }
                Console.WriteLine();

                Console.Write("Please select an option ({0} - {1}): ", posInList, optionAmount); // Choose an option               
                optionInput = Console.ReadLine();                                                // Read the input

                DirectoryInfo folderInfo = new DirectoryInfo(startingDirectory);                 // Load the folder
                FileInfo[] files = folderInfo.GetFiles();                                        // Declare file directory and put the contents into an array 
                optionChoice options;                                                            // Using enum list optionChoice
                validInput = Enum.TryParse(optionInput, out options);                            // Try to convert the string into local variable

                switch (options) // Switch user input 
                {
                    case optionChoice.FullFileListing:
                        TitleScreen();                                  // Load title screen
                        FileInformation();
                        FullFileListing(files);                         // Load the full file listing function
                        break;
                    case optionChoice.FilteredFileListing:
                        TitleScreen();
                        FileInformation();
                        FilteredFileListing(folderInfo, files);         // Load the filtered file listing function
                        break;
                    case optionChoice.FolderStatistics:
                        TitleScreen();
                        FileInformation();
                        FolderStatistics(files);                        // Load the folder statistics function
                        break;
                    case optionChoice.CustomFileDirectory:
                        TitleScreen();
                        FileInformation();
                        CustomDirectory(files);                         // Load the custom directory function
                        break;
                    case optionChoice.InputFileLength:
                        TitleScreen();
                        FileInformation();
                        FileLength(files);                              // Load the file size filter (between 1MB) function
                        break;
                    case optionChoice.FilesAccessedInLastMonth:
                        TitleScreen();
                        FileInformation();
                        FilesAccessedInLastMonth(files, currentDate);   // Load files accessed in last month function
                        break;
                    case optionChoice.SortingOrder:
                        TitleScreen();
                        FileInformation();
                        FileOrderSorting(files);                        // Load file order sorting function
                        break;
                    case optionChoice.ExecuteFiles:
                        TitleScreen();
                        FileInformation();
                        ExecuteFiles(files);                            // Load execute files function
                        break;
                    case optionChoice.ModifyFiles:
                        TitleScreen();
                        FileInformation();
                        ModifyingFiles(files);                          // Load the modifying files function
                        break;
                    case optionChoice.ChangeColours:
                        TitleScreen();
                        FileInformation();
                        ColourThemes();                                 // Load colour themes method
                        break;
                    case optionChoice.CreateFolder:
                        TitleScreen();
                        FileInformation();
                        CreateFolder(folderInfo);                       // Load create folder method
                        break;
                    case optionChoice.FolderListing:
                        TitleScreen();
                        FileInformation();
                        FolderListing(files);                           // Load folder listing
                        break;
                    case optionChoice.SearchForFile:
                        TitleScreen();
                        FileInformation();                               
                        FileSearch(files);                              // Load file searching
                        break;
                    case optionChoice.FilesAccessedInSpecificMonth:     // Load files accessed in specific month
                        TitleScreen();
                        FileInformation();
                        FilesAccessedInSpecificMonth(files, currentDate);                        
                        break;
                    case optionChoice.QuitProgram:
                        QuitProgramOption();                            // Load quit program method
                        break;
                    default:                                            // If user enters invalid input, display error message
                        ErrorMessage();
                        break;
                }
                validInput = false;                                     // Reset input for next time
            } while (!validInput);                                      // Repeating until the user has not entered an invalid input.
        }

        static void FullFileListing(FileInfo[] files) // Displaying all the files
        {   // Function data types
            bool isNumber = false;  // Used to decide if a valid integer number has been entered
            bool isInvalid = true;  // Used to decide if the number entered is in range and is exactly a numbee
            const int minNumber = 0;// Always starts at 0
            int fileNumber = 0;     // Used to decide what file details are going to be shown
            int ln = 1;             // Line number next to each file
            int filesPerPage = 38;  // Number of files per page
            int pageNumber = 1;     // Current page number

            SortMe(files);                                                  // Get the sorting method                     
            FileListingHeader();                                            // Display file listing header
            NoFilterListing(files, filesPerPage, pageNumber, ln);           // Used to display all files in pages

            do // Ask user to input a number until its valid.
            {
                Console.Write("Please enter a file number to see more details (0 to skip): ");
                isNumber = int.TryParse(Console.ReadLine(), out fileNumber);                   // Try parse user input to a integer
                isInvalid = fileNumber < minNumber || fileNumber > files.Length || !isNumber;  // If any of the conditions are broken, it is an invalid input.
                YesOrNo(isInvalid);                                                            // Display error message if invalid input
            } while (isInvalid);                                                               // Repeat while not in range and while not an integer
            
            for (int i = 0; i < files.Length; i++)                                       // Display all the files on the screen (from the given directory)
            {
                if (fileNumber == posInList + i)                                         // Display specific file information
                {
                    Console.WriteLine("File:          {0}", files[i]);                   // File name neatly displayed
                    Console.WriteLine("File path:     {0}", files[i].FullName);          // File path neatly displayed
                    Console.WriteLine("File size:     {0}", files[i].Length + " bytes"); // File size neatly displayed
                    Console.WriteLine("Created:       {0}", files[i].CreationTime);      // Created date neatly displayed
                    Console.WriteLine("Last accessed: {0}", files[i].LastAccessTime);    // Last accessed date neatly displayed
                    Console.WriteLine("Last modified: {0}", files[i].LastWriteTime);     // Last modified time
                }
            }
            ReturnToMenu(); // Press any key to continue
        }

        static void FilteredFileListing(DirectoryInfo folderInfo, FileInfo[] files)
        {   // Function data types
            string userInput = null; // Stores the user input
            int ln = 1;              // Stores file line number
            int pageNumber = 1;      // Store page number
            int filesPerPage = 38;   // Files per page
            bool isInvalid = true;   // Checks to see if the user input is invalid
            ContentSeperator();      // Neat ─ across

            do                                                                                                                  
            {
                Console.Write("Please enter a file filter, such as: *.exe (leave blank to show all files): ");
                userInput = Console.ReadLine().ToLower();                                                                       // Store userinput in lowercase
                isInvalid = !userInput.StartsWith("*.") && !userInput.EndsWith("*.*") && !String.IsNullOrWhiteSpace(userInput); // Determine if user input is invalid
                if (userInput.StartsWith("*.") || userInput.EndsWith("*.*"))                                                    // See if user input starts with shown  
                {
                    Console.WriteLine("File filter: {0}", userInput);                                                           // Show the user what they entered as a file filter
                    files = folderInfo.GetFiles(userInput);                                                                     // Display all of the files that meet that filter
                }
                else if (String.IsNullOrWhiteSpace(userInput))                                                                  // If the user just presses any key
                {
                    Console.WriteLine("Displaying all files (no filter entered): ");
                }
                else
                {
                    YesOrNo(isInvalid);                                                                                         // Display error message if invalid input
                }
            } while (isInvalid);

            SortMe(files);                                  // Get the sorting method
            FileListingHeader();                            // Display File Header

            NoFilterListing(files, filesPerPage, pageNumber, ln);           // Used to display all files in pages

            ReturnToMenu();                                 // Press any key to continue
        }

        static void FolderStatistics(FileInfo[] files)     // Displaying the folder statistics of the given directory  
        {   // Function data types
            long totalSize = 0;         // Total size of all the files
            long largestFileSize = 0;   // Stores the largest file size
            long averageFileSize = 0;   // Stores the average file size
            string largestFileName = null;  // Stores the largest file name
            ContentSeperator();             // Neat ─ across
            Console.WriteLine("Files in:                {0}", files[0].DirectoryName); 
            Console.WriteLine("Total files:             {0}", files.Length);

            for (int i = 0; i < files.Length; i++)      // For all the files in this directory
            {
                totalSize += files[i].Length;           // Add up all of their file sizes
                if (files[i].Length > largestFileSize)  // Compare file sizes
                {
                    largestFileSize = files[i].Length;  // Compares each file size in list and displays largest
                    largestFileName = files[i].Name;    // Retrieve this files name
                }
            }

            averageFileSize = totalSize / files.Length;                                                 // Work out the average file size
            averageFileSize /= byteInKB;                                                                // Convert to KB and round it to nearest whole number
            largestFileSize /= byteInMB;                                                                // Convert to MB and round it to nearest whole number
            totalSize /= byteInMB;                                                                      // Convert to MB and round it to nearest whole number
            Console.WriteLine("Total size of all files: {0} MB", totalSize);                            // Display total file size
            Console.WriteLine("Largest file:            {0} {1} MB", largestFileName, largestFileSize); // Display largest file name and its size
            Console.WriteLine("Average file size:       {0} KB", averageFileSize);                      // Display average file size
            ReturnToMenu();                                                                             // Press any key to continue
        }

        static string CustomDirectory(FileInfo[] files) // Changing the current file directory
        {   // Function data types
            bool validDir = true; // Determines if new directory is valid
            ContentSeperator();   // Neat ─ across

            do                                                                            // Repeat the code below until a valid directory is given.
            {
                Console.Write(@"Please enter a Windows directory (i.e, C:\Windows): ");
                startingDirectory = Console.ReadLine();                                   // Read the starting directory
                validDir = Directory.Exists(startingDirectory);                           // Check if it exists
                ValidDirectory(validDir);                                                 // Write error message if user enters an invalid directory  
            } while (!validDir);                                                          // Repeat until a valid directory is entered

            Console.WriteLine("New directory: {0} ", startingDirectory);                  // Display new directory once complete
            ReturnToMenu();                                                               // Press any key to continue
            return startingDirectory;                                                     // Update the programs file and folder listing directory
        }

        static void FileLength(FileInfo[] files) // Filter file listing based on their file size                        
        {   // Function data types
            bool isInvalid = true;    // Determines if a valid input has been entered
            bool isNumber = false;    // Determines if a valid number has been entered
            long fileSize = 0;        // Stores the file size 
            long minFileSize = 0;     // Stores the number 1MB smaller than the file size entered
            long maxFileSize = 0;     // Stores the number 1MB larger than the file size entered
            int ln = 1;               // Line number for each file
            ContentSeperator();       // Neat ─ across

            do                                                                                        // Repeat the code below while input is invalid
            {
                Console.Write("Please enter a file size(MB) to filter for: "); 
                isNumber = long.TryParse(Console.ReadLine(), out fileSize);                         // Try and convert user input to a long
                isInvalid = !isNumber || fileSize < posInList || fileSize > long.MaxValue;          // Check to see if user entered an invalid input
                YesOrNo(isInvalid);                                                                   // Print error message if user enters invalid input
            } while (isInvalid);

            fileSize *= byteInMB;              // Convert MB to bytes
            minFileSize = fileSize - byteInMB; // 1MB smaller than the inputted file size
            maxFileSize = fileSize + byteInMB; // 1MB bigger than the inputted file size
            Console.WriteLine("Filtering file size between (1MB lower and 1MB higher)");
            SortMe(files);                     // Get the sorting method  
            FileListingHeader();               // Write file listing header

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Length >= minFileSize && files[i].Length <= maxFileSize) // If the file lengths are within 1MB of user input, display them on screen
                {
                    WriteFileList(files, i, ln); // Write these files on the console neatly
                    ln++;             // Assign line number to each file
                }
            }

            ReturnToMenu();                      // Press any key to continue
        }

        static void FilesAccessedInLastMonth(FileInfo[] files, DateTime currentDate) // Display files accessed within the last month
        {   // Function data types
            const int oneMonthOld = -1;
            int ln = 1;
            DateTime lastMonthDate = DateTime.Now.AddMonths(oneMonthOld); // Set last months date
            SortMe(files);                                                // Get the sorting method
            ContentSeperator();     
            FileListingHeader();                                          // Display File Header

            for (int i = 0; i < files.Length; i++)                        // For all of the files in the given directory
            {
                if (files[i].LastAccessTime >= lastMonthDate)             // If the files were accessed in the last month
                {
                    WriteFileList(files, i, ln);                          // Display them on the console
                    ln++;                                                 // Assign line number to each file
                }
            }
            ReturnToMenu();                                               // Press any key to continue
        }

        static string FileOrderSorting(FileInfo[] files) // Change the sorting order
        {   // Function data types
            bool validInput = false;    // Check for valid conversion
            string optionInput = null;  // Stores user input
            const int optionAmount = 7; // Number of options
            string[] optionList = new string[optionAmount] 
            {
                "Default order",
                "Alphabetical order",
                "File size order",
                "Creation date order",        
                "Accessed date order",
                "Modified date order",
                "Extension order" }; // Option names
            ContentSeperator();         // Neat ─ across

            do                                                                                 // Repeat the code below until user inputs a valid sorting order
            {
                for (int option = 0; option < optionAmount; option++)                          // For all of the options
                {
                    Console.WriteLine("{0:00}. {1}", posInList + option, optionList[option]);     // Display them on the console
                }
                Console.WriteLine("Note: You can reverse the chosen list order when loading a file listing.");
                Console.Write("Choose which type of ordering would you like to use? ({0}-{1}): ", posInList, optionAmount);
                optionInput = Console.ReadLine();                                             // Read user input
                sortBy sortMethod;                                                            // Using enum list optionChoice
                validInput = Enum.TryParse(optionInput, out sortMethod);                      // Try and convert user input
                switch (sortMethod) // Change the sorting method based on what the user inputted
                {
                    case sortBy.Default:
                        sortingMethod = "Default";       // Default                  
                        break;
                    case sortBy.Alphabetical:
                        sortingMethod = "Alphabetical";  // Alphabetical Order
                        break;
                    case sortBy.Size:
                        sortingMethod = "File Size";     // File Size Order
                        break;
                    case sortBy.CreationDate:
                        sortingMethod = "Creation Date"; // Creation date order
                        break;
                    case sortBy.LastAccessed:
                        sortingMethod = "Last Accessed"; // Last Accessed Order
                        break;
                    case sortBy.LastModified:
                        sortingMethod = "Last Modified"; // Last Modified order
                        break;
                    case sortBy.Extension:
                        sortingMethod = "Extension";     // Extension order
                        break;
                    default:                             // If not one of the options above were entered
                        ErrorMessage();                  // Display error message
                        validInput = false;              // Not a valid input
                        break;
                }
            } while (!validInput);                      // Repeat until valid input
            ReturnToMenu();                             // Press any key to continue
            return sortingMethod;                       // Update the programs sorting method
        }

        static void ExecuteFiles(FileInfo[] files) // Execute a file from a list
        {   // Function data types
            string userInput = null;                // Store user input
            int executeFileNumber = 0;              // Store file number chosen
            int ln = 1;                             // Line number for each file
            int pageNumber = 1;                     // Start page number
            int filesPerPage = 38;                  // Files per page
            bool isValidNumber = false;             // Checks if valid file number entered
            bool isInvalid = true;                  // Checks if invalid input
            SortMe(files);                          // Get the sorting method
            ContentSeperator();     
            FileListingHeader();                    // Display the File Header

            NoFilterListing(files, filesPerPage, pageNumber, ln);           // Used to display all files in pages

            do                                      // Repeat the code below until the user says y or n
            {
                Console.Write("Would you like to execute a file? (y/n): ");
                userInput = Console.ReadLine().ToLower();         // Convert user input to lower case
                isInvalid = userInput != "y" && userInput != "n"; // Check if ivnalid input
                YesOrNo(isInvalid);                               // Display error message if user enters an invalid input
            } while (isInvalid);                                  // Repeat until a valid input is entered

            if (userInput == "y")
            {
                do // Keep asking for user input until it is within the range and a number
                {
                    Console.Write("Choose a file to execute ({0}-{1}): ", posInList, files.Length);
                    isValidNumber = int.TryParse(Console.ReadLine(), out executeFileNumber);                                   // Try and validate user input
                    isInvalid = executeFileNumber < posInList || executeFileNumber > files.Length || !isValidNumber;           // Decide if user has entered an invalid input
                    executeFileNumber--;                                                                                       // Store the correct file number
                    YesOrNo(isInvalid);                                                                                        // Display error message if user enters an invalid input
                } while (isInvalid);                                                                                           // Repeat until valid input

                Process.Start(files[executeFileNumber].Name);                                                                  // Open the file.
                Console.WriteLine("File {0}. {1} is now open.", executeFileNumber + posInList, files[executeFileNumber].Name); // Tell user action has happened
            }
            ReturnToMenu();                                                                                                    // Press any key to continue
        }

        static void ModifyingFiles(FileInfo[] files) // Allow user to modify files (Copy, Move, Delete and Write To)
        {   // Function data types
            const int optionAmount = 4; // Number of options
            int ln = 1;                 // Line number for each file
            int filesPerPage = 38;      // Files per apge
            int pageNumber = 1;         // Page number
            bool isValidNumber = false; // Valid file number entered
            string userInput = null;    // Store user input
            bool isInvalid = true;      // Check if file number exists 
            bool validInput = false;    // Check if valid string to enum conversion 

            string[] optionList = new string[optionAmount]
            {
                "Copy",
                "Move",
                "Delete",
                "Write to"
            }; // List of options

            fileModifiers modify; // Switch user input

            SortMe(files);       // Get the sorting method
            ContentSeperator();  // # across the screen
            FileListingHeader(); // Display File Header

            NoFilterListing(files, filesPerPage, pageNumber, ln);  // Used to display all files in pages

            ContentSeperator();  // Neat ─ across

            do
            {         
                Console.Write("Input a file number from ({0}-{1}): ", posInList, files.Length);
                isValidNumber = int.TryParse(Console.ReadLine(), out modifyFileNumber);                        // Try and validate user input
                isInvalid = modifyFileNumber < posInList || modifyFileNumber > files.Length || !isValidNumber; // Decide if user has entered a valid input
                modifyFileNumber--;                                                                            // Get the correct file number
                YesOrNo(isInvalid);                                                                            // Display error message if the user enters an invalid input
            } while (isInvalid);                                                                               // Repeat until valid file number entered

            do // Repeat below until a valid option is selected
            {
                for (int option = 0; option < optionAmount; option++)                      // For all of the options
                {
                    Console.WriteLine("{0:00}. {1}", posInList + option, optionList[option]); // Display them on the screen
                    ln++;                                                                     // Assign a line number to each one
                }

                Console.Write("Please choose an option from ({0}-{1}): ", posInList, optionAmount);
                userInput = Console.ReadLine();                    // Read user input
                validInput = Enum.TryParse(userInput, out modify); // Try and convert user input to a local variable

                switch (modify) // Find relevant action based on what the user has chosen they want to do
                {
                    case fileModifiers.Copy:    // Copy File  
                        CopyFile(files);        // Load copy file function
                        break;
                    case fileModifiers.Move:    // Move File
                        MoveFile(files);        // Load move file function
                        break;
                    case fileModifiers.Delete:  // Delete File
                        DeleteFile(files);      // Load delete file function
                        break;
                    case fileModifiers.WriteTo: // Write To Text File
                        WriteToTextFile(files); // Write to text files
                        break;
                    default: // Display error message if it was an invalid input
                        ErrorMessage();
                        validInput = false;
                        break;
                }
            } while (!validInput); // Repeat until valid input
            ReturnToMenu(); // Press any key to continue
        }

        static void ColourThemes()
        {
            bool validInput = false;      // String to enum conversion
            const int colourSchemes = 10; // Number of options
            int ln = 1;                   // Line number for each file
            string userInput = null;      // Store user input

            string[] optionList = new string[colourSchemes]
            {
                "White on Black",
                "Yellow on Black",
                "Green on Black",
                "Cyan on Black",
                "Dark Green on Black",
                "Red on Black",
                "White on Blue",
                "White on Dark Blue",
                "White on Dark Green",
                "Default Console Colours"
            }; // List of options

            do
            {
                for (int i = 0; i < colourSchemes; i++)                                    // For all of the options
                {
                    Console.WriteLine("{0:00}. {1}", posInList + i, optionList[i]);           // Display them on the screen
                    ln++;                                                                     // Assign a line number to each one
                }

                Console.Write("Please choose an option from {0} to {1}: ", posInList, colourSchemes);
                userInput = Console.ReadLine(); // Read user input
                colourChoices colourScheme; // Using enum list optionChoice
                validInput = Enum.TryParse(userInput, out colourScheme); // Try and convert user input to local variable

                switch (colourScheme) // Find relevant action based on what the user has chosen they want to do
                {
                    case colourChoices.WhiteOnBlack:
                        Console.BackgroundColor = ConsoleColor.Black;     // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.White;     // Text colour on the Console
                        break;
                    case colourChoices.YellowOnBlack:
                        Console.BackgroundColor = ConsoleColor.Black;     // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.Yellow;    // Text colour on the Console
                        break;
                    case colourChoices.GreenOnBlack:
                        Console.BackgroundColor = ConsoleColor.Black;     // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.Green;     // Text colour on the Console
                        break;
                    case colourChoices.CyanOnBlack:
                        Console.BackgroundColor = ConsoleColor.Black;     // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.Cyan;      // Text colour on the Console
                        break;
                    case colourChoices.DarkGreenOnBlack:
                        Console.BackgroundColor = ConsoleColor.Black;     // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.DarkGreen; // Text colour on the Console
                        break;
                    case colourChoices.RedOnBlack:
                        Console.BackgroundColor = ConsoleColor.Black;     // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.Red;       // Text colour on the Console
                        break;
                    case colourChoices.WhiteOnBlue:
                        Console.BackgroundColor = ConsoleColor.Blue;      // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.White;     // Text colour on the Console
                        break;
                    case colourChoices.WhiteOnDarkBlue:
                        Console.BackgroundColor = ConsoleColor.DarkBlue;  // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.White;     // Text colour on the Console
                        break;
                    case colourChoices.WhiteOnDarkGreen:
                        Console.BackgroundColor = ConsoleColor.DarkGreen; // Background colour on the Console
                        Console.ForegroundColor = ConsoleColor.White;     // Text colour on the Console
                        break;
                    case colourChoices.DefaultConsoleColours:
                        Console.ResetColor();                             // Default console colour scheme
                        break;
                    default:
                        ErrorMessage();                                   // Display error message
                        validInput = false;                               // Invalid if not one of the above options
                        break;
                }
            } while (!validInput);
            ReturnToMenu();  // Press any key to continue
        }
        static void CreateFolder(DirectoryInfo folderInfo)
        {
            string folderName; // Store folder name
            Console.Write("Please enter the name you want the folder to be called: ");
            folderName = Console.ReadLine();

            try
            {
                startDir = Path.Combine(startingDirectory, folderName); // Try to create a starting directory path 
                if (Directory.Exists(startDir))
                {
                    FolderErrorMessage();
                }
                else
                {
                    folderInfo.CreateSubdirectory(folderName);
                    Console.WriteLine("You have successfully created the folder.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                FileModificationErrorMessage(); // Unauthorised access
            }
            catch (ArgumentException)
            {
                InvalidFileNameErrorMessage(); // Invalid folder name input
            }
            catch (IOException)
            {
                ErrorMessage(); // Catches names too long or no name at all
            }
        }

        static void FolderListing(FileInfo[] files)
        {
            int folderLN = 1;                                               // Folder line number
            string[] folders = Directory.GetDirectories(startingDirectory); // Store the folders
            ContentSeperator();                                             // Neaten the display
            FolderListingHeader();                                          // Display Folder Header

            for (int i = 0; i < folders.Length; i++)                        // For all the folders in the directory
            {
                WriteFolderList(folders, i, folderLN);                      // Display them
                folderLN++;                                                 // Increment folderLn by 1
            }

            ContentSeperator();                                             // Neatly seperate after all listed   
        }

        static void FileSearch(FileInfo[] files)
        {
            string fileName;
            int ln = 1;
            Console.Write("Please enter a file name to search for: ");
            fileName = Console.ReadLine();
            Console.WriteLine("Searching for files...");
            FileListingHeader();

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.Contains(fileName))
                {
                    WriteFileList(files, i, ln);
                    ln++;
                }
            }

            Console.WriteLine("Search finished");
            ReturnToMenu(); // Press any key to return to main menu
        }

        static void FilesAccessedInSpecificMonth(FileInfo[] files, DateTime currentDate)
        {
            bool validYear = false;  // Validate year 
            bool validMonth = false; // Validate month
            bool inRange;            // Validate in range
            int numOfMonths = 12;    // Number of months in a year
            int minYear = 1950;      // Min year to search for
            int yearToSearch;        // Store year to search
            int monthToSearch;       // Store month to search
            int ln = 1;              // Store line number

            do
            {
                Console.Write("Please input a Year to search in between ({0}-{1}): ", minYear, currentDate.Year);
                validYear = int.TryParse(Console.ReadLine(), out yearToSearch);
                inRange = yearToSearch >= minYear && yearToSearch <= currentDate.Year;
                if (!validYear || !inRange)
                {
                    ErrorMessage();
                }
            } while (!validYear && !inRange);
            inRange = false; // Put back to false so can use again below

            do
            {
                Console.Write("Please input a Month number to search in between ({0}-{1}): ", posInList, numOfMonths);
                validMonth = int.TryParse(Console.ReadLine(), out monthToSearch);
                inRange = monthToSearch >= 1 && monthToSearch <= numOfMonths;
                if (!validMonth || !inRange)
                {
                    ErrorMessage();
                }
            } while (!validMonth && !inRange);

            FileListingHeader();

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].LastAccessTime.Year == yearToSearch && files[i].LastAccessTime.Month == monthToSearch)
                {
                    WriteFileList(files, i, ln);
                    ln++;
                }
            }
            ReturnToMenu();
        }
        static void SortMe(FileInfo[] files) // Sorting function
        {   // Function data types
            bool isInvalid = true;   // Check if y or n entered
            string userInput = null; // Store user input

            do
            {
                Console.Write("Would you like to reverse the {0} order? (y/n): ", sortingMethod); // Ask if they would like to reverse order their current sorting method
                userInput = Console.ReadLine().ToLower(); // Save user input in lowercase
                reverse = userInput == "y"; // Change state of reverse based on the user input
                isInvalid = !reverse && userInput != "n"; // Decide if the user has entered an invalid input
                YesOrNo(isInvalid); // If an invalid input has been entered display error message
            } while (isInvalid); // Repeat until valid input entered

            Console.WriteLine("Reverse = {0}", reverse); // Tell the user that the program is now reverse ordering (or not) for the current sorting method
            ContentSeperator();

            switch (sortingMethod)
            {
                case "Default":       // Default order
                    break;   
                case "Alphabetical":  // Alphabetical order
                    Array.Sort(files, (a, b) => String.Compare(a.Name, b.Name));
                    break;
                case "File Size":     // Size order
                    Array.Sort(files, (a, b) => a.Length.CompareTo(b.Length));
                    break;
                case "Creation Date": // Creation date order
                    Array.Sort(files, (a, b) => DateTime.Compare(a.CreationTime, b.CreationTime));
                    break;
                case "Last Accessed": // Last accessed order                    
                    Array.Sort(files, (a, b) => DateTime.Compare(a.LastAccessTime, b.LastAccessTime));
                    break;
                case "Last Modified": // Last modified order
                    Array.Sort(files, (a, b) => DateTime.Compare(a.LastWriteTime, b.LastWriteTime));
                    break;
                case "Extension":     // Extension order
                    Array.Sort(files, (a, b) => String.Compare(a.Extension, b.Extension));
                    break;
            } 
            CheckReverse(files);     // Check to see if the user has wanted to reverse the file listing
        }

        static void CheckReverse(FileInfo[] files) // Decide if the program is going to reverse order the current sorting method
        {
            if (reverse) // If they want to reverse files
            {
                Array.Reverse(files); // Reverse the order of the current sorting method if in reverse
            }
        }

        static void CopyFile(FileInfo[] files) // Allow the user to copy files, providing they have permission
        {   // Function data types
            bool validDir = false; // Check if file directory exists

            do // Repeat below until a valid directory has been entered
            {
                Console.Write("Input a directory to copy the file to: ");
                newDirectory = Console.ReadLine().ToLower(); // Convert new directory to lower case
                validDir = Directory.Exists(newDirectory);   // Check if the directory exists
                ValidDirectory(validDir);                    // Print message if invalid directory
            } while (!validDir);

            try
            {
                startDir = Path.Combine(startingDirectory, files[modifyFileNumber].Name); // Try to create a starting directory path 
                finalDir = Path.Combine(newDirectory, files[modifyFileNumber].Name);      // Try to combine the destination directory path
                Directory.CreateDirectory(newDirectory);                                  // Create this directory if it exists
                File.Copy(startDir, finalDir);                                            // Copy the file if no errors occured during the process
                Console.WriteLine("{0} has been copied to {1}", startDir, finalDir);      // Tell the user that the copy action worked successfully
            }
            catch (UnauthorizedAccessException)                                           // If unauthorized access occurs (repeated below)
            {
                FileModificationErrorMessage();                                           // Tell them they do not have the relevant permissions (repeated below)
            } 
            catch (IOException)                                                           // If they enter an invalid input (repeated below)
            {
                ErrorMessage();                                                           // Tell the user invalid input (repeated below)
            }
            ReturnToMenu();                                                               // Press any key to continue
        }

        static void MoveFile(FileInfo[] files) // Allow the user to move files, providing they have permission
        {   // Function data type
            bool validDir = true;

            do // Repeat below until a valid move directory has been assigned
            {
                Console.Write("Input a directory to move file number {0} to: ", modifyFileNumber);
                newDirectory = Console.ReadLine().ToLower();
                validDir = Directory.Exists(newDirectory);
                ValidDirectory(validDir);
            } while (!validDir);

            try
            {
                startDir = Path.Combine(startingDirectory, files[modifyFileNumber].Name);
                finalDir = Path.Combine(newDirectory, files[modifyFileNumber].Name);
                Directory.CreateDirectory(newDirectory);
                File.Move(startDir, finalDir);                                            // Try and move the file
                Console.WriteLine("The file was moved successfully.");                    // If it worked then tell the user
            }
            catch (UnauthorizedAccessException)                                           // Catch any exceptions
            {
                FileModificationErrorMessage();
            }
            catch (IOException)
            {
                ErrorMessage();
            }
            ReturnToMenu();                                                                  // Press any key to continue
        }

        static void DeleteFile(FileInfo[] files) // Allow the user to delete files, providing they have permission
        {
            try
            {
                startDir = Path.Combine(startingDirectory, files[modifyFileNumber].Name); // Try bind the path and file together
                File.Delete(startDir);                                                    // Try and delete the file   
                Console.WriteLine("The file was deleted successfully.");                  // If file deleted, tell them
            }
            catch (UnauthorizedAccessException)                                           // Catch any exceptions
            {
                FileModificationErrorMessage();                                     
            }
            catch (IOException)
            {
                ErrorMessage();
            }
            ReturnToMenu(); // Press any key to continue
        }

        static void WriteToTextFile(FileInfo[] files) // Allow the user to write to a file, providing they have permission
        {   // Function data type
            string writeText = null;
            Console.WriteLine("Type a sentence to add to the file: ");
            writeText = Console.ReadLine();

            try
            {
                startDir = Path.Combine(startingDirectory, files[modifyFileNumber].Name); // Try bind the path and file together
                File.WriteAllText(startDir, "\n" + writeText + "\n");                     // Try write to the file
            }
            catch (UnauthorizedAccessException)                                           // Catch any exceptions
            {
                FileModificationErrorMessage();
            }
            catch (IOException)
            {
                ErrorMessage();
            }
            ReturnToMenu(); // Press any key to continue
        }

        static string ChopEndOfName(string fileName) // Remove end of name function if it is too long
        {   // Function data type
            const int maxLength = 27;                            // Maximum number of characters for a file name
            if (fileName.Length > maxLength)                     // Check to see if the file names are too long
            {
                return fileName.Substring(0, maxLength) + "..."; // Shortern the file name, adding ... to the end
            }
            else
            {
                return fileName;                                 // Don't change the file name
            }
        }

        static void FolderListingHeader() // Folder Header listing method
        {                                 // Folder columns neatly seperated
            Console.WriteLine("{0} {1,10} {2,51}", "####", "Folder Name", "Folders inside");                  
        }

        static void FileListingHeader()   // File Header Listing method
        {                                 // Column names, neatly seperated
            Console.WriteLine("{0} {1,10} {2,51} {3,10}", "####", "File Name", "File Size", "Last accessed"); 
        }

        static void NoFilterListing(FileInfo[] files, int filesPerPage, int pageNumber, int ln) // Used in full file listing, execute file listing, and when showing files on the modify file listings
        {
            for (int i = 0; i < files.Length; i++)                          // Display all the files on the console
            {
                if (i < filesPerPage)                                       // For the first page of files
                {
                    WriteFileList(files, i, ln);                            // Display them
                    ln++;                                                  // Assign a line number to each one
                }
                if (i == filesPerPage)                                      // The first n files have been listed. Page over.
                {
                    Console.WriteLine("Page: {0}. Press any key for next page...", pageNumber);
                    Console.ReadKey();                                      // Wait for key press and then proceed below
                    filesPerPage += 38;                                     // Show the next 38 files
                    pageNumber += 1;                                        // Next page
                    FileListingHeader();                                    // Shoe file listing header each page
                }
                else if (i == files.Length - posInList)                       // Check if at the end of the files
                {
                    Console.WriteLine("You have reached the end of the file listing.");
                    ContentSeperator();
                }
            }
        }
        static void WriteFileList(FileInfo[] files, int i, int ln) // Writing the files on the screen
        {   // Function data types
            long displayFileSize = 0;                                    // The file size that is going to be displayed                               
            string writeFileSize = null;                                     // Text next to the file size
            string myFileName = ChopEndOfName(files[i].Name);                // Displays the file name (shortened if too long)
            // Check to see how large each file size.
            if (files[i].Length < byteInKB)                                  // File size less than a KB?
            {
                displayFileSize = files[i].Length;                           // File size in bytes is the same
                writeFileSize = " Bytes";                                    // Display the size as bytes
            }
            else if (files[i].Length < byteInMB)                             // File size less than a MB?
            {
                displayFileSize = files[i].Length / byteInKB;                // Work out file size to display in KB
                writeFileSize = " KB";                                       // Display the size as bytes
            }
            else if (files[i].Length < byteInGB)                             // File size less than a GB?
            {
                displayFileSize = files[i].Length / byteInMB;                // Work out file size to display in MB
                writeFileSize = " MB";                                       // Display the size as bytes
            }
            else                                                             // For all file sizes GB or more
            {
                displayFileSize = files[i].Length / byteInGB;                // Work out file size to display in GB
                writeFileSize = " GB";                                       // Display the size as bytes
            }
            // Display each file neatly
            Console.WriteLine("{0:0000}. {1,-30} {2,30} {3,5}", ln, myFileName, displayFileSize + writeFileSize, files[i].LastAccessTime); 
        }

        static void WriteFolderList(string[] folders, int i, int folderLN) // Display all folders 
        {   // Folder Line Numbers, Folder Names & number of sub directories inside
            Console.WriteLine("{0:0000}. {1,-50} {2}", folderLN, new DirectoryInfo(folders[i]).Name, folders[i].Length); 
        }

        static void ErrorMessage() // Error message method
        {
            Console.WriteLine("Sorry, your input is not valid."); // Appropiate message
            ContentSeperator();
        }

        static void FolderErrorMessage() // Error message method
        {
            Console.WriteLine("Sorry, that folder already exists (or no name entered at all)! "); // Appropiate message
            ContentSeperator();
        }

        static void FileModificationErrorMessage() // File modifying error method
        {
            Console.WriteLine("Sorry, Access Denied. You do not have the permissions in this directory."); // Appropiate message
            ContentSeperator();
        }

        static void InvalidFileNameErrorMessage()
        {
            Console.WriteLine("Sorry, you entered invalid characters into the folder name!");
            ContentSeperator();
        }

        static void ReturnToMenu() // Return to main menu method
        {
            Console.WriteLine("Press any key to return to home..."); // Appropiate message 
            Console.ReadKey();                                       // Wait for user input
            Console.Clear();                                         // Clear the screen
        }

        static void WelcomeMessage() // Display welcome message method
        {
            Console.WriteLine("Welcome to my File Listing and Searching Assignment"); // Tab across the text
            Console.WriteLine();      
        }

        static void TitleScreen() // Display TitleScreen
        {
            Console.Clear();
            ContentSeperator();                                                                   // Neat - across
            Console.WriteLine("{0}Introduction to Programming - Assignment 1", spacing);          // Tab across the text      
            Console.WriteLine("{0}Programmed by Adam Rushton", spacing);                          // Tab across the text
            Console.WriteLine("{0}Last refreshed: {1} ", spacing, DateTime.Now.ToString());       // Tab across the text and display current time
            ContentSeperator();                                                                   // Neat - across
        }
        
        static void FileInformation() // Display current directory, sorting method
        {
            Console.WriteLine("{0}Current file directory: {1}", spacing, startingDirectory);             // Current file directory
            Console.WriteLine("{0}Current sorting method: {1} order", spacing, sortingMethod);           // Sorting method currently being used
            ContentSeperator();                                                                          // Neat - across
        }
        static void ContentSeperator() // Line seperate method
        {
            for (int i = 0; i < consoleWindowWidth; i++) // For the width of the console
            {
                Console.Write("─");                      // display ─
            }
        }

        static void YesOrNo(bool isInvalid) // Write error message if invalid input
        {
            if (isInvalid)      // If invalid input
            {
                ErrorMessage(); // Display error message
            }
        }

        static void ValidDirectory(bool validDir) // Write error message if invalid directory
        {
            if (!validDir)      // If invalid directory
            {
                ErrorMessage(); // Display error message
            }
        }

        static void QuitProgramOption() // Quit program method using ternary operator
        {   // Data types
            string userInput = null;  // Read user input
            bool isInvalid = true;    // Decide if user enters a valid input
            bool quitConsole = false; // See if user wants to quit the console
            ContentSeperator();       // Neatly seperate the sections

            do 
            {
                Console.Write("Are you sure you want to quit? (y/n): ");
                userInput = Console.ReadLine().ToLower();         // Convert user input to lowercase
                quitConsole = userInput == "y";                   // If the userInput is yes, quitConsole is true
                isInvalid = !quitConsole && userInput != "n";     // Invalid input if the user does not enter n or y
                YesOrNo(isInvalid);                               // Display error message if invalid input
            } while (isInvalid);                                  // Repeat this block of code until user enters y or n

            (quitConsole ? (Action)QuitProgram : ReturnToMenu)(); // Respond to user input - quit the program or "Press any key to continue" to return to main menu
        }

        static void QuitProgram() // Quit program method
        {
            int exitCode = 0;           // Exit code number
            Environment.Exit(exitCode); // Exit the program giving the correct exit code.
        }
    }
}