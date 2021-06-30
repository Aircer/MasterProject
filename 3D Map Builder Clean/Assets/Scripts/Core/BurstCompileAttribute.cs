using System;

namespace MapTileGridCreator.Core
{
    internal class BurstCompileAttribute : Attribute
    {
        public bool CompileSynchronously { get; set; }
    }
}