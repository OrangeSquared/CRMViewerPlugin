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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctlCRMViewer));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbBack = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbOpenInBrowser = new System.Windows.Forms.ToolStripButton();
            this.stbCustomFields = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tslCached = new System.Windows.Forms.ToolStripLabel();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSearch = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tslRecordID = new System.Windows.Forms.ToolStripLabel();
            this.tstbRecordID = new System.Windows.Forms.ToolStripTextBox();
            this.tsbLoadRecord = new System.Windows.Forms.ToolStripButton();
            this.tssRecord = new System.Windows.Forms.ToolStripSeparator();
            this.gbMain = new System.Windows.Forms.GroupBox();
            this.dgvMain = new System.Windows.Forms.DataGridView();
            this.toolStripMenu.SuspendLayout();
            this.gbMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbBack,
            this.toolStripSeparator3,
            this.tsbOpenInBrowser,
            this.stbCustomFields,
            this.tssSeparator1,
            this.tslCached,
            this.tsbRefresh,
            this.toolStripSeparator1,
            this.tsbSearch,
            this.toolStripLabel1,
            this.toolStripSeparator2,
            this.tslRecordID,
            this.tstbRecordID,
            this.tsbLoadRecord,
            this.tssRecord});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1128, 25);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            this.toolStripMenu.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripMenu_KeyDown);
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
            // tsbOpenInBrowser
            // 
            this.tsbOpenInBrowser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbOpenInBrowser.Image = ((System.Drawing.Image)(resources.GetObject("tsbOpenInBrowser.Image")));
            this.tsbOpenInBrowser.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpenInBrowser.Name = "tsbOpenInBrowser";
            this.tsbOpenInBrowser.Size = new System.Drawing.Size(98, 22);
            this.tsbOpenInBrowser.Text = "Open in Browser";
            this.tsbOpenInBrowser.Click += new System.EventHandler(this.tsbOpenInBrowser_Click);
            // 
            // stbCustomFields
            // 
            this.stbCustomFields.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stbCustomFields.Image = ((System.Drawing.Image)(resources.GetObject("stbCustomFields.Image")));
            this.stbCustomFields.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stbCustomFields.Name = "stbCustomFields";
            this.stbCustomFields.Size = new System.Drawing.Size(81, 22);
            this.stbCustomFields.Text = "Show Default";
            this.stbCustomFields.Click += new System.EventHandler(this.stbCustomFields_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 25);
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
            // tslRecordID
            // 
            this.tslRecordID.Name = "tslRecordID";
            this.tslRecordID.Size = new System.Drawing.Size(55, 22);
            this.tslRecordID.Text = "RecordID";
            // 
            // tstbRecordID
            // 
            this.tstbRecordID.Name = "tstbRecordID";
            this.tstbRecordID.Size = new System.Drawing.Size(225, 25);
            this.tstbRecordID.Text = "00000000-0000-0000-0000-000000000000";
            // 
            // tsbLoadRecord
            // 
            this.tsbLoadRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbLoadRecord.Image = ((System.Drawing.Image)(resources.GetObject("tsbLoadRecord.Image")));
            this.tsbLoadRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoadRecord.Name = "tsbLoadRecord";
            this.tsbLoadRecord.Size = new System.Drawing.Size(37, 22);
            this.tsbLoadRecord.Text = "Load";
            this.tsbLoadRecord.Click += new System.EventHandler(this.tsbLoadRecord_Click);
            // 
            // tssRecord
            // 
            this.tssRecord.Name = "tssRecord";
            this.tssRecord.Size = new System.Drawing.Size(6, 25);
            // 
            // gbMain
            // 
            this.gbMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbMain.Controls.Add(this.dgvMain);
            this.gbMain.Location = new System.Drawing.Point(3, 28);
            this.gbMain.Name = "gbMain";
            this.gbMain.Size = new System.Drawing.Size(1122, 405);
            this.gbMain.TabIndex = 6;
            this.gbMain.TabStop = false;
            this.gbMain.Text = "gbMain";
            // 
            // dgvMain
            // 
            this.dgvMain.AllowUserToAddRows = false;
            this.dgvMain.AllowUserToDeleteRows = false;
            this.dgvMain.AllowUserToResizeRows = false;
            this.dgvMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMain.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMain.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvMain.Location = new System.Drawing.Point(6, 19);
            this.dgvMain.MultiSelect = false;
            this.dgvMain.Name = "dgvMain";
            this.dgvMain.ReadOnly = true;
            this.dgvMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMain.Size = new System.Drawing.Size(1110, 380);
            this.dgvMain.TabIndex = 0;
            this.dgvMain.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMain_CellDoubleClick);
            this.dgvMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvMain_KeyDown);
            // 
            // ctlCRMViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbMain);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "ctlCRMViewer";
            this.Size = new System.Drawing.Size(1128, 436);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.gbMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbBack;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.GroupBox gbMain;
        private System.Windows.Forms.DataGridView dgvMain;
        private System.Windows.Forms.ToolStripLabel tslCached;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbOpenInBrowser;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox tsbSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel tslRecordID;
        private System.Windows.Forms.ToolStripTextBox tstbRecordID;
        private System.Windows.Forms.ToolStripButton tsbLoadRecord;
        private System.Windows.Forms.ToolStripSeparator tssRecord;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton stbCustomFields;
    }
}
