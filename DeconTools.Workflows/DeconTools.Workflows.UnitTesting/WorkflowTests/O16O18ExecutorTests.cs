﻿using System.IO;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class O16O18ExecutorTests
    {





        [Test]
        public void Test1()
        {
            string executorParameterFile =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\Workflow_Parameters\workflow_executor.xml";
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            executorParameters.CopyRawFileLocal = false;


            string resultsFolderLocation = executorParameters.ResultsFolder;
            string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\RawData\Alz_P01_D12_144_26Apr12_Roc_12-03-18.RAW";
            string testDatasetName = Path.GetFileName(testDatasetPath).Replace(".RAW", "");

            string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }



            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

        }


        [Test]
        public void TestErnestosData1()
        {
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsBaseFolder = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\O16O18_Targeted\2012_06_27_Ernesto";
            executorParameters.WorkflowParameterFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\O16O18_Targeted\2012_06_27_Ernesto\O16O18WorkflowParameters_2011_08_23_sum5.xml";

            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            

            string testDatasetPath = @"\\protoapps\UserData\Slysz\Data\O16O18\Ernesto\PSI_URW_1to1_01A_18Jun12_Falcon_12-03-37.RAW";

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();


        }




        [Test]
        public void Test2()
        {
            string executorParameterFile =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\Workflow_Parameters\workflow_executor - copy.xml";
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            string resultsFolderLocation = executorParameters.ResultsFolder;
            string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\RawData\Alz_P01_D12_144_26Apr12_Roc_12-03-18.RAW";
            string testDatasetName = Path.GetFileName(testDatasetPath).Replace(".RAW", "");

            string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }



            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

        }


    }
}