﻿using System;
using System.IO;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class TargetedWorkflowExecutorTests
    {
        [Test]
        public void targetedWorkflow_alignUsingDataFromFiles()
        {
    
            string executorParameterFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QCShew_OrbiStandard_workflowExecutorParameters.xml";
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            string resultsFolderLocation = executorParameters.ResultsFolder;
            string testDatasetPath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            string testDatasetName = "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";

            string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

           

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
            Backend.Results.TargetedResultRepository repository= importer.Import();

            Assert.AreEqual(10, repository.Results.Count);

            TargetedResultDTO result1 = repository.Results[2];

            Assert.AreEqual(24702, result1.TargetID);
            Assert.AreEqual(3, result1.ChargeState);
            Assert.AreEqual(8119, result1.ScanLC);
            Assert.AreEqual(0.41724m, (decimal)Math.Round(result1.NET, 5));
            Assert.AreEqual(0.002534m, (decimal)Math.Round(result1.NETError, 6));
            Assert.AreEqual(2920.53082m, (decimal)Math.Round(result1.MonoMass, 5));
            Assert.AreEqual(2920.53733m, (decimal)Math.Round(result1.MonoMassCalibrated, 5));
            Assert.AreEqual(-1.83m, (decimal)Math.Round(result1.MassErrorInPPM, 2));
       

                //Dataset	MassTagID	ChargeState	Scan	ScanStart	ScanEnd	NET	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoMZ	IntensityRep	FitScore	IScore	FailureType
            
                //QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18	24702	3	8119	8112	8124	0.4172	2	1	2920.53082	974.51755	1379489	0.1136	0.0000	

        }


        [Test]
        public void targetedWorkflow_noAlignment()
        {
            string executorParameterFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QCShew_OrbiStandard_workflowExecutorParameters.xml";
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);




            string resultsFolderLocation = executorParameters.ResultsFolder;
            string testDatasetPath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            string testDatasetName = "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";

            string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }


            FileInfo rawFileInfo = new FileInfo(testDatasetPath);
            executorParameters.AlignmentInfoFolder = rawFileInfo.DirectoryName;


            //delete alignment files
            string mzalignmentFile = rawFileInfo.DirectoryName + "\\" + testDatasetName + "_mzAlignment.txt";
            string netAlignmentFile = rawFileInfo.DirectoryName + "\\" + testDatasetName + "_netAlignment.txt";
            if (File.Exists(mzalignmentFile)) File.Delete(mzalignmentFile);
            if (File.Exists(netAlignmentFile)) File.Delete(netAlignmentFile);


            executorParameters.TargetedAlignmentIsPerformed = false;    //no targeted alignment

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
            Backend.Results.TargetedResultRepository repository = importer.Import();

            Assert.AreEqual(10, repository.Results.Count);

            TargetedResultDTO result1 = repository.Results[2];


            Assert.AreEqual(24702, result1.TargetID);
            Assert.AreEqual(3, result1.ChargeState);
            Assert.AreEqual(8119, result1.ScanLC);
            Assert.AreEqual(0.41724m, (decimal)Math.Round(result1.NET, 5));
            Assert.AreEqual(0.002534m, (decimal)Math.Round(result1.NETError, 6));
            Assert.AreEqual(2920.53082m, (decimal)Math.Round(result1.MonoMass, 5));
            Assert.AreEqual(2920.53733m, (decimal)Math.Round(result1.MonoMassCalibrated, 5));
            Assert.AreEqual(-1.83m, (decimal)Math.Round(result1.MassErrorInPPM, 2));
       

        }



        [Test]
        public void targetedWorkflow_withTargetedAlignment_test()
        {

            string executorParameterFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QCShew_OrbiStandard_workflowExecutorParameters.xml";
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            string resultsFolderLocation = executorParameters.ResultsFolder;
            string testDatasetPath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            string testDatasetName = "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";

            string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }


            FileInfo rawFileInfo = new FileInfo(testDatasetPath);
            executorParameters.AlignmentInfoFolder = rawFileInfo.DirectoryName;

            string mzalignmentFile = rawFileInfo.DirectoryName +"\\" + testDatasetName + "_mzAlignment.txt";
            string netAlignmentFile = rawFileInfo.DirectoryName + "\\" + testDatasetName + "_netAlignment.txt";
            if (File.Exists(mzalignmentFile)) File.Delete(mzalignmentFile);
            if (File.Exists(netAlignmentFile)) File.Delete(netAlignmentFile);




            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
            Backend.Results.TargetedResultRepository repository = importer.Import();

            Assert.AreEqual(10, repository.Results.Count);

            TargetedResultDTO result1 = repository.Results[2];


            Assert.AreEqual(24702, result1.TargetID);
            Assert.AreEqual(3, result1.ChargeState);
            Assert.AreEqual(8119, result1.ScanLC);
            Assert.AreEqual(0.41724m, (decimal)Math.Round(result1.NET, 5));
            Assert.AreEqual(0.002534m, (decimal)Math.Round(result1.NETError, 6));
            Assert.AreEqual(2920.53082m, (decimal)Math.Round(result1.MonoMass, 5));
            Assert.AreEqual(2920.53879m, (decimal)Math.Round(result1.MonoMassCalibrated, 5));
            Assert.AreEqual(-2.33m, (decimal)Math.Round(result1.MassErrorInPPM, 2));

            //Dataset	MassTagID	ChargeState	Scan	ScanStart	ScanEnd	NET	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoMZ	IntensityRep	FitScore	IScore	FailureType

            //QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18	24702	3	8119	8112	8124	0.4172	2	1	2920.53082	974.51755	1379489	0.1136	0.0000	

        }



        [Test]
        public void copyToLocalTest1()
        {

            string executorParameterFile = @"\\protoapps\UserData\Slysz\Data\WorkflowExecutor_Parameters\basicTargetedWorkflowExecutorParameters_CopyToLocalTestCase2.xml";
            string resultsFolderLocation = @"D:\Temp\results";
            string datasetPath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            string testDatasetName = "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";

            string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
            Backend.Results.TargetedResultRepository repository = importer.Import();

            Assert.AreEqual(10, repository.Results.Count);

            TargetedResultDTO result1 = repository.Results[0];

            Assert.AreEqual(24702, result1.TargetID);
            Assert.AreEqual(3, result1.ChargeState);
            Assert.AreEqual(8119, result1.ScanLC);



            //Dataset	MassTagID	ChargeState	Scan	ScanStart	ScanEnd	NET	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoMZ	IntensityRep	FitScore	IScore	FailureType

            //QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18	24702	3	8119	8112	8124	0.4172	2	1	2920.53082	974.51755	1379489	0.1136	0.0000	

        }


    }
}