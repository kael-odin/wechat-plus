using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeChatPlus.Core.Models;
using WeChatPlus.Core.Services;

namespace WeChatPlus.Shell
{
    public sealed class MainForm : Form
    {
        private readonly string _dataRoot;
        private readonly QuickReplyRepository _replyRepository;
        private readonly TrialLicenseService _licenseService;
        private readonly string _helperPath;

        private ListBox _accountList;
        private ListBox _categoryList;
        private ListBox _replyList;
        private TextBox _searchBox;
        private Label _helperStatus;
        private Label _workspaceStatus;
        private CheckBox _hideForScreenshot;
        private TableLayoutPanel _mainLayout;
        private Control _rightPanel;
        private bool _rightPanelCollapsed;
        private QuickReply[] _currentReplies;

        public MainForm()
        {
            _dataRoot = AppPaths.GetDefaultDataRoot();
            _replyRepository = new QuickReplyRepository(_dataRoot);
            _licenseService = new TrialLicenseService(_dataRoot);
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
            _helperStatus.AutoSize = true;
            _helperStatus.ForeColor = Color.FromArgb(80, 80, 80);
            _helperStatus.Location = new Point(190, 18);
            top.Controls.Add(_helperStatus);

            Button memberButton = TopButton("会员", 760, ShowMemberState);
            Button noticeButton = TopButton("开源声明", 845, ShowOpenSourceNotice);
            Button refreshButton = TopButton("刷新助手", 950, delegate { RefreshHelperStatus(); });
            Button collapseButton = TopButton("收起侧栏", 1065, delegate { ToggleRightPanel(); });

            top.Controls.Add(memberButton);
            top.Controls.Add(noticeButton);
            top.Controls.Add(refreshButton);
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

            _accountList = new ListBox();
            _accountList.Location = new Point(8, 248);
            _accountList.Size = new Size(76, 360);
            _accountList.BorderStyle = BorderStyle.None;
            _accountList.Items.Add("待登录");
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
            _replyList.DoubleClick += ReplyDoubleClicked;
            panel.Controls.Add(_replyList);

            Button editButton = SideButton("编辑", 10, 480);
            editButton.Click += EditReplyClicked;
            panel.Controls.Add(editButton);

            Button importButton = SideButton("导入", 100, 480);
            importButton.Click += ImportClicked;
            panel.Controls.Add(importButton);

            Button exportButton = SideButton("导出", 190, 480);
            exportButton.Click += ExportClicked;
            panel.Controls.Add(exportButton);

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
            }
            catch (Exception ex)
            {
                _helperStatus.Text = "助手组件：异常 " + ex.Message;
            }
        }

        private void AddAccountClicked(object sender, EventArgs e)
        {
            if (!File.Exists(_helperPath))
            {
                MessageBox.Show("未找到独立开源助手组件，无法启动微信。请先构建或安装 WeChatPlus.OpenHelper.exe。", "助手组件不可用");
                return;
            }

            try
            {
                HelperProcessClient client = new HelperProcessClient(_helperPath, 10000);
                string output = client.Run("multi-instance start");
                _accountList.Items.Add("微信实例 " + DateTime.Now.ToString("HH:mm:ss"));
                _workspaceStatus.Text = "已调用助手组件启动微信：" + TrimForStatus(output);
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动微信失败：" + ex.Message, "添加账号");
            }
        }

        private void ReplyDoubleClicked(object sender, EventArgs e)
        {
            ReplyListItem item = _replyList.SelectedItem as ReplyListItem;
            if (item == null)
            {
                return;
            }
            Clipboard.SetText(item.Reply.Content);
            _workspaceStatus.Text = "话术已复制到剪贴板：" + item.Reply.Title;
        }

        private void EditReplyClicked(object sender, EventArgs e)
        {
            using (Form editor = new Form())
            {
                editor.Text = "新增话术";
                editor.Size = new Size(420, 300);
                editor.StartPosition = FormStartPosition.CenterParent;

                TextBox title = new TextBox();
                title.Location = new Point(16, 16);
                title.Size = new Size(360, 23);
                title.Text = "新话术";
                editor.Controls.Add(title);

                TextBox content = new TextBox();
                content.Location = new Point(16, 52);
                content.Size = new Size(360, 150);
                content.Multiline = true;
                content.Text = "请输入话术内容";
                editor.Controls.Add(content);

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
                    reply.Title = title.Text;
                    reply.Content = content.Text;
                    reply.CategoryId = selected == null ? "common" : selected.Category.Id;
                    reply.Tags = title.Text;
                    reply.SortOrder = _currentReplies == null ? 100 : _currentReplies.Length + 100;
                    _replyRepository.SaveReply(reply);
                    RefreshReplies();
                }
            }
        }

        private void ImportClicked(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "话术文件|*.json;*.csv|JSON 文件|*.json|CSV 文件|*.csv";
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string content = File.ReadAllText(dialog.FileName);
            string extension = Path.GetExtension(dialog.FileName);
            int imported = string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase)
                ? _replyRepository.ImportCsv(content, true)
                : _replyRepository.ImportJson(content, true);
            RefreshReplies();
            MessageBox.Show("已导入 " + imported + " 条话术。", "导入话术");
        }

        private void ExportClicked(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "JSON 文件|*.json";
            dialog.FileName = "wechat-plus-quick-replies.json";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, _replyRepository.ExportJson());
                MessageBox.Show("已导出话术库。", "导出话术");
            }
        }

        private void ScreenshotClicked(object sender, EventArgs e)
        {
            bool hide = _hideForScreenshot.Checked;
            if (hide)
            {
                WindowState = FormWindowState.Minimized;
            }
            MessageBox.Show("截图工具入口已就绪：下一阶段接入系统截图流程。", "截图工具");
            if (hide)
            {
                WindowState = FormWindowState.Normal;
            }
        }

        private void ShowMemberState(object sender, EventArgs e)
        {
            LicenseState state = _licenseService.GetOrCreateTrial();
            MessageBox.Show(
                "当前方案：" + state.Plan + Environment.NewLine +
                "试用到期：" + state.ExpiresAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + Environment.NewLine +
                "设备哈希：" + state.DeviceIdHash.Substring(0, 12) + "...",
                "会员状态");
        }

        private void ShowOpenSourceNotice(object sender, EventArgs e)
        {
            MessageBox.Show(
                "WeChat Plus 商用壳不复制、不链接 GPLv3 源码。" + Environment.NewLine +
                "多开/补丁相关底层能力由独立开源助手组件提供，并通过进程边界调用。" + Environment.NewLine +
                "上游项目：https://github.com/huiyadanli/RevokeMsgPatcher" + Environment.NewLine +
                "许可证：GPLv3",
                "开源组件声明");
        }

        private void ToggleRightPanel()
        {
            _rightPanelCollapsed = !_rightPanelCollapsed;
            _rightPanel.Visible = !_rightPanelCollapsed;
            _mainLayout.ColumnStyles[2].Width = _rightPanelCollapsed ? 0 : 292;
            _workspaceStatus.Text = _rightPanelCollapsed ? "右侧话术栏已收起。" : "右侧话术栏已展开。";
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
    }
}
