using ProjectSoenen.Modules;
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

namespace WpfApp1.Modules;

/// <summary>
/// Interaction logic for ConveyorControl.xaml
/// </summary>
public partial class ConveyorControl : UserControl
{
    public ConveyorControl(ConveyorModule module)
    {
        InitializeComponent();
        this.DataContext = module;
    }
}
