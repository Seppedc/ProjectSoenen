using ProjectSoenen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using WpfApp1.Modules;

namespace ProjectSoenen.Modules
{
    public class CylinderModule : IMachineModule
    {
        public ModuleType ModuleName => ModuleType.Conveyor;
        public string ModuleId { get; set; }
        public bool IsActive { get; set; }

        public int PSI { get; set; }

        private int _oldPSI;
        private bool _oldIsActive;

        public void Initialize(string configData)
        {
            if (string.IsNullOrEmpty(configData))
            {
                throw new ArgumentException("Config data cannot be null or empty.");
            }

            XElement config = XElement.Parse(configData);
            ModuleId = config.Attribute("Id")?.Value;
            IsActive = string.Equals(config.Attribute("IsActive")?.Value, "true", StringComparison.OrdinalIgnoreCase);
            var psiAttr = config.Descendants("Parameter")
                             .FirstOrDefault(p => p.Attribute("Name")?.Value == "PSI")
                             ?.Attribute("Value")?.Value;

            if (int.TryParse(psiAttr, out int s)) PSI = s;

            this.SynchSnapshot();
        }
        public void Start() { }
        public void Stop() { }

        public UserControl GetControl()
        {
            return new CylinderControl(this);
        }

        public string GetLoggingData(int UserId)
        {
            if(IsActive == _oldIsActive && PSI == _oldPSI)
            {
                return null;  // No changes, no log entry needed
            }
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return $"{timestamp} | User: {UserId} | ModuleId: {ModuleId} |ModuleName: {ModuleName} | IsActive: {IsActive} (was {_oldIsActive}) | PSI: {PSI} (was {_oldPSI})";
        }

        public XElement CreateXml()
        {
            return new XElement("Module",
                new XAttribute("Type", ModuleName.ToString()),
                new XAttribute("Id", ModuleId),
                new XAttribute("IsActive", IsActive),
                new XElement("Parameters",
                    new XElement("Parameter", new XAttribute("Name", "PSI"), new XAttribute("Value", PSI))
                )
            );
        }

        public void SynchSnapshot()
        {
            this._oldIsActive = this.IsActive;
            this._oldPSI = this.PSI;
        }
        public void UpdateXml(XElement moduleElement)
        {
            moduleElement.SetAttributeValue("IsActive", IsActive.ToString().ToLower());
            var paramsContainer = moduleElement.Element("Parameters");
            if (paramsContainer != null)
            {
                UpdateParameterValue(paramsContainer, "PSI", PSI.ToString());
            }
        }
        private void UpdateParameterValue(XElement container, string name, string value)
        {
            var param = container.Elements("Parameter")
                .FirstOrDefault(p => p.Attribute("Name")?.Value == name);

            param?.SetAttributeValue("Value", value);
        }
    }

}
