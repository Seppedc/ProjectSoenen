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
    public class ConveyorModule : IMachineModule
    {
        public ModuleType ModuleName => ModuleType.Conveyor;
        public string ModuleId { get; set; }
        public bool IsActive {get; set; }

        public int Speed { get; set; }
        public int Direction { get; set; }

        private int _oldsSpeed;
        private int _oldDirection;
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
                               .FirstOrDefault(p => p.Attribute("Name")?.Value == "Speed")
                               ?.Attribute("Value")?.Value;

            var dirAttr = config.Descendants("Parameter")
                             .FirstOrDefault(p => p.Attribute("Name")?.Value == "Direction")
                             ?.Attribute("Value")?.Value;
            if (int.TryParse(speedAttr, out int s)) Speed = s;
            if (int.TryParse(dirAttr, out int d)) Direction = d;

            this.SynchSnapshot();
        }
        public void Start() { }
        public void Stop() { }

        public UserControl GetControl()
        {
            return new ConveyorControl(this);
        }

        public string GetLoggingData(int UserId)
        {
           if(IsActive == _oldIsActive && Speed == _oldsSpeed && Direction == _oldDirection)
           {
                return null; // No changes, no log entry needed
           }
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return $"{timestamp} | User: {UserId} | ModuleId: {ModuleId} |ModuleName: {ModuleName} | IsActive: {IsActive} (was {_oldIsActive}) | Speed: {Speed} (was {_oldsSpeed}) | Direction: {Direction} (was {_oldDirection})";
        }

        public XElement CreateXml()
        {
            return new XElement("Module",
                new XAttribute("Type", ModuleName.ToString()),
                new XAttribute("Id", ModuleId),
                new XAttribute("IsActive", IsActive),
                new XElement("Parameters",
                    new XElement("Parameter", new XAttribute("Name", "Speed"), new XAttribute("Value", Speed)),
                    new XElement("Parameter", new XAttribute("Name", "Direction"), new XAttribute("Value", Direction))
                )
            );
        }

        public void SynchSnapshot()
        {
            this._oldDirection = this.Direction;
            this._oldsSpeed = this.Speed;
            this._oldIsActive = this.IsActive;
        }

        public void UpdateXml(XElement moduleElement)
        {
            moduleElement.SetAttributeValue("IsActive", IsActive.ToString().ToLower());
            var paramsContainer = moduleElement.Element("Parameters");
            if (paramsContainer != null)
            {
                UpdateParameterValue(paramsContainer, "Speed", Speed.ToString());
                UpdateParameterValue(paramsContainer, "Direction", Direction.ToString());
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
