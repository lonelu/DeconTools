﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MSFeatureToTextFileExporterIMF:TextFileExporter<IsosResult>
    {
        #region Constructors
        public MSFeatureToTextFileExporterIMF(string fileName)
            : this(fileName, ',')
        {

        }

        public MSFeatureToTextFileExporterIMF(string fileName, char delimiter)
        {
            this.Name = "MSFeatureToTextFileExporterIMF";
            this.Delimiter = delimiter;
            this.FileName = fileName;

            initializeAndWriteHeader();

        }
        #endregion

 

        #region Private Methods
        protected override string buildResultOutput(IsosResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(result.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.ChargeState);
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetAbundance());
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetMZ().ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.Score.ToString("0.####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.AverageMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetFWHM().ToString("0.####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetMonoAbundance());
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetMonoPlusTwoAbundance());
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.OriginalIntensity);
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.Original_Total_isotopic_abundance);
            sb.Append(Delimiter);
            sb.Append(DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("scan_num");
            sb.Append(Delimiter);
            sb.Append("charge");
            sb.Append(Delimiter);
            sb.Append("abundance");
            sb.Append(Delimiter);
            sb.Append("mz");
            sb.Append(Delimiter);
            sb.Append("fit");
            sb.Append(Delimiter);
            sb.Append("average_mw");
            sb.Append(Delimiter);
            sb.Append("monoisotopic_mw");
            sb.Append(Delimiter);
            sb.Append("mostabundant_mw");
            sb.Append(Delimiter);
            sb.Append("fwhm");
            sb.Append(Delimiter);
            sb.Append("signal_noise");
            sb.Append(Delimiter);
            sb.Append("mono_abundance");
            sb.Append(Delimiter);
            sb.Append("mono_plus2_abundance");
            sb.Append(Delimiter);
            sb.Append("orig_intensity");
            sb.Append(Delimiter);
            sb.Append("TIA_orig_intensity");
            sb.Append(Delimiter);
            sb.Append("flag");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
        #endregion

  
    }
}
