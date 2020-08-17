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
using System.Runtime.Remoting.Contexts;
using OrangeSquared.Xrm.CRMViewer;
using System.Web.UI.WebControls;
using System.Net;
using System.Activities.XamlIntegration;

namespace CRMViewerPlugin
{
    //comment
    public partial class ctlCRMViewer : PluginControlBase, IStatusBarMessenger, IXrmToolBoxPluginControl
    {
        private Settings mySettings;
        private Cache mycache;
        private List<Result> results;

        public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;

        #region Plugin Management
        private void LoadSettings()
        {
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

            if (!SettingsManager.Instance.TryLoad(GetType(), out mycache, ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).ConnectedOrgUniqueName + "_cache"))
            {
                mycache = new Cache();
                LogWarning("Cache not found => a new cache file has been created");
                SettingsManager.Instance.Save(GetType(), mycache.ToArray(), ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).ConnectedOrgUniqueName + "_cache");
            }
            else
            {
                LogInfo("Cache found and loaded");
            }
        }
        private void SaveSettings()
        {
            SettingsManager.Instance.Save(GetType(), mySettings);
            SettingsManager.Instance.Save(GetType(), mycache.ToArray(), "OS." + ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).ConnectedOrgUniqueName + "_cache");
        }

        private void LoadCRMInstance()
        {
            results = new List<Result>();
            ExecuteMethod(LoadEntityList);
        }
        #endregion


        #region plugin stuff
        private void ctlCRMViewer_Load(object sender, EventArgs e) { }

        private void ctlCRMViewer_OnCloseTool(object sender, EventArgs e) { SaveSettings(); }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        private void ctlCRMViewer_ConnectionUpdated(object sender, ConnectionUpdatedEventArgs e)
        {
            LoadCRMInstance();
        }
        #endregion

        #region Async stuff
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
            results.Add(results[0]);
            results.Add(results[0]);
            results.Add(results[0]);

            PaintResults();
        }
        #endregion


        #region UI
        private void PaintResults()
        {
            gbMain.SuspendLayout();
            gbMain.Controls.Clear();
            DataGridView dataGridView;
            //if there's only one result then don't mess with splitters
            if (results.Count == 1)
            {
                dataGridView = new DataGridView();
                gbMain.Controls.Add(dataGridView);
                SetupGridView(dataGridView, false, results[0]);
            }
            else
            {
                //first splitcontainer is the one that will have the last result in the right pane
                List<SplitContainer> splitContainers = new List<SplitContainer>();
                bool displayResult = true;
                for (int i = results.Count - 1; i >= 0; i--)
                {
                    if (i > 0)
                    {
                        SplitContainer splitContainer = new SplitContainer();
                        if (splitContainers.Count == 0)
                            gbMain.Controls.Add(splitContainer);
                        else
                            splitContainers.Last().Panel1.Controls.Add(splitContainer);

                        splitContainers.Add(splitContainer);
                        splitContainer.Dock = DockStyle.Fill;
                        dataGridView = new DataGridView();
                        if (i > 0)
                            splitContainer.Panel2.Controls.Add(dataGridView);
                        else
                            splitContainer.Panel1.Controls.Add(dataGridView);
                        SetupGridView(dataGridView, !displayResult, results[i]);
                    }
                    else
                    {
                        dataGridView = new DataGridView();
                        splitContainers.Last().Panel1.Controls.Add(dataGridView);
                        SetupGridView(dataGridView, !displayResult, results[i]);
                    }
                    //initial sizing
                    //if (displayResult)
                    //    dataGridView.Width = dataGridView.RowHeadersWidth + dataGridView.Columns[1].Width + 19;
                    //else
                    //    dataGridView.Width = (flpMain.Width - 100) - dataGridView.Left;

                    displayResult = false;
                }
            }

            gbMain.ResumeLayout();
            SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(string.Format("{0} results loaded", results.Last().Data.Tables[0].Rows.Count)));
        }

        private void SetupGridView(DataGridView dataGridView, bool useSummary, Result result)
        {
            if (useSummary)
                dataGridView.DataSource = result.Data.Tables[1];
            else
                dataGridView.DataSource = result.Data.Tables[0];

            //dataGridView.Columns["Key"].Visible = false;

            for (int col = 1; col < dataGridView.Columns.Count; col++)
                dataGridView.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            dataGridView.Dock = DockStyle.Fill;
        }
        #endregion






        #region Events

        //private void tsbSearch_TextChanged(object sender, EventArgs e)
        //{
        //    DataTable dt = results.Peek().Data.Tables[0];
        //    List<string> newFilter = new List<string>();
        //    for (int i = 1; i < dt.Columns.Count; i++)
        //        newFilter.Add(string.Format("[{0}] LIKE '%{1}%'", dt.Columns[i].ColumnName, tsbSearch.Text));

        //    dt.DefaultView.RowFilter = string.Join(" OR ", newFilter);
        //    results.Peek().SearchText = tsbSearch.Text;
        //}

        //private void dgvMain_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex < 0) return;
        //    NavigateIn(e.RowIndex);
        //}
        //private void dgvRelationships_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex < 0) return;
        //    WorkAsyncInfo wai = new WorkAsyncInfo
        //    {
        //        Message = "Retrieving info from CRM...",
        //        Work = (worker, args) =>
        //        {
        //            Result result = null;
        //            result = Browser.GetEntityResult(Service, cache, dgvRelationships.Rows[e.RowIndex].Cells[3].Value.ToString(), worker);
        //            if (result != null)
        //            {
        //                results.Push(result);
        //                if (!result.FromCache)
        //                    SaveCache();
        //            }

        //        },
        //        ProgressChanged = ProgressChanged,
        //        PostWorkCallBack = NewResultsAvailable,
        //        AsyncArgument = null,
        //        MessageHeight = 150,
        //        MessageWidth = 340
        //    };
        //    WorkAsync(wai);
        //}

        //private void tsbBack_Click(object sender, EventArgs e) { PageBack(); }

        //private void dgvMain_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Escape)
        //        PageBack();

        //    else if (e.KeyCode == Keys.Enter)
        //        NavigateIn(dgvMain.SelectedRows[0].Index);

        //    else if (e.KeyCode == Keys.F2)
        //        tsbSearch.Focus();

        //    else if (e.KeyCode == Keys.F5)
        //        Refresh();

        //    else if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-".Contains(e.KeyCode.ToString()))
        //    {
        //        tsbSearch.Text += e.KeyCode.ToString();
        //    }

        //    e.SuppressKeyPress = true;
        //}

        //private void tsbRefresh_Click(object sender, EventArgs e) { Refresh(); }


        //private void dgvMain_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (dgvMain.HitTest(e.X, e.Y).RowIndex >= 0)
        //        dgvMain.Rows[dgvMain.HitTest(e.X, e.Y).RowIndex].Selected = true;
        //}

        //private void dgvMain_MouseUp(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        MenuItem miCopyKey = new MenuItem("Copy Key");
        //        miCopyKey.Click += (object ss, EventArgs ee) => Clipboard.SetText(dgvMain.SelectedRows[0].Cells[0].Value.ToString());
        //        MenuItem miOpenInBrowser = new MenuItem("Open in Browser");
        //        miOpenInBrowser.Click += MiOpenInBrowser_Click;
        //        MenuItem miOpenRecord = new MenuItem("Open Record");
        //        miOpenRecord.Click += MiOpenRecord_Click;

        //        ContextMenu contextMenu = new ContextMenu();

        //        switch (results.Peek().DataType)
        //        {
        //            case Result.ResultType.EntityList:
        //                contextMenu = new ContextMenu(new MenuItem[]
        //                    {
        //                        miCopyKey,
        //                        miOpenInBrowser,
        //                    });
        //                break;
        //            case Result.ResultType.Entity:
        //                contextMenu = new ContextMenu(new MenuItem[]
        //                    {
        //                        miCopyKey,
        //                        miOpenInBrowser,
        //                        miOpenRecord,
        //                    }); break;
        //            case Result.ResultType.PickList:
        //                contextMenu = new ContextMenu(new MenuItem[]
        //                    {
        //                        miCopyKey,
        //                    });
        //                break;
        //            case Result.ResultType.Record:
        //                contextMenu = new ContextMenu(new MenuItem[]
        //                    {
        //                        miCopyKey,
        //                    });
        //                break;
        //            default:
        //                break;
        //        }

        //        contextMenu.Show(dgvMain, e.Location);
        //    }

        //}

        //private void MiOpenRecord_Click(object sender, EventArgs e)
        //{
        //    string possibleid = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
        //    Guid id = Guid.Empty;

        //    if (!Guid.TryParse(possibleid, out id))
        //    {
        //        string fetchXML = string.Format(@"<fetch top='1' >  <entity name='{0}' >    <attribute name='{0}id' />  </entity></fetch>", results.Peek().EntityLogicalName);
        //        EntityCollection ec = Service.RetrieveMultiple(new FetchExpression(fetchXML));
        //        if (ec.Entities.Count > 0)
        //            id = ec.Entities[0].Id;
        //    }
        //    if (id != Guid.Empty)
        //    {
        //        WorkAsyncInfo wai = new WorkAsyncInfo
        //        {
        //            Message = "Retrieving info from CRM...",
        //            Work = (worker, args) =>
        //            {
        //                Result result = Browser.GetRecordResult(Service, cache, results.Peek().EntityLogicalName, id, worker);
        //                if (result != null)
        //                {
        //                    results.Push(result);
        //                    if (!result.FromCache)
        //                        SaveCache();
        //                }

        //            },
        //            ProgressChanged = ProgressChanged,
        //            PostWorkCallBack = NewResultsAvailable,
        //            AsyncArgument = null,
        //            MessageHeight = 150,
        //            MessageWidth = 340
        //        };
        //        WorkAsync(wai);
        //    }
        //}

        //private void MiOpenInBrowser_Click(object sender, EventArgs e)
        //{
        //    string rootURI = string.Format("https://{0}:{1}",
        //        ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).CrmConnectOrgUriActual.Host,
        //        ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).CrmConnectOrgUriActual.Port);
        //    string targetUri = null;

        //    switch (results.Peek().DataType)
        //    {
        //        case Result.ResultType.EntityList:
        //            targetUri = string.Format("{0}//main.aspx?etn={1}&pagetype=entitylist", rootURI, dgvMain.SelectedRows[0].Cells[0].Value.ToString());
        //            break;
        //        case Result.ResultType.Entity:
        //            targetUri = string.Format("{0}//main.aspx?etn={1}&pagetype=entitylist", rootURI, results.Peek().EntityLogicalName);
        //            break;
        //        case Result.ResultType.PickList:
        //            break;
        //        case Result.ResultType.Record:
        //            //targetUri = string.Format("{0}//main.aspx?etn={1}&pagetype=entityrecord&id={2}", rootURI, results.Peek().EntityLogicalName, recordId);
        //            break;
        //        default:
        //            break;
        //    }

        //    System.Diagnostics.Process.Start(targetUri);
        //}

        //private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        //{
        //    if (mySettings != null && !insetup)
        //    {
        //        mySettings.SplitterPosition = splitContainer1.SplitterDistance;
        //        SettingsManager.Instance.Save(GetType(), mySettings);
        //    }
        //}

        //private void ctlCRMViewer_Paint(object sender, PaintEventArgs e)
        //{
        //    if (insetup)
        //    {
        //        splitContainer1.SplitterDistance = mySettings.SplitterPosition;
        //        insetup = false;
        //    }
        //}

        #endregion

        #region Functions
        private void LoadEntityList()
        {
            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Retrieving info from CRM...",
                Work = (worker, args) =>
                {
                    Result result = Browser.GetEntityListResult(Service, worker);

                    if (result != null)
                        results.Add(result);
                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }


        private void PageBack()
        {
            if (results.Count > 1)
            {
                results.RemoveAt(results.Count - 1);
                PaintResults();
            }
        }

        #endregion

        public ctlCRMViewer()
        {
            InitializeComponent();
        }


        #region Navigation
        //private void NavigateIn(int row)
        //{
        //    WorkAsyncInfo wai = new WorkAsyncInfo
        //    {
        //        Message = "Retrieving info from CRM...",
        //        Work = (worker, args) =>
        //        {
        //            Result result = null;

        //            switch (results.Peek().DataType)
        //            {
        //                case Result.ResultType.EntityList:
        //                    result = Browser.GetEntityResult(Service, cache, dgvMain.Rows[row].Cells[0].Value.ToString(), worker);
        //                    break;
        //                case Result.ResultType.Entity:
        //                    string datatype = dgvMain.Rows[row].Cells[4].Value.ToString();
        //                    if (datatype == "Picklist")
        //                        result = Browser.GetPicklistResult(Service, results.Peek().EntityLogicalName, dgvMain.Rows[row].Cells[0].Value.ToString(), worker);
        //                    else if (datatype == "Lookup")
        //                        result = Browser.GetEntityResult(Service, cache, dgvMain.Rows[row].Cells[5].Value.ToString(), worker);
        //                    break;
        //                case Result.ResultType.PickList:
        //                    break;
        //                case Result.ResultType.Record:
        //                    break;
        //                default:
        //                    break;
        //            }
        //            if (result != null)
        //            {
        //                results.Push(result);
        //                if (!result.FromCache)
        //                    SaveCache();
        //            }

        //        },
        //        ProgressChanged = ProgressChanged,
        //        PostWorkCallBack = NewResultsAvailable,
        //        AsyncArgument = null,
        //        MessageHeight = 150,
        //        MessageWidth = 340
        //    };
        //    WorkAsync(wai);
        //}

        //private void Refresh()
        //{
        //    Result r = results.Peek();
        //    Result newResult = null;
        //    WorkAsyncInfo wai = new WorkAsyncInfo
        //    {
        //        Message = "Retrieving info from CRM...",
        //        Work = (worker, args) =>
        //        {

        //            switch (r.DataType)
        //            {
        //                case Result.ResultType.EntityList:
        //                    newResult = Browser.GetEntityListResult(Service, worker);
        //                    newResult.SearchText = r.SearchText;
        //                    break;
        //                case Result.ResultType.Entity:
        //                    cache.Remove(r.Key);
        //                    newResult = Browser.GetEntityResult(Service, cache, r.Key, worker);
        //                    newResult.SearchText = r.SearchText;
        //                    break;
        //                case Result.ResultType.PickList:
        //                    break;
        //                case Result.ResultType.Record:
        //                    break;
        //                default:
        //                    break;
        //            }
        //            if (newResult != null)
        //            {
        //                results.Pop();
        //                results.Push(newResult);
        //            }
        //        },
        //        ProgressChanged = ProgressChanged,
        //        PostWorkCallBack = NewResultsAvailable,
        //        AsyncArgument = null,
        //        MessageHeight = 150,
        //        MessageWidth = 340
        //    };
        //    WorkAsync(wai);
        //}






        #endregion










        //private void DoQuickSearch()
        //{
        //    if (!string.IsNullOrEmpty(quickSearch) && quickSearch.Length >= 3)
        //    {
        //        if (dgvMain.SelectedRows.Count == 0)
        //            if (quickSearchReverse)
        //                dgvMain.Rows[dgvMain.RowCount - 1].Selected = true;
        //            else
        //                dgvMain.Rows[0].Selected = true;

        //        int selected = dgvMain.SelectedRows[0].Index;
        //        bool found = false;
        //        while (selected < dgvMain.Rows.Count && selected > -1)
        //        {
        //            foreach (DataGridViewCell col in dgvMain.Rows[selected].Cells)
        //                if (col.Value.ToString().ToUpper().Contains(quickSearch))
        //                {
        //                    dgvMain.Rows[selected].Selected = true;
        //                    dgvMain.Rows[selected].Visible = true;
        //                    dgvMain.CurrentCell = dgvMain.Rows[selected].Cells[1];
        //                    SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(string.Format("found '{0}'", quickSearch)));
        //                    found = true;
        //                    break;
        //                }
        //            if (found) break;
        //            selected += quickSearchReverse ? -1 : 1;
        //        }
        //    }
        //}

        //private void toolStripMenu_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Escape)
        //        PageBack();

        //    if (e.KeyCode == Keys.F2)
        //        tsbSearch.Focus();

        //    if (e.KeyCode == Keys.F5)
        //        Refresh();

        //    if (e.KeyCode == Keys.F4)
        //        ((Browser)results.Peek()).OpenInBrowser();

        //}

        //private void tsbLoadRecord_Click(object sender, EventArgs e)
        //{
        //    if (results.Peek().GetType().Name == "Browser" && ((Browser)results.Peek()).currentSelectionType == Browser.SelectionType.Entity)
        //    {
        //        Guid recordId = Guid.Empty;
        //        if (Guid.TryParse(tstbRecordID.Text.Trim(), out recordId))
        //        {
        //            WorkAsyncInfo wai = new WorkAsyncInfo
        //            {
        //                Message = "Retrieving info from CRM...",
        //                Work = (worker, args) =>
        //                {
        //                    Browser browser = new Browser(Service);
        //                    browser.LoadRecord(((Browser)results.Peek()).EntityLogicalName, recordId, stbCustomFields.Checked, worker);
        //                    Result result = browser;

        //                    if (result != null)
        //                        results.Push(result);
        //                },
        //                ProgressChanged = ProgressChanged,
        //                PostWorkCallBack = NewResultsAvailable,
        //                AsyncArgument = null,
        //                MessageHeight = 150,
        //                MessageWidth = 340
        //            };
        //            WorkAsync(wai);


        //        }
        //    }
        //}

        //private void stbCustomFields_Click(object sender, EventArgs e)
        //{
        //    stbCustomFields.Checked = !stbCustomFields.Checked;
        //    mySettings.ShowDefaultAttributes = stbCustomFields.Checked;
        //    SettingsManager.Instance.Save(GetType(), mySettings);
        //    WorkAsyncInfo wai = new WorkAsyncInfo
        //    {
        //        Message = "Retrieving info from CRM...",
        //        Work = (worker, args) =>
        //        {
        //            results.Peek().Refresh(worker);
        //        },
        //        ProgressChanged = ProgressChanged,
        //        PostWorkCallBack = NewResultsAvailable,
        //        AsyncArgument = null,
        //        MessageHeight = 150,
        //        MessageWidth = 340
        //    };
        //    WorkAsync(wai);

        //}

        //internal string SettingValue(string key)
        //{
        //    switch (key)
        //    {
        //        case "showDefaultAttributes": return stbCustomFields.Checked ? "true" : "false"; break;
        //        default: return ""; break;
        //    }
        //}


        //private void miCopyKey_Click(object sender, EventArgs e)
        //{
        //    Clipboard.SetText(dgvMain.SelectedRows[0].Cells[1].Value.ToString());
        //}

        //private void miCopyAll_Click(object sender, EventArgs e)
        //{
        //    DataTable dt = results.Peek().Data;
        //    string[,] cb = new string[dt.Rows.Count, dt.Columns.Count - 1];
        //    StringBuilder sb = new StringBuilder();

        //    for (int row = 0; row < dt.Rows.Count; row++)
        //    {
        //        for (int col = 0; col < cb.GetUpperBound(1); col++)
        //            cb[row, col] = dt.Rows[row][col + 1].ToString();
        //        object[] notfirstcolumn = new object[dt.Columns.Count - 1];
        //        for (int i = 0; i <= notfirstcolumn.GetUpperBound(0); i++)
        //            notfirstcolumn[i] = dt.Rows[row].ItemArray[i + 1];
        //        sb.Append(string.Join("\t", Array.ConvertAll<object, string>(notfirstcolumn, ConvertObjectToString)) + "\r\n");
        //    }
        //    //Clipboard.SetDataObject(cb);
        //    Clipboard.SetText(sb.ToString());
        //}

        //private string ConvertObjectToString(object input)
        //{
        //    return input?.ToString() ?? string.Empty;
        //}
    }
}