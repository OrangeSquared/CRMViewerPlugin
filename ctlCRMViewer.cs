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
using XrmToolBox.Extensibility.Interfaces;
using XrmToolBox.Extensibility.Args;

namespace CRMViewerPlugin
{
    public partial class ctlCRMViewer : PluginControlBase, IStatusBarMessenger
    {
        private Settings mySettings;
        private Stack<Result> results;
        private DataTable activeList;

        private string quickSearch = null;
        private DateTime quickSearchTimeout;
        private bool quickSearchReverse = false;

        public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;

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
            //results.Push(new TopMenu(Service));

            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Retrieving info from CRM...",
                Work = (worker, args) =>
                {
                    Browser browser = new Browser(Service);
                    browser.LoadEntitiesList(worker);

                    if (browser != null)
                        results.Push(browser);
                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }

        private void PaintResults()
        {
            activeList = results.Peek().Data;
            tslCached.Visible = results.Peek().FromCache;

            List<string> header = new List<string>();
            foreach (Result r in results)
                header.Add(r.Header);
            gbMain.Text = string.Join(" <= ", header);

            if (string.IsNullOrEmpty(results.Peek().SearchText))
            {
                dgvMain.DataSource = activeList;
            }
            else
            {
                List<string> filter = new List<string>();
                for (int i = 1; i < activeList.Columns.Count; i++)
                    filter.Add(string.Format("[{0}] LIKE '*{1}*'", activeList.Columns[i].ColumnName, results.Peek().SearchText));
                string fullfilter = string.Join(" OR ", filter);
                DataView dv = new DataView(activeList);
                dv.RowFilter = fullfilter;
                dgvMain.DataSource = dv;
            }

            dgvMain.Columns["Key"].Visible = false;
            dgvMain.RowHeadersWidth = dgvMain.ColumnHeadersHeight;
            tsbSearch.Text = results.Peek().SearchText;
            tsbOpenInBrowser.Visible = (results.Peek().GetType().Name == "Browser");

            if (results.Peek().GetType().Name == "Browser" && ((Browser)results.Peek()).currentSelectionType == Browser.SelectionType.Entity)
            {
                tslRecordID.Visible = true;
                tstbRecordID.Visible = true;
                tsbLoadRecord.Visible = true;
                tssRecord.Visible = true;

            }
            else
            {
                tslRecordID.Visible = false;
                tstbRecordID.Visible = false;
                tsbLoadRecord.Visible = false;
                tssRecord.Visible = false;
            }

            SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(string.Format("{0} results loaded", dgvMain.Rows.Count)));
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
            int tic = 1;
            if (progressChangedEventArgs.ProgressPercentage > 0)
                tic += progressChangedEventArgs.ProgressPercentage % 3;
            SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(progressChangedEventArgs.ProgressPercentage, "Loading" + new string('.', tic)));
        }

        private void NewResultsAvailable(RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            PaintResults();
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
            Refresh();
        }

        private void Refresh()
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
            if (tsbSearch.Text != results.Peek().SearchText)
            {
                results.Peek().SearchText = tsbSearch.Text;
                PaintResults();
            }
        }



        private void dgvMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                PageBack();

            else if (e.KeyCode == Keys.F2)
                tsbSearch.Focus();

            else if (e.KeyCode == Keys.F5)
                Refresh();

            else if (e.KeyCode == Keys.F4)
                ((Browser)results.Peek()).OpenInBrowser();

            else if (e.KeyCode == Keys.Left)
            {
                quickSearchReverse = true;
                if (dgvMain.SelectedRows[0].Index == 0)
                    dgvMain.Rows[dgvMain.Rows.Count - 1].Selected = true;
                else if (dgvMain.SelectedRows[0].Index == (dgvMain.Rows.Count - 1))
                    dgvMain.Rows[0].Selected = true;
                else
                    dgvMain.Rows[dgvMain.SelectedRows[0].Index - 1].Selected = true;

                DoQuickSearch();
            }

            else if (e.KeyCode == Keys.Right)
            {
                quickSearchReverse = false;
                if (dgvMain.SelectedRows[0].Index == (dgvMain.Rows.Count - 1))
                    dgvMain.Rows[0].Selected = true;
                else
                    dgvMain.Rows[dgvMain.SelectedRows[0].Index + 1].Selected = true;

                DoQuickSearch();
            }

            else if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-".Contains(e.KeyCode.ToString()))
            {
                quickSearchReverse = false;
                if (quickSearchTimeout > DateTime.Now)
                    quickSearch += e.KeyCode.ToString();
                else
                    quickSearch = e.KeyCode.ToString();
                quickSearchTimeout = DateTime.Now.AddMilliseconds(500);
                SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(string.Format("searching '{0}'", quickSearch)));
                DoQuickSearch();
            }
        }

        private void DoQuickSearch()
        {
            if (!string.IsNullOrEmpty(quickSearch) && quickSearch.Length >= 3)
            {
                if (dgvMain.SelectedRows.Count == 0)
                    if (quickSearchReverse)
                        dgvMain.Rows[dgvMain.RowCount - 1].Selected = true;
                    else
                        dgvMain.Rows[0].Selected = true;

                int selected = dgvMain.SelectedRows[0].Index;
                bool found = false;
                while (selected < dgvMain.Rows.Count && selected > -1)
                {
                    foreach (DataGridViewCell col in dgvMain.Rows[selected].Cells)
                        if (col.Value.ToString().ToUpper().Contains(quickSearch))
                        {
                            dgvMain.Rows[selected].Selected = true;
                            dgvMain.Rows[selected].Visible = true;
                            dgvMain.CurrentCell = dgvMain.Rows[selected].Cells[1];
                            SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(string.Format("found '{0}'", quickSearch)));
                            found = true;
                            break;
                        }
                    if (found) break;
                    selected += quickSearchReverse ? -1 : 1;
                }
            }
        }

        private void toolStripMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                PageBack();

            if (e.KeyCode == Keys.F2)
                tsbSearch.Focus();

            if (e.KeyCode == Keys.F5)
                Refresh();

            if (e.KeyCode == Keys.F4)
                ((Browser)results.Peek()).OpenInBrowser();

        }

        private void tsbLoadRecord_Click(object sender, EventArgs e)
        {
            if (results.Peek().GetType().Name == "Browser" && ((Browser)results.Peek()).currentSelectionType == Browser.SelectionType.Entity)
            {
                Guid recordId = Guid.Empty;
                if (Guid.TryParse(tstbRecordID.Text.Trim(), out recordId))
                {
                    WorkAsyncInfo wai = new WorkAsyncInfo
                    {
                        Message = "Retrieving info from CRM...",
                        Work = (worker, args) =>
                        {
                            Browser browser = new Browser(Service);
                            browser.LoadRecord(((Browser)results.Peek()).EntityLogicalName, recordId, worker);
                            Result result = browser;

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
            }
        }
    }
}