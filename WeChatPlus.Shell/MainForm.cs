using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeChatPlus.Core.Contracts;
using WeChatPlus.Core.Models;
using WeChatPlus.Core.Services;

namespace WeChatPlus.Shell
{
    public sealed class MainForm : Form
    {
        private readonly string _dataRoot;
        private readonly QuickReplyRepository _replyRepository;
        private readonly TrialLicenseService _licenseService;
        private readonly AccountRepository _accountRepository;
        private readonly ComponentRepository _componentRepository;
        private readonly LicenseApiClient _licenseApiClient;
        private readonly DiagnosticsLogService _diagnosticsLog;
        private readonly string _helperPath;

        private ListBox _accountList;
        private ListBox _categoryList;
        private ListBox _replyList;
        private TextBox _searchBox;
        private Label _helperStatus;
        private Label _workspaceStatus;
        private Label _processStatus;
        private CheckBox _hideForScreenshot;
        private TableLayoutPanel _mainLayout;
        private Control _rightPanel;
        private bool _rightPanelCollapsed;
        private QuickReply[] _currentReplies;
        private AccountRecord[] _currentAccounts;

        public MainForm()
        {
            _dataRoot = AppPaths.GetDefaultDataRoot();
            _replyRepository = new QuickReplyRepository(_dataRoot);
            _licenseService = new TrialLicenseService(_dataRoot);
            _accountRepository = new AccountRepository(_dataRoot);
            _componentRepository = new ComponentRepository(_dataRoot);
            _licenseApiClient = new LicenseApiClient("https://license.example.invalid/api");
            _diagnosticsLog = new DiagnosticsLogService(_dataRoot);
            _helperPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WeChatPlus.OpenHelper.exe");

            InitializeComponent();
            LoadData();
            RefreshHelperStatus();
        }

        private void InitializeComponent()
        {
            Text = "微信多开商用工作台 V0.1";
            MinimumSize = new Size(1180, 720);
            Size = new Size(1260, 800);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.FromArgb(246, 247, 249);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 2;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(CreateTopBar(), 0, 0);
            root.Controls.Add(CreateMainArea(), 0, 1);
        }

        private Control CreateTopBar()
        {
            Panel top = new Panel();
            top.Dock = DockStyle.Fill;
            top.BackColor = Color.White;
            top.Padding = new Padding(18, 10, 18, 10);

            Label title = new Label();
            title.AutoSize = true;
            title.Text = "微信多开商用工作台";
            title.Font = new Font(Font, FontStyle.Bold);
            title.Location = new Point(18, 18);
            top.Controls.Add(title);

            _helperStatus = new Label();
            _helperStatus.AutoSize = false;
            _helperStatus.Size = new Size(350, 20);
            _helperStatus.ForeColor = Color.FromArgb(80, 80, 80);
            _helperStatus.Location = new Point(190, 18);
            top.Controls.Add(_helperStatus);

            Button memberButton = TopButton("会员", 560, ShowMemberState);
            Button updateButton = TopButton("检查更新", 645, CheckUpdatesClicked);
            Button noticeButton = TopButton("开源声明", 750, ShowOpenSourceNotice);
            Button refreshButton = TopButton("刷新助手", 855, delegate { RefreshHelperStatus(); });
            Button diagnosticsButton = TopButton("诊断", 960, ExportDiagnosticsClicked);
            Button collapseButton = TopButton("收起侧栏", 1065, delegate { ToggleRightPanel(); });

            top.Controls.Add(memberButton);
            top.Controls.Add(updateButton);
            top.Controls.Add(noticeButton);
            top.Controls.Add(refreshButton);
            top.Controls.Add(diagnosticsButton);
            top.Controls.Add(collapseButton);
            return top;
        }

        private Button TopButton(string text, int left, EventHandler click)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(90, 28);
            button.Location = new Point(left, 13);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(210, 216, 222);
            button.Click += click;
            return button;
        }

