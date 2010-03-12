﻿using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class DeconToolsPeakDetector : IPeakDetector
    {
        DeconToolsV2.Peaks.clsPeakProcessor peakProcessor;


        #region Properties
        private double peakBackgroundRatio;

        public double PeakBackgroundRatio
        {
            get { return peakBackgroundRatio; }
            set { peakBackgroundRatio = value; }
        }


        private double sigNoiseThreshold;

        public double SigNoiseThreshold
        {
            get { return sigNoiseThreshold; }
            set { sigNoiseThreshold = value; }
        }

        private DeconTools.Backend.Globals.PeakFitType peakFitType;
        public DeconTools.Backend.Globals.PeakFitType PeakFitType
        {
            get { return peakFitType; }
            set { peakFitType = value; }
        }



        private bool isDataThresholded;

        public bool IsDataThresholded
        {
            get { return isDataThresholded; }
            set { isDataThresholded = value; }
        }

        private bool storePeakData;
        public bool StorePeakData
        {
            get { return storePeakData; }
            set { storePeakData = value; }
        }

        DeconToolsV2.Peaks.clsPeakProcessorParameters deconEngineParameters;

        public DeconToolsV2.Peaks.clsPeakProcessorParameters DeconEngineParameters
        {
            get { return deconEngineParameters; }
            internal set { deconEngineParameters = value; }
        }


        #endregion

        #region Constructors
        public DeconToolsPeakDetector()
        {
            this.deconEngineParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            this.setDefaults();
        }

        private void setDefaults()
        {
            this.peakFitType = Globals.PeakFitType.QUADRATIC;
            this.peakBackgroundRatio = 1.3;
            this.sigNoiseThreshold = 2;
            this.StorePeakData = false;
            
        }

        public DeconToolsPeakDetector(DeconToolsV2.Peaks.clsPeakProcessorParameters parameters)
        {
            this.DeconEngineParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            convertDeconEngineParameters(parameters);
        }


        #endregion

        #region Private Methods
        private void updateDeconEngineParameters()
        {
            this.DeconEngineParameters.PeakBackgroundRatio = this.peakBackgroundRatio;
            this.DeconEngineParameters.SignalToNoiseThreshold = this.sigNoiseThreshold;
            this.DeconEngineParameters.PeakFitType = getDeconPeakFitType(this.peakFitType);
            this.DeconEngineParameters.ThresholdedData = this.isDataThresholded;
            this.DeconEngineParameters.WritePeaksToTextFile = this.StorePeakData;

        }

        private DeconToolsV2.Peaks.PEAK_FIT_TYPE getDeconPeakFitType(Globals.PeakFitType peakFitType)
        {
            switch (peakFitType)
            {
                case Globals.PeakFitType.Undefined:
                    throw new Exception("Failed to convert PeakType to DeconEnginePeakType");
                case Globals.PeakFitType.APEX:
                    return DeconToolsV2.Peaks.PEAK_FIT_TYPE.APEX;
                case Globals.PeakFitType.LORENTZIAN:
                    return DeconToolsV2.Peaks.PEAK_FIT_TYPE.LORENTZIAN;
                case Globals.PeakFitType.QUADRATIC:
                    return DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
                default:
                    return DeconToolsV2.Peaks.PEAK_FIT_TYPE.APEX;
            }
        }

        private void convertDeconEngineParameters(DeconToolsV2.Peaks.clsPeakProcessorParameters parameters)
        {

            //this.isDataThresholded = parameters.ThresholdedData;     //IsDataThresholded will now be extracted from the Run class
            this.peakBackgroundRatio = parameters.PeakBackgroundRatio;
            this.peakFitType = getPeakFitType(parameters.PeakFitType);
            this.sigNoiseThreshold = parameters.SignalToNoiseThreshold;
            this.StorePeakData = parameters.WritePeaksToTextFile;

        }

        private Globals.PeakFitType getPeakFitType(DeconToolsV2.Peaks.PEAK_FIT_TYPE pEAK_FIT_TYPE)
        {
            switch (pEAK_FIT_TYPE)
            {
                case DeconToolsV2.Peaks.PEAK_FIT_TYPE.APEX:
                    return Globals.PeakFitType.APEX;
                case DeconToolsV2.Peaks.PEAK_FIT_TYPE.LORENTZIAN:
                    return Globals.PeakFitType.LORENTZIAN;
                case DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC:
                    return Globals.PeakFitType.QUADRATIC;
                default:
                    return Globals.PeakFitType.Undefined;
            }
        }

        private List<DeconTools.Backend.Core.IPeak> ConvertDeconEnginePeakList(DeconToolsV2.Peaks.clsPeak[] peaklist)
        {
            List<IPeak> returnedList = new List<IPeak>();

            for (int i = 0; i < peaklist.Length; i++)
            {
                MSPeak peak = new MSPeak();
                peak.XValue = peaklist[i].mdbl_mz;
                peak.Height = (int)peaklist[i].mdbl_intensity;
                peak.SN = (float)peaklist[i].mdbl_SN;
                peak.Width = (float)peaklist[i].mdbl_FWHM;

                peak.DataIndex = peaklist[i].mint_data_index;      // this points to the index value of the raw xy values - I think
                

                returnedList.Add(peak);

            }
            return returnedList;
        }
        #endregion

        #region Public Methods
        public override void FindPeaks(ResultCollection resultList)
        {
            Check.Require(resultList.Run != null, "Run is null");
            Check.Require(resultList.Run.CurrentScanSet != null, "Current_ScanSet has not been set");

            
            //initialize DeconEngine's peakFinding class
            peakProcessor = new DeconToolsV2.Peaks.clsPeakProcessor();

            //initialize options
            this.isDataThresholded = resultList.Run.IsDataThresholded;
            updateDeconEngineParameters();
            peakProcessor.SetOptions(this.deconEngineParameters);

            //Find peaks using DeconEngine
            float[] xvals = new float[1];
            float[] yvals = new float[1];

            resultList.Run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);

            DeconToolsV2.Peaks.clsPeak[] peaklist = new DeconToolsV2.Peaks.clsPeak[0];
            try
            {
                peakProcessor.DiscoverPeaks(ref xvals, ref yvals, ref peaklist, (float)(resultList.Run.MSParameters.MinMZ), (float)(resultList.Run.MSParameters.MaxMZ));

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("DeconEngine's PeakDetector had a critical error in Scan/Frame =  " + resultList.Run.GetCurrentScanOrFrame(), Logger.Instance.OutputFilename);
            }

            resultList.Run.CurrentScanSet.BackgroundIntensity = peakProcessor.GetBackgroundIntensity(ref yvals);
            resultList.Run.PeakList = ConvertDeconEnginePeakList(peaklist);    // peak data is stored here on a per scan basis (cleared after each task execution)

            if (this.StorePeakData)    //store all peak data;   (Exporters are triggered to access this and export info and clear the MSPeakResults)
            {
                resultList.FillMSPeakResults();    //data from the MSPeakList is transferred to 'MSPeakResults'
            }

            resultList.Run.CurrentScanSet.NumPeaks = resultList.Run.PeakList.Count;    //used in ScanResult
            resultList.Run.CurrentScanSet.BasePeak = getBasePeak(resultList.Run.PeakList);     //Used in ScanResult

            resultList.Run.DeconToolsPeakList = peaklist;

            //if (resultList.Run is DeconToolsRun)
            //{
            //    ((DeconToolsRun)(resultList.Run)).DeconToolsPeakList = peaklist;
            //}


        }


        #endregion


      

        protected override void addDataToScanResult(ResultCollection resultList, ScanResult scanresult)
        {

            //scanresult.SetNumPeaks(resultList.Run.MSPeakList.Count);
            //scanresult.SetBasePeak(getBasePeak(resultList.Run.MSPeakList));
        }

        
    }
}
