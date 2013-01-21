﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.LabeledIsotopicDistUtilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.TheoreticalIsotopicProfileTests
{
    [TestFixture]
    public class LabeledIsotopicProfileUtilitiesTests
    {
        [Test]
        public void GetMixedProfileTest_C13()
        {
            LabeledIsotopicProfileUtilities isoCreator = new LabeledIsotopicProfileUtilities();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";

            int lightIsotope = 12;
            int heavyIsotope = 13;

            double percentLabelled1 = 0;        // first peptide population is unlabelled (0%)
            double percentLabelled2 = 10;       // second peptide polulation has 8% of its carbons labelled. 
            double fractionPopulationLabelled = 0.50;     // fraction of peptides that have heavy label. 

            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled1);
            var labelledIso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled2);


            isoCreator.AddIsotopicProfile(iso, 1 - fractionPopulationLabelled, "unlabelled");
            isoCreator.AddIsotopicProfile(labelledIso, fractionPopulationLabelled, "labelled");

            var mixedIso = isoCreator.GetMixedIsotopicProfile();

            TestUtilities.DisplayIsotopicProfileData(mixedIso);
        }


        [Test]
        public void GetMixedN15_Test1()
        {
            LabeledIsotopicProfileUtilities isoCreator = new LabeledIsotopicProfileUtilities();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "N";

            int lightIsotope = 14;
            int heavyIsotope = 15;

            double percentLabelled1 = 0;        // first peptide population is unlabelled (0%)
            double percentLabelled2 = 20;       // second peptide polulation has 8% of its carbons labelled. 
            double fractionPopulationLabelled = 0.50;     // fraction of peptides that have heavy label. 

            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled1);
            var labelledIso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled2);


            isoCreator.AddIsotopicProfile(iso, 1 - fractionPopulationLabelled, "unlabelled");
            isoCreator.AddIsotopicProfile(labelledIso, fractionPopulationLabelled, "labelled");

            var mixedIso = isoCreator.GetMixedIsotopicProfile();

            TestUtilities.DisplayIsotopicProfileData(mixedIso);
        }


    }
}
