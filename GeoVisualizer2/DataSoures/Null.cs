using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elte.GeoVisualizer.Lib.DataSources
{
    public class Null : DataSource
    {
        public override bool ReadNext(object[] values)
        {
            return false;
        }

        public override string[] GetColumnNames()
        {
            return new string[0];
        }

        public override void Open()
        {
            
        }

        public override void Close()
        {
            
        }
    }
}
