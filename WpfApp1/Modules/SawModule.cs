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
    public class SawModule : IMachineModule
    {
        public ModuleType ModuleName => ModuleType.Saw;

        public string ModuleId { get; set; }
        public bool IsActive { get; set; }

        public double BladeSpeed { get; set; }
        public string SawAngle { get; set; }

        private double _oldBladeSpeed;
        private string _oldSawAngle;
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

            var speedAttr = config.Descendants("Parameter")
                               .FirstOrDefault(p => p.Attribute("Name")?.Value == "BladeSpeed")
                               ?.Attribute("Value")?.Value;

            var angleAttr = config.Descendants("Parameter")
                             .FirstOrDefault(p => p.Attribute("Name")?.Value == "SawAngle")
                             ?.Attribute("Value")?.Value;

            if (int.TryParse(speedAttr, out int s)) BladeSpeed = s;
            SawAngle = angleAttr ?? "0";

            this.SynchSnapshot();
        }
        public void Start() { }
        public void Stop() { }

        public UserControl GetControl()
        {
            return new SawControl(this);
        }

        public string GetLoggingData(int UserId)
        {
            if(IsActive == _oldIsActive && BladeSpeed == _oldBladeSpeed && SawAngle == _oldSawAngle)
            {
                return null; // No changes, no log entry needed
            }
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return $"{timestamp} | User: {UserId} | ModuleId: {ModuleId} |ModuleName: {ModuleName} | IsActive: {IsActive} (was {_oldIsActive}) | BladeSpeed: {BladeSpeed} (was {_oldBladeSpeed}) | SawAngle: {SawAngle} (was {_oldSawAngle})";
        }

        public XElement CreateXml()
        {
            return new XElement("Module",
                new XAttribute("Type", ModuleName.ToString()),
                new XAttribute("Id", ModuleId),
                new XAttribute("IsActive",IsActive),
                new XElement("Parameters",
                    new XElement("Parameter",new XAttribute("Name", "BladeSpeed"),new XAttribute("Value", BladeSpeed)),
                    new XElement("Parameter",new XAttribute("Name", "SawAngle"),new XAttribute("Value", SawAngle))
                )
            );
        }

        public void SynchSnapshot()
        {
            this._oldIsActive = this.IsActive;
            this._oldBladeSpeed = this.BladeSpeed;
            this._oldSawAngle = this.SawAngle;
        }

        public void UpdateXml(XElement moduleElement)
        {
            moduleElement.SetAttributeValue("IsActive", IsActive.ToString().ToLower());
            var paramsContainer = moduleElement.Element("Parameters");
            if (paramsContainer != null)
            {
                UpdateParameterValue(paramsContainer, "BladeSpeed", BladeSpeed.ToString());
                UpdateParameterValue(paramsContainer, "SawAngle", SawAngle.ToString());

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
