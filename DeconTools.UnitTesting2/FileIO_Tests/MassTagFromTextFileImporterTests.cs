﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.FileIO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MassTagFromTextFileImporterTests
    {

        string massTagTestFile1 = "..\\..\\..\\TestFiles\\FileIOTests\\top40MassTags.txt";
        string massTagTestFile2 = "..\\..\\..\\TestFiles\\FileIOTests\\importedMassTagsFormat2.txt";
        private string massTagsWithModsFile1 = "..\\..\\..\\TestFiles\\FileIOTests\\massTagsWithModsSample.txt";

        [Test]
        public void test1()
        {
            TargetCollection mtc = new TargetCollection();

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagTestFile1);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            Assert.AreEqual(101, mtc.TargetList.Count);

            PeptideTarget testMassTag = (PeptideTarget)mtc.TargetList[0];


            Assert.AreEqual("AVAFGEALRPEFK", testMassTag.Code);
            Assert.AreEqual(2, testMassTag.ChargeState);
            Assert.AreEqual(0.3649905m, (decimal)testMassTag.NormalizedElutionTime);
            Assert.AreEqual("C67H103N17O18", testMassTag.EmpiricalFormula);
            Assert.AreEqual(872, testMassTag.RefID);
            Assert.AreEqual("ABA80002 SHMT serine hydroxymethyltransferase", testMassTag.ProteinDescription);
            Assert.AreEqual(1433.766629m, (decimal)testMassTag.MonoIsotopicMass);
            Assert.AreEqual(717.8905912m, (decimal)testMassTag.MZ);
            Assert.AreEqual(75, testMassTag.ObsCount);
            Assert.AreEqual(4225609, testMassTag.ID);

            
        }

        [Test]
        public void ImportPeptidesWithModsTest1()
        {
             TargetCollection mtc = new TargetCollection();

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagsWithModsFile1);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            Assert.AreEqual(1868, mtc.TargetList.Count);

            PeptideTarget testMassTag = (PeptideTarget)mtc.TargetList[1021];
            Assert.AreEqual(testMassTag.EmpiricalFormula, "C56H82N10O13");
            Assert.AreEqual(1,testMassTag.ModCount);
            Assert.AreEqual(testMassTag.ModDescription, "NH3_Loss:1");
           

            //250663994	1102.60623	QFPILLDFK	2	C56H82N10O13	1	NH3_Loss:1

        }


        [Test]
        public void importFromSQLManagmentStyleTextFile_test1()
        {
            TargetCollection mtc = new TargetCollection();

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagTestFile2);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            Assert.AreEqual(13, mtc.TargetList.Count);

            PeptideTarget testMassTag = mtc.TargetList[0] as PeptideTarget;


            Assert.AreEqual("AVTTADQVQQEVER", testMassTag.Code);
            Assert.AreEqual(0, testMassTag.ChargeState);
            Assert.AreEqual(0.2365603m, (decimal)testMassTag.NormalizedElutionTime);
            Assert.AreEqual("C64H108N20O26", testMassTag.EmpiricalFormula);
            Assert.AreEqual(137, testMassTag.RefID);
            Assert.AreEqual(1572.774283m, (decimal)testMassTag.MonoIsotopicMass);
            Assert.AreEqual(0, (decimal)testMassTag.MZ);
            Assert.AreEqual(6, testMassTag.ObsCount);
            Assert.AreEqual(354885422, testMassTag.ID);
        }

    }
}
