﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class IterativeTFF : TFFBase
    {

       
        
        private bool _peakDetectorIsDataThresholded;
        private double _peakDetectorSigNoiseRatioThreshold;
        
        private Globals.PeakFitType _peakDetectorPeakFitType;

        #region Constructors
        public IterativeTFF(IterativeTFFParameters parameters)
        {
            PeakBRStep = parameters.PeakBRStep;
            _peakDetectorIsDataThresholded = parameters.PeakDetectorIsDataThresholded;
            _peakDetectorSigNoiseRatioThreshold = parameters.PeakDetectorSigNoiseRatioThreshold;
            PeakDetectorPeakBR = parameters.PeakDetectorPeakBR;
            _peakDetectorPeakFitType = parameters.PeakDetectorPeakFitType;
            PeakBRMin = parameters.PeakDetectorMinimumPeakBR;

            MaxPeaksToInclude = 30;
            MinRelIntensityForPeakInclusion = parameters.MinimumRelIntensityForForPeakInclusion;


            NeedMonoIsotopicPeak = parameters.RequiresMonoIsotopicPeak;
            ToleranceInPPM = parameters.ToleranceInPPM;

            NumPeaksUsedInAbundance = parameters.NumPeaksUsedInAbundance;
            IsotopicProfileType = parameters.IsotopicProfileType;




            MSPeakDetector = new DeconToolsPeakDetectorV2(PeakDetectorPeakBR, _peakDetectorSigNoiseRatioThreshold,
                 _peakDetectorPeakFitType, _peakDetectorIsDataThresholded);

            //this.MSPeakDetector = new DeconToolsPeakDetectorV2();
            //this.MSPeakDetector.PeakToBackgroundRatio = PeakDetectorPeakBR;
            //this.MSPeakDetector.SignalToNoiseThreshold = _peakDetectorSigNoiseRatioThreshold;
            //this.MSPeakDetector.IsDataThresholded = _peakDetectorIsDataThresholded;
            //this.MSPeakDetector.PeakFitType = _peakDetectorPeakFitType;

        }


        #endregion

        #region Properties
        public double PeakDetectorPeakBR { get; set; }
        public DeconToolsPeakDetectorV2 MSPeakDetector { get; set; }
        public double PeakBRMin { get; set; }
        public double PeakBRStep { get; set; }

        /// <summary>
        /// When finding peaks in the isotopic profile, this number represents the maximum number of peaks added
        /// </summary>
        public int MaxPeaksToInclude { get; set; }

        /// <summary>
        /// When finding peaks in the isotopic profile, this number represents the minimum relative intensity of peaks to be added. Ranges between 0 and 1
        /// </summary>
        public double MinRelIntensityForPeakInclusion { get; set; }


        #endregion

        #region Public Methods
        public override void Execute(ResultCollection resultList)
        {

            Check.Require(resultList != null && resultList.Run != null, string.Format("{0} failed. Run is empty.", Name));
            Check.Require(resultList.Run.CurrentMassTag != null, string.Format("{0} failed. CurrentMassTag hasn't been defined.", Name));

            var result = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);

            var theorFeature = CreateTargetIso(resultList.Run);
            resultList.IsosResultBin.Clear();


            var msPeakList = new List<Peak>();
            var iso = IterativelyFindMSFeature(resultList.Run.XYData, theorFeature, out msPeakList);
            
            result.Run.PeakList = msPeakList;     //this is important for subsequent tasks that use the peaks that were detected here. 


            AddFeatureToResult(result, iso);


            var isoIsGood = (iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0);
            if (isoIsGood)
            {
                //GORD: check here if there is an error in IQ intensities
                result.IntensityAggregate = sumPeaks(iso, NumPeaksUsedInAbundance, 0);
            }
            else
            {
                result.FailedResult = true;
                result.FailureType = Globals.TargetedResultFailureType.MsfeatureNotFound;
            }

            resultList.IsosResultBin.Add(result);


        }

        private double sumPeaks(IsotopicProfile profile, int numPeaksUsedInAbundance, int defaultVal)
        {
            if (profile.Peaklist == null || profile.Peaklist.Count < numPeaksUsedInAbundance) return defaultVal;
            var peakListIntensities = new List<float>();
            foreach (var peak in profile.Peaklist)
            {
                peakListIntensities.Add(peak.Height);

            }
            peakListIntensities.Sort();
            peakListIntensities.Reverse();    // i know... this isn't the best way to do this!
            double summedIntensities = 0;

            for (var i = 0; i < peakListIntensities.Count; i++)
            {
                if (i < numPeaksUsedInAbundance)
                {
                    summedIntensities += peakListIntensities[i];
                }


            }

            return summedIntensities;

        }


        public virtual IsotopicProfile IterativelyFindMSFeature(XYData massSpecXyData, IsotopicProfile theorIso)
        {

            var peakList = new List<Peak>();
            return IterativelyFindMSFeature(massSpecXyData, theorIso, out peakList);

        }




        public virtual IsotopicProfile IterativelyFindMSFeature(XYData massSpecXyData, IsotopicProfile theorIso, out List<Peak> peakList)
        {
            if (massSpecXyData == null)
            {
                peakList = new List<Peak>();
                return null;
            }
                

            IsotopicProfile iso = null;

            MSPeakDetector.MinX = theorIso.MonoPeakMZ - 10;
            MSPeakDetector.MaxX = theorIso.MonoPeakMZ + 20;


            //start with high PeakBR and rachet it down, so as to detect more peaks with each pass.  Stop when you find the isotopic profile. 
            peakList = new List<Peak>();
            
            for (var d = PeakDetectorPeakBR; d >= PeakBRMin; d = d - PeakBRStep)
            {
                MSPeakDetector.PeakToBackgroundRatio = d;

                peakList=  MSPeakDetector.FindPeaks(massSpecXyData.Xvalues, massSpecXyData.Yvalues);
                iso = FindMSFeature(peakList, theorIso);

                bool isoIsGoodEnough;

                if (iso==null)
                {
                    isoIsGoodEnough = false;
                }
                else if (iso.Peaklist.Count<2)
                {
                    isoIsGoodEnough = false;
                }
                else
                {
                    double maxIntensity = iso.getMostIntensePeak().Height;
                    double minIntensityPeak = iso.Peaklist.Min(p => p.Height);

                    if (minIntensityPeak/maxIntensity< MinRelIntensityForPeakInclusion)
                    {
                        isoIsGoodEnough = true;
                    }
                    else
                    {
                        isoIsGoodEnough = false;
                    }

                }

                if (isoIsGoodEnough)
                {
                    break;
                }

            }

            return iso;

        }



        private void AddFeatureToResult(TargetedResultBase result, IsotopicProfile iso)
        {

            //TODO:   the IsotopicProfileType is wrong...

            switch (IsotopicProfileType)
            {
                case Globals.IsotopicProfileType.UNLABELLED:
                    result.IsotopicProfile = iso;
                    break;
                case Globals.IsotopicProfileType.LABELLED:
                    result.AddLabelledIso(iso);
                    break;
                default:
                    result.IsotopicProfile = iso;
                    break;
            }

            
        }

        #endregion


    }
}
