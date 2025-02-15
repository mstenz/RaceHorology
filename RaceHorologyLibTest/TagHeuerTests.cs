﻿/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for TagHeuerTests
  /// </summary>
  [TestClass]
  public class TagHeuerTests
  {
    public TagHeuerTests()
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
    public void Parse()
    {
      TagHeuerParser parser = new TagHeuerParser();

      {
        var r = parser.ParseRR("\nRR 0010 0232   05:27:51.01040\t");
        Assert.AreEqual(10, r.Rank);
        Assert.AreEqual(232, r.Number);
        Assert.AreEqual(new TimeSpan(0, 5, 27, 51, 10).AddMicroseconds(400), r.Time);
      }

      {
        var r = parser.ParseSynchroTime("\n!T 08:14:00 01/03/20\t");
        Assert.AreEqual(new DateTime(2020,3,1,8,14,0), r);
      }
    }

    [TestMethod, TestCategory("HardwareDependent")]
    public void RetrieveTimingData()
    {
      string comport = "COM6";

      using (TagHeuer tagHeuer = new TagHeuer(comport))
      {
        var progress = new Progress<StdProgress>();
        tagHeuer.DoProgressReport(progress);

        StdProgress lastProgress = null;
        int progressCounter = 0;
        progress.ProgressChanged += (s, e) => { lastProgress = e; progressCounter++; };

        tagHeuer.Connect();
        tagHeuer.StartGetTimingData();

        foreach (var t in tagHeuer.TimingData())
        {
          //Assert.IsFalse(lastProgress.Finished);
          TestContext.WriteLine(t.Time.ToString());
        }

        Assert.IsTrue(progressCounter > 0);
        Assert.IsTrue(lastProgress.Finished);
      }


      // Check Dispose => no exception should occure (i.e. COMPort should be available)
      using (TagHeuer tagHeuer = new TagHeuer(comport))
      {
        tagHeuer.Connect();
      }
    }
  }
}
