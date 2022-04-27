using Stenn.EntityConventions.Contacts;

namespace Stenn.EntityFrameworkCore.EntityConventions
{
    public sealed class EntityConventionsOptions
    {
        /// <summary>
        /// Include common entity conventions
        /// </summary>
        public bool IncludeCommonConventions { get; set; } = true;
        
        public EntityConventionsCommonDefaultsOptions Defaults { get;  } = new();
    }
}