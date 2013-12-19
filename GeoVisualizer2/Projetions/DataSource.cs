using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elte.GeoVisualizer.Lib
{
    public abstract class DataSource
    {
        private static readonly DataSources.Null nullDataSource = new DataSources.Null();

        public static DataSources.Null Null
        {
            get { return nullDataSource; }
        }

        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public abstract string[] GetColumnNames();
        public abstract void Open();
        public abstract void Close();
        public abstract bool ReadNext(object[] values);
    }
}
