﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Algorithms.Quantifiers;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class O16O18TFFTask:TFFBase
    {
        O16O18FeatureFinder m_featureFinder = new O16O18FeatureFinder();
        BasicO16O18Quantifier m_quantifier = new BasicO16O18Quantifier();

        #region Constructors
        public O16O18TFFTask(double toleranceInPPM)
        {
            this.ToleranceInPPM = toleranceInPPM;

        }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        //public override void FindFeature(DeconTools.Backend.Core.ResultCollection resultColl)
        //{
            
            
        //    resultColl.ResultType = Globals.ResultType.O16O18_MASSTAG_RESULT;

        //    MassTagResultBase result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);
        //    if (result == null)
        //    {
        //        result = resultColl.CreateMassTagResult(resultColl.Run.CurrentMassTag);
        //    }

        //    if (result.ScanSet == null)
        //    {
        //        result.ScanSet = resultColl.Run.CurrentScanSet;

        //    }

        //    result.IsotopicProfile = m_featureFinder.FindFeature(resultColl.Run.PeakList, this.TheorFeature, this.ToleranceInPPM, this.NeedMonoIsotopicPeak);
        //    addInfoToResult(result);

            
        //}

        private void addInfoToResult(TargetedResultBase result)
        {
            if (result.IsotopicProfile != null)
            {
                result.IsotopicProfile.ChargeState = result.Target.ChargeState;
                result.IsotopicProfile.MonoIsotopicMass = (result.IsotopicProfile.GetMZ() - Globals.PROTON_MASS) * result.Target.ChargeState;
                result.IsotopicProfile.IntensityAggregate = result.IsotopicProfile.getMostIntensePeak().Height;     // may need to change this to sum the top n peaks. 
            }

            ((O16O18TargetedResultObject)result).RatioO16O18 = m_quantifier.GetAdjusted_I0_I4_YeoRatio(result.IsotopicProfile, result.Target.IsotopicProfile);

            //((O16O18_TResult)result).RatioO16O18 =  m_quantifier.Get_I0_I4_ratio(result.IsotopicProfile);
        }

       

        

        #endregion

        #region Private Methods
        #endregion
    }
}
