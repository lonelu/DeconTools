﻿using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Algorithms
{
    public class BasicMSFeatureFinder
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public IsotopicProfile FindMSFeature(List<Peak> peakList, IsotopicProfile theorFeature, double toleranceInPPM, bool requireMonoPeak)
        {
            return null;



        }
        [Obsolete("Unused")]
        private Peak findMostIntensePeak(IReadOnlyList<Peak> peaksWithinTol)
        {
            double maxIntensity = 0;
            Peak mostIntensePeak = null;

            for (var i = 0; i < peaksWithinTol.Count; i++)
            {
                var obsIntensity = peaksWithinTol[i].Height;
                if (obsIntensity > maxIntensity)
                {
                    maxIntensity = obsIntensity;
                    mostIntensePeak = peaksWithinTol[i];
                }
            }
            return mostIntensePeak;
        }

        [Obsolete("Unused")]
        private Peak findClosestToXValue(IReadOnlyList<Peak> peaksWithinTol, double targetVal)
        {
            var diff = double.MaxValue;
            Peak closestPeak = null;

            for (var i = 0; i < peaksWithinTol.Count; i++)
            {

                var obsDiff = Math.Abs(peaksWithinTol[i].XValue - targetVal);

                if (obsDiff < diff)
                {
                    diff = obsDiff;
                    closestPeak = peaksWithinTol[i];
                }

            }

            return closestPeak;
        }




        #endregion

        #region Private Methods
        #endregion
    }
}
