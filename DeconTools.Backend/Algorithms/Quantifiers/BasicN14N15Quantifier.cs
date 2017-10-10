﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Algorithms.Quantifiers
{

    public enum RatioType
    {
        ISO1_OVER_ISO2,
        ISO2_OVER_ISO1

    }

    public class BasicN14N15Quantifier : N14N15Quantifier
    {
        private double MSToleranceInPPM;


        #region Constructors

        public BasicN14N15Quantifier(double msToleranceInPPM)
        {
            // TODO: Complete member initialization
            this.MSToleranceInPPM = msToleranceInPPM;
            this.RatioType = Quantifiers.RatioType.ISO1_OVER_ISO2;
        }

        public BasicN14N15Quantifier(double msToleranceInPPM, RatioType ratioType)
            : this(msToleranceInPPM)
        {
            this.RatioType = ratioType;
        }



        #endregion

        #region Properties

        public RatioType RatioType { get; set; }


        #endregion

        #region Public Methods
        public override double GetRatio(double[] xvals, double[] yvals,
       DeconTools.Backend.Core.IsotopicProfile iso1, DeconTools.Backend.Core.IsotopicProfile iso2,
       double backgroundIntensity)
        {
            double returnVal = -1;
            returnVal = GetRatioBasedOnAreaUnderPeaks(xvals, yvals, iso1, iso2, backgroundIntensity);
            return returnVal;
        }


        public void GetRatioBasedOnTopPeaks(IsotopicProfile iso1, IsotopicProfile iso2,
                    IsotopicProfile theorIso1, IsotopicProfile theorIso2, double backgroundIntensity, int numPeaks,
                    out double ratio, out double ratioContribForIso1, out double ratioContribForIso2)
        {


            //Find top n peaks of the theor profile.  Reference them by their peak index so that they can be looked up in the experimental. 
            //List<int> sortedTheorIso1Indexes = getIndexValsOfTopPeaks(theorIso1);
            //List<int> sortedTheorIso2Indexes = getIndexValsOfTopPeaks(theorIso2);

            IList<Peak> topTheorIso1Peaks = getTopPeaks(theorIso1, numPeaks);
            IList<Peak> topTheorIso2Peaks = getTopPeaks(theorIso2, numPeaks);



            //get the top n peaks of the experimental profile, based on peaks of the theor profile
            //adjust intensity (subtract background intensity)
            var topIso1Peaks = getTopPeaksBasedOnTheor(iso1, topTheorIso1Peaks, MSToleranceInPPM);
            var topIso2Peaks = getTopPeaksBasedOnTheor(iso2, topTheorIso2Peaks, MSToleranceInPPM);

            //Since the number of top experimental iso peaks may be less than the number of top theor iso peaks,
            //we have to filter and ensure that they have the same peak numbers, so that the correction factor
            // (applied below) is properly applied.   HOWEVER,  this may induce some differences between runs 
            var filteredTopTheorIso1Peaks = getTopPeaksBasedOnTheor(theorIso1, topIso1Peaks, MSToleranceInPPM);
            var filteredTopTheorIso2Peaks = getTopPeaksBasedOnTheor(theorIso2, topIso2Peaks, MSToleranceInPPM);

            var summedTopIso1PeakIntensities = PeakUtilities.GetSumOfIntensities(topIso1Peaks, backgroundIntensity);
            var summedTopIso2PeakIntensities = PeakUtilities.GetSumOfIntensities(topIso2Peaks, backgroundIntensity);

            //Console.WriteLine(backgroundIntensity);

            //need to find out the contribution of the top n peaks to the overall isotopic envelope.  Base this on the theor profile
            double summedTheorIso1Intensities = filteredTopTheorIso1Peaks.Sum(p => (p.Height));
            double summedTheorIso2Intensities = filteredTopTheorIso2Peaks.Sum(p => (p.Height));


            var fractionTheor1 = summedTheorIso1Intensities / theorIso1.Peaklist.Sum(p => p.Height);
            var fractionTheor2 = summedTheorIso2Intensities / theorIso2.Peaklist.Sum(p => p.Height);

            //use the above ratio to correct the intensities based on how much the top n peaks contribute to the profile. 
            var correctedIso1SummedIntens = summedTopIso1PeakIntensities / fractionTheor1;
            var correctedIso2SummedIntens = summedTopIso2PeakIntensities / fractionTheor2;

            var summedAllIsos1PeakIntensities = iso1.Peaklist.Sum(p => (p.Height - backgroundIntensity));
            var summedAllIsos2PeakIntensities = iso2.Peaklist.Sum(p => (p.Height - backgroundIntensity));


            switch (this.RatioType)
            {
                case RatioType.ISO1_OVER_ISO2:
                    ratio = correctedIso1SummedIntens / correctedIso2SummedIntens;
                    break;
                case RatioType.ISO2_OVER_ISO1:
                    ratio = correctedIso2SummedIntens/ correctedIso1SummedIntens ;
                    break;
                default:
                    ratio = correctedIso1SummedIntens / correctedIso2SummedIntens;
                    break;
            }


            ratioContribForIso1 = summedTopIso1PeakIntensities / summedAllIsos1PeakIntensities * 1 / fractionTheor1;   //we expect a value of '1'
            ratioContribForIso2 = summedTopIso2PeakIntensities / summedAllIsos2PeakIntensities * 1 / fractionTheor2;




        }

        public double GetRatioBasedOnAreaUnderPeaks(double[] xvals, double[] yvals,
    DeconTools.Backend.Core.IsotopicProfile iso1, DeconTools.Backend.Core.IsotopicProfile iso2,
    double backgroundIntensity)
        {
            double returnRatio = -1;

            //define starting m/z value based on peak m/z and width
            var leftMostMZ = iso1.Peaklist[0].XValue - 1;  //    4σ = peak width at base;  '-1' ensures we are starting outside the peak. 

            //find starting point (use binary search)
            var indexOfStartingPoint = Utilities.MathUtils.BinarySearchWithTolerance(xvals, leftMostMZ, 0, xvals.Length - 1, 0.1);
            if (indexOfStartingPoint == -1) indexOfStartingPoint = 0;


            var area1 = IsotopicProfileUtilities.CalculateAreaOfProfile(iso1, xvals, yvals, backgroundIntensity, ref indexOfStartingPoint);
            var area2 = IsotopicProfileUtilities.CalculateAreaOfProfile(iso2, xvals, yvals, backgroundIntensity, ref indexOfStartingPoint);

            if (area1 < 0) area1 = 0;


            if (area2 > 0)
            {
                returnRatio = area1 / area2;
            }
            else
            {
                returnRatio = 9999;    //  this will indicate the problem of the second isotope having a 0 or negative area. 
            }

            return returnRatio;









            //iterate over peaks from isotopic profile 1


            //iterate over raw m/z values. 

            //If within peak m/z range, calculate area (trapazoid)


            //repeat the above with isotopic profile 2



        }

        public double GetRatioBasedOnMostIntensePeak(DeconTools.Backend.Core.IsotopicProfile iso1, DeconTools.Backend.Core.IsotopicProfile iso2)
        {
            double returnVal = -1;

            if (iso1 == null || iso2 == null) return returnVal;

            double iso1MaxIntens = iso1.getMostIntensePeak().Height;
            double iso2MaxIntens = iso2.getMostIntensePeak().Height;

            return returnVal;

        }
        #endregion

        #region Private Methods
        private IList<Peak> getTopPeaksBasedOnTheor(IsotopicProfile iso1, IList<Peak> topTheorIso1Peaks, double toleranceInPPM)
        {
            var topPeaks = new List<Peak>();

            foreach (var item in topTheorIso1Peaks)
            {
                var mzTolerance = toleranceInPPM * item.XValue / 1e6;

                Peak isoPeak = IsotopicProfileUtilities.GetPeakAtGivenMZ(iso1, item.XValue, mzTolerance);

                if (isoPeak != null)
                {
                    topPeaks.Add(isoPeak);
                }
            }

            return topPeaks;
        }

        private List<Peak> getTopPeaks(IsotopicProfile iso, int numPeaks)
        {
            var sortedList = new List<Peak>();

            //add peaks to new List so we don't mess up the old one
            for (var i = 0; i < iso.Peaklist.Count; i++)
            {
                sortedList.Add(iso.Peaklist[i]);
            }


            sortedList.Sort(delegate(Peak peak1, Peak peak2)
            {
                return peak2.Height.CompareTo(peak1.Height);
            }
            );


            //sortedList.Reverse();

            //take top n peaks
            sortedList = sortedList.Take(numPeaks).ToList();

            return sortedList;



        }

        [Obsolete("Unused")]
        private double getSummedIntensitiesForPeakIndices(IsotopicProfile iso, IReadOnlyList<int> sortedIndexes, int numPeaks)
        {
            double summedIntensities = 0;

            for (var i = 0; i < numPeaks; i++)
            {
                var targetIndexVal = sortedIndexes[i];
                if (iso.Peaklist.Count > targetIndexVal)
                {
                    summedIntensities += iso.Peaklist[targetIndexVal].Height;
                }
            }
            return summedIntensities;
        }

        [Obsolete("Unused")]
        private List<int> getIndexValsOfTopPeaks(IsotopicProfile theorIso1)
        {
            var indexValsOfTopPeaks = new List<int>();

            indexValsOfTopPeaks.Add(0);
            if (theorIso1.Peaklist.Count > 1)
            {
                for (var i = 1; i < theorIso1.Peaklist.Count; i++)
                {
                    if (theorIso1.Peaklist[i].Height > theorIso1.Peaklist[indexValsOfTopPeaks[0]].Height)
                    {
                        indexValsOfTopPeaks.Insert(0, i);
                    }
                    else
                    {
                        indexValsOfTopPeaks.Add(i);
                    }

                }

            }
            return indexValsOfTopPeaks;

        }
        #endregion









    }
}
