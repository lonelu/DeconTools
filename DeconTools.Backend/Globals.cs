﻿
namespace DeconTools.Backend
{
    public class Globals
    {

        public const double PROTON_MASS = 1.00727649;
        public const double MASS_DIFF_BETWEEN_ISOTOPICPEAKS = 1.00235;
        public const double N14_MASS = 14.003074007418;    // IUPAC, 2002
        public const double N15_MASS = 15.000108973000;




          
        public enum MSFileType
        {
            Undefined,
            Agilent_WIFF,
            Agilent_D,
            Ascii,
            Bruker,
            Bruker_Ascii,
            Finnigan,
            ICR2LS_Rawdata,
            Micromass_Rawdata,
            MZXML_Rawdata,
            PNNL_IMS,
            PNNL_UIMF,
            SUNEXTREL,
            YAFMS,
            Bruker_V2

        }

        public enum PeakFitType
        {
            Undefined,
            APEX,
            LORENTZIAN,
            QUADRATIC
        }

        public enum IsotopicProfileFitType
        {
            Undefined,
            AREA,
            CHISQ,
            PEAK
        }
        public enum DeconState
        {
            IDLE = 0,
            RUNNING_DECON,
            RUNNING_TIC,
            DONE,
            ERROR
        }

        public enum DeconvolutionType
        {
            THRASH,
            RAPID,
            THRASH_then_RAPID
        }


        public enum ExporterType
        {
            TEXT,
            SQLite
        }


        public enum PeakSelectorMode            //for selecting the best peak from a list of peaks
        {
            CLOSEST_TO_TARGET,
            MOST_INTENSE,
            INTELLIGENT_MODE,
            N15IntelligentMode
        }


        public enum MassTagDBImporterMode
        {
            Std_four_parameter_mode,
            List_of_MT_IDs_Mode

        }

        public enum MassTagResultType
        {
            BASIC_MASSTAG_RESULT,
            O16O18_MASSTAG_RESULT,
            N14N15_MASSTAG_RESULT
        }

        public enum ScanSelectionMode
        {
            ASCENDING,
            DESCENDING,
            CLOSEST
        }


        public enum ProjectControllerType
        {
            UNDEFINED, 
            STANDARD,
            BONES_CONTROLLER,
            RUN_MERGER_CONTROLLER,
            KOREA_IMS_PEAKSONLY_CONTROLLER,
            UIMF_MS_Only_TestController
        }

        public enum LabellingType
        {
            NONE,
            O18,
            N15
        }

        public enum TargetedResultFailureType
        {
            NONE,
            CHROM_XYDATA_NOT_FOUND,
            CHROM_PEAKS_NOT_DETECTED,
            CHROMPEAK_NOT_FOUND_WITHIN_TOLERANCES,
            MSPEAKS_NOT_DETECTED,
            MSFEATURE_NOT_FOUND

        }

        public enum TargetedFeatureFinderType
        {
            BASIC,
            ITERATIVE
        }






    }
}
