using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace CRMViewerPlugin
{
    class Result
    {
        public enum ResultType { EntityList, Entity, PickList, Record }

        private DataSet _data;
        public DataSet Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private DataTable _data2;
        public DataTable Data2
        {
            get { return _data2; }
            set { _data2 = value; }
        }

        public string Header { get; set; }

        public bool FromCache { get; set; }

        public string SearchText { get; set; }

        public ResultType DataType { get; set; }

        public string Key { get; set; }
    }
}
