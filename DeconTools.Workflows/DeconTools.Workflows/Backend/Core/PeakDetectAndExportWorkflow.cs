using System;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.Runs;

namespace DeconTools.Workflows.Backend.Core
{
    public class PeakDetectAndExportWorkflow : WorkflowBase
    {

        DeconToolsPeakDetectorV2 _ms1PeakDetector;
        DeconToolsPeakDetectorV2 _ms2PeakDetectorForCentroidedData;
        DeconToolsPeakDetectorV2 _ms2PeakDetectorForProfileData;

        PeakDetectAndExportWorkflowParameters _workflowParameters;
        private BackgroundWorker backgroundWorker;

        PeakListTextExporter peakExporter;

        #region Constructors

        public PeakDetectAndExportWorkflow(Run run)
            : this(run, new PeakDetectAndExportWorkflowParameters())
        {

        }

        public PeakDetectAndExportWorkflow(Run run, PeakDetectAndExportWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;
            this.Run = run;

        }

        public PeakDetectAndExportWorkflow(Run run, PeakDetectAndExportWorkflowParameters parameters, BackgroundWorker bw)
            : this(run, parameters)
        {
            this.backgroundWorker = bw;
        }



        #endregion


        public override void InitializeWorkflow()
        {
            _ms1PeakDetector = new DeconToolsPeakDetectorV2(this._workflowParameters.PeakBR, this._workflowParameters.SigNoiseThreshold,
                this._workflowParameters.PeakFitType, this._workflowParameters.IsDataThresholded);


            _ms2PeakDetectorForProfileData = new DeconToolsPeakDetectorV2(_workflowParameters.MS2PeakDetectorPeakBR,
                                                                          _workflowParameters.MS2PeakDetectorSigNoiseThreshold,
                                                                          _workflowParameters.PeakFitType,
                                                                          _workflowParameters.MS2PeakDetectorDataIsThresholded);
            

            _ms2PeakDetectorForCentroidedData = new DeconToolsPeakDetectorV2(0, 0, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            _ms2PeakDetectorForCentroidedData.RawDataType=DeconTools.Backend.Globals.RawDataType.Centroided;

            _ms2PeakDetectorForProfileData.PeaksAreStored = true;
            _ms2PeakDetectorForCentroidedData.PeaksAreStored = true;
            _ms1PeakDetector.PeaksAreStored = true;


        }

        public override void Execute()
        {
            InitializeWorkflow();

            PrepareOutputFolder(_workflowParameters.OutputFolder);

            string outputPeaksFileName = getOutputPeaksFilename();

            peakExporter = new PeakListTextExporter(Run.MSFileType, outputPeaksFileName);

            int numTotalScans = Run.ScanSetCollection.ScanSetList.Count;
            int scanCounter = 0;

            if (Run.MSFileType == DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
            {
                var uimfrun = Run as UIMFRun;

                int numTotalFrames = uimfrun.ScanSetCollection.ScanSetList.Count;
                int frameCounter = 0;

                foreach (var frameSet in uimfrun.ScanSetCollection.ScanSetList)
                {
                    frameCounter++;
                    uimfrun.CurrentScanSet = frameSet;
                    uimfrun.ResultCollection.MSPeakResultList.Clear();

                    foreach (var scanSet in uimfrun.IMSScanSetCollection.ScanSetList)
                    {
                        uimfrun.CurrentIMSScanSet = (IMSScanSet) scanSet;
                        MSGenerator.Execute(uimfrun.ResultCollection);
                        this._ms1PeakDetector.Execute(uimfrun.ResultCollection);

                    }
                    peakExporter.WriteOutPeaks(uimfrun.ResultCollection.MSPeakResultList);

                    if (frameCounter % 5 == 0 || scanCounter == numTotalFrames)
                    {
                        double percentProgress = frameCounter * 100 / numTotalFrames;
                        reportProgress(percentProgress);
                    }

                }

            }
            else
            {
                foreach (var scan in Run.ScanSetCollection.ScanSetList)
                {
                    scanCounter++;

                    Run.CurrentScanSet = scan;

                    Run.ResultCollection.MSPeakResultList.Clear();

                    MSGenerator.Execute(Run.ResultCollection);
                    if (Run.GetMSLevel(scan.PrimaryScanNumber)==1)
                    {
                        this._ms1PeakDetector.Execute(Run.ResultCollection);
                    }
                    else
                    {
                        var dataIsCentroided = Run.IsDataCentroided(scan.PrimaryScanNumber);
                        if (dataIsCentroided)
                        {
                            _ms2PeakDetectorForCentroidedData.Execute(Run.ResultCollection);
                        }
                        else
                        {
                            _ms2PeakDetectorForProfileData.Execute(Run.ResultCollection);
                        }
                    }

                    peakExporter.WriteOutPeaks(Run.ResultCollection.MSPeakResultList);

                    if (scanCounter % 50 == 0 || scanCounter == numTotalScans)
                    {
                        double percentProgress = scanCounter * 100 / numTotalScans;
                        reportProgress(percentProgress);
                    }

                }
            }





            Run.ResultCollection.MSPeakResultList.Clear();

        }

        private void PrepareOutputFolder(string outputFolder)
        {
            if (string.IsNullOrEmpty(outputFolder))
            {
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                try
                {
                    Directory.CreateDirectory(outputFolder);
                }
                catch (Exception ex)
                {

                    throw new DirectoryNotFoundException("PeakExporter cannot create output folder.\n\nDetails: " + ex.Message + "\n" + ex.StackTrace, ex);

                }
            }

        }

        private void reportProgress(double percentProgress)
        {
            if (backgroundWorker != null)
            {
                backgroundWorker.ReportProgress((int)percentProgress);
            }
            else
            {
                Console.WriteLine("Peak creation progress: " + percentProgress + "%");
            }
        }

        private string getOutputPeaksFilename()
        {
            string expectedPeaksFilename;

            if (this._workflowParameters.OutputFolder == String.Empty)
            {
                expectedPeaksFilename = Path.Combine(Run.DataSetPath, Run.DatasetName + "_peaks.txt");
            }
            else
            {
                expectedPeaksFilename = Path.Combine(_workflowParameters.OutputFolder, Run.DatasetName + "_peaks.txt");
            }

            return expectedPeaksFilename;
        }

        public override void InitializeRunRelatedTasks()
        {
            if (Run != null)
            {
                MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
                int minLCScan;
                int maxLCScan;

                if (this._workflowParameters.LCScanMax == -1 || this._workflowParameters.LCScanMin == -1)
                {
                    if (Run is UIMFRun)
                    {
                        minLCScan = ((UIMFRun)Run).MinLCScan;
                        maxLCScan = ((UIMFRun)Run).MaxLCScan;
                    }
                    else
                    {
                        minLCScan = Run.MinLCScan;
                        maxLCScan = Run.MaxLCScan;
                    }



                }
                else
                {
                    minLCScan = this._workflowParameters.LCScanMin;
                    maxLCScan = this._workflowParameters.LCScanMax;
                }

                if (Run.MSFileType == DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
                {
                    var uimfRun = Run as UIMFRun;

                    uimfRun.ScanSetCollection .Create(uimfRun, minLCScan, maxLCScan,
                                                                       _workflowParameters.Num_LC_TimePointsSummed, 1,
                                                                       _workflowParameters.ProcessMSMS);


                    bool sumAllIMSScans = (_workflowParameters.NumIMSScansSummed == -1 ||
                                        _workflowParameters.NumIMSScansSummed > uimfRun.MaxLCScan);

                    if (sumAllIMSScans)
                    {
                        int primaryIMSScan = Run.MinLCScan;

                        uimfRun.IMSScanSetCollection.ScanSetList.Clear();
                        var imsScanset = new IMSScanSet(primaryIMSScan, uimfRun.MinIMSScan, uimfRun.MaxIMSScan);
                        uimfRun.IMSScanSetCollection.ScanSetList.Add(imsScanset);
                    }
                    else
                    {
                        uimfRun.IMSScanSetCollection .Create(Run, uimfRun.MinIMSScan, uimfRun.MaxIMSScan,
                                                                         _workflowParameters.NumIMSScansSummed, 1);
                    }



                }
                else
                {
                    Run.ScanSetCollection .Create(Run, minLCScan, maxLCScan,
                   this._workflowParameters.Num_LC_TimePointsSummed, 1, this._workflowParameters.ProcessMSMS);

                }


            }
        }


        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                return _workflowParameters;
            }
            set
            {
                _workflowParameters = value as PeakDetectAndExportWorkflowParameters;
            }
        }


        

    }
}
