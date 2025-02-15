﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for RefereeProtocolTest
  /// </summary>
  [TestClass]
  public class RefereeProtocolTest
  {
    public RefereeProtocolTest()
    {
    }

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion


    [TestMethod]
    [DeploymentItem(@"TestOutputs\RefereeProtocol_Empty.pdf")]
    public void RefereeProtocol_Empty()
    {
      string workingDir = TestUtilities.CreateWorkingFolder(testContextInstance.TestDeploymentDir);

      TestDataGenerator tg = new TestDataGenerator(workingDir);
      {
        IPDFReport report = new RefereeProtocol(tg.Model.GetRace(0).GetRun(0));
        Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"RefereeProtocol_Empty.pdf", 1));
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    [DeploymentItem(@"TestOutputs\1554MSBS\1554MSBS - Schiedsrichterprotokoll 1. Durchgang.pdf")]
    public void RefereeProtocol_IntegrationTest()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);
      {
        IPDFReport report = new RefereeProtocol(model.GetRace(0).GetRun(0));
        Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"1554MSBS - Schiedsrichterprotokoll 1. Durchgang.pdf", 2));
      }
    }

  }
}
