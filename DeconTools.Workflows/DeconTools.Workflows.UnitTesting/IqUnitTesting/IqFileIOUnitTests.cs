﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
	class IqFileIOUnitTests
	{
		[Test]
		public void IqExportResultsTest()
		{
			//Reference JIRA: https://jira.pnnl.gov/jira/browse/OMCS-832

			string testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
			string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
			string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

			Run run = RunUtilities.CreateAndAlignRun(testFile, peaksTestFile);

			TargetCollection mtc = new TargetCollection();
			MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
			mtc = mtimporter.Import();

			int testMassTagID = 24800;
			var oldStyleTarget = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

			TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
			parameters.ChromatogramCorrelationIsPerformed = true;
			parameters.ChromSmootherNumPointsInSmooth = 9;
			parameters.ChromPeakDetectorPeakBR = 1;
			parameters.ChromPeakDetectorSigNoise = 1;

			WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ResultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";

			IqWorkflow iqWorkflow = new BasicIqWorkflow(run, parameters);

			IqTarget target = new IqChargeStateTarget(iqWorkflow);

			target.ID = oldStyleTarget.ID;
			target.MZTheor = oldStyleTarget.MZ;
			target.ElutionTimeTheor = oldStyleTarget.NormalizedElutionTime;
			target.MonoMassTheor = oldStyleTarget.MonoIsotopicMass;
			target.EmpiricalFormula = oldStyleTarget.EmpiricalFormula;
			target.ChargeState = oldStyleTarget.ChargeState;

			var targets = new List<IqTarget>(1);
			targets.Add(target);

			var executor = new IqExecutor(executorBaseParameters);
			//executor.ChromSourceDataFilePath = peaksTestFile;
			executor.Execute(targets);

            Assert.IsTrue(File.Exists(@"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_iqResults.txt"));
            using (var reader = new StreamReader(@"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_iqResults.txt"))
			{
				string temp = reader.ReadLine();
				Assert.AreEqual(
					"ID	code\tEmpiricalFormula\tChargeState\tMonomassTheor\tMZTheor\tElutionTimeTheor\tMonoMassObs\tMZObs\tElutionTimeObs\tNumPeaksWithinTolerance\tScanset\tAbundance\tFitScore\tInterferenceScore",
					temp);
				temp = reader.ReadLine();
				Assert.AreEqual(
					"24800\t\tC64H110N18O19\t2\t1434.8193888\t718.41697089\t0.321639209985733\t1434.81096195126\t718.412757465632\t0.318046778440475\t0\t5942\t1352176\t0.0850492709063084\t0.0942918054560866",
					temp);
			}
		}

        [Test]
        public void ImportUnlabeledIqTargetsTest1()
        {
            string targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            LabelFreeIqTargetImporter importer = new LabelFreeIqTargetImporter(targetsFile);

            var targets=  importer.Import();
            Assert.IsNotNull(targets);
            Assert.IsTrue(targets.Any());

            Assert.IsTrue(targets.Count > 10);
            foreach (IqTarget iqTarget in targets.Take(10))
            {
                Console.WriteLine(iqTarget.ToString());
            }
        }

        [Test]
        public void ImportUnlabeledIqTargetsFromResultsFileTest1()
        {
            string targetsFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ForImportOnly_iqResults.txt";

            LabelFreeIqTargetImporter importer = new LabelFreeIqTargetImporter(targetsFile);

            var targets = importer.Import();
            Assert.IsNotNull(targets);
            Assert.IsTrue(targets.Any());

            Assert.IsTrue(targets.Count > 0);
            foreach (IqTarget iqTarget in targets.Take(10))
            {
                Console.WriteLine(iqTarget.ToString());
            }
        }


	}
}