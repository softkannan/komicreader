using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using ComicViewer.Properties;
using System.Windows.Media.Effects;
using System.Threading;

namespace ComicViewer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        IArchive curFile = null;
        ArchiveImages pages = null;
                
        #region DataBinding Properties
        private string zoomText = "Fit Width";

        public string ZoomText
        {
            get { return zoomText; }
            set 
            { 
                zoomText = value;
                this.InternalPropertyChanged("ZoomText");
            }
        }

        int goToPage = 0;
        public int GoToPage
        {
            get { return goToPage; }
            private set
            {
                goToPage = value;
                this.InternalPropertyChanged("GoToPage");
            }
        }

        RotatePage rotatePage = RotatePage.RotateNormal;

        public RotatePage RotatePage
        {
            get { return rotatePage; }
            private set 
            {
                rotatePage = value;
                this.InternalPropertyChanged("RotatePage");
            }
        }
        int currentPage = 0;
        public int CurrentPage 
        {
            get { return currentPage; }
            set
            {
                currentPage = value;
                this.GoToPage = value;
            }
        }

        Size pageSize = new Size(1, 1);

        public Size PageSize
        {
            get { return pageSize; }
            private set
            {
                pageSize = value;
                this.InternalPropertyChanged("PageSize");
            }
        }

        public ArchiveImages Pages
        {
            get { return pages; }
            private set
            {
                pages = value;
                this.InternalPropertyChanged("Pages");
            }
        }

        bool isFileOpened = false;
        public bool IsFileOpened
        {
            get
            {
                return this.isFileOpened;
            }
            private set
            {
                isFileOpened = value;
                this.InternalPropertyChanged("IsFileOpened");
            }
        }

        //private ItemSizingType sizingType = ItemSizingType.FitWidth;

        //public ItemSizingType SizingType
        //{
        //    get { return sizingType; }
        //    set 
        //    { 
        //        sizingType = value;
        //        this.InternalPropertyChanged("ZoomText");
        //        this.InternalPropertyChanged("SizingType");
        //    }
        //}

        private PanelMode panelMode = PanelMode.SinglePage;

        public PanelMode PanelMode
        {
            get { return panelMode; }
            set 
            {
                panelMode = value;
                this.InternalPropertyChanged("PanelMode");
            }
        }

        private ImageEffect imageEffect = ImageEffect.None;

        public ImageEffect ImageEffect
        {
            get { return imageEffect; }
            set 
            { 
                imageEffect = value;
                switch (imageEffect)
                {
                    case ImageEffect.BW:
                        actualEffect = new ComicViewer.Effects.BWEffect();
                        break;
                    case ImageEffect.Grey:
                        actualEffect = new ComicViewer.Effects.GrayscaleEffect();
                        break;
                    default:
                        actualEffect = null;
                        break;
                }
                this.InternalPropertyChanged("ImageEffect");
                this.InternalPropertyChanged("ActualEffect");
            }
        }

        private Effect actualEffect = null;

        public Effect ActualEffect
        {
            get { return actualEffect; }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //this.ImageEffect = new GrayscaleEffect.GrayscaleEffect();
        }

        #region Helper methods
        private void OpenCB(string fileInfo)
        {
            curFile = ArchiveUtility.OpenArchive(fileInfo);
            if (curFile != null)
            {
                if (curFile.Pages.Count > 0)
                {

                    for (int index = 1; index <= curFile.Pages.Count; index++)
                    {
                        pagesCombo.Items.Add(
                        String.Format("{0}/{1}", index, curFile.Pages.Count));
                    }

                    this.CurrentPage = 0;
                    CommandManager.InvalidateRequerySuggested();

                    this.PageSize = curFile.PageSize;
                    this.IsFileOpened = true;
                    this.Pages = curFile.Pages;
                }
            }
        }

        private void CloseCB()
        {
            if (this.IsFileOpened)
            {
                this.IsFileOpened = false;
            }
            if (this.Pages != null)
            {
                this.Pages = null;
            }
            if (curFile != null)
            {
                curFile.Close();
            }
            pagesCombo.Items.Clear();
            Thread.Sleep(1000);
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region Open  and Page Moveent Handlers
        private int ChildPerRow
        {
            get
            {
                return this.PanelMode == PanelMode.DoubleContniousPage || this.PanelMode == PanelMode.DoublePage ? 2 : 1;
            }
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseCB();
            OpenFileDialog opnDia = new OpenFileDialog();
            if (opnDia.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenCB(opnDia.FileName);
            }
        }

        private void OpenFolder_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseCB();
            FolderBrowserDialog folderDia = new FolderBrowserDialog();
            if (folderDia.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenCB(folderDia.SelectedPath);
            }
        }

        private void Is_FileOpen(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsFileOpened;
        }

        private void PreviousPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentPage -= this.ChildPerRow;
        }

        private void NextPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentPage += this.ChildPerRow;
        }

        private void FirstPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsFileOpened && this.CurrentPage > 0;
        }

        private void FirstPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentPage = 0;
        }

        private void LastPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsFileOpened && this.CurrentPage < this.Pages.Count - 1;
        }

        private void LastPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentPage = this.Pages.Count-1;
        }

        #endregion

        #region Min Max and Exit Handlers
        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
        private void Maximise_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Minimise_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void InternalPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Zoom Handlers
        private void ZoomFit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ZoomText = "Fit Page";
        }

        private void ZoomOriginal_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ZoomText = "Actul Size";
        }

        private void ZoomFitWidth_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ZoomText = "Fit Width";
        } 
        #endregion

        #region Mode Menu Handlers
        private void ModeSinglePage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.PanelMode = PanelMode.SinglePage;
        }

        private void ModeContinuousPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.PanelMode = PanelMode.ContniousPage;
        }

        private void ModeDoublePage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.PanelMode = PanelMode.DoublePage;
        }
        private void ModeDoubleContinuousPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.PanelMode = PanelMode.DoubleContniousPage;
        }
        #endregion

        #region Rotate Menu Handlers
        private void RotateNormal_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.RotatePage = RotatePage.RotateNormal;
        }

        private void Rotate90_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.RotatePage = RotatePage.Rotate90;
        }

        private void Rotate180_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.RotatePage = RotatePage.Rotate180;
        }

        private void Rotate270_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.RotatePage = RotatePage.Rotate90CounterClock;
        } 
        #endregion

        #region Expander Handler
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander tempExpander = sender as Expander;
            if (tempExpander != null)
            {
                tempExpander.Width = 150;
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Expander tempExpander = sender as Expander;
            if (tempExpander != null)
            {
                tempExpander.Width = 23;
            }

        }
        #endregion

        #region Misc Event Handler
        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.RotatePage = (RotatePage)Settings.Default.RotateMode;
            this.ZoomText = Settings.Default.ZoomMode;
            this.PanelMode = (PanelMode)Settings.Default.PanelMode;

            App app = System.Windows.Application.Current as App;

            if (app != null)
            {
                app.StartupNextInstance += app_StartupNextInstance;
            }

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                this.OpenCB(Environment.GetCommandLineArgs()[1]);
                //System.Windows.MessageBox.Show(Environment.GetCommandLineArgs()[1]);
            }

            FileTypeRegister.RegisterFile(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        void app_StartupNextInstance(object sender, Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
        {
            if (e.CommandLine.Count > 0)
            {
                this.CloseCB();
                this.OpenCB(e.CommandLine[0]);
            }
        }
        private void mainWindow_Closed(object sender, EventArgs e)
        {
            Settings.Default.RotateMode = (int)this.RotatePage;
            ItemSizingType tempVal = ZoomToSizingType.GetItemSizeType(this.ZoomText);
            string zoomVal = this.ZoomText;
            if(tempVal == ItemSizingType.Custom)
            {
                zoomVal = "Fit Width";
            }
            Settings.Default.ZoomMode = zoomVal;
            Settings.Default.PanelMode = (int)this.PanelMode;
            Settings.Default.Save();
        }
        private void ShowAbout_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void MainImagePanel_CurrentPageChanged(object sender, CurrentPageChangedEventArgs e)
        {
            this.currentPage = e.CurrentPage;
            this.InternalPropertyChanged("CurrentPage");
            thumNailViewer.ScrollIntoView(thumNailViewer.SelectedItem);
        }
        private void ListBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region Effect Menu Item
        private void EffectNone_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ImageEffect = ImageEffect.None;
        }

        private void EffectGrey_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ImageEffect = ImageEffect.Grey;
        }

        private void EffectBW_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ImageEffect = ImageEffect.BW;
        } 
        #endregion
        
    }
    public enum ImageEffect
    {
        None,
        Grey,
        BW
    }
    /// <summary>
    /// Item Sizing type
    /// </summary>
    public enum ItemSizingType
    {
        Custom = -1,
        Original,
        FitWidth,
        Fit,
        Z8DOT33,
        Z12DOT5,
        Z25,
        Z33DOT33,
        Z50,
        Z66DOT67,
        Z75,
        Z100,
        Z125,
        Z150,
        Z200,
        Z300,
        Z400,
        Z600,
        Z800,
        Z1200,
        Z1600,
        Z3200,
        Z6400
    }
    /// <summary>
    /// Layout Mode
    /// </summary>
    public enum PanelMode
    {
        SinglePage,
        ContniousPage,
        DoublePage,
        DoubleContniousPage
    }
    public enum RotatePage
    {
        RotateNormal,
        Rotate90,
        Rotate180,
        Rotate90CounterClock
    }
}
