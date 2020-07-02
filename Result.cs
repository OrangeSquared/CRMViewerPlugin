using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmToolBox.Extensibility;

namespace CRMViewerPlugin
{
    abstract class Result
    {
        internal IOrganizationService service;

        private DataTable _data;
        public DataTable Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public abstract string Header { get; }
        public abstract Result ProcessSelection(object sender, object selection, System.ComponentModel.BackgroundWorker worker);
        public abstract void Refresh(System.ComponentModel.BackgroundWorker worker);

        public abstract string Status { get;  }

        public bool FromCache { get; set; }

        public string SearchText { get; set; }

    }
}
