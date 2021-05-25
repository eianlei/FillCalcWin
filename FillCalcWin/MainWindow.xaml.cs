// (c) 2021 Ian Leiman, ian.leiman@hotmail.com
// https://github.com/eianlei/WinFillCalc_private
//
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using System.Windows.Controls.Ribbon;
using FillCalcWin;
using Microsoft.Win32;
using System.IO;
using System.Windows.Documents;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

//[assembly: AssemblyVersion("1.1.0.0")]

namespace FillCalcWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        Gas gas = new Gas
        {
            start_bar = 100,
            start_o2 = 21,
            start_he = 35,
            stop_bar = 200,
            stop_o2 = 21,
            stop_he = 35,
            tank_liters = 24,
            o2_price = 4.14,
            he_price = 25.00,
            compressor_price = 5.00,
            selected_tab = (int) TmxCalcClass.FILLTYPE.pp
        };
        public Window1 HelpWindow;

        public MainWindow()
        {

            this.DataContext = gas;

            InitializeComponent();
            this.HelpWindow = InitHelpWin(); // initialize the help window with contents
            // get handles to the TextBlock outputs
            gas.txt_air = this.txt_air;
            gas.txt_nitrox = this.txt_nitrox;
            gas.txt_tmx = this.txt_tmx;
            gas.txt_pp = this.txt_pp;
            gas.txt_henx = this.txt_henx;
            gas.txt_cost = this.txt_cost;
            // do the first calculation with default values
            gas.CalculateGas();

            Console.WriteLine("The version of the currently executing assembly is: {0}",
            typeof(MainWindow).Assembly.GetName().Version);
        }

        private Window1 InitHelpWin()
        // initialize the help window with contents
        {
            //throw new NotImplementedException();
            // string fileName = "D:\\OneDrive\\c_sharp\\scuba\\FillCalc\\FillCalcWPF\\FCribbon\\FCribbon\\fillcalc2.rtf";
            // this is the way to read help file from filesystem
            // using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            //    { txtRange.Load(fileStream, DataFormats.Rtf);  }

            Window1 HelpWin = new Window1();
            // must have this in App.xaml to close subwindows at MainWindow close: ShutdownMode="OnMainWindowClose"

            // reading from embedded resource at runtime
            var flowDoc = new FlowDocument();
            var txtRange = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);

            var assembly = Assembly.GetExecutingAssembly();
            // make sure the file at Resources is "Build Action = Embedded Resource", otherwise it will not be found
            var resourceName = "FillCalcWin.Resources.fillcalc2.rtf";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                txtRange.Load(stream, DataFormats.Rtf);
            }
            // now deal with the hyperlinks
            SubscribeToAllHyperlinks(flowDoc);
            // assign the floowDoc to window
            HelpWin.HelpView.Document = flowDoc;

            return HelpWin;
        }

        /// <summary>
        /// needed for opening hyperlinks from FlowDocuments
        /// </summary>
        /// <param name="flowDocument"></param>
        void SubscribeToAllHyperlinks(FlowDocument flowDocument)
        {
            var hyperlinks = GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
                link.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(link_RequestNavigate);
        }

        public static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var descendants in GetVisuals(child))
                    yield return descendants;
            }
        }

        void link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            //http://stackoverflow.com/questions/2288999/how-can-i-get-a-flowdocument-hyperlink-to-launch-browser-and-go-to-url-in-a-wpf
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public class Gas : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private int _start_bar;
            private int _start_o2;
            private int _start_he;
            private int _stop_bar;
            private int _stop_o2;
            private int _stop_he;
            private int _selected_tab;
            private TextBlock _txt_air, _txt_nitrox, _txt_tmx, _txt_pp, _txt_henx;
            public TextBlock txt_cost;
            private int _tank_liters;
            double _o2_price;
            double _he_price;
            double _compressor_price;
            public string calc_result;
            public string cost_result;
            private bool _vdw;

            public TextBlock txt_air { get { return _txt_air; } set { _txt_air = value; } }
            public TextBlock txt_nitrox { get { return _txt_nitrox; } set { _txt_nitrox = value; } }
            public TextBlock txt_tmx { get { return _txt_tmx; } set { _txt_tmx = value; } }
            public TextBlock txt_pp { get { return _txt_pp; } set { _txt_pp = value; } }
            public TextBlock txt_henx { get { return _txt_henx; } set { _txt_henx = value; } }

            public bool VdW
            {
                get { return _vdw; }
                set { _vdw = value; CalculateGas(); }
            }
            public int selected_tab
            {
                get { return _selected_tab; }
                set { _selected_tab = value; CalculateGas(); }
            }
            public int tank_liters
            {
                get { return _tank_liters; }
                set { if (value != _tank_liters) { _tank_liters = value; OnPropertyChanged(); CalculateGas(); } }
            }
            public double o2_price
            {
                get { return _o2_price; }
                set { if (value != _o2_price) { _o2_price = value; OnPropertyChanged(); CalculateGas(); } }
            }
            public double he_price
            {
                get { return _he_price; }
                set { if (value != _he_price) { _he_price = value; OnPropertyChanged(); CalculateGas(); } }
            }
            public double compressor_price
            {
                get { return _compressor_price; }
                set { if (value != _compressor_price) { _compressor_price = value; OnPropertyChanged(); CalculateGas(); } }
            }

            public int start_bar
            {
                get { return _start_bar; }
                set
                {
                    if (value != _start_bar)
                    {
                        _start_bar = value;
                        OnPropertyChanged();
                        CalculateGas();
                    }
                }
            }
            public class StdGas
            {
                public string name;
                public int id;
                public int o2;
                public int he;

                public StdGas(string name, int id, int o2, int he)
                {
                    this.name = name;
                    this.id = id;
                    this.o2 = o2;
                    this.he = he;
                }
                public string Name { get { return this.name; } }
                public int Id { get { return this.id; } }
                public int O2 { get { return this.o2; } }
                public int He { get { return this.he; } }


            }
            public static readonly StdGas[] _stdGasList =
                {
                new StdGas("Air", 0, 21, 0),
                new StdGas("EAN32", 1, 32, 0),
                new StdGas("EAN50", 2, 50, 0),
                new StdGas("TMX 21/35", 3, 21, 35),
                new StdGas("TMX 30/30", 4, 30, 30),
                new StdGas("Oxygen", 5, 100, 0),
            };
            public StdGas[] StdGasList { get { return _stdGasList; } }
            public int start_o2
            {
                get { return _start_o2; }
                set
                {
                    if (value != _start_o2)
                    {
                        _start_o2 = value;
                        OnPropertyChanged();
                        CalculateGas();
                    }
                }
            }
            public int start_he
            {
                get { return _start_he; }
                set
                {
                    if (value != _start_he)
                    {
                        _start_he = value;
                        OnPropertyChanged();
                        CalculateGas();
                    }
                }
            }
            public int stop_bar
            {
                get { return _stop_bar; }
                set
                {
                    if (value != _stop_bar)
                    {
                        _stop_bar = value;
                        OnPropertyChanged();
                        CalculateGas();
                    }
                }
            }
            public int stop_o2
            {
                get { return _stop_o2; }
                set
                {
                    if (value != _stop_o2)
                    {
                        _stop_o2 = value;
                        OnPropertyChanged();
                        CalculateGas();
                    }
                }
            }
            public int stop_he
            {
                get { return _stop_he; }
                set
                {
                    if (value != _stop_he)
                    {
                        _stop_he = value;
                        OnPropertyChanged();
                        CalculateGas();
                    }
                }
            }

            // the calculation trigeer and output to TextBlocks
            public void CalculateGas()
            {
                TmxCalcClass.TmxResult result;
                VanDerWaals.VdW_Result vdw_result = new VanDerWaals.VdW_Result {status_code = 66, status_txt = "dazed and confused"};

                if (_txt_air != null)
                {
                    //_txt_air.Text = $"start \n"+
                    //    $"{start_bar} {start_o2} {start_he}\n" +
                    //    $"{stop_bar} {stop_o2} {stop_he}";
                    string filltype;
                    //string[] filltypes = { "air", "nx", "tmx", "pp", "cfm" };

                    filltype = TmxCalcClass.filltypes[selected_tab];
                    result = TmxCalcClass.TmxCalc(filltype, start_bar, start_o2, start_he,
                                                stop_bar, stop_o2, stop_he);
                    if (_vdw == true) {
                        vdw_result = VanDerWaals.vdw_calc(start_bar, stop_bar, start_o2, start_he, stop_o2, stop_he, tank_liters, 20.0);
                    }
                    switch (selected_tab)
                    {
                        case (int)TmxCalcClass.FILLTYPE.air:
                            _txt_air.Text = result.status_txt;
                            break;
                        case (int)TmxCalcClass.FILLTYPE.nx:
                            _txt_nitrox.Text = result.status_txt;
                            break;
                        case (int)TmxCalcClass.FILLTYPE.tmx:
                            _txt_tmx.Text = result.status_txt;
                            break;
                        case (int)TmxCalcClass.FILLTYPE.pp:
                            if (_vdw)
                            {
                                _txt_pp.Text = vdw_result.status_txt;
                            }
                            else
                            {
                                _txt_pp.Text = result.status_txt;
                            }
                            break;
                        case (int)TmxCalcClass.FILLTYPE.cfm:
                            _txt_henx.Text = result.status_txt;
                            break;
                        default:
                            selected_tab = (int)TmxCalcClass.FILLTYPE.air;
                            _txt_henx.Text = "Internal error at CalculateGas()";
                            break;
                    }
                    var result_c = TmxCalcClass.Tmx_cost_calc(tank_liters, stop_bar, result.add_o2, result.add_he,
                        o2_price, he_price, compressor_price);
                    txt_cost.Text = result_c.status_txt;

                    calc_result = result.status_txt;
                    cost_result = result_c.status_txt;
                }

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Gas.StdGas selectedGas;
            int selectedId;
            selectedId = (int)rgStdGas.SelectedValue;
            selectedGas = (Gas.StdGas)Gas._stdGasList.Where(i => i.id == selectedId).FirstOrDefault();
            gas.stop_o2 = selectedGas.o2;
            gas.stop_he = selectedGas.he;
            //Console.WriteLine($"{selectedId} {selectedGas.name} {selectedGas.o2} {selectedGas.he}");
        }

        private void rgStdGas_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Gas.StdGas selectedGas;
            int selectedId;
            selectedId = (int)rgStdGas.SelectedValue;
            selectedGas = (Gas.StdGas)Gas._stdGasList.Where(i => i.id == selectedId).FirstOrDefault();
            gas.stop_o2 = selectedGas.o2;
            gas.stop_he = selectedGas.he;
            //Console.WriteLine($"{selectedId} {selectedGas.name} {selectedGas.o2} {selectedGas.he}");
        }

        private void rButton_EmptyTank(object sender, RoutedEventArgs e)
        {
            gas.start_bar = 0;
            gas.start_o2 = 0;
            gas.start_he = 0;
        }

        private void rButton_Copy(object sender, RoutedEventArgs e)
        {
            // copy result text to clipboard
            Clipboard.SetText(gas.calc_result + gas.cost_result);
        }

        private void rBotton_Save(object sender, RoutedEventArgs e)
        {
            string SaveText = gas.calc_result + gas.cost_result;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FileName = "FillCalc";
            saveFileDialog.DefaultExt = "txt";

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, SaveText);
            Console.WriteLine($"wrote file {saveFileDialog.FileName}");
        }

        private void rButton_Print(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Printing function not implemented yet\n" +
                "You can use SAVE to file\nor Copy and then paste to a document",
                "Print report");
        }

        private void rButton_Help(object sender, RoutedEventArgs e)
        {
            //Window1 win1 = new Window1();
            if (HelpWindow != null & HelpWindow.IsLoaded == true)
            {
                HelpWindow.Show();
            }
            else
            {
                // the HelpWindow was not constructed or user closed it
                HelpWindow = InitHelpWin();
                HelpWindow.Show();
            }
        }

        private void rAppMenuExit(object sender, RoutedEventArgs e)
        {
            // close the app
            System.Windows.Application.Current.Shutdown(); // can only call from main thread, calls exit events
        }

        private void rAppMenuAbout(object sender, RoutedEventArgs e)
        {
            Window2 AboutWin = new Window2();
            AboutWin.Show();
        }

        private void rBugs(object sender, RoutedEventArgs e)
        {
            // Launch browser to github issues...
            System.Diagnostics.Process.Start("https://github.com/eianlei/FillCalcWin/issues");
        }
    }
}

