using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;
using System.Dynamic;

namespace CRMViewerPlugin
{
    public partial class ctlCRMViewer : PluginControlBase
    {
        private Settings mySettings;
        private Stack<Result> results;
        private DataTable activeList;

        public string CurrentSearchString { get; private set; }

        public ctlCRMViewer()
        {
            InitializeComponent();
            results = new Stack<Result>();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            //ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");

                SettingsManager.Instance.Save(GetType(), mySettings);
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            results = new Stack<Result>();
            ExecuteMethod(LoadTopMenu);
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void tsbSample_Click(object sender, EventArgs e)
        {
            // The ExecuteMethod method handles connecting to an
            // organization if XrmToolBox is not yet connected
            ExecuteMethod(GetAccounts);
        }

        private void GetAccounts()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting accounts",
                Work = (worker, args) =>
                {
                    args.Result = Service.RetrieveMultiple(new QueryExpression("account")
                    {
                        TopCount = 50
                    });
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    var result = args.Result as EntityCollection;
                    if (result != null)
                    {
                        MessageBox.Show($"Found {result.Entities.Count} accounts");
                    }
                }
            });
        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
                LoadTopMenu();
            }
        }

        private void LoadTopMenu()
        {
            results.Push(new TopMenu(Service));
            PaintResults();
        }

        private void PaintResults()
        {
            activeList = results.Peek().Data;
            tslCached.Visible = results.Peek().FromCache;

            List<string> header = new List<string>();
            foreach (Result r in results)
                header.Add(r.Header);
            gbMain.Text = string.Join(" <= ", header);

            if (string.IsNullOrEmpty(CurrentSearchString))
            {
                dgvMain.DataSource = activeList;
            }
            else
            {
                List<string> filter = new List<string>();
                for (int i = 1; i < activeList.Columns.Count; i++)
                    filter.Add(string.Format("[{0}] LIKE '{1}*'", activeList.Columns[i].ColumnName, CurrentSearchString));
                string fullfilter = string.Join(" OR ", filter);
                DataView dv = new DataView(activeList);
                dv.RowFilter = fullfilter;
                dgvMain.DataSource = dv;
            }

            dgvMain.Columns["Key"].Visible = false;
            dgvMain.RowHeadersWidth = dgvMain.ColumnHeadersHeight;

            tsbOpenInBrowser.Visible = (results.Peek().GetType().Name == "Browser");
        }

        private void dgvMain_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var key = dgvMain.Rows[e.RowIndex].Cells["Key"].Value;
            int stackcount = results.Count;

            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Retrieving info from CRM...",
                Work = (worker, args) =>
                  {
                      Result result = results.Peek().ProcessSelection(key, worker);


                      if (result != null)
                          results.Push(result);
                  },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }

        private void ProgressChanged(ProgressChangedEventArgs progressChangedEventArgs)
        {
            SetWorkingMessage(string.Format("{0}% complete", progressChangedEventArgs.ProgressPercentage));
        }

        private void NewResultsAvailable(RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            CurrentSearchString = null;
            PaintResults();
            tsbSearch.Text = null;
        }

        private void tsbBack_Click(object sender, EventArgs e)
        {
            PageBack();
        }

        private void PageBack()
        {
            if (results.Count > 1)
            {
                results.Pop();
                PaintResults();
            }
        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {

            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Retrieving info from CRM...",
                Work = (worker, args) => { results.Peek().Refresh(worker); },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);


        }

        private void tsbOpenInBrowser_Click(object sender, EventArgs e)
        {
            ((Browser)results.Peek()).OpenInBrowser();
        }

        private void tsbSearch_TextChanged(object sender, EventArgs e)
        {
            CurrentSearchString= tsbSearch.Text;
            PaintResults();
        }



        private void dgvMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                PageBack();
        }

        private void toolStripMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                PageBack();

        }
    }
}