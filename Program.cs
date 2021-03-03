using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ksync
{
    class Program
    {
        static int Main(string[] args)
        {
            //Change the console fonts to use Unicode
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //Check for command line atguments
            if (args.Length != 0)
            {
                //Is the -t option present in the command line arguments
                bool optionTest = OptionTestPresent(args);
                //Is the -ss option present in the command line arguments
                bool optionSkipSystem = OptionSkipSystemPresent(args);
                //Is the -sh option present in the command line arguments
                bool optionSkipHidden = OptionSkipHiddenPresent(args);
                //Is the -c option present in the command line arguments
                bool optionConfirm = OptionConfirmPresent(args);

                //Is the option to mirror/echo present in the arguments
                string optionMirrorEcho = OptionMirrorEchoPresent(args).ToLower();
                //If the option is not present, assume echo
                if (optionMirrorEcho == "mirror") optionMirrorEcho = "Mirroring"; 
                    else optionMirrorEcho = "Echoing";
                //Is option -h for help present
                if (OptionHelpPresent(args))
                {
                    PrintUsage();
                    return 0;
                }
                //Initialize source and destination directories
                //Get the source directory
                string srcDir = GetSrcDir(args);
                if (!Directory.Exists(srcDir))
                {
                    Console.WriteLine("Invalid source directory. Exiting...");
                    return 1;
                }
                //Get the destination directory
                string dstDir = GetDstDir(args);
                if (!Directory.Exists(dstDir))
                {
                    //Create the destination if it doesn't exist
                    try
                    {
                        if (!optionTest) Directory.CreateDirectory(@dstDir);
                    }
                    //Something wrong while creating the destination directory, exit
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return 1;
                    }
                }
                //Check if there are any exceptions
                //Comma delimited list
                string optionExceptions = GetExceptions(args);                                
                List<string> lstExceptions = new List<string>() { };
                if (!string.IsNullOrEmpty(optionExceptions)) lstExceptions = optionExceptions.Split(',').ToList();

                //If the test option is present, print the options and the values
                if (optionTest) Console.WriteLine("- {0} \n- Source: {1}\n- " +
                    "Destination: {2} \n- Confirm: {3}\n- Skip system: {4}\n- Skip hidden: {5}\n- Exceptions:\n\t {6}", 
                    optionMirrorEcho, srcDir, dstDir,optionConfirm.ToString(), optionSkipSystem.ToString(), 
                    optionSkipHidden.ToString(), String.Join("\n\t ", optionExceptions));

                //isCopyOK is a place holder for a return value for the main copy routine
                //Call the main copy routine
                MainCopy(srcDir, dstDir, lstExceptions, optionTest, optionConfirm,
                    optionSkipSystem, optionSkipHidden);
                //if (isCopyOK != 0) return 1;
                //If mirroring, repeat the copy but with reversed source and destination
                if (optionMirrorEcho == "Mirroring")
                {
                    MainCopy(dstDir, srcDir, lstExceptions, optionTest,
                        optionConfirm, optionSkipSystem, optionSkipHidden);
                    //if (isCopyOK != 0) return 1;
                }
            }
            //If no command line arguments print help and exit
            else PrintHelp();

            return 0;            
        }

        //Get the source directory from the command line arguments
        static string GetSrcDir(string[] args)
        {
            return ParseStringOptions(args, "-s", "--source");
        }

        //Get the destination directory from the command line arguments
        static string GetDstDir(string[] args)
        {
            return ParseStringOptions(args, "-d", "--destination");
        }

        //Check if the option --exception or -e is present
        static string GetExceptions(string[] args)
        {
            return ParseStringOptions(args, "-e", "--exception");
        }

        //Check if mirror or echo is present or absent
        //If the option is absent assume echo
        static string OptionMirrorEchoPresent(string[] args)
        {
            return ParseStringOptions(args, "-o", "--option");
        }

        //Check if the help option is present
        static bool OptionHelpPresent(string[] args)
        {
            return ParseBoolOptions(args, "-h", "--help");
        }

        //Check if the confirm option is present
        static bool OptionConfirmPresent(string[] args)
        {
            return ParseBoolOptions(args, "-c", "--confirm");
        }

        //Check if the option --skip-system or -ss is present
        static bool OptionSkipSystemPresent(string[] args)
        {
            return ParseBoolOptions(args, "-ss", "--skip-system");
        }

        //Check if the option --skip-hidden or -sd is present
        static bool OptionSkipHiddenPresent(string[] args)
        {
            return ParseBoolOptions(args, "-sh", "--skip-hidden");
        }

        //Check if the test option is present
        static bool OptionTestPresent(string[] args)
        {
            return ParseBoolOptions(args, "-t", "--test");
        }

        //Parse input options that return bool, present or not
        static bool ParseBoolOptions(string[] args, string p1, string p2)
        {
            bool bFind = false;
            for (int i = 0; i < args.Length; i++)
            {

                if (args[i] == p1 || args[i] == p2)
                {
                    bFind = true;
                    return bFind;
                }
                else bFind = false;

            }
            return bFind;
        }

        //Parse input options that contain return parameter, e.g. source, destination
        static string ParseStringOptions(string[] args, string p1, string p2)
        {
            bool bFind = false;
            int i;
            for (i = 0; i < args.Length; i++)
            {
                if (args[i] == p1 || args[i] == p2)
                {
                    bFind = true;
                    break;
                }
                else bFind = false;
            }
            if (bFind) return args[i + 1]; else return "";
        }

        //Print usage
        static void PrintUsage()
        {
            Console.WriteLine("ksync is a small utility that syncs two directories.");
            Console.WriteLine("Type ksync without any parameters for a list of options.");
            Console.WriteLine("There are two sync options, echo and mirror. If you don't specify the -o option, echo is the default.");
            Console.WriteLine(" --- Echo copies the missing files and the newer files (if destination file exists) from the source to the destination. (A -> B).");
            Console.WriteLine(" --- Mirror does the same, but then checks the destination against source. (A -> B, B -> A).");
            Console.WriteLine("There is no deletion of any files in both echo and mirror sync options.");
            Console.WriteLine("If you delete a file at the source, the same file won't be deleted at the destination.");
            Console.WriteLine("Always run the initial sync with -t or --test option to see what's going to happen.");
            Console.WriteLine("If you more granularity, use -c or --confirm option. It will prompt to copy each file at the destination.");
            Console.WriteLine("Do not copy (N) is the default option, so hitting enter will skip the file.");
            Console.WriteLine("You can use an exception list if you want to skip some files or directories.");
            Console.WriteLine("Don't use wildcards in the exception list for directories. Wildcard * is for file extensions only.");
            Console.WriteLine("For example if you want to skip all directories that have the word temp in the name, use -e temp.");
            Console.WriteLine("This option will skip \"temporary Files\", \"Some temp dir\", \"my temp\" etc.");
            Console.WriteLine("Use comma delimited separator for more exceptions, e.g. -e *.jpg,*.tmp,temp. Use wildcards to skip file extensions.");
            Console.WriteLine("If you want to skip system or hidden files or directories, use -ss and -sh options.");
        }
        
        //Print help
        static void PrintHelp()
        {
            Console.WriteLine("ksync v0.0.1 - 2021 (c) Simplified BSD License");
            Console.WriteLine("Usage:");
            Console.WriteLine("  ksync [options]");
            Console.WriteLine("    -s,  --source <source folder>");
            Console.WriteLine("    -d,  --destination <destination folder");
            Console.WriteLine("    -o,  --option <mirror or echo>");
            Console.WriteLine("    -e,  --exception <folders or extensions>");
            Console.WriteLine("    -t,  --test");
            Console.WriteLine("    -c,  --confirm");
            Console.WriteLine("    -h,  --help");
            Console.WriteLine("    -ss, --skip-system");
            Console.WriteLine("    -sh, --skip-hidden");
            Console.WriteLine("Example:");
            Console.WriteLine("    ksync -s c:\\windows -d d:\\backup -o mirror");
            Console.WriteLine("    ksync -s \"c:\\Program Files\" -d d:\\backup -e " +
                "*.jpg,\"c:\\temp\" --ss -c");
        }

        //Main copy routine
        static void MainCopy(string srcDir, string dstDir, List<string> exceptions, 
            bool optionTest, bool optionConfirm, bool skipSystem, bool skipHidden)
        {
            //Get the source files   
            IEnumerable<string> allFiles = GetAllFiles(srcDir, "*");

            //Get info and attributes  about the directory
            DirectoryInfo dirInfo;
            bool isroot;
            dirInfo = new DirectoryInfo(srcDir);
            isroot = dirInfo.Root.FullName.Equals(dirInfo.FullName);
            
            //If --skip-system option is present and the directory has system attribute
            if (skipSystem)
            {
                if (dirInfo.Attributes.HasFlag(FileAttributes.System) && !isroot)
                {
                    return;
                }
            }
            //If --skip-hidden option is present and the directory has hidden attribute
            if (skipHidden)
            {
                if (dirInfo.Attributes.HasFlag(FileAttributes.Hidden) && !isroot)
                {
                    return;
                }
            }

            //Loop through each file
            foreach (string srcFile in allFiles)
            {
                //Get the directory of the file
                string fileDirectory = Path.GetDirectoryName(srcFile);
                //Get the extension of the src file
                string srcExtension = "*" + Path.GetExtension(srcFile);
                //If the extension is in the exceptions -> skip
                if (exceptions.Contains(srcExtension)) continue;
                //If the directory is in the exceptions -> skip                
                if (exceptions.Count != 0)
                {
                    foreach (string exc in exceptions)
                    {
                        if (fileDirectory.Contains(exc))
                        {
                            goto skip;
                        }
                    }
                }

                FileInfo fileInfo = new FileInfo(srcFile);
                //If --skip-system option is present and the file has system attribute
                if (skipSystem)
                {
                    if (fileInfo.Attributes.HasFlag(FileAttributes.System))
                    {
                        continue;
                    }
                }
                //If --skip-hidden option is present and the file has hidden attribute
                if (skipHidden)
                {
                    if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        continue;
                    }
                }

                //Get destination counterpart file
                string dstFile = dstDir + @"\" + srcFile.Substring(Path.GetPathRoot(srcFile).Length);

                //Get the destination directory name
                string dstdirectoryName = Path.GetDirectoryName(dstFile);
                try
                {
                    //Try to create the dir
                    if (!optionTest) Directory.CreateDirectory(@dstdirectoryName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
                //Does destination file exist
                if (File.Exists(dstFile))
                {
                    //Get modified date of the src file
                    DateTime dtSrcFile = File.GetLastWriteTime(srcFile);
                    //Get modified date of the dst file
                    DateTime dtDstFile = File.GetLastWriteTime(dstFile);
                    //Are the dates the same
                    //Ignore if the files are the same modify date
                    //Otherwise overwrite with the source
                    if (dtSrcFile == dtDstFile) continue;
                    else
                    {
                        if (optionTest)
                        {
                            Console.WriteLine("Copying {0} to {1}", srcFile, dstFile);
                        }
                        else
                        {
                            try
                            {
                                //If the option to confirm is present
                                Console.Write("Copying {0} to {1}", srcFile, dstFile);                                
                                if (optionConfirm)
                                {
                                    Console.Write("[yN]");
                                    char charYesOrNo = Console.ReadKey().KeyChar;
                                    if (char.ToLower(charYesOrNo) == 'y')
                                    {
                                        Console.WriteLine("...");
                                        File.Copy(srcFile, dstFile, true);
                                    }
                                    else
                                    {
                                        Console.WriteLine("");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("...");
                                    File.Copy(srcFile, dstFile, true);                                    
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                return;
                            }                            
                        }
                    }
                }
                else
                //if destination file doesn't exist, copy it over
                {
                    if (optionTest)
                    {
                        Console.WriteLine("Copying {0} to {1}", srcFile, dstFile);
                    }
                    else
                    {
                        try
                        {
                            Console.Write("Copying {0} to {1}", srcFile, dstFile);
                            if (optionConfirm)
                            {
                                Console.Write(" [yN]");
                                char charYesOrNo = Console.ReadKey().KeyChar;
                                if (char.ToLower(charYesOrNo) == 'y')
                                {
                                    Console.WriteLine("...");
                                    File.Copy(srcFile, dstFile);
                                }
                                else
                                {
                                    Console.WriteLine("");
                                }
                            }
                            else
                            {
                                Console.WriteLine("...");
                                File.Copy(srcFile, dstFile);                                
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return;
                        }                        
                    }
                }
            skip:
                continue;
            }                       
            return;
        }

        //Get all the files recursively, skip if Access Denied exception
        static IEnumerable<String> GetAllFiles(string path, string searchPattern)
        {
            return System.IO.Directory.EnumerateFiles(path, searchPattern).Union(
                System.IO.Directory.EnumerateDirectories(path).SelectMany(d =>
                {
                    try
                    {
                        return GetAllFiles(d, searchPattern);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                        return Enumerable.Empty<String>();
                    }
                }));
        }
    }
}
