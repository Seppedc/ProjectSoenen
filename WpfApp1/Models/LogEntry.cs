using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSoenen.Models;

public class LogEntry
{
    public required DateTime Timestamp { get; set; }
    public required int userId { get; set; }
    public required string Parametername { get; set; }
    public required string OldValue { get; set; }
    public required string NewValue { get; set; }
}
