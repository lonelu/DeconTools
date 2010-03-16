﻿using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters;

namespace DeconTools.Backend.Data
{
    public class ScansExporterFactory
    {
        DeconTools.Backend.Globals.ExporterType exporterType;

        public Task CreateScansExporter(Globals.MSFileType fileType, Globals.ExporterType exporterType, string outputFileName)
        {
            Task scansExporter;
            switch (fileType)
            {
                case Globals.MSFileType.PNNL_UIMF:

                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            scansExporter = new UIMFScanResult_TextFileExporter(outputFileName);
                            break;
                        case Globals.ExporterType.SQLite:
                            scansExporter = new UIMFScanResult_SqliteExporter(outputFileName);
                            break;
                        default:
                            scansExporter = new UIMFScanResult_TextFileExporter(outputFileName);
                            break;
                    }
                    break;
                default:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            scansExporter = new BasicScanResult_TextFileExporter(outputFileName);
                            break;
                        case Globals.ExporterType.SQLite:
                            scansExporter = new BasicScanResult_SqliteExporter(outputFileName);
                            break;
                        default:
                            scansExporter = new BasicScanResult_TextFileExporter(outputFileName);
                            break;
                    }
                    
                    break;
            }
            return scansExporter;

        }



    }
}