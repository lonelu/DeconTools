﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class SaturationIMSWorkflowTests
    {

        [Test]
        public void saturatedFixingTest1()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 4;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 3;
            parameters.ThrashParameters.MaxFit = 0.6;
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 180;
            parameters.MSGeneratorParameters.MaxLCScan = 183;
            parameters.MSGeneratorParameters.SumSpectraAcrossLC = true;
            parameters.MSGeneratorParameters.SumSpectraAcrossIms = true;
            parameters.MSGeneratorParameters.NumLCScansToSum = 1;
            parameters.MSGeneratorParameters.NumImsScansToSum = 3;
            parameters.MiscMSProcessingParameters.UseZeroFilling = true;
            parameters.ThrashParameters.MinIntensityForDeletion = 10;

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            int minFrame = parameters.MSGeneratorParameters.MinLCScan;
            int maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            int minScan = 102;
            int maxScan = 125;

            //this one elutes ScanLC at 180 - 195
            double targetMass = 860.3987;
            int chargestate = 2;

            //targetMass = 1444.748171;
            //chargestate = 3;

            //targetMass = 1079.559447;
            //chargestate = 2;

            //non-saturated feature:
            //targetMass = 1064.485;
            //chargestate = 2;

            //targetMass = 1224.5497;
            //chargestate = 1;


            //targetMass = 949.454723;
            //chargestate = 1;
            //minScan = 220;
            //maxScan = 228;

            double tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargestate
                 select n).Select<IsosResult, UIMFIsosResult>(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            MathUtils mathUtils = new MathUtils();

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }


        [Test]
        public void saturatedFixingTest2()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            parameters.LoadFromOldDeconToolsParameterFile(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR2_PeptideBR3_SN3_SumScans3_NoLCSum_Sat50000_2012-02-27_frames_180_195.xml");

            parameters.MSGeneratorParameters.UseLCScanRange= true;
            parameters.MSGeneratorParameters.MinLCScan = 375;
            parameters.MSGeneratorParameters.MaxLCScan = 420;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            //return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            int minFrame = parameters.MSGeneratorParameters.MinLCScan;
            int maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            int minScan = 102;
            int maxScan = 125;

            double targetMass = 819.48169;    //elutes at 199 - 205
            int chargestate = 2;

           

            double tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargestate
                 select n).Select<IsosResult, UIMFIsosResult>(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            MathUtils mathUtils = new MathUtils();

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }


        [Test]
        public void saturatedFixingTest_peaksNotExporting()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            parameters.LoadFromOldDeconToolsParameterFile(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR2_PeptideBR3_SN3_SumScans3_NoLCSum_Sat50000_2012-02-27_frames_180_195.xml");

            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 375;
            parameters.MSGeneratorParameters.MaxLCScan = 378;
            parameters.ScanBasedWorkflowParameters.ExportPeakData=true;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            //return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            int minFrame = parameters.MSGeneratorParameters.MinLCScan;
            int maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            int minScan = 102;
            int maxScan = 125;

            double targetMass = 819.48169;    //elutes at 199 - 205
            int chargestate = 2;



            double tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargestate
                 select n).Select<IsosResult, UIMFIsosResult>(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            MathUtils mathUtils = new MathUtils();

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }

        [Test]
        public void saturatedFixingTest3()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            parameters.LoadFromOldDeconToolsParameterFile(
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR2_PeptideBR3_SN3_SumScans3_NoLCSum_Sat50000_2012-02-27_frames_180_195.xml");

            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 394;
            parameters.MSGeneratorParameters.MaxLCScan = 404;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            //return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            int minFrame = parameters.MSGeneratorParameters.MinLCScan;
            int maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            int minScan = 102;
            int maxScan = 127;

            double targetMass = 1059.55169;
            int chargestate = 2;



            double tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargestate
                 select n).Select<IsosResult, UIMFIsosResult>(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            MathUtils mathUtils = new MathUtils();

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }


        [Test]
        public void saturatedFixing_RedmineIssue966()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            string parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_saturationRepair_Frame_1-500.xml";

            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);

            parameters=new DeconToolsParameters();
            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 4;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 3;
            parameters.ThrashParameters.MaxFit = 0.6;
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 320;
            parameters.MSGeneratorParameters.MaxLCScan = 328;
            parameters.MSGeneratorParameters.SumSpectraAcrossLC = true;
            parameters.MSGeneratorParameters.SumSpectraAcrossIms = true;
            parameters.MSGeneratorParameters.NumLCScansToSum = 1;
            parameters.MSGeneratorParameters.NumImsScansToSum = 3;
            parameters.MSGeneratorParameters.UseMZRange = false;
            parameters.MiscMSProcessingParameters.UseZeroFilling = true;
            parameters.ThrashParameters.MinIntensityForDeletion = 10;

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";
            parameters.Save(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_saturationRepair_Frame_1-500_copy.xml");

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            //return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            int minFrame = parameters.MSGeneratorParameters.MinLCScan;
            int maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            int minScan = 117;
            int maxScan = 131;

            int chargestate = 2;
            double targetMass = (605.82 - 1.00727649) * chargestate;

            double tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargestate
                 select n).Select<IsosResult, UIMFIsosResult>(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            MathUtils mathUtils = new MathUtils();

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }




        [Test]
        public void noFixingTest1()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();


            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 4;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 3;
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 180;
            parameters.MSGeneratorParameters.MaxLCScan = 180;
            parameters.MSGeneratorParameters.SumSpectraAcrossLC = true;
            parameters.MSGeneratorParameters.SumSpectraAcrossIms = true;
            parameters.MSGeneratorParameters.NumLCScansToSum = 1;
            parameters.MSGeneratorParameters.NumImsScansToSum = 3;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);

            workflow.Execute();

        }


        [Test]
        public void WorkflowTypeIsCorrectTest1()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName= "standard";
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            Assert.IsTrue(workflow is IMSScanBasedWorkflow);

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";
            workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            Assert.IsTrue(workflow is SaturationIMSScanBasedWorkflow);



        }


        [ExpectedException("System.ArgumentOutOfRangeException")]
        [Test]
        public void WorkflowTypeIsWrongTest1()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();


            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "incorrectTextProblem";
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);

        }




        private static void OutputFeatureIntensityData(List<UIMFIsosResult> featureData, int minFrame, int maxFrame, int maxScan, int minScan)
        {

            
                

            StringBuilder sb = new StringBuilder();
            for (int scan = minScan; scan <= maxScan; scan++)
            {
                for (int frame = minFrame; frame <= maxFrame; frame++)
                {
                    var feature =
                        (from n in featureData
                         where n.IMSScanSet.PrimaryScanNumber == scan && n.ScanSet.PrimaryScanNumber == frame
                         select n).FirstOrDefault();

                    double intensity;
                    if (feature == null)
                    {
                        intensity = 0;
                    }
                    else
                    {
                        intensity = feature.IsotopicProfile.OriginalIntensity;
                    }

                    sb.Append(intensity.ToString("0"));
                    sb.Append("\t");
                }

                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }


    }
}
