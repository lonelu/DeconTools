# DeconTools

DeconTools is a software package used for detecting features in MS data by using the isotopic signatures of
expected components. The tool uses different algorithms for different parts of the deconvolution process: noise
reduction, peak detection, prediction of theoretical isotopic envelope and scoring functions that quantitate
the quality of signature observed in the data.

## Installation

Download the DeconTools installer from https://github.com/PNNL-Comp-Mass-Spec/DeconTools/releases \
Preferably use the 64-bit installer: [DeconTools_Installer_x64.exe](https://github.com/PNNL-Comp-Mass-Spec/DeconTools/releases/download/v1.1.6397/DeconTools_Installer_x64.exe)

### Requirements

You need either Xcalibur or MSFileReader to process Thermo .raw files.  Obtain MS File Reader from Thermo using these steps:
* Create an account at https://thermo.flexnetoperations.com/control/thmo/login
* Login and choose `Utility Software`
* Click `MS File Reader 3.1 SP2` then download `MSFileReader_x64.exe`
  * SP4 and SP3 are also available and may work, but we have had some systems where SP2 works while SP4 and SP3 do not work
* Run the installer and install both the 32-bit and 64-bit versions.

DeconTools can read the following file formats.  However, some of the file formats require that you 
install the vendor-supplied data browsing software so that DeconTools has access to the necessary DLLs.
* mzXML files   (Requires ProteoWizard installation)
* mzML files    (Requires ProteoWizard installation)
* mz5 files     (Requires ProteoWizard installation)
* Thermo Finnigan .Raw files (requires Xcalibur or MSFileReader)
* Agilent .Wiff files (requires Analyst)
* Agilent .D folders (requires Chemstation)
* Micromass files (requires MassLynx)
* Bruker acqu files
* IMS .IMF and .UIMF files

Download ProteoWizard from http://proteowizard.sourceforge.net/
You typically will need the 64-bit version.

## Using DeconTools

DeconTools has both a GUI version (DeconToolsAutoProcessor) and a command-line version (DeconConsole).

### DeconConsole Syntax

```DeconConsole.exe DataFilePath ParameterFilePath [OutputDirectoryPath]```

The first argument is the data file to process (typically .Raw or .mzML)

The second argument is the DeconTools parameter file.
* Example parameter files are available at https://github.com/PNNL-Comp-Mass-Spec/DeconTools/tree/master/Parameter_Files

The third argument is optional.  If not defined, the output files are created in the same directory as the input files.

#### Example usage:

```DeconConsole.exe QCDataset.raw SampleParameterFile.xml```


## Results files

For each dataset processed, DeconTools creates a pair of .CSV (comma-separated value) files 
containing information on the spectra in the input files and the deisotoped data found.

The _scans.csv file contains information about each mass spectrum (aka scan); columns are:

Column          | Description
--------------- | -------------
scan_num        | The scan number, aka spectrum number
scan_time       | The number of minutes from the start of the analysis
type            | 1 for MS spectra, 2 for MS/MS spectra
bpi             | The Base Peak Intensity value
bpi_mz          | The m/z of the Base Peak ion
tic             | The Total Ion Current value
num_peaks       | The number of data points in the spectrum above the background noise level
num_deisotoped  | The number of peaks that were successfully deisotoped
info            | For Thermo Raw files, the scan header

The _isos.csv file contains the deisotoped data; columns are:

Column                | Description
--------------------- | -------------
scan_num              | The spectrum number containing the data point
charge                | The charge determined via the deisotoping process
abundance             | The data point's abundance
mz                    | The Mass-to-Charge Ratio for the data point
fit                   | The Isotopic fit value: the least square error between the theoretical data and the experimental data. Values closer to 0 are better.  Values larger than ~0.15 are typically low quality results.
average_mw            | The Average Mass determined
monoisotopic_mw       | The Monoisotopic Mass determined
mostabundant_mw       | The mass of the most abundant ion in the isotopic distribution
fwhm                  | Full width at half maximum
signal_noise          | Signal to noise ratio
mono_abundance        | For 16O/18O processing, the abundance of the 16O peak
mono_plus2_abundance  | For 16O/18O processing, the abundance of the peak 2 Da away from the 16O peak
flag                  | Specialized column for ion mobility (IMS) data
interference_score    | Measures the likelihood that another isotopic distribution is overlapping with the given distribution (0 means no interference)


## Contacts

Written by Gordon Slysz for the Department of Energy (PNNL, Richland, WA)
E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov \
Website: https://omics.pnl.gov or https://panomics.pnl.gov/

## License

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0
