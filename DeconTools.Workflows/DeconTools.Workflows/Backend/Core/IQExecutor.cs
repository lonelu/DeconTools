﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqExecutor
    {
        private BackgroundWorker _backgroundWorker;

        private readonly IqResultUtilities _iqResultUtilities = new IqResultUtilities();
        private readonly IqTargetUtilities _targetUtilities = new IqTargetUtilities();
        private RunFactory _runFactory = new RunFactory();

        private string _resultsFolder;
        private string _alignmentFolder;

        #region Constructors

        public IqExecutor(WorkflowExecutorBaseParameters parameters, Run run)
        {
			Results = new List<IqResult>();
			IsDataExported = true;
			DisposeResultDetails = true;
            _parameters = parameters;
	        _run = run;
			SetupLogging();
			IqLogger.Log.Info("Log started for dataset: " + _run.DatasetName);
			IqLogger.Log.Info(Environment.NewLine + "Parameters: " + Environment.NewLine + _parameters.ToStringWithDetails());
        }

        #endregion

        #region Properties


        protected IqMassAndNetAligner IqMassAndNetAligner { get; set; }

	    private Run _run;
		protected Run Run { 
			get { return _run; }
			set 
			{ 
				_run = value;
				SetupLogging();
			} 
		}

        private WorkflowExecutorBaseParameters _parameters;
        

        protected WorkflowExecutorBaseParameters Parameters 
		{ 
			get { return _parameters; }
			set
			{
				_parameters = value;
				SetupLogging();
			}
		}

		protected bool IsDataExported { get; set; }

        public bool DisposeResultDetails { get; set; }

        public IqTargetImporter TargetImporter { get; set; }

		public string ChromSourceDataFilePath { get; set; }

		public List<IqResult> Results { get; set; }

		public List<IqTarget> Targets { get; set; }

		protected ResultExporter ResultExporter { get; set; }


		public TargetedWorkflowParameters IqWorkflowParameters { get; set; }

		protected bool ChromDataIsLoaded
		{
			get
			{
				if (Run != null)
				{
					return Run.ResultCollection.MSPeakResultList.Count > 0;
				}

				return false;
			}
		}

		protected bool RunIsInitialized
		{
			get { throw new NotImplementedException(); }
		}

	
        #endregion

        #region Public Methods


        public void SetupMassAndNetAlignment()
        {
            WorkflowExecutorBaseParameters massNetAlignerParameters = new BasicTargetedWorkflowExecutorParameters();
            IqMassAndNetAligner = new IqMassAndNetAligner(massNetAlignerParameters, Run);

            //check if alignment info exists already

            SetupAlignmentFolder();

            string expectedAlignmentFilename = _alignmentFolder + Path.DirectorySeparatorChar + Run.DatasetName + "_iqAlignmentResults.txt";
            bool alignmentResultsExist = (File.Exists(expectedAlignmentFilename));

            if (alignmentResultsExist)
            {
                IqLogger.Log.Info("Using the IQ alignment results from here: " + expectedAlignmentFilename);
                IqMassAndNetAligner.LoadPreviousIqResults(expectedAlignmentFilename);
                return;
            }

            string targetFileForAlignment = Parameters.TargetsUsedForAlignmentFilePath;


            if (string.IsNullOrEmpty(targetFileForAlignment))
            {
                IqLogger.Log.Info("Alignment not performed - No target file has been specified for alignment.");
                return;
            }

            if (!File.Exists(targetFileForAlignment))
            {
                IqLogger.Log.Info("Alignment not performed - Target file for alignment has been specified but a FILE NOT FOUND error has occured.");
                return;
            }

            bool isFirstHitsFile = targetFileForAlignment.EndsWith("_fht.txt");

            if (!isFirstHitsFile)
            {
                IqLogger.Log.Info("Alignment not performed - target file for alignment must be a first hits file (_fht.txt)");
                return;
            }

            IqMassAndNetAligner.LoadAndInitializeTargets(targetFileForAlignment);

            

            if (!string.IsNullOrEmpty(Parameters.TargetsUsedForLookupFilePath))
            {
                IqTargetImporter massTagImporter = new BasicIqTargetImporter(Parameters.TargetsUsedForLookupFilePath);
                var massTagRefs = massTagImporter.Import();

                IqMassAndNetAligner.SetMassTagReferences(massTagRefs);
                IqLogger.Log.Info("IQ Net aligner - "+ massTagRefs.Count+ " reference targets were loaded successfully." );
            }
            else
            {
                IqLogger.Log.Info("IQ Net aligner INACTIVE - no reference tags were loaded. You need to define 'TargetsUsedForLookupFilePath'");
            }



        }


        public void DoAlignment()
        {
            if (Parameters.IsMassAlignmentPerformed)
            {
                Run.MassAlignmentInfo = IqMassAndNetAligner.DoMassAlignment();
            }

            if (Parameters.IsNetAlignmentPerformed)
            {
                Run.NetAlignmentInfo = IqMassAndNetAligner.DoNetAlignment();
            }
        }




        public void Execute()
        {
            Execute(Targets);
        }

        public void Execute(List<IqTarget> targets)
        {
	        int totalTargets = targets.Count;
	        int targetCount = 1;
			IqLogger.Log.Info("Total targets being processed: " + totalTargets);
			IqLogger.Log.Info("Processing...");

	        foreach (var target in targets)
            {
                Run = target.GetRun();

                if (!ChromDataIsLoaded)
                {
                    LoadChromData(Run);
                }

				ReportGeneralProgress(targetCount, totalTargets);

                target.DoWorkflow();
                var result = target.GetResult();

                if (IsDataExported)
                {
                    ExportResults(result);
                }

                Results.Add(result);

                if (DisposeResultDetails)
                {
                    result.Dispose();
                }
	            targetCount++;
            }

			IqLogger.Log.Info("Processing Complete!" + Environment.NewLine + Environment.NewLine);
        }


	    public virtual void LoadAndInitializeTargets()
        {
            LoadAndInitializeTargets(Parameters.TargetsFilePath);

         

        }


        public virtual void LoadAndInitializeTargets(string targetsFilePath)
        {
            if (TargetImporter == null)
            {
                TargetImporter = new BasicIqTargetImporter(targetsFilePath);
            }

			IqLogger.Log.Info("Target Loading Started...");

            Targets = TargetImporter.Import();

            _targetUtilities.CreateChildTargets(Targets, 
                Parameters.MinMzForDefiningChargeStateTargets,
                Parameters.MaxMzForDefiningChargeStateTargets,
                Parameters.MaxNumberOfChargeStateTargetsToCreate);

			IqLogger.Log.Info("Targets Loaded Successfully. Total targets loaded= "+ Targets.Count);
        }


        protected virtual void ExportResults(IqResult iqResult)
        {
            List<IqResult> resultsForExport = _iqResultUtilities.FlattenOutResultTree(iqResult);

            var orderedResults = resultsForExport.OrderBy(p => p.Target.ChargeState).ToList();

	        var exportedResults = orderedResults.Where(orderedResult => orderedResult.IsExported).ToList();

	        if (ResultExporter == null)
            {
                ResultExporter = iqResult.Target.Workflow.CreateExporter();
            }
            
            SetupResultsFolder();

            ResultExporter.WriteOutResults(_resultsFolder + Path.DirectorySeparatorChar + Run.DatasetName + "_iqResults.txt", exportedResults);
        }

        private void SetupAlignmentFolder()
        {
            if (string.IsNullOrEmpty(Parameters.OutputFolderBase))
            {
                _alignmentFolder = GetDefaultOutputFolder();
            }
            else
            {
                _alignmentFolder = Parameters.OutputFolderBase + "\\AlignmentInfo";
            }

            if (!Directory.Exists(_alignmentFolder)) Directory.CreateDirectory(_alignmentFolder);

        }


        private void SetupResultsFolder()
        {
            if (string.IsNullOrEmpty(Parameters.OutputFolderBase))
            {
                _resultsFolder = GetDefaultOutputFolder();
            }
            else
            {
                _resultsFolder = Parameters.OutputFolderBase + "\\IqResults";
            }

            if (!Directory.Exists(_resultsFolder)) Directory.CreateDirectory(_resultsFolder);


        }

        #endregion

		#region Private Methods


		private string CreatePeaksForChromSourceData()
        {
            var parameters = new PeakDetectAndExportWorkflowParameters();

            parameters.PeakBR = Parameters.ChromGenSourceDataPeakBR;
            parameters.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            parameters.SigNoiseThreshold = Parameters.ChromGenSourceDataSigNoise;
            parameters.ProcessMSMS = Parameters.ChromGenSourceDataProcessMsMs;
            parameters.IsDataThresholded = Parameters.ChromGenSourceDataIsThresholded;

            var peakCreator = new PeakDetectAndExportWorkflow(this.Run, parameters, _backgroundWorker);
            peakCreator.Execute();

            var peaksFilename = this.Run.DataSetPath + "\\" + this.Run.DatasetName + "_peaks.txt";
            return peaksFilename;

        }


        private string GetPossiblePeaksFile()
        {
            string baseFileName;
            baseFileName = this.Run.DataSetPath + "\\" + this.Run.DatasetName;

            string possibleFilename1 = baseFileName + "_peaks.txt";

            if (File.Exists(possibleFilename1))
            {
                return possibleFilename1;
            }
            else
            {
                return string.Empty;
            }
        }



        public void LoadChromData(Run run)
        {
            if (string.IsNullOrEmpty(ChromSourceDataFilePath))
            {
                ChromSourceDataFilePath = GetPossiblePeaksFile();
            }

            if (string.IsNullOrEmpty(ChromSourceDataFilePath))
            {
                //ReportGeneralProgress("Creating _Peaks.txt file for extracted ion chromatogram (XIC) source data ... takes 1-5 minutes");
				IqLogger.Log.Info("Creating _Peaks.txt");
                ChromSourceDataFilePath = CreatePeaksForChromSourceData();
            }
            else
            {
				IqLogger.Log.Info("Using Existing _Peaks.txt");
            }

			IqLogger.Log.Info("Peak Loading Started...");

            PeakImporterFromText peakImporter = new PeakImporterFromText(ChromSourceDataFilePath, _backgroundWorker);
            peakImporter.ImportPeaks(this.Run.ResultCollection.MSPeakResultList);

			IqLogger.Log.Info("Peak Loading Complete. Number of peaks loaded= " + Run.ResultCollection.MSPeakResultList.Count);
        }

        



		private void ReportGeneralProgress(int currentTarget, int totalTargets)
		{
			double currentProgress =  (currentTarget/(double)totalTargets);
			
			if (currentTarget % 50 == 0)
			{
                IqLogger.Log.Info("Processing target " + currentTarget + " of " + totalTargets + "; " + (Math.Round(currentProgress *100, 1)) + "% Complete." );
			}
		}




		
		private void SetupLogging()
		{
		    string loggingFolder;
            if (string.IsNullOrEmpty(Parameters.OutputFolderBase))
            {
                loggingFolder = GetDefaultOutputFolder();
            }
            else
            {
                loggingFolder = Parameters.OutputFolderBase + "\\IqLogs";
            }


            if (!Directory.Exists(loggingFolder)) Directory.CreateDirectory(loggingFolder);


			IqLogger.LogDirectory = loggingFolder;
			IqLogger.InitializeIqLog(_run.DatasetName);
		}

        private string GetDefaultOutputFolder()
        {
            string defaultOutputFolder = _run.DataSetPath;
            return defaultOutputFolder;
        }

        #endregion

    }
}
