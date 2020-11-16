using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace MyCalendar.Model
{
    public class FeatureGroupRole
    {
        public string RoleIds { get; set; }
        public int FeatureId { get; set; }
        public string FeatureRoleName { get; set; }
        public bool ReadRight { get; set; }
        public bool SaveRight { get; set; }
        public bool DeleteRight { get; set; }
        public bool FullRights { get; set; }
        public string FeatureName { get; set; }
        public string GroupName { get; set; }
        public int GroupId { get; set; }
        [DbIgnore]
        public IEnumerable<Guid> RoleIdsList => (RoleIds != null && RoleIds.Any() ? RoleIds.Split(',').Select(x => Guid.Parse(x)) : Enumerable.Empty<Guid>());
    }
}
