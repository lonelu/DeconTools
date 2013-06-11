﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities;


namespace IQ.Console
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;


        static int Main(string[] args)
        {
            //IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            //SetConsoleMode(handle, ENABLE_EXTENDED_FLAGS);     // sets it so that keyboard use does not interrupt things.


            var options = new IqConsoleOptions();


            List<string> datasetList = new List<string>();


            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {

                string inputFile = options.InputFile;

                

                bool inputFileIsAListOfDatasets = inputFile.ToLower().EndsWith(".txt");
                if (inputFileIsAListOfDatasets)
                {
                    using (StreamReader reader = new StreamReader(inputFile))
                    {
                        
                        while (reader.Peek() != -1)
                        {
                        
                            string datsetName = reader.ReadLine();
                            datasetList.Add(datsetName);

                        }
                    }
                }
                else
                {
                 

                    datasetList.Add(options.InputFile);

                }


                int numDatasets = datasetList.Count;
                int datasetCounter = 0;

                foreach (var dataset in datasetList)
                {
                    datasetCounter++;

                    bool datasetNameContainsPath = dataset.Contains("\\");

                    string currentDatasetPath = dataset;

                    if (datasetNameContainsPath)
                    {
                        currentDatasetPath = dataset;
                    }
                    else
                    {

                        if (string.IsNullOrEmpty(options.TemporaryWorkingFolder))
                        {
                            IqLogger.Log.Fatal("Trying to grab .raw file from DMS, but no temporary working folder was declared. Use option -f. ");
                            break;

                        }


                        if (string.IsNullOrEmpty(options.OutputFolder))
                        {
                            options.OutputFolder = options.TemporaryWorkingFolder;
                        }



                        var datasetutil = new DatasetUtilities();

                        //TODO: figure out how to do this while supporting other file types
                        currentDatasetPath = datasetutil.GetDatasetPath(dataset) + "\\" + dataset + ".raw";

                        if (currentDatasetPath.ToLower().Contains("purged"))
                        {
                            currentDatasetPath = datasetutil.GetDatasetPathArchived(dataset) + "\\" + dataset + ".raw";
                        }
                    }


                    if (!File.Exists(currentDatasetPath))
                    {
                        IqLogger.Log.Fatal("!!!!!!!!! Dataset not found! Dataset path = " + currentDatasetPath);
                    }

                    if (string.IsNullOrEmpty(options.OutputFolder))
                    {
                        options.OutputFolder = RunUtilities.GetDatasetParentFolder(currentDatasetPath);
                    }
                    
                    var executorParameters = GetExecutorParameters(options);

                    IqLogger.Log.Info("IQ analyzing dataset " + datasetCounter + " of " + numDatasets + ". Dataset = " + dataset);

                    TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, currentDatasetPath);
                    executor.Execute();

                }


            }

            return 0;



        }

        private static BasicTargetedWorkflowExecutorParameters GetExecutorParameters(IqConsoleOptions options)
        {
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsFilePath = options.TargetFile;
            executorParameters.OutputFolderBase = options.OutputFolder;
            executorParameters.TargetedAlignmentIsPerformed = options.IsAlignmentPerformed;
            executorParameters.TargetsUsedForAlignmentFilePath = options.TargetFileForAlignment;
            executorParameters.WorkflowParameterFile = options.WorkflowParameterFile;
            executorParameters.TargetedAlignmentWorkflowParameterFile = options.AlignmentParameterFile;
            executorParameters.IsMassAlignmentPerformed = options.IsMassAlignmentPerformed;
            executorParameters.IsNetAlignmentPerformed = options.IsNetAlignmentPerformed;


            if (!string.IsNullOrEmpty(options.TemporaryWorkingFolder))
            {
                executorParameters.CopyRawFileLocal = true;

                executorParameters.FolderPathForCopiedRawDataset = options.TemporaryWorkingFolder;
                executorParameters.DeleteLocalDatasetAfterProcessing = true;
            }
            return executorParameters;
        }

      
    }
}
