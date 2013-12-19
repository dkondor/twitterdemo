using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Elte.GeoVisualizer.Lib.DataSources
{
    public class Geography : DataSource
    {
        private List<SqlGeography> geographies;
        private IEnumerator<SqlGeography> enumerator;

        public List<SqlGeography> Geographies
        {
            get { return geographies; }
        }

        public Geography()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            this.geographies = new List<SqlGeography>();
        }

        public override string[] GetColumnNames()
        {
            return new string[] { "geom" };
        }

        public override void Open()
        {
            enumerator = geographies.GetEnumerator();
        }

        public override void Close()
        {
            enumerator = null;
        }

        public override bool ReadNext(object[] values)
        {
            if (enumerator.MoveNext())
            {
                values[0] = enumerator.Current;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
