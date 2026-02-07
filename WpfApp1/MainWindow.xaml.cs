using ProjectSoenen.Models;
using ProjectSoenen.Modules;
using System.IO;
using System.Reflection;
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
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<IMachineModule> _activeModules = new List<IMachineModule>();
        private string ProjectRoot => System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private string folderPath => System.IO.Path.Combine(ProjectRoot, "Data", "Machines");
        private string logPath => System.IO.Path.Combine(ProjectRoot, "Data", "ChangeLog.txt");
        public MainWindow()
        {
            InitializeComponent();
            LoadAvailableMachines();
        }
        private void LoadAvailableMachines()
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The directory '{folderPath}' does not exist.");
            }
            var files = Directory.GetFiles(folderPath, "*.xml");
            foreach (var file in files)
            {
                try
                {
                    var doc = XDocument.Load(file);

                    string name = doc.Element("MachineConfig")?.Element("MachineName")?.Value ?? "Unknown";
                    CboMachineSelector.Items.Add(new MachineProfile
                    {
                        MachineName = name,
                        FilePath = file
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading machine profile from file '{file}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void CboMachineSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = CboMachineSelector.SelectedItem as MachineProfile;
            if (selected == null) return;
            MainModuleContainer.Children.Clear();
            XDocument doc = XDocument.Load(selected.FilePath);
            var modules = doc.Descendants("Module")
                .Select(m =>
                {
                    var mod = CreateModuleFromXml(m);
                    mod?.Initialize(m.ToString());
                    return mod;
                })
                .Where(m => m != null)
                .ToList();
            foreach (var module in modules)
            {
                _activeModules.Add(module);
                MainModuleContainer.Children.Add(module.GetControl());
            }
        }
        private IMachineModule CreateModuleFromXml(XElement moduleElement)
        {
            string type = moduleElement.Attribute("Type")?.Value;
            if (string.IsNullOrEmpty(type)) return null;
            switch (type)
            {
                case "Conveyor":
                    return new ConveyorModule();
                case "Cylinder":
                    return new CylinderModule();
                case "Saw":
                    return new SawModule();
                default:
                    MessageBox.Show($"Unknown module type '{type}' in XML configuration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
            }
        }
        private void BtnStartMachine_Click(object sender, RoutedEventArgs e)
        {

        }
       
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var selected = CboMachineSelector.SelectedItem as MachineProfile;
            if (selected == null) return;
            
            try
            {
                this.AddLogging(AppSession.CurrentUser.Id);

                this.UpdateMachineXml(selected.FilePath);

                foreach (var module in _activeModules)
                {
                    module.SynchSnapshot();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message);
            }
        }
        private void AddLogging(int UserId)
        {
            StringBuilder logEntries = new StringBuilder();
            foreach (var module in _activeModules)
            {
                string changes = module.GetLoggingData(AppSession.CurrentUser.Id);
                if (changes != null) logEntries.AppendLine(changes);
            }
            if (logEntries.Length > 0)
            {
                File.AppendAllText(logPath, logEntries.ToString());
            }
        }
        private void UpdateMachineXml(string filePath)
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);

                foreach (var module in _activeModules)
                {
                    var moduleElement = doc.Descendants("Module")
                        .FirstOrDefault(m => m.Attribute("Id")?.Value == module.ModuleId);

                    if (moduleElement != null)
                    {
                        module.UpdateXml(moduleElement);
                    }
                }

                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update machine file: {ex.Message}");
            }
        }
    }
}