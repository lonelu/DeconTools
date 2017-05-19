﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSPeakDetectionTests
{
    [TestFixture]
    public class MSPeakDetectionTests
    {

        [Test]
        public void PeakDetectorDemo1()
        {
            //In this list on Scan 6005 we can see what the peak of the parent is. Slide 4 1059.45898 is the Monoisotopic peak of Scan 6009
            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var testScan = new ScanSet(6009);

            run.CurrentScanSet = testScan;

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDetector = new DeconToolsPeakDetectorV2();

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            var sb = new StringBuilder();
            foreach (var peak in run.PeakList)
            {
                sb.Append(peak.XValue + "\t" + peak.Height + "\t" + peak.Width + "\n");
            }

            Console.WriteLine("----------------------PeakList---------------");
            Console.WriteLine("m/z\tintensity\twidth");
            Console.WriteLine(sb.ToString());
        }



        [Test]
        public void DetectPeaksTest1()
        {
            var peakBR = 1.3;
            double sigNoise = 2;
            var isThresholded = true;
            var peakfitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;

            var testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun2(testFile);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            run.CurrentScanSet = new ScanSet(6005);

            msgen.Execute(run.ResultCollection);

            var peakDet = new DeconToolsPeakDetectorV2(peakBR, sigNoise, peakfitType, isThresholded);
            peakDet.PeaksAreStored = true;

            var peakList=  peakDet.FindPeaks(run.XYData, 0, 50000);

            TestUtilities.DisplayPeaks(peakList);

        }




        [Test]
        public void DetectPeaksInOrbitrapData()
        {
            var peakBR = 1.3;
            double sigNoise = 2;
            var isThresholded = true;
            var peakfitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;

            var testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun2(testFile);

            //create list of target scansets
            run.ScanSetCollection .Create(run, 6000, 6015, 1, 1);


            //in the 'run' object there is now a list of scans : run.ScanSetCollection
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            var peakDet = new DeconToolsPeakDetectorV2(peakBR, sigNoise, peakfitType, isThresholded);
            peakDet.PeaksAreStored = true;

            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                //set the target scan:
                run.CurrentScanSet = scan;

                msgen.Execute(run.ResultCollection);
                peakDet.Execute(run.ResultCollection);




            }

        }


    }
}
