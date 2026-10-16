using System;

namespace Treynessen.Database.Entities
{
    public class SecurityInstallation
    {
        public int ID { get; set; }
        public int SchemaVersion { get; set; }
        public DateTime InitializedUtc { get; set; }
        public bool LegacyPasswordResetCompleted { get; set; }
    }
}
