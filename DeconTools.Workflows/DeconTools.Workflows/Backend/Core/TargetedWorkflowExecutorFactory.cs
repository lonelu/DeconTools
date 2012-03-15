﻿
namespace DeconTools.Workflows.Backend.Core
{
    public class TargetedWorkflowExecutorFactory
    {

        public static TargetedWorkflowExecutor CreateTargetedWorkflowExecutor(WorkflowExecutorBaseParameters workflowParameters, string datasetPath)
        {
           

            switch (workflowParameters.WorkflowType)
            {
                case Globals.TargetedWorkflowTypes.BasicTargetedWorkflowExecutor1:
                    return new BasicTargetedWorkflowExecutor(workflowParameters, datasetPath);
                    break;
                case Globals.TargetedWorkflowTypes.LcmsFeatureTargetedWorkflowExecutor1:
                    return new LcmsFeatureTargetedWorkflowExecutor(workflowParameters, datasetPath);
                    break;
                case Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1:
                    return new SipperWorkflowExecutor(workflowParameters, datasetPath);
                default:
                    throw new System.ArgumentException("Workflow type: " + workflowParameters.WorkflowType +
                                                       " is not an executor type of workflow");
            }
        }

    }
}
