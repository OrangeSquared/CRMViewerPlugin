namespace CRMViewerPlugin
{
    partial class ctlCRMViewer
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctlCRMViewer));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbBack = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tslCached = new System.Windows.Forms.ToolStripLabel();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSearch = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tslRecordId = new System.Windows.Forms.ToolStripLabel();
            this.tstbRecordId = new System.Windows.Forms.ToolStripTextBox();
            this.tsbLoadRecordId = new System.Windows.Forms.ToolStripButton();
            this.tsbLoadNewest = new System.Windows.Forms.ToolStripButton();
            this.tsbLoadLatest = new System.Windows.Forms.ToolStripButton();
            this.gbMain = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgvMain = new System.Windows.Forms.DataGridView();
            this.dgvRelationships = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenu.SuspendLayout();
            this.gbMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelationships)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbBack,
            this.toolStripSeparator3,
            this.tslCached,
            this.tsbRefresh,
            this.toolStripSeparator1,
            this.tsbSearch,
            this.toolStripLabel1,
            this.toolStripSeparator2,
            this.tslRecordId,
            this.tstbRecordId,
            this.tsbLoadRecordId,
            this.tsbLoadNewest,
            this.tsbLoadLatest});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1128, 25);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbBack
            // 
            this.tsbBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbBack.Name = "tsbBack";
            this.tsbBack.Size = new System.Drawing.Size(36, 22);
            this.tsbBack.Text = "Back";
            this.tsbBack.Click += new System.EventHandler(this.tsbBack_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tslCached
            // 
            this.tslCached.BackColor = System.Drawing.SystemColors.Control;
            this.tslCached.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tslCached.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tslCached.ForeColor = System.Drawing.Color.DarkRed;
            this.tslCached.Name = "tslCached";
            this.tslCached.Size = new System.Drawing.Size(77, 22);
            this.tslCached.Text = "Cached Copy";
            this.tslCached.Visible = false;
            // 
            // tsbRefresh
            // 
            this.tsbRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbRefresh.Image = ((System.Drawing.Image)(resources.GetObject("tsbRefresh.Image")));
            this.tsbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefresh.Name = "tsbRefresh";
            this.tsbRefresh.Size = new System.Drawing.Size(50, 22);
            this.tsbRefresh.Text = "Refresh";
            this.tsbRefresh.Click += new System.EventHandler(this.tsbRefresh_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbSearch
            // 
            this.tsbSearch.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbSearch.Name = "tsbSearch";
            this.tsbSearch.Size = new System.Drawing.Size(200, 25);
            this.tsbSearch.TextChanged += new System.EventHandler(this.tsbSearch_TextChanged);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(42, 22);
            this.toolStripLabel1.Text = "Search";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tslRecordId
            // 
            this.tslRecordId.Name = "tslRecordId";
            this.tslRecordId.Size = new System.Drawing.Size(57, 22);
            this.tslRecordId.Text = "Record Id";
            this.tslRecordId.Visible = false;
            // 
            // tstbRecordId
            // 
            this.tstbRecordId.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tstbRecordId.Name = "tstbRecordId";
            this.tstbRecordId.Size = new System.Drawing.Size(240, 25);
            this.tstbRecordId.Visible = false;
            this.tstbRecordId.TextChanged += new System.EventHandler(this.tstbRecordId_TextChanged);
            // 
            // tsbLoadRecordId
            // 
            this.tsbLoadRecordId.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbLoadRecordId.Enabled = false;
            this.tsbLoadRecordId.Image = ((System.Drawing.Image)(resources.GetObject("tsbLoadRecordId.Image")));
            this.tsbLoadRecordId.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoadRecordId.Name = "tsbLoadRecordId";
            this.tsbLoadRecordId.Size = new System.Drawing.Size(37, 22);
            this.tsbLoadRecordId.Text = "Load";
            this.tsbLoadRecordId.Visible = false;
            this.tsbLoadRecordId.Click += new System.EventHandler(this.tsbLoadRecordId_Click);
            // 
            // tsbLoadNewest
            // 
            this.tsbLoadNewest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbLoadNewest.Image = ((System.Drawing.Image)(resources.GetObject("tsbLoadNewest.Image")));
            this.tsbLoadNewest.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoadNewest.Name = "tsbLoadNewest";
            this.tsbLoadNewest.Size = new System.Drawing.Size(50, 22);
            this.tsbLoadNewest.Text = "Newest";
            this.tsbLoadNewest.ToolTipText = "Get Newest";
            this.tsbLoadNewest.Visible = false;
            this.tsbLoadNewest.Click += new System.EventHandler(this.tsbLoadNewest_Click);
            // 
            // tsbLoadLatest
            // 
            this.tsbLoadLatest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbLoadLatest.Image = ((System.Drawing.Image)(resources.GetObject("tsbLoadLatest.Image")));
            this.tsbLoadLatest.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoadLatest.Name = "tsbLoadLatest";
            this.tsbLoadLatest.Size = new System.Drawing.Size(42, 22);
            this.tsbLoadLatest.Text = "Latest";
            this.tsbLoadLatest.Click += new System.EventHandler(this.tsbLoadLatest_Click);
            // 
            // gbMain
            // 
            this.gbMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbMain.Controls.Add(this.splitContainer1);
            this.gbMain.Location = new System.Drawing.Point(3, 28);
            this.gbMain.Name = "gbMain";
            this.gbMain.Size = new System.Drawing.Size(1122, 405);
            this.gbMain.TabIndex = 6;
            this.gbMain.TabStop = false;
            this.gbMain.Text = "gbMain";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 16);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgvMain);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgvRelationships);
            this.splitContainer1.Size = new System.Drawing.Size(1116, 386);
            this.splitContainer1.SplitterDistance = 442;
            this.splitContainer1.TabIndex = 1;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // dgvMain
            // 
            this.dgvMain.AllowUserToAddRows = false;
            this.dgvMain.AllowUserToDeleteRows = false;
            this.dgvMain.AllowUserToResizeRows = false;
            this.dgvMain.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvMain.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMain.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvMain.Location = new System.Drawing.Point(0, 0);
            this.dgvMain.MultiSelect = false;
            this.dgvMain.Name = "dgvMain";
            this.dgvMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMain.Size = new System.Drawing.Size(442, 386);
            this.dgvMain.TabIndex = 0;
            this.dgvMain.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMain_CellDoubleClick);
            this.dgvMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgvMain_MouseDown);
            this.dgvMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgvMain_MouseUp);
            // 
            // dgvRelationships
            // 
            this.dgvRelationships.AllowUserToAddRows = false;
            this.dgvRelationships.AllowUserToDeleteRows = false;
            this.dgvRelationships.AllowUserToResizeRows = false;
            this.dgvRelationships.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvRelationships.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRelationships.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRelationships.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvRelationships.Location = new System.Drawing.Point(0, 0);
            this.dgvRelationships.MultiSelect = false;
            this.dgvRelationships.Name = "dgvRelationships";
            this.dgvRelationships.ReadOnly = true;
            this.dgvRelationships.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRelationships.Size = new System.Drawing.Size(670, 386);
            this.dgvRelationships.TabIndex = 1;
            this.dgvRelationships.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRelationships_CellDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // ctlCRMViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbMain);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "ctlCRMViewer";
            this.Size = new System.Drawing.Size(1128, 436);
            this.OnCloseTool += new System.EventHandler(this.MyPluginControl_OnCloseTool);
            this.ConnectionUpdated += new XrmToolBox.Extensibility.PluginControlBase.ConnectionUpdatedHandler(this.ctlCRMViewer_ConnectionUpdated);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ctlCRMViewer_Paint);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.gbMain.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRelationships)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbBack;
        private System.Windows.Forms.GroupBox gbMain;
        private System.Windows.Forms.DataGridView dgvMain;
        private System.Windows.Forms.ToolStripLabel tslCached;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox tsbSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgvRelationships;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripLabel tslRecordId;
        private System.Windows.Forms.ToolStripTextBox tstbRecordId;
        private System.Windows.Forms.ToolStripButton tsbLoadRecordId;
        private System.Windows.Forms.ToolStripButton tsbLoadNewest;
        private System.Windows.Forms.ToolStripButton tsbLoadLatest;
    }
}
