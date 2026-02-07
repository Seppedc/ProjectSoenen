using ProjectSoenen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ProjectSoenen.Modules;

public interface IMachineModule
{
    ModuleType ModuleName { get; }
    string ModuleId { get; }
    bool IsActive { get; }

    void Initialize(string configData);
    void Start();
    void Stop();

    UserControl GetControl();

    //logging
    String GetLoggingData(int UserId);
    XElement CreateXml();
    void SynchSnapshot();
    void UpdateXml(XElement moduleElement);
}
