﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Utilities.Converters
{
    public static class PeakTypeConverter
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public static List<MSPeak> ConvertToMSPeaks(List<Peak> peakList)
        {
            var mspeakList = new List<MSPeak>();

            foreach (var peak in peakList)
            {
                if (peak is MSPeak)
                {
                    mspeakList.Add((MSPeak)peak);
                }

                
            }
            return mspeakList;

        }


        #endregion

        #region Private Methods
        #endregion
    }
}
