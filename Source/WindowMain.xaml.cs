using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using LogViewer2.DataTypes;
using Microsoft.Win32;

namespace LogViewer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        #region Private Variables

        private CancellationTokenSource cancellationTokenSource;
        private WaitCursor waitCursor;
        private bool processing;
        private Configuration configuration;

        #endregion

        #region Constants

        private const string configurationFilename = "LogViewer.toml";

        #endregion

        public WindowMain()
        {
            InitializeComponent();
        }

        #region Window Event Handlers

        /// <summary>
        /// Run when the window has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            string errorMessage;
            
            configuration = ConfigurationUtilities.LoadConfigurationFromFile(configurationFilename, out errorMessage);
            if(configuration == null)
            {
                MessageBox.Show(errorMessage, Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            mnuFileOpen.IsEnabled = true;
            mnuFileClose.IsEnabled = false;
            toolBtnSearch.IsEnabled = false;
        }
        #endregion

        #region Menu Event Handlers
       
        /// <summary>
        /// Display the load file dialog and open the selected file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileOpenClickHandler(object sender, RoutedEventArgs e)
        {
            //LoadFile("access.log.txt", false);

            OpenFileDialog openFileDialog = CreateOpenFileDialogForLogFiles();
            if (openFileDialog.ShowDialog(this) == false)
            {
                return;
            }

            LoadFile(openFileDialog.FileName, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MnuFileClose_Click(object sender, RoutedEventArgs e)
        {
            if (tabMain.SelectedItem == null)
            {
                MessageBox.Show("Cannot identify current tab", Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            TabItem ti = (TabItem)tabMain.SelectedItem;
            ti.Header = "...";
            ti.ToolTip = "";
            ControlLog cl = (ControlLog)ti.Content;
            cl.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MnuToolsConfiguration_Click(object sender, RoutedEventArgs e)
        {
            WindowConfiguration wc = new WindowConfiguration(this.configuration);
            // Hack to set owner from a user control?!
            HwndSource source = HwndSource.FromVisual(this) as HwndSource;
            if (source != null)
            {
                WindowInteropHelper helper = new WindowInteropHelper(wc);
                helper.Owner = source.Handle;
            }
            if (wc.ShowDialog() == false)
            {
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MnuToolsMultiStringSearch_Click(object sender, RoutedEventArgs e)
        {
            ControlLog cl = (ControlLog)((TabItem)tabMain.SelectedItem).Content;
            cl.ShowMultiSearch();

            //LogFile lf = logs[tabControl.SelectedTab.Tag.ToString()];

            //using (FormSearch f = new FormSearch(lf.Searches))
            //{
            //    DialogResult dr = f.ShowDialog(this);
            //    if (dr == DialogResult.Cancel)
            //    {
            //        return;
            //    }

            //    // Clear any existing filter ID's as we will only show the multi-string search
            //    lf.FilterIds.Clear();
            //    lf.Searches.Reset();
            //    foreach (SearchCriteria sc in f.NewSearches)
            //    {
            //        // Add the ID so that any matches show up straight away
            //        lf.FilterIds.Add(sc.Id);
            //        lf.Searches.Add(sc);
            //    }

            //    this.processing = true;
            //    this.hourGlass = new HourGlass(this);
            //    SetProcessingState(false);
            //    statusProgress.Visible = true;
            //    this.cancellationTokenSource = new CancellationTokenSource();
            //    lf.SearchMulti(f.NewSearches, cancellationTokenSource.Token, config.NumContextLines);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private void LoadFile(string filePath, bool newTab)
        {
            SetProcessingState(false);
            this.cancellationTokenSource = new CancellationTokenSource();

            if (newTab == true)
            {
                ControlLog controlLog = new ControlLog(this.configuration)
                {                   
                    ViewMode = Global.ViewMode.Standard,                   
                };

                controlLog.ProgressUpdate += LogFile_LoadProgress;
                controlLog.LoadComplete += LogFile_LoadComplete;
                controlLog.SearchComplete += LogFile_SearchComplete;
                controlLog.ExportInitiated += LogFile_ExportInitiated;
                controlLog.ExportComplete += LogFile_ExportComplete; ;
                controlLog.LoadError += LogFile_LoadError;
                controlLog.MultiSearchInitiated += LogFile_MultiSearchInitiated;
                controlLog.DragEnter += LogFile_DragEnter;
                controlLog.Drop += LogFile_Drop;
                
                TabItem tabItem = new TabItem
                {
                    Content = controlLog,
                    Header = Path.GetFileName(filePath),
                    Tag = controlLog.Guid,
                    ToolTip = filePath
                };

                tabMain.Items.Add(tabItem);
                tabMain.SelectedIndex = tabMain.Items.Count - 1;
                
                controlLog.Load(filePath, cancellationTokenSource.Token);
            }
            else
            {
                if (tabMain.SelectedItem == null)
                {
                    MessageBox.Show("Cannot identify current tab", Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                ControlLog cl = (ControlLog)((TabItem)tabMain.SelectedItem).Content;
                ((TabItem)tabMain.SelectedItem).ToolTip = filePath;
                cl.Dispose();
                cl.Load(filePath, cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void SearchFile()
        {
            ControlLog cl = (ControlLog)((TabItem)tabMain.SelectedItem).Content;

            SearchCriteria sc = new SearchCriteria();
            sc.Type = (Global.SearchType)comboType.SelectedIndex;
            sc.Pattern = txtSearch.Text;
            sc.Id = cl.Searches.Add(sc, (bool)toolBtnCumulative.IsChecked);

            if (sc.Id == 0)
            {
                MessageBox.Show("Search pattern already exists", Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Add the ID so that any matches show up straight away
            cl.FilterIds.Add(sc.Id);

            SetProcessingState(false);
            this.cancellationTokenSource = new CancellationTokenSource();
            cl.Search(sc, (bool)toolBtnCumulative.IsChecked, cancellationTokenSource.Token, configuration.NumContextLines);
        }
        #endregion

        #region Log File Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogFile_ExportInitiated(ControlLog cl, bool all)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "All Files|*.*";
            sfd.FileName = "*.*";
            sfd.Title = "Select export file";

            if (sfd.ShowDialog() == false)
            {
                return;
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            SetProcessingState(false);

            if (all == true)
            {
                cl.Export(sfd.FileName, cancellationTokenSource.Token);
            }
            else
            {
                cl.ExportSelected(sfd.FileName, cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cl"></param>
        private void LogFile_MultiSearchInitiated(ControlLog cl, List<SearchCriteria> searches)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            SetProcessingState(false);
            
            cl.SearchMulti(searches, cancellationTokenSource.Token, configuration.NumContextLines);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cl"></param>
        /// <param name="fileName"></param>
        /// <param name="duration"></param>
        /// <param name="cancelled"></param>
        private void LogFile_ExportComplete(ControlLog cl, string fileName, TimeSpan duration, bool cancelled)
        {
            Dispatcher.Invoke(() =>
            {
                this.cancellationTokenSource.Dispose();
                SetProcessingState(true);
                UpdateStatusLabel("Export complete # Duration: " + duration + " (" + fileName + ")");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cl"></param>
        /// <param name="fileName"></param>
        /// <param name="duration"></param>
        /// <param name="cancelled"></param>
        private void LogFile_LoadComplete(ControlLog cl, string fileName, TimeSpan duration, bool cancelled)
        {
            Dispatcher.Invoke(() =>
            {
                // Update the UI
                cl.ItemsSource = cl.Lines;
                cl.UpdateColumnWidths();
                tabMain.UpdateLayout();

                // Enable the menu items that only apply if a file is open
                mnuFileClose.IsEnabled = true;
                mnuFileOpen.IsEnabled = true;

                UpdateStatusLabel(cl.Lines.Count + " Lines # Duration: " + duration + " (" + fileName + ")");
                this.cancellationTokenSource.Dispose();
                SetProcessingState(true);

            });        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percent"></param>
        private void LogFile_LoadProgress(int percent)
        {
            Dispatcher.Invoke(() =>
            {
                statusProgressBar.Value = (int)percent;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void LogFile_LoadError(string fileName, string message)
        {
            MessageBox.Show(message + " (" + fileName + ")", Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error);

            Dispatcher.Invoke(() =>
            {
                this.cancellationTokenSource.Dispose();
                SetProcessingState(true);

                // Lets clear the LogFile state and set the UI correctly
                MnuFileClose_Click(this, null);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        private void LogFile_SearchComplete(ControlLog cl, string fileName, TimeSpan duration, long matches, int numTerms, bool cancelled)
        {
            Dispatcher.Invoke(() =>
            {
                // Update the list
                cl.ItemsSource = null;
                cl.ItemsSource = cl.Lines;

                UpdateStatusLabel("Matched " + matches + " lines (Search Terms: " + numTerms + ") # Duration: " + duration + " (" + fileName + ")");
                this.cancellationTokenSource.Dispose();                
                SetProcessingState(true);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogFile_Drop(object sender, DragEventArgs e)
        {
            if (processing == true)
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0)
            {
                return;
            }

            if (files.Length > 1)
            {
                MessageBox.Show("Only one file can be processed at one time", Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            LoadFile(files[0], false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogFile_DragEnter(object sender, DragEventArgs e)
        {
            if (processing == true)
            {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }
        #endregion

        #region UI Methods

        /// <summary>
        /// Disable or enable components while an operation is in progress.
        /// </summary>
        /// <param name="enabled"></param>
        private void SetProcessingState(bool enabled)
        {
            Dispatcher.Invoke(() =>
            {
                mnuFileOpen.IsEnabled = enabled;
                mnuFileClose.IsEnabled = enabled;
                mnuFileExit.IsEnabled = enabled;
                toolBtnCumulative.IsEnabled = enabled;
                toolBtnSearch.IsEnabled = enabled;

                if (enabled == true)
                {
                    this.waitCursor.Dispose();
                    this.processing = false;
                    statusProgressBar.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.waitCursor = new WaitCursor();
                    this.processing = true;
                    statusProgressBar.Visibility = Visibility.Visible;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        private void UpdateStatusLabel(string text)
        {
            Dispatcher.Invoke(() =>
            {
                statusLabelMain.Content = text;
            });
        }
        #endregion

        #region Toolbar Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolBtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (comboType.SelectedIndex == -1)
            {
                MessageBox.Show("Type is not selected", Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                comboType.Focus();
                return;
            }

            SearchFile();
        }
        #endregion        

        private OpenFileDialog CreateOpenFileDialogForLogFiles()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files|*.*";
            openFileDialog.FileName = "*.*";
            openFileDialog.Title = "Select log file";

            return openFileDialog;
        }
    }
}
