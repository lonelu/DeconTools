﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.Core;
using System.Diagnostics;
using DeconTools.Backend.Utilities;
using ProteinCalc;

namespace DeconTools.UnitTesting.UtilitiesTests.TomIsotopicDistributionTests
{
    [TestFixture]
    public class TomIsotopicDistributionTests
    {
        [Test]
        public void test1()
        {

            IsotopicProfile cluster = TomIsotopicPattern.GetAvnPattern(2000, false);


            Console.WriteLine(cluster.Peaklist.Count);

            StringBuilder sb = new StringBuilder();

            TestUtilities.ReportIsotopicProfileData(sb, cluster);


            Console.WriteLine(sb.ToString());
        }


        public void compareTomIsotopicDist_with_Mercury()
        {

            double mz = 1154.98841279744;    //mono MZ
            int chargestate = 2;
            double fwhm = 0.0290254950523376;   //from second peak of isotopic profile

            double monoMass = 1154.98841279744 * chargestate - chargestate * 1.00727649;
            double resolution = mz / fwhm;

            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(monoMass, chargestate, resolution);

            distcreator.getIsotopicProfile();
            Assert.AreEqual(8, distcreator.IsotopicProfile.GetNumOfIsotopesInProfile());

            StringBuilder sb = new StringBuilder();
            TestUtilities.ReportIsotopicProfileData(sb, distcreator.IsotopicProfile);

            IsotopicProfile cluster = TomIsotopicPattern.GetAvnPattern(monoMass, false);
            sb.Append(Environment.NewLine);
            TestUtilities.ReportIsotopicProfileData(sb, cluster);

            Console.Write(sb.ToString());

        }

        public void test2()
        {

            Peptide testPeptide = new Peptide("TTPSIIAYTDDETIVGQPAKR");
            IsotopicProfile cluster = TomIsotopicPattern.GetIsotopePattern(testPeptide.GetEmpiricalFormulaIntArray(), TomIsotopicPattern.aafIsos);
          
            TestUtilities.DisplayIsotopicProfileData(cluster);






        }



    }
}
