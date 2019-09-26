﻿using DSVAlpin2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DSVAlpin2
{
  /// <summary>
  /// Interaction logic for RaceUC.xaml
  /// </summary>
  public partial class RaceUC : UserControl
  {
    // Input Data
    AppDataModel _dataModel;
    Race _currentRace;
    LiveTimingMeasurement _liveTimingMeasurement;

    // Working Data
    RaceRun _currentRaceRun;

    RemainingStartListViewProvider _rslVP;

    ScrollToMeasuredItemBehavior dgResultsScrollBehavior;
    ScrollToMeasuredItemBehavior dgTotalResultsScrollBehavior;


    public RaceUC(AppDataModel dm, Race race, LiveTimingMeasurement liveTimingMeasurement)
    {
      _dataModel = dm;
      _currentRace = race;
      _liveTimingMeasurement = liveTimingMeasurement;
      _liveTimingMeasurement.LiveTimingMeasurementStatusChanged += OnLiveTimingMeasurementStatusChanged;

      InitializeComponent();

      InitializeConfiguration();

      InitializeTiming();

      InitializeTotalResults();
    }

    public Race GetRace() { return _currentRace; }
    public RaceRun GetRaceRun() { return _currentRaceRun; }


    #region Configuration

    RaceConfiguration _raceConfiguration;

    private void InitializeConfiguration()
    {

      _raceConfiguration = new RaceConfiguration
      {
        Runs = 2,
        DefaultGrouping = "Participant.Class",
        RaceResultView = "Sum",

        Run1_StartistView = "Startlist_1stRun_StartnumberAscending",
        Run1_StartistViewGrouping = "Participant.Class",

        Run2_StartistView = "Startlist_2nd_StartnumberAscending",
        Run2_StartistViewGrouping = "Participant.Class"
      };


      // Configuration Screen
      cmbRuns.Items.Add(new CBItem { Text = "1", Value = 1 });
      cmbRuns.Items.Add(new CBItem { Text = "2", Value = 2 });

      // Result
      FillGrouping(cmbConfigErgebnisGrouping);
      
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Bester Durchgang", Value = "BestOfTwo" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe", Value = "Sum" });


      // Run 1
      FillGrouping(cmbConfigStartlist1Grouping);
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Startnummer (aufsteigend)", Value = "Startlist_1stRun_StartnumberAscending" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (ersten 15 gelost)", Value = "Startlist_1stRun_Points_15" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (ersten 30 gelost)", Value = "Startlist_1stRun_Points_30" });

      // Run 2
      FillGrouping(cmbConfigStartlist2Grouping);
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Startnummer (aufsteigend)", Value = "Startlist_2nd_StartnumberAscending" });
      //cmbConfigStartlist2.Items.Add(new GroupingCBItem { Text = "Startnummer (aufsteigend, inkl. ohne Ergebnis)", Value = "Startlist_2nd_StartnumberAscending" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Startnummer (absteigend)", Value = "Startlist_2nd_StartnumberDescending" });
      //cmbConfigStartlist2.Items.Add(new GroupingCBItem { Text = "Startnummer (absteigend, inkl. ohne Ergebnis)", Value = "Startlist_2nd_StartnumberDescending" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit", Value = "Startlist_2nd_PreviousRunOnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRunAlsoWithoutResults" });

      ResetConfigurationSelection(_raceConfiguration);
    }

    private void ResetConfigurationSelection(RaceConfiguration cfg)
    {
      cmbRuns.SelectCBItem(cfg.Runs);
      cmbConfigErgebnisGrouping.SelectCBItem(cfg.DefaultGrouping);
      cmbConfigErgebnis.SelectCBItem(cfg.RaceResultView);
      cmbConfigStartlist1.SelectCBItem(cfg.Run1_StartistView);
      cmbConfigStartlist1Grouping.SelectCBItem(cfg.Run1_StartistViewGrouping);
      cmbConfigStartlist2.SelectCBItem(cfg.Run2_StartistView);
      cmbConfigStartlist2Grouping.SelectCBItem(cfg.Run2_StartistViewGrouping);
    }

    private void StoreConfigurationSelection(RaceConfiguration cfg)
    {
      cfg.Runs = (int)((CBItem)cmbRuns.SelectedValue).Value;
      cfg.DefaultGrouping = (string)((CBItem)cmbConfigErgebnisGrouping.SelectedValue).Value;
      cfg.RaceResultView = (string)((CBItem)cmbConfigErgebnis.SelectedValue).Value;
      cfg.Run1_StartistView = (string)((CBItem)cmbConfigStartlist1.SelectedValue).Value;
      cfg.Run1_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist1Grouping.SelectedValue).Value;
      cfg.Run2_StartistView = (string)((CBItem)cmbConfigStartlist2.SelectedValue).Value;
      cfg.Run2_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist2Grouping.SelectedValue).Value;
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
      ResetConfigurationSelection(_raceConfiguration);
    }

    private void BtnApply_Click(object sender, RoutedEventArgs e)
    {
      RaceConfiguration cfg = new RaceConfiguration();
      StoreConfigurationSelection(cfg);

      string configFile = System.IO.Path.Combine(_dataModel.GetDB().GetDBPathDirectory(), _dataModel.GetDB().GetDBFileName() + ".config");
      System.IO.File.WriteAllText(configFile, Newtonsoft.Json.JsonConvert.SerializeObject(cfg, Newtonsoft.Json.Formatting.Indented));

      //string sf = System.IO.File.ReadAllText(@"alpinconfig.json");
      //RaceConfiguration config = new RaceConfiguration();
      //Newtonsoft.Json.JsonConvert.PopulateObject(sf, config);


    }


    #endregion


    #region Timing


    private void InitializeTiming()
    {
      FillCmbRaceRun();

      FillGrouping(cmbStartListGrouping);
      FillGrouping(cmbResultGrouping);
    }


    private void FillCmbRaceRun()
    {
      // Fill Runs
      List<KeyValuePair<RaceRun, string>> races = new List<KeyValuePair<RaceRun, string>>();
      for (int i = 0; i < _currentRace.GetMaxRun(); i++)
      {
        string sz1 = String.Format("{0}. Durchgang", i + 1);
        races.Add(new KeyValuePair<RaceRun, string>(_currentRace.GetRun(i), sz1));
      }
      cmbRaceRun.ItemsSource = races;
      cmbRaceRun.DisplayMemberPath = "Value";
      cmbRaceRun.SelectedValuePath = "Key";

      cmbRaceRun.SelectedIndex = 0;
    }

    private void CmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      _currentRaceRun = (sender as ComboBox).SelectedValue as RaceRun;
      if (_currentRaceRun != null)
      {
        _dataModel.SetCurrentRaceRun(_currentRaceRun);

        dgStartList.ItemsSource = _currentRace.GetParticipants();

        _rslVP = new RemainingStartListViewProvider();
        _rslVP.Init(_currentRaceRun.GetStartListProvider(), _currentRaceRun);
        dgRemainingStarters.ItemsSource = _rslVP.GetView();

        dgRemainingStartersSrc.ItemsSource = _currentRaceRun.GetStartListProvider().GetView();

        dgRunning.ItemsSource = _currentRaceRun.GetOnTrackList();
        dgResults.ItemsSource = _currentRaceRun.GetResultView();
        dgResultsScrollBehavior = new ScrollToMeasuredItemBehavior(dgResults, _dataModel);
      }
      else
      {
        dgStartList.ItemsSource = null;
        dgRemainingStarters.ItemsSource = null;
        dgRemainingStartersSrc.ItemsSource = null;
        dgRunning.ItemsSource = null;
        dgResults.ItemsSource = null;
        dgResultsScrollBehavior = null;
      }
    }


    /// <summary>
    /// Enables / disable the recerun combobox depending on whether LiveTimingMeasurement is performed or not
    /// </summary>
    private void OnLiveTimingMeasurementStatusChanged(object sender, bool isRunning)
    {
      cmbRaceRun.IsEnabled = !isRunning;

      System.Diagnostics.Debug.Assert(cmbRaceRun.SelectedValue == _currentRaceRun);
    }



    private void BtnManualTimeStore_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan? start = null, finish = null, run = null;

      try { start = TimeSpan.Parse(txtStart.Text); } catch (Exception) { }
      try { finish = TimeSpan.Parse(txtFinish.Text); } catch (Exception) { }
      try { run = TimeSpan.Parse(txtRun.Text); } catch (Exception) { }

      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }
      RaceParticipant participant = _currentRace.GetParticipant(startNumber);

      if (participant != null)
      {
        if (start != null || finish != null)
          _currentRaceRun.SetStartFinishTime(participant, start, finish);
        else if (run != null)
          _currentRaceRun.SetRunTime(participant, run);
      }
    }

    private void BtnManualTimeFinish_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan finish = DateTime.Now - DateTime.Today;
      txtFinish.Text = finish.ToString();
      UpdateRunTime();
    }

    private void BtnManualTimeStart_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan start = DateTime.Now - DateTime.Today;
      txtStart.Text = start.ToString();
      UpdateRunTime();
    }

    private void UpdateRunTime()
    {
      try
      {
        TimeSpan start = TimeSpan.Parse(txtStart.Text);
        TimeSpan finish = TimeSpan.Parse(txtFinish.Text);
        TimeSpan run = finish - start;
        txtRun.Text = run.ToString(@"mm\:ss\,ff");
      }
      catch (Exception)
      { }

    }

    private void DgRemainingStarters_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      RaceParticipant participant = (dgRemainingStarters.SelectedItem as StartListEntry)?.Participant;

      if (participant != null)
      {
        RunResult result = _currentRaceRun.GetResultList().SingleOrDefault(r => r._participant == participant);

        if (result != null)
        {
          txtStart.Text = result.GetStartTime()?.ToString();
          txtFinish.Text = result.GetFinishTime()?.ToString();
          txtRun.Text = result.GetRunTime()?.ToString();
        }
        else
        {
          txtStart.Text = txtFinish.Text = txtRun.Text = "";
        }

        txtStartNumber.Text = participant.StartNumber.ToString();
      }
    }

    private void CmbStartListGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbStartListGrouping.SelectedValue is CBItem grouping)
        _rslVP.ChangeGrouping((string)grouping.Value);
    }

    private void CmbResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbResultGrouping.SelectedValue is CBItem grouping)
        _currentRaceRun.GetResultViewProvider().ChangeGrouping((string)grouping.Value);
    }

    #endregion


    #region TotalResults

    private void InitializeTotalResults()
    {
      // Race Results
      FillGrouping(cmbTotalResultGrouping);

      dgTotalResults.ItemsSource = _currentRace.GetTotalResultView();
      dgTotalResultsScrollBehavior = new ScrollToMeasuredItemBehavior(dgTotalResults, _dataModel);
    }


    private void CmbTotalResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTotalResultGrouping.SelectedValue is CBItem grouping)
        _currentRace.GetResultViewProvider().ChangeGrouping((string)grouping.Value);
    }

    #endregion



    public static void FillGrouping(ComboBox comboBox)
    {
      comboBox.Items.Add(new CBItem { Text = "---", Value = null });
      comboBox.Items.Add(new CBItem { Text = "Klasse", Value = "Participant.Class" });
      comboBox.Items.Add(new CBItem { Text = "Gruppe", Value = "Participant.Group" });
      comboBox.Items.Add(new CBItem { Text = "Kategorie", Value = "Participant.Sex" });
      comboBox.SelectedIndex = 0;
    }

  }



  #region Utilities

  public class ScrollToMeasuredItemBehavior
  {
    DataGrid _dataGrid;
    AppDataModel _dataModel;
    Participant _scrollToParticipant;

    System.Timers.Timer _timer;


    public ScrollToMeasuredItemBehavior(DataGrid dataGrid, AppDataModel dataModel)
    {
      _dataGrid = dataGrid;
      _dataModel = dataModel;
      _dataModel.ParticipantMeasuredEvent += OnParticipantMeasured;
      _timer = null;
    }


    void OnParticipantMeasured(object sender, Participant participant)
    {
      _scrollToParticipant = participant;

      _timer = new System.Timers.Timer(200);
      _timer.Elapsed += OnTimedEvent;
      _timer.AutoReset = false;
      _timer.Enabled = true;

    }

    void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        if (_dataGrid.ItemsSource != null)
        {
          foreach (var x in _dataGrid.ItemsSource)
          {
            Participant xp = null;
            xp = (x as RunResult)?.Participant.Participant;
            if (xp == null)
              xp = (x as RaceResultItem)?.Participant.Participant;

            if (xp == _scrollToParticipant)
            {
              _dataGrid.ScrollIntoView(x);
              break;
            }
          }
        }
      });
    }
  }

  #endregion

}
