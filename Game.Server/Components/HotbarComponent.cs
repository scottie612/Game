using Arch.Core;
using System.Collections.Generic;

namespace Game.Server.Components
{
    public struct HotbarComponent
    {
        public List<EntityReference> Hotbar;
        public int SelectedIndex;
    }
}
