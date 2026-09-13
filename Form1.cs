/*
 * My Computer is a shit (Celeron Dual Core with 4GB RAM)
 * My Wi-fi is a shit
 * I'm bad on CSharp
 * The pain i gonna feel coding this
 * 
 * So... Version 1.02 (I've tested installation Setup and don't worked)
*/

using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glowstone
{
    public partial class Form1 : Form
    {
        private MenuStrip menuStrip;
        private ToolStripMenuItem menuLanguage;
        private ToolStripMenuItem menuHelp;
        private Panel toolBar;
        private Panel secondtoolBar;
        private Button btnBack;
        private Button btnForward;
        private Button btnStop;
        private Button btnRefresh;
        private Button btnHome;
        private TextBox txtAddress;
        private Label lblAddress;
        private Button btnGo;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private WebView2 webViewer;

        private string currentLang = "en";
        private Dictionary<string, Dictionary<string, string>> translations;
        private readonly Color ieClassicGray = Color.FromArgb(241, 239, 226);

        private const string ServerURL = "http://localhost:4650";

        public Form1()
        {
            InitializeTranslations();
            DetectSystemLanguage();
            InitializeComponentLayout();
            InitializeWebView();
        }

        private void InitializeTranslations()
        {
            translations = new Dictionary<string, Dictionary<string, string>>();

            translations["pt"] = new Dictionary<string, string>
            {
                { "Title", "GlowStone 1.02" },
                { "Back", " ⇦ Voltar" },
                { "Forward", "⇨ Avançar" },
                { "Stop", "✕ Parar" },
                { "Refresh", "↻ Atualizar" },
                { "Home", "⌂ Inicial" },
                { "Go", "Ir" },
                { "StatusDone", "Concluído" },
                { "StatusLoading", "Abrindo a página {0}..." },
                { "MenuLang", "&Idioma" },
                { "LabelAddr", "Endereço" },
                { "AboutTitle", "Sobre" },
                { "AboutText", "GlowStone 1.02\n\nDesenvolvedor: Danoni631" }
            };

            translations["en"] = new Dictionary<string, string>
            {
                { "Title", "GlowStone 1.02" },
                { "Back", " ⇦ Back" },
                { "Forward", "⇨ Forward" },
                { "Stop", "✕ Stop" },
                { "Refresh", "↻ Refresh" },
                { "Home", "⌂ Home" },
                { "Go", "Go" },
                { "StatusDone", "Done" },
                { "StatusLoading", "Opening page {0}..." },
                { "MenuLang", "&Language" },
                { "MenuHelp", "&Help" },
                { "LabelAddr", "Address" },
                { "AboutTitle", "About" },
                { "AboutText", "GlowStone 1.02\n\nDeveloper: Danoni631" }
            };

            translations["es"] = new Dictionary<string, string>
            {
                { "Title", "GlowStone 1.02" },
                { "Back", " ⇦ Atrás" },
                { "Forward", "⇨ Adelante" },
                { "Stop", "✕ Detener" },
                { "Refresh", "↻ Actualizar" },
                { "Home", "⌂ Inicio" },
                { "Go", "Ir" },
                { "StatusDone", "Listo" },
                { "StatusLoading", "Abriendo la página {0}..." },
                { "MenuLang", "&Idioma" },
                { "MenuHelp", "&Ayuda" },
                { "LabelAddr", "Dirección" },
                { "AboutTitle", "Sobre" },
                { "AboutText", "GlowStone versión 1.02\n\nNavegador hecho desde cero, de código abierto, para ser una imitación estable del Internet Explorer con motor Edge/WebView2 y así lograr un mejor rendimiento que el Internet Explorer original.\n\nEl Internet Explorer original fue hecho por Microsoft y este navegador no tiene nada de su código.\n\n© Danoni631 - 2026-2026" },

            };
        }

        private void DetectSystemLanguage()
        {
            string sysLang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();

            if (translations.ContainsKey(sysLang))
                currentLang = sysLang;
            else
                currentLang = "en";
        }

        private void InitializeComponentLayout()
        {
            this.Size = new Size(1024, 768);
            this.BackColor = ieClassicGray;
            this.Icon = SystemIcons.WinLogo;

            menuStrip = new MenuStrip { BackColor = ieClassicGray };
            menuLanguage = new ToolStripMenuItem();

            var menuPt = new ToolStripMenuItem("Português", null, (s, e) => ChangeLanguage("pt"));
            var menuEn = new ToolStripMenuItem("English", null, (s, e) => ChangeLanguage("en"));
            var menuEs = new ToolStripMenuItem("Español", null, (s, e) => ChangeLanguage("es"));

            menuLanguage.DropDownItems.AddRange(new ToolStripItem[] { menuPt, menuEn, menuEs });
            menuStrip.Items.Add(menuLanguage);

            var menuAbout = new ToolStripMenuItem("Sobre | About | Sobre", null, (s, e) => ShowAboutBox());

            toolBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = ieClassicGray,
                Padding = new Padding(5)
            };

            secondtoolBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = ieClassicGray,
                Padding = new Padding(5)
            };

            btnBack = CreateIEButton(0);
            btnBack.Click += (s, e) =>
            {
                if
                (webViewer?.CoreWebView2 != null && webViewer.CanGoBack) webViewer.GoBack();
            };

            btnForward = CreateIEButton(80);
            btnForward.Click += (s, e) =>
            {
                if (webViewer?.CoreWebView2 != null && webViewer.CanGoForward)
                    webViewer.GoForward();
            };

            btnStop = CreateIEButton(160);
            btnStop.Click += (s, e) => webViewer?.CoreWebView2?.Stop();

            btnRefresh = CreateIEButton(230);
            btnRefresh.Click += (s, e) => webViewer?.Reload();

            btnHome = CreateIEButton(310);
            btnHome.Click += (s, e) =>
            webViewer?.CoreWebView2?.Navigate
            (
                // Changed the path of main.html to Glowstone homepage server
                ServerURL
            );

            txtAddress = new TextBox
            {
                Location = new Point(130, 12),
                Width = 2,
                Font = new Font("Tahoma", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            txtAddress.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) NavigateToUrl(); };

            btnGo = new Button
            {
                Location = new Point(150, 10),
                Size = new Size(40, 25),
                Font = new Font("Tahoma", 8, FontStyle.Bold),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.System
            };
            btnGo.Click += (s, e) => NavigateToUrl();

            lblAddress = new Label
            {
                Location = new Point(20, 10),
                Size = new Size(100, 25),
                Font = new Font("Tahoma", 16, FontStyle.Regular),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                FlatStyle = FlatStyle.System
            };

            toolBar.Controls.AddRange
            (
                new Control[]
                {
                    btnBack, btnForward, btnStop,
                    btnRefresh, btnHome
                }
            );

            secondtoolBar.Controls.AddRange
            (
                new Control[]
                {
                    lblAddress, txtAddress, btnGo
                }
            );

            statusStrip = new StatusStrip { BackColor = ieClassicGray };
            statusLabel = new ToolStripStatusLabel { Font = new Font("Tahoma", 8) };
            statusStrip.Items.Add(statusLabel);

            webViewer = new WebView2 { Dock = DockStyle.Fill };

            webViewer.NavigationStarting += (s, e) =>
            {
                statusLabel.Text = string.Format(translations[currentLang]["StatusLoading"], e.Uri);
            };

            webViewer.NavigationCompleted += (s, e) =>
            {
                statusLabel.Text = translations[currentLang]["StatusDone"];
                if (webViewer.Source != null)
                    txtAddress.Text = webViewer.Source.ToString();
            };

            ApplyLanguageStrings();

            this.Controls.Add(webViewer);
            this.Controls.Add(secondtoolBar);
            this.Controls.Add(toolBar);
            this.Controls.Add(menuStrip);
            this.Controls.Add(statusStrip);
            this.MainMenuStrip = menuStrip;
        }

        private Button CreateIEButton(int xPosition)
        {
            return new Button
            {
                Location = new Point(xPosition, 8),
                Size = new Size(75, 28),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Tahoma", 8.5f),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ieClassicGray
            };
        }

        private void ApplyLanguageStrings()
        {
            var langDict = translations[currentLang];

            this.Text = langDict["Title"];
            menuLanguage.Text = langDict["MenuLang"];
            btnBack.Text = langDict["Back"];
            btnForward.Text = langDict["Forward"];
            btnStop.Text = langDict["Stop"];
            btnRefresh.Text = langDict["Refresh"];
            btnHome.Text = langDict["Home"];

            btnGo.Text = langDict["Go"];
            lblAddress.Text = langDict["LabelAddr"];


            if (webViewer?.CoreWebView2 == null || !webViewer.CanGoBack && !webViewer.CanGoForward)
            {
                statusLabel.Text = langDict["StatusDone"];
            }
        }

        private void ChangeLanguage(string langCode)
        {
            if (translations.ContainsKey(langCode))
            {
                currentLang = langCode;
                ApplyLanguageStrings();
            }
        }

        private async void InitializeWebView()
        {
            await webViewer.EnsureCoreWebView2Async(null);
            webViewer.CoreWebView2.Navigate
            (ServerURL);

            // Don't used the path more, now we use server becuase the path only works on my computer
        }


        private void NavigateToUrl()
        {
            string url = txtAddress.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            webViewer?.CoreWebView2?.Navigate(url);
        }

        private void ShowAboutBox()
        {
            var langDict = translations[currentLang];

            MessageBox.Show
            (
                langDict["AboutText"],
                langDict["AboutTitle"],
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

    }
}
