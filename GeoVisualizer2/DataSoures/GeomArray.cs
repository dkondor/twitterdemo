using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using System.Drawing;


namespace Elte.GeoVisualizer.Lib.DataSources
{
    public class GeomArray : DataSource
    {
        private List<SqlGeography> geom;
        private List<int> geomids;
        private double[] values;
        private Color[] colors;
        private int geomcnt;
        private int counter;
        private int counter2;
        public Color StaticColor;

        /// <summary>
        /// if set, we return the IDs instead of the Geom objects
        /// </summary>
        public bool UseIDs;

        /// <summary>
        /// "focused" region, this is displayed with the specified static color
        /// </summary>
        public int RFocus;

        /// <summary>
        /// set everything to zero
        /// </summary>
        public GeomArray()
        {
            geom = null;
            values = null;
            geomids = null;
            colors = null;
            geomcnt = 0;
            counter = 0;
            counter2 = 0;
            RFocus = -1;
            StaticColor = Color.Gray;
        }

        /// <summary>
        /// Get the IDs of the Geoms currently stored
        /// </summary>
        /// <returns></returns>
        public int[] GetIDs() {
            return geomids.ToArray();
        }
        
        /// <summary>
        /// load the geom objects from the database
        /// </summary>
        /// <param name="query">An sql query, which returns the ids and the actual geography objects to be used. The result MUST be ordered by the ids.</param>
        /// <param name="cstr">Connection string to use</param>
        public void LoadGeom(string query, string cstr)
        {
            if (geom != null) {
                geom = null;
            }
            if (values != null) {
                values = null;
            }
            if (geomids != null) {
                geomids = null;
            }
            geom = new List<SqlGeography>();
            geomids = new List<int>();
            geomcnt = 0;
            SqlConnection connection = new SqlConnection(cstr);
            connection.Open();
            SqlCommand cmd = new SqlCommand(query, connection);
            SqlDataReader sr = cmd.ExecuteReader();

            Object[] data = new Object[2];
            while (sr.Read())
            {
                int cnt = sr.GetValues(data);
                if (cnt < 2) {
                    throw new Exception("GeomArray::LoadGeom(): error executing query!\n");
                }
                int id = (int)data[0];
                if (geomcnt > 0) {
                    if (id <= geomids[geomcnt - 1]) {
                        throw new Exception("GeomArray::LoadGeom(): query result is not ordered, or duplicate IDs!\n");
                    }
                }
                geom.Add((SqlGeography)data[1]);
                geomids.Add(id);
                geomcnt++;
            }
            connection.Close();
        }

        /// <summary>
        /// load the array to be displayed
        /// </summary>
        /// <param name="ids">IDs corresponding to the values to display -- this MUST be ordered, regions missing will have 0.0 value</param>
        /// <param name="val">values to display (should be between 0.0 and 1.0)</param>
        public void LoadArray(int[] ids, double[] val)
        {
            values = new double[geomcnt];
            colors = null;
            int i = 0, j = 0;
            while (i < ids.Length) {
                while (j < geomcnt) {
                    if (geomids[j] >= ids[i]) break;
                    values[j] = 0.0;
                    j++;
                }
                if (j == geomcnt) break;
                if (geomids[j] == ids[i]) {
                    values[j] = val[i];
                    j++;
                }
                i++;
            }

            for (; j < geomcnt; j++) values[j] = 0.0;
        }

        public void LoadArrayColors(int[] ids, Color[] c1) {
            colors = new Color[geomcnt];
            values = null;
            int i = 0, j = 0;
            while (i < ids.Length) {
                while (j < geomcnt) {
                    if (geomids[j] >= ids[i]) break;
                    colors[j] = StaticColor;
                    j++;
                }
                if (j == geomcnt) break;
                if (geomids[j] == ids[i]) {
                    colors[j] = c1[i];
                    j++;
                }
                i++;
            }

            for (; j < geomcnt; j++) colors[j] = StaticColor;
        }

        public override string[] GetColumnNames()
        {
            if (geom == null || (values == null && colors == null) )
            {
                throw new Exception("GeomArray::GetColumnNames(): no data loaded!\n");
            }
            if (colors != null) return new string[] { "Geom", "color" };
            else return new string[] { "Geom", "value" };
        }

        public override void Open()
        {
            if (geom == null || (values == null && colors == null) )
            {
                throw new Exception("GeomArray::GetColumnNames(): no data loaded!\n");
            }
            counter = 0;
        }

        public override void Close()
        {
            if (geom == null || (values == null && colors == null) )
            {
                throw new Exception("GeomArray::GetColumnNames(): no data loaded!\n");
            }
            counter = 0;
        }

        public override bool ReadNext(object[] values1)
        {
            if (geom == null || (values == null && colors == null) )
            {
                throw new Exception("GeomArray::GetColumnNames(): no data loaded!\n");
            }
            if (counter == geomcnt) return false;
            if (UseIDs) values1[0] = geomids[counter];
            else values1[0] = geom[counter];
            if (colors != null) values1[1] = colors[counter];
            else {
                if (RFocus > 0 && geomids[counter] == RFocus) values1[1] = StaticColor;
                else values1[1] = values[counter];
            }
            counter++;
            return true;
        }

        /// <summary>
        /// Get the Geoms and their IDs (instead of the values)
        /// </summary>
        public void OpenForIDs() {
            if (geom == null || geomids == null) {
                throw new Exception("GeomArray.OpenForIDs(): no data loaded!\n");
            }
            counter2 = 0;
        }

        /// <summary>
        /// Get a Geom and a corresponding ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="geom"></param>
        /// <returns></returns>
        public bool ReadNextIDs(out int id, out SqlGeography geom) {
            if (this.geom == null || geomids == null) {
                throw new Exception("GeomArray.ReadNextIDs(): no data loaded!\n");
            }
            if (counter2 == geomcnt) {
                id = 0;
                geom = null;
                return false;
            }
            id = geomids[counter2];
            geom = this.geom[counter2];
            counter2++;
            return true;
        }

        public void CloseForIDs() {
            counter2 = 0;
        }
    }
}
