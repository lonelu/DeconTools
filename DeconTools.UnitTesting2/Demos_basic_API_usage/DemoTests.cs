using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;
using ThermoRawFileReader;

namespace DeconTools.UnitTesting2.Demos_basic_API_usage
{
    [TestFixture]
    public class DemoTests
    {
        [Test]
        public void createRunAndReadMSSpectraTest1()
        {
            //Create the run
            var runFactory = new RunFactory();
            var run = runFactory.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);


            //Create the task
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            //Create the target scan list (MS1 scans from 6005 - 6020).
            run.ScanSetCollection .Create(run, 6005, 6020, 1, 1);



            //iterate over each scan in the target collection get the mass spectrum
            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                Console.WriteLine("Working on Scan " + scan.PrimaryScanNumber);
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);

                Console.WriteLine("XYPair count = " + run.XYData.Xvalues.Length);
            }





        }


        [Test]
        public void getMSMSDataTest1()
        {
            //Create the run
            var runFactory = new RunFactory();
            var run = runFactory.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            //Create the task
            Task msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakdetector = new DeconToolsPeakDetectorV2();

            var startScan = 6000;
            var stopScan = 7000;

            for (var i = startScan; i < stopScan; i++)
            {
                if (run.GetMSLevel(i) == 2)
                {
                    var scanset = new ScanSet(i);
                    run.CurrentScanSet = scanset;
                    msgen.Execute(run.ResultCollection);
                    peakdetector.Execute(run.ResultCollection);

                    Console.Write("Working on Scan " + scanset.PrimaryScanNumber);
                    Console.WriteLine("; XYPair count = " + run.XYData.Xvalues.Length);

                    Console.WriteLine("num peaks= "+ run.PeakList.Count);
                }
            }
        }



        [Test]
        public void tempGetMSMSDataTest2()
        {
            var reader = new ThermoRawFileReader.XRawFileIO(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var startScan = 6000;
            var stopScan = 7000;

            for (var i = startScan; i < stopScan; i++)
            {
                ThermoRawFileReader.clsScanInfo scanInfo;
                var success = reader.GetScanInfo(i, out scanInfo);

                Console.Write("Working on Scan " + i);
                Console.WriteLine("; Scan info = " + scanInfo.ToString());

            }


        }

    }
}