        private Control CreateMainArea()
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 3;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 292));
            layout.Controls.Add(CreateAccountRail(), 0, 0);
            layout.Controls.Add(CreateWorkspace(), 1, 0);
            _rightPanel = CreateQuickReplyPanel();
            layout.Controls.Add(_rightPanel, 2, 0);
            _mainLayout = layout;
            return layout;
        }

        private Control CreateAccountRail()
        {
            Panel rail = new Panel();
            rail.Dock = DockStyle.Fill;
            rail.BackColor = Color.White;
            rail.Padding = new Padding(10);

            Button addButton = RailButton("+", "添加账号", 18);
            addButton.Click += AddAccountClicked;
            rail.Controls.Add(addButton);

            Button lockButton = RailButton("锁", "隐私锁", 92);
            lockButton.Click += delegate { _workspaceStatus.Text = "隐私锁已启用：会话区域已隐藏。"; };
            rail.Controls.Add(lockButton);

            Button splitButton = RailButton("拆", "拆分窗口", 166);
            splitButton.Click += delegate { _workspaceStatus.Text = "拆分窗口将在窗口嵌入能力完成后启用。"; };
            rail.Controls.Add(splitButton);

            Button renameButton = RailButton("备", "备注", 240);
            renameButton.Click += RenameAccountClicked;
            rail.Controls.Add(renameButton);

            Button deleteButton = RailButton("删", "删除", 314);
            deleteButton.Click += DeleteAccountClicked;
            rail.Controls.Add(deleteButton);

            Button moveUpButton = RailButton("上", "上移", 388);
            moveUpButton.Click += MoveAccountUpClicked;
            rail.Controls.Add(moveUpButton);

            Button moveDownButton = RailButton("下", "下移", 462);
            moveDownButton.Click += MoveAccountDownClicked;
            rail.Controls.Add(moveDownButton);

            _accountList = new ListBox();
            _accountList.Location = new Point(8, 536);
            _accountList.Size = new Size(76, 96);
            _accountList.BorderStyle = BorderStyle.None;
            _accountList.SelectedIndexChanged += AccountSelectionChanged;
            rail.Controls.Add(_accountList);

            return rail;
        }

        private Button RailButton(string glyph, string label, int top)
        {
            Button button = new Button();
            button.Text = glyph + Environment.NewLine + label;
            button.Size = new Size(64, 56);
            button.Location = new Point(14, top);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(220, 225, 230);
            return button;
        }

        private Control CreateWorkspace()
        {
            Panel workspace = new Panel();
            workspace.Dock = DockStyle.Fill;
            workspace.BackColor = Color.FromArgb(250, 250, 250);
            workspace.Padding = new Padding(12);

            Panel canvas = new Panel();
            canvas.Dock = DockStyle.Fill;
            canvas.BackColor = Color.White;
            canvas.BorderStyle = BorderStyle.FixedSingle;
            workspace.Controls.Add(canvas);

            _processStatus = new Label();
            _processStatus.Text = "微信进程：未刷新";
            _processStatus.ForeColor = Color.FromArgb(80, 88, 96);
            _processStatus.AutoSize = true;
            _processStatus.Location = new Point(14, 14);
            canvas.Controls.Add(_processStatus);

            _workspaceStatus = new Label();
            _workspaceStatus.Text = "微信窗口承载区：点击左侧添加账号启动微信；窗口嵌入将在后续迭代接入。";
            _workspaceStatus.ForeColor = Color.FromArgb(80, 88, 96);
            _workspaceStatus.AutoSize = false;
            _workspaceStatus.TextAlign = ContentAlignment.MiddleCenter;
            _workspaceStatus.Dock = DockStyle.Fill;
            canvas.Controls.Add(_workspaceStatus);

            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 50;
            bottom.BackColor = Color.FromArgb(246, 247, 249);
            workspace.Controls.Add(bottom);
            bottom.BringToFront();

            Button screenshotButton = new Button();
            screenshotButton.Text = "截图工具";
            screenshotButton.Size = new Size(110, 32);
            screenshotButton.Location = new Point(10, 9);
            screenshotButton.Click += ScreenshotClicked;
            bottom.Controls.Add(screenshotButton);

            Button refreshButton = new Button();
            refreshButton.Text = "刷新账号";
            refreshButton.Size = new Size(110, 32);
            refreshButton.Location = new Point(260, 9);
            refreshButton.Click += delegate { RefreshWeChatWindows(); };
            bottom.Controls.Add(refreshButton);

            Button closeAllButton = new Button();
            closeAllButton.Text = "关闭微信";
            closeAllButton.Size = new Size(110, 32);
            closeAllButton.Location = new Point(380, 9);
            closeAllButton.Click += CloseAllWeChatClicked;
            bottom.Controls.Add(closeAllButton);

            Button closeCurrentButton = new Button();
            closeCurrentButton.Text = "关闭当前";
            closeCurrentButton.Size = new Size(110, 32);
            closeCurrentButton.Location = new Point(500, 9);
            closeCurrentButton.Click += CloseCurrentWeChatClicked;
            bottom.Controls.Add(closeCurrentButton);

            _hideForScreenshot = new CheckBox();
            _hideForScreenshot.Text = "截图时隐藏当前窗口";
            _hideForScreenshot.AutoSize = true;
            _hideForScreenshot.Location = new Point(136, 15);
            bottom.Controls.Add(_hideForScreenshot);

            return workspace;
        }

        private Control CreateQuickReplyPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(10);
            panel.Tag = "right-panel";

            _searchBox = new TextBox();
            _searchBox.Location = new Point(10, 12);
            _searchBox.Size = new Size(262, 23);
            _searchBox.TextChanged += delegate { RefreshReplies(); };
            panel.Controls.Add(_searchBox);

            _categoryList = new ListBox();
            _categoryList.Location = new Point(10, 46);
            _categoryList.Size = new Size(100, 160);
            _categoryList.SelectedIndexChanged += delegate { RefreshReplies(); };
            panel.Controls.Add(_categoryList);

            _replyList = new ListBox();
            _replyList.Location = new Point(116, 46);
            _replyList.Size = new Size(156, 420);
            _replyList.SelectedIndexChanged += ReplySelectionChanged;
            _replyList.DoubleClick += ReplyDoubleClicked;
            panel.Controls.Add(_replyList);

            Button editButton = SideButton("编辑", 10, 480);
            editButton.Text = "编辑/新增";
            editButton.Click += EditReplyClicked;
            panel.Controls.Add(editButton);

            Button importButton = SideButton("导入", 100, 480);
            importButton.Text = "导入";
            importButton.Click += ImportClicked;
            panel.Controls.Add(importButton);

            Button exportButton = SideButton("导出", 190, 480);
            exportButton.Text = "导出";
            exportButton.Click += ExportClicked;
            panel.Controls.Add(exportButton);

            Button deleteButton = SideButton("删除", 100, 520);
            deleteButton.Click += DeleteReplyClicked;
            panel.Controls.Add(deleteButton);

            return panel;
        }

        private Button SideButton(string text, int left, int top)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(80, 32);
            button.Location = new Point(left, top);
            return button;
        }

        private void LoadData()
        {
            _replyRepository.EnsureSeedData();
            RefreshAccounts();

            _categoryList.Items.Clear();
            foreach (ReplyCategory category in _replyRepository.GetCategories().OrderBy(x => x.SortOrder))
            {
                _categoryList.Items.Add(new CategoryListItem(category));
            }
            if (_categoryList.Items.Count > 0)
            {
                _categoryList.SelectedIndex = 0;
            }

            RefreshReplies();
        }

        private void RefreshAccounts()
        {
            _currentAccounts = _accountRepository.GetAll();
            if (_accountList == null)
            {
                return;
            }

            _accountList.Items.Clear();
            if (_currentAccounts.Length == 0)
            {
                _accountList.Items.Add("待登录");
                return;
            }

            for (int i = 0; i < _currentAccounts.Length; i++)
            {
                _accountList.Items.Add(new AccountListItem(_currentAccounts[i]));
            }
        }

        private void RefreshReplies()
        {
            string keyword = _searchBox == null ? string.Empty : _searchBox.Text;
            QuickReply[] replies = _replyRepository.Search(keyword);
            CategoryListItem item = _categoryList == null ? null : _categoryList.SelectedItem as CategoryListItem;
            if (item != null && string.IsNullOrWhiteSpace(keyword))
            {
                replies = replies.Where(x => string.Equals(x.CategoryId, item.Category.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
            }

            _currentReplies = replies;
            _replyList.Items.Clear();
            for (int i = 0; i < replies.Length; i++)
            {
                _replyList.Items.Add(new ReplyListItem(replies[i]));
            }
        }

        private void RefreshHelperStatus()
        {
            if (!File.Exists(_helperPath))
            {
                _helperStatus.Text = "助手组件：未找到";
                return;
            }

            try
            {
                HelperProcessClient client = new HelperProcessClient(_helperPath, 3000);
                string json = client.Run("version --json");
                _helperStatus.Text = "助手组件：可用 " + TrimForStatus(json);
                RefreshWeChatProcessStatus();
            }
            catch (Exception ex)
            {
                LogDiagnostic("helper.status", "Refresh helper status failed.", ex);
                _helperStatus.Text = "助手组件：异常 " + ex.Message;
            }
        }

        private void AddAccountClicked(object sender, EventArgs e)
        {
            LicenseState state = _licenseService.GetOrCreateTrial();
            if (!LicenseFeaturePolicy.CanAddAccount(state, _accountRepository.GetAll().Length))
            {
                string message = LicenseFeaturePolicy.GetAccountLimitMessage(state);
                LogDiagnostic("license.account-limit", message, null);
                _workspaceStatus.Text = message;
                MessageBox.Show(message, "添加账号");
                return;
            }

            if (!File.Exists(_helperPath))
            {
                MessageBox.Show("未找到独立开源助手组件，无法启动微信。请先构建或安装 WeChatPlus.OpenHelper.exe。", "助手组件不可用");
                return;
            }

            try
            {
                HelperProcessClient client = new HelperProcessClient(_helperPath, 10000);
                client.Run("multi-instance close-all-mutex");
                string output = client.Run("multi-instance start");
                AccountRecord account = _accountRepository.UpsertFromProcess(ExtractProcessId(output), "微信实例 " + DateTime.Now.ToString("HH:mm:ss"), "Launched");
                RefreshAccounts();
                SelectAccount(account.Id);
                _workspaceStatus.Text = "已调用助手组件启动微信：" + TrimForStatus(output);
                RefreshWeChatProcessStatus();
            }
            catch (Exception ex)
            {
                LogDiagnostic("helper.start", "Start WeChat failed.", ex);
                MessageBox.Show("启动微信失败：" + ex.Message, "添加账号");
            }
        }

        private void ReplyDoubleClicked(object sender, EventArgs e)
        {
            CopySelectedReplyToClipboard("话术已准备粘贴/发送：");
        }

        private void ReplySelectionChanged(object sender, EventArgs e)
        {
            CopySelectedReplyToClipboard("话术已复制到剪贴板：");
        }

        private void CopySelectedReplyToClipboard(string statusPrefix)
        {
            ReplyListItem item = _replyList.SelectedItem as ReplyListItem;
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(item.Reply.Content))
            {
                _workspaceStatus.Text = "当前话术没有可复制内容：" + item.Reply.Title;
                return;
            }

            Clipboard.SetText(item.Reply.Content);
            _workspaceStatus.Text = statusPrefix + item.Reply.Title;
        }

        private void EditReplyClicked(object sender, EventArgs e)
        {
            ReplyListItem editingItem = _replyList.SelectedItem as ReplyListItem;
            QuickReply editing = editingItem == null ? null : editingItem.Reply;

            using (Form editor = new Form())
            {
                editor.Text = editing == null ? "新增话术" : "编辑话术";
                editor.Size = new Size(420, 300);
                editor.StartPosition = FormStartPosition.CenterParent;

                TextBox title = new TextBox();
                title.Location = new Point(16, 16);
                title.Size = new Size(360, 23);
                title.Text = editing == null ? "新话术" : editing.Title;
                editor.Controls.Add(title);

                TextBox content = new TextBox();
                content.Location = new Point(16, 52);
                content.Size = new Size(360, 150);
                content.Multiline = true;
                content.Text = editing == null ? "请输入话术内容" : editing.Content;
                editor.Controls.Add(content);

                TextBox tags = new TextBox();
                tags.Location = new Point(16, 210);
                tags.Size = new Size(260, 23);
                tags.Text = editing == null ? title.Text : editing.Tags;
                editor.Controls.Add(tags);

                Button save = new Button();
                save.Text = "保存";
                save.Location = new Point(300, 214);
                save.DialogResult = DialogResult.OK;
                editor.Controls.Add(save);
                editor.AcceptButton = save;

                if (editor.ShowDialog(this) == DialogResult.OK)
                {
                    CategoryListItem selected = _categoryList.SelectedItem as CategoryListItem;
                    QuickReply reply = new QuickReply();
                    if (editing != null)
                    {
                        reply.Id = editing.Id;
                        reply.CreatedAtUtc = editing.CreatedAtUtc;
                    }

                    reply.Title = title.Text;
                    reply.Content = content.Text;
                    reply.CategoryId = selected == null
                        ? (editing == null || string.IsNullOrEmpty(editing.CategoryId) ? "common" : editing.CategoryId)
                        : selected.Category.Id;
                    reply.Tags = string.IsNullOrWhiteSpace(tags.Text) ? title.Text : tags.Text;
                    reply.SortOrder = editing == null ? (_currentReplies == null ? 100 : _currentReplies.Length + 100) : editing.SortOrder;
                    reply.IsFavorite = editing != null && editing.IsFavorite;
                    _replyRepository.SaveReply(reply);
                    RefreshReplies();
                    _workspaceStatus.Text = editing == null ? "话术已新增：" + reply.Title : "话术已更新：" + reply.Title;
                }
            }
        }

        private void DeleteReplyClicked(object sender, EventArgs e)
        {
            ReplyListItem item = _replyList.SelectedItem as ReplyListItem;
            if (item == null)
            {
                _workspaceStatus.Text = "请先选择要删除的话术。";
                return;
            }

            DialogResult result = MessageBox.Show("确认删除话术“" + item.Reply.Title + "”？", "删除话术", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result != DialogResult.OK)
            {
                return;
            }

            if (_replyRepository.DeleteReply(item.Reply.Id))
            {
                RefreshReplies();
                _workspaceStatus.Text = "话术已删除：" + item.Reply.Title;
                return;
            }

            _workspaceStatus.Text = "未找到要删除的话术：" + item.Reply.Title;
        }

        private void ImportClicked(object sender, EventArgs e)
        {
            LicenseState state = _licenseService.GetOrCreateTrial();
            if (!LicenseFeaturePolicy.CanImportOrExportReplies(state))
            {
                string message = LicenseFeaturePolicy.GetImportExportLimitMessage(state);
                LogDiagnostic("license.quick-reply-import", message, null);
                _workspaceStatus.Text = message;
                MessageBox.Show(message, "导入话术");
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "话术文件|*.json;*.csv|JSON 文件|*.json|CSV 文件|*.csv";
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                string content = File.ReadAllText(dialog.FileName);
                string extension = Path.GetExtension(dialog.FileName);
                int imported = string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase)
                    ? _replyRepository.ImportCsv(content, true)
                    : _replyRepository.ImportJson(content, true);
                RefreshReplies();
                MessageBox.Show("已导入 " + imported + " 条话术。", "导入话术");
            }
            catch (Exception ex)
            {
                LogDiagnostic("quick-reply.import", "Import quick replies failed.", ex);
                MessageBox.Show("导入失败：" + ex.Message, "导入话术");
            }
        }

        private void ExportClicked(object sender, EventArgs e)
        {
            LicenseState state = _licenseService.GetOrCreateTrial();
            if (!LicenseFeaturePolicy.CanImportOrExportReplies(state))
            {
                string message = LicenseFeaturePolicy.GetImportExportLimitMessage(state);
                LogDiagnostic("license.quick-reply-export", message, null);
                _workspaceStatus.Text = message;
                MessageBox.Show(message, "导出话术");
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "JSON 文件|*.json";
            dialog.FileName = "wechat-plus-quick-replies.json";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, _replyRepository.ExportJson());
                    MessageBox.Show("已导出话术库。", "导出话术");
                }
                catch (Exception ex)
                {
                    LogDiagnostic("quick-reply.export", "Export quick replies failed.", ex);
                    MessageBox.Show("导出失败：" + ex.Message, "导出话术");
                }
            }
        }

        private void ScreenshotClicked(object sender, EventArgs e)
        {
            bool hide = _hideForScreenshot.Checked;
            FormWindowState originalState = WindowState;
            try
            {
                if (hide)
                {
                    WindowState = FormWindowState.Minimized;
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(200);
                }

                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                    }
                    Clipboard.SetImage(bitmap);
                }

                _workspaceStatus.Text = "截图已复制到剪贴板。";
            }
            catch (Exception ex)
            {
                LogDiagnostic("screenshot", "Screenshot failed.", ex);
                MessageBox.Show("截图失败：" + ex.Message, "截图工具");
            }
            finally
            {
                if (hide)
                {
                    WindowState = originalState;
                    Activate();
                }
            }
        }

        private void ShowMemberState(object sender, EventArgs e)
        {
            LicenseState state = _licenseService.GetOrCreateTrial();
            using (Form dialog = new Form())
            {
                dialog.Text = "会员状态";
                dialog.Size = new Size(520, 300);
                dialog.StartPosition = FormStartPosition.CenterParent;

                Label stateLabel = new Label();
                stateLabel.Location = new Point(16, 16);
                stateLabel.Size = new Size(470, 100);
                stateLabel.Text =
                    "当前方案：" + state.Plan + Environment.NewLine +
                    "授权到期：" + state.ExpiresAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + Environment.NewLine +
                    "离线宽限：" + state.OfflineGraceUntilUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + Environment.NewLine +
                    "设备哈希：" + state.DeviceIdHash.Substring(0, 12) + "..." + Environment.NewLine +
                    "激活码：" + (string.IsNullOrEmpty(state.LicenseKeyMasked) ? "未激活" : state.LicenseKeyMasked);
                dialog.Controls.Add(stateLabel);

                TextBox licenseKey = new TextBox();
                licenseKey.Location = new Point(16, 132);
                licenseKey.Size = new Size(330, 23);
                licenseKey.Text = "ABC-123-SECRET";
                dialog.Controls.Add(licenseKey);

                Label apiLabel = new Label();
                apiLabel.Location = new Point(16, 166);
                apiLabel.Size = new Size(470, 48);
                apiLabel.ForeColor = Color.FromArgb(80, 88, 96);
                apiLabel.Text = "激活会保存本地授权状态，并构造预留云端请求；当前不联网、不包含真实密钥。";
                dialog.Controls.Add(apiLabel);

                Button activate = new Button();
                activate.Text = "本地激活";
                activate.Location = new Point(366, 130);
                activate.Size = new Size(92, 28);
                activate.Click += delegate
                {
                    LicenseActivationRequest request = _licenseApiClient.BuildActivationRequest(licenseKey.Text, state);
                    LicenseState activated = _licenseService.ApplyActivation(licenseKey.Text, "personal", DateTime.UtcNow.AddDays(365));
                    stateLabel.Text =
                        "当前方案：" + activated.Plan + Environment.NewLine +
                        "授权到期：" + activated.ExpiresAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + Environment.NewLine +
                        "离线宽限：" + activated.OfflineGraceUntilUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + Environment.NewLine +
                        "设备哈希：" + activated.DeviceIdHash.Substring(0, 12) + "..." + Environment.NewLine +
                        "激活码：" + activated.LicenseKeyMasked;
                    apiLabel.Text = "预留接口：" + request.Method + " " + request.Url;
                    _workspaceStatus.Text = "会员状态已本地激活：" + activated.Plan;
                };
                dialog.Controls.Add(activate);

                Button close = new Button();
                close.Text = "关闭";
                close.Location = new Point(390, 220);
                close.DialogResult = DialogResult.OK;
                dialog.Controls.Add(close);
                dialog.AcceptButton = close;

                dialog.ShowDialog(this);
            }
        }

        private void ShowOpenSourceNotice(object sender, EventArgs e)
        {
            OpenSourceComponent[] components = _componentRepository.GetAll();
            string componentLines = string.Empty;
            for (int i = 0; i < components.Length; i++)
            {
                OpenSourceComponent component = components[i];
                componentLines += component.Name + " " + component.Version + Environment.NewLine +
                    "许可证：" + component.License + Environment.NewLine +
                    "源码：" + component.SourceUrl + Environment.NewLine;
            }

            MessageBox.Show(
                "WeChat Plus 商用壳不复制、不链接 GPLv3 源码。" + Environment.NewLine +
                "多开/补丁相关底层能力由独立开源助手组件提供，并通过进程边界调用。" + Environment.NewLine +
                Environment.NewLine +
                componentLines,
                "开源组件声明");
        }

        private void CheckUpdatesClicked(object sender, EventArgs e)
        {
            try
            {
                string manifestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update-manifest.json");
                if (!File.Exists(manifestPath))
                {
                    _workspaceStatus.Text = "更新检查已预留：未找到本地 update-manifest.json，后续可接入云端版本接口。";
                    return;
                }

                string json = File.ReadAllText(manifestPath);
                UpdateManifest manifest = UpdateManifest.Parse(json);
                UpdateCheckStatus status = UpdateCheckService.Evaluate(manifest, "0.1.0", GetHelperVersion());
                _workspaceStatus.Text = status.StatusText;
                MessageBox.Show(status.StatusText, "检查更新");
            }
            catch (Exception ex)
            {
                LogDiagnostic("update.check", "Check updates failed.", ex);
                MessageBox.Show("检查更新失败：" + ex.Message, "检查更新");
            }
        }

        private void ExportDiagnosticsClicked(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "导出诊断日志";
                dialog.Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FileName = "wechat-plus-diagnostics.log";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    _diagnosticsLog.ExportTo(dialog.FileName);
                    _workspaceStatus.Text = "诊断日志已导出：" + dialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出诊断日志失败：" + ex.Message, "诊断日志");
                }
            }
        }

        private void ToggleRightPanel()
        {
            _rightPanelCollapsed = !_rightPanelCollapsed;
            _rightPanel.Visible = !_rightPanelCollapsed;
            _mainLayout.ColumnStyles[2].Width = _rightPanelCollapsed ? 0 : 292;
            _workspaceStatus.Text = _rightPanelCollapsed ? "右侧话术栏已收起。" : "右侧话术栏已展开。";
        }

        private void RefreshWeChatProcessStatus()
        {
            if (_processStatus == null)
            {
                return;
            }

            if (!File.Exists(_helperPath))
            {
                _processStatus.Text = "微信进程：助手组件未找到";
                return;
            }

            try
            {
                HelperProcessClient client = new HelperProcessClient(_helperPath, 3000);
                string json = client.Run("multi-instance status");
                _processStatus.Text = "微信进程状态：" + TrimForStatus(json);
            }
            catch (Exception ex)
            {
                LogDiagnostic("helper.process-status", "Refresh WeChat process status failed.", ex);
                _processStatus.Text = "微信进程状态：刷新失败 " + ex.Message;
            }
        }

        private void RefreshWeChatWindows()
        {
            if (!File.Exists(_helperPath))
            {
                MessageBox.Show("未找到助手组件，无法刷新微信窗口。", "刷新账号");
                return;
            }

            try
            {
                HelperProcessClient client = new HelperProcessClient(_helperPath, 3000);
                string json = client.Run("multi-instance windows");
                HelperWindowInfo[] windows = HelperWindowResultParser.ParseWindows(json);
                int[] activeProcessIds = ExtractProcessIds(windows);
                int offlineCount = _accountRepository.MarkMissingProcessesOffline(activeProcessIds);
                if (windows.Length == 0)
                {
                    _workspaceStatus.Text = offlineCount > 0 ? "当前没有检测到微信窗口，已更新离线账号：" + offlineCount + " 个。" : "当前没有检测到微信窗口。";
                    RefreshAccounts();
                    RefreshWeChatProcessStatus();
                    return;
                }

                AccountRecord selected = null;
                for (int i = 0; i < windows.Length; i++)
                {
                    HelperWindowInfo window = windows[i];
                    string displayName = string.IsNullOrWhiteSpace(window.Title)
                        ? "微信窗口 " + window.ProcessId
                        : window.Title;
                    AccountRecord record = _accountRepository.UpsertFromProcess(window.ProcessId, displayName, window.HasWindow ? "Detected" : "Starting");
                    _accountRepository.UpdateStatus(record.Id, window.HasWindow ? "Detected" : "Starting", record.ProcessId, window.WindowHandle);
                    selected = record;
                }

                RefreshAccounts();
                if (selected != null)
                {
                    SelectAccount(selected.Id);
                }
                _workspaceStatus.Text = offlineCount > 0 ? "已刷新微信窗口：" + windows.Length + " 个，离线账号：" + offlineCount + " 个。" : "已刷新微信窗口：" + windows.Length + " 个。";
                RefreshWeChatProcessStatus();
            }
            catch (Exception ex)
            {
                LogDiagnostic("helper.windows", "Refresh WeChat windows failed.", ex);
                MessageBox.Show("刷新微信窗口失败：" + ex.Message, "刷新账号");
            }
        }

        private void CloseAllWeChatClicked(object sender, EventArgs e)
        {
            if (!File.Exists(_helperPath))
            {
                MessageBox.Show("未找到助手组件，无法关闭微信。", "关闭微信");
                return;
            }

            DialogResult result = MessageBox.Show("确认关闭所有微信进程？", "关闭微信", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result != DialogResult.OK)
            {
                return;
            }

            try
            {
                HelperProcessClient client = new HelperProcessClient(_helperPath, 5000);
                string output = client.Run("multi-instance close-all");
                AccountRecord[] accounts = _accountRepository.GetAll();
                for (int i = 0; i < accounts.Length; i++)
                {
                    _accountRepository.UpdateStatus(accounts[i].Id, "Offline", 0, accounts[i].WindowHandle);
                }
                RefreshAccounts();
                RefreshWeChatProcessStatus();
                _workspaceStatus.Text = "已请求关闭微信：" + TrimForStatus(output);
            }
            catch (Exception ex)
            {
                LogDiagnostic("helper.close-all", "Close all WeChat processes failed.", ex);
                MessageBox.Show("关闭微信失败：" + ex.Message, "关闭微信");
            }
        }

        private void CloseCurrentWeChatClicked(object sender, EventArgs e)
        {
            AccountListItem item = _accountList.SelectedItem as AccountListItem;
            if (item == null)
            {
                _workspaceStatus.Text = "请先选择要关闭的微信账号。";
                return;
            }

            if (!File.Exists(_helperPath))
            {
                MessageBox.Show("未找到助手组件，无法关闭当前微信。", "关闭当前");
                return;
            }

            if (item.Account.ProcessId <= 0)
            {
                _workspaceStatus.Text = "当前账号没有可关闭的微信进程。";
                return;
            }

            DialogResult result = MessageBox.Show("确认关闭“" + item.Account.DisplayName + "”对应的微信进程？", "关闭当前", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result != DialogResult.OK)
            {
                return;
            }

            try
            {
                HelperProcessClient client = new HelperProcessClient(_helperPath, 5000);
                string output = client.Run("multi-instance close --pid " + item.Account.ProcessId);
                bool closed = ExtractBool(output, "closed");
                if (closed)
                {
                    _accountRepository.UpdateStatus(item.Account.Id, "Offline", 0, item.Account.WindowHandle);
                    RefreshAccounts();
                    SelectAccount(item.Account.Id);
                }

                RefreshWeChatProcessStatus();
                _workspaceStatus.Text = closed ? "已关闭当前微信：" + TrimForStatus(output) : "未关闭当前微信：" + TrimForStatus(output);
            }
            catch (Exception ex)
            {
                LogDiagnostic("helper.close-current", "Close current WeChat process failed.", ex);
                MessageBox.Show("关闭当前微信失败：" + ex.Message, "关闭当前");
            }
        }

        private void RenameAccountClicked(object sender, EventArgs e)
        {
            AccountListItem item = _accountList.SelectedItem as AccountListItem;
            if (item == null)
            {
                _workspaceStatus.Text = "请先选择要备注的账号。";
                return;
            }

            using (Form editor = new Form())
            {
                editor.Text = "编辑账号备注";
                editor.Size = new Size(360, 150);
                editor.StartPosition = FormStartPosition.CenterParent;

                TextBox name = new TextBox();
                name.Location = new Point(16, 18);
                name.Size = new Size(310, 23);
                name.Text = item.Account.DisplayName;
                editor.Controls.Add(name);

                Button save = new Button();
                save.Text = "保存";
                save.Location = new Point(246, 58);
                save.DialogResult = DialogResult.OK;
                editor.Controls.Add(save);
                editor.AcceptButton = save;

                if (editor.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (_accountRepository.UpdateDisplayName(item.Account.Id, name.Text))
                {
                    RefreshAccounts();
                    SelectAccount(item.Account.Id);
                    _workspaceStatus.Text = "账号备注已更新：" + name.Text.Trim();
                    return;
                }

                _workspaceStatus.Text = "账号备注更新失败。";
            }
        }

        private void DeleteAccountClicked(object sender, EventArgs e)
        {
            AccountListItem item = _accountList.SelectedItem as AccountListItem;
            if (item == null)
            {
                _workspaceStatus.Text = "请先选择要删除的本地账号记录。";
                return;
            }

            DialogResult result = MessageBox.Show("删除本地账号记录“" + item.Account.DisplayName + "”？这不会关闭微信进程。", "删除账号记录", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result != DialogResult.OK)
            {
                return;
            }

            _accountRepository.Delete(item.Account.Id);
            RefreshAccounts();
            _workspaceStatus.Text = "本地账号记录已删除：" + item.Account.DisplayName;
        }

        private void MoveAccountUpClicked(object sender, EventArgs e)
        {
            MoveSelectedAccount(-1);
        }

        private void MoveAccountDownClicked(object sender, EventArgs e)
        {
            MoveSelectedAccount(1);
        }

        private void MoveSelectedAccount(int direction)
        {
            AccountListItem item = _accountList.SelectedItem as AccountListItem;
            if (item == null)
            {
                _workspaceStatus.Text = "请先选择要排序的账号。";
                return;
            }

            if (_accountRepository.MoveAccount(item.Account.Id, direction))
            {
                RefreshAccounts();
                SelectAccount(item.Account.Id);
                _workspaceStatus.Text = direction < 0 ? "账号已上移：" + item.Account.DisplayName : "账号已下移：" + item.Account.DisplayName;
                return;
            }

            _workspaceStatus.Text = "账号已经在边界位置：" + item.Account.DisplayName;
        }

        private void AccountSelectionChanged(object sender, EventArgs e)
        {
            AccountListItem item = _accountList.SelectedItem as AccountListItem;
            if (item == null)
            {
                return;
            }

            if (File.Exists(_helperPath) && !string.IsNullOrWhiteSpace(item.Account.WindowHandle))
            {
                try
                {
                    HelperProcessClient client = new HelperProcessClient(_helperPath, 3000);
                    string output = client.Run("multi-instance focus --handle " + item.Account.WindowHandle);
                    _workspaceStatus.Text = WorkspaceStatusFormatter.FormatFocusMode(item.Account, ExtractBool(output, "focused") ? "成功" : "失败", output);
                    return;
                }
                catch (Exception ex)
                {
                    LogDiagnostic("helper.focus", "Focus WeChat window failed.", ex);
                    _workspaceStatus.Text = WorkspaceStatusFormatter.FormatFocusFailure(item.Account, ex.Message);
                    return;
                }
            }

            _workspaceStatus.Text = WorkspaceStatusFormatter.FormatFocusMode(item.Account, false, string.Empty);
        }

        private void SelectAccount(string id)
        {
            if (string.IsNullOrEmpty(id) || _accountList == null)
            {
                return;
            }

            for (int i = 0; i < _accountList.Items.Count; i++)
            {
                AccountListItem item = _accountList.Items[i] as AccountListItem;
                if (item != null && string.Equals(item.Account.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    _accountList.SelectedIndex = i;
                    return;
                }
            }
        }

        private static int ExtractProcessId(string helperJson)
        {
            return ExtractInt(helperJson, "processId");
        }

        private void LogDiagnostic(string area, string message, Exception exception)
        {
            try
            {
                _diagnosticsLog.Write(area, message, exception);
            }
            catch
            {
            }
        }

        private string GetHelperVersion()
        {
            if (!File.Exists(_helperPath))
            {
                return "0.0.0";
            }

            try
            {
                HelperProcessClient client = new HelperProcessClient(_helperPath, 3000);
                string json = client.Run("version --json");
                string version = HelperVersionResultParser.ParseVersion(json);
                return string.IsNullOrWhiteSpace(version) ? "0.0.0" : version;
            }
            catch (Exception ex)
            {
                LogDiagnostic("helper.version", "Read helper version failed.", ex);
                return "0.0.0";
            }
        }

        private static int[] ExtractProcessIds(HelperWindowInfo[] windows)
        {
            if (windows == null || windows.Length == 0)
            {
                return new int[0];
            }

            int[] processIds = new int[windows.Length];
            for (int i = 0; i < windows.Length; i++)
            {
                processIds[i] = windows[i].ProcessId;
            }

            return processIds;
        }

        private static int ExtractInt(string json, string propertyName)
        {
            string value = ExtractRawNumber(json, propertyName);
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }

        private static bool ExtractBool(string json, string propertyName)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            string marker = "\"" + propertyName + "\":";
            int markerIndex = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                return false;
            }

            int start = markerIndex + marker.Length;
            if (json.Length >= start + 4 && string.Equals(json.Substring(start, 4), "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static string ExtractRawNumber(string json, string propertyName)
        {
            if (string.IsNullOrEmpty(json))
            {
                return string.Empty;
            }

            string marker = "\"" + propertyName + "\":";
            int markerIndex = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                return string.Empty;
            }

            int start = markerIndex + marker.Length;
            int end = start;
            while (end < json.Length && char.IsDigit(json[end]))
            {
                end++;
            }

            return json.Substring(start, end - start);
        }

        private static string ExtractString(string json, string propertyName)
        {
            if (string.IsNullOrEmpty(json))
            {
                return string.Empty;
            }

            string marker = "\"" + propertyName + "\":\"";
            int markerIndex = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                return string.Empty;
            }

            int start = markerIndex + marker.Length;
            int end = json.IndexOf("\"", start, StringComparison.Ordinal);
            if (end < start)
            {
                return string.Empty;
            }

            return json.Substring(start, end - start);
        }

        private static string TrimForStatus(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            value = value.Replace(Environment.NewLine, " ").Trim();
            return value.Length > 80 ? value.Substring(0, 80) + "..." : value;
        }

        private sealed class CategoryListItem
        {
            public CategoryListItem(ReplyCategory category)
            {
                Category = category;
            }

            public ReplyCategory Category { get; private set; }

            public override string ToString()
            {
                return Category.Name;
            }
        }

        private sealed class ReplyListItem
        {
            public ReplyListItem(QuickReply reply)
            {
                Reply = reply;
            }

            public QuickReply Reply { get; private set; }

            public override string ToString()
            {
                string content = Reply.Content ?? string.Empty;
                if (content.Length > 18)
                {
                    content = content.Substring(0, 18) + "...";
                }
                return Reply.Title + " " + content;
            }
        }

        private sealed class AccountListItem
        {
            public AccountListItem(AccountRecord account)
            {
                Account = account;
            }

            public AccountRecord Account { get; private set; }

            public override string ToString()
            {
                return Account.DisplayName;
            }
        }
    }
}
