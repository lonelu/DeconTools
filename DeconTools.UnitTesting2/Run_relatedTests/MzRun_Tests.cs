﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class MzRun_Tests
    {
        [Test]
        public void constructorTest1()
        {
            string testfile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            Run run = new MzRun(testfile);
            Assert.AreEqual(Backend.Globals.MSFileType.MZXML_Rawdata, run.MSFileType);
            Assert.AreEqual(18505, run.GetNumMSScans());

            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(18504, run.MaxScan);

            Assert.AreEqual("QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18", run.DatasetName);
            Assert.AreEqual(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML", run.DataSetPath);

        }


        [Test]
        public void GetMassSpectrumTest1()
        {
            int testscan = 6004;

            string testfile =
               @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            Run run = new MzRun(testfile);

            ScanSet scanSet = new ScanSet(testscan);
            run.GetMassSpectrum(scanSet);

            Assert.AreEqual(481.274514196002, (decimal)run.XYData.Xvalues[3769]);
            Assert.AreEqual(13084442, run.XYData.Yvalues[3769]);

            //TestUtilities.DisplayXYValues(run.XYData);
        }

        [Test]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void GetMassSpectrum_higherThanTotalScans()
        {
            int testscan = 18506;

            string testfile =
               @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            Run run = new MzRun(testfile);

            ScanSet scanSet = new ScanSet(testscan);
            run.GetMassSpectrum(scanSet);
        }

        [Test]
        public void GetMSLevelsTest1()
        {

            int testscan = 6004;
            int testscan2 = 6005;

            string testfile =
               @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            Run run = new MzRun(testfile);

            int level=  run.GetMSLevel(testscan);

            var scanTime = run.GetTime(6004);


            Assert.AreEqual(1, run.GetMSLevel(6004));
            Assert.AreEqual(2, run.GetMSLevel(6005));

            Assert.AreEqual(1961.65, (decimal)run.GetTime(6004));

        }


        [Test]
        public void GetScanInfoTest1()
        {
            string testfile =
             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mz5";

            Run run = new MzRun(testfile);

            int ms2scan = 6005;

            string info= run.GetScanInfo(ms2scan);

            Console.WriteLine(info);

        }



        [Test]
        public void Speedtest1()
        {
            //string testfile =
            //  @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            string testfile2 =
             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mz5";


            string testfile3 =
            @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzML";

            Run run = new MzRun(testfile2);

            Stopwatch stopwatch = new Stopwatch();

            int startScan = 5500;
            int stopScan = 6500;

            ScanSet scanSet = new ScanSet(startScan);
            List<long> times = new List<long>();

            run.GetMSLevel(startScan);


            for (int i = startScan; i < stopScan; i++)
            {
                scanSet = new ScanSet(i);

                if (run.GetMSLevel(i) == 2) continue;
                

                stopwatch.Start();
                run.GetMassSpectrum(scanSet);
                stopwatch.Stop();

                
                times.Add(stopwatch.ElapsedMilliseconds);

                stopwatch.Reset();
            }


            //foreach (var time in times)
            //{
            //    Console.WriteLine(time);
            //}

            Console.WriteLine("Average = " + times.Average());






        }


    }
}