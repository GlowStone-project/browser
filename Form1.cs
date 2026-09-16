/*
 * Now, Glowstone 1.03
 * 
 * CHANGES:
 * homepage widgets language bug fixed
 * minimal ui changes
*/

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlowStone
{
    public partial class Form1 : Form
    {
        private MenuStrip menuStrip;
        private ToolStripMenuItem menuLanguage;
        private ToolStripMenuItem menuHelp;
        private ToolStripMenuItem menuSpecial;
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
        private Button btnKotost;
        private Label lblErr;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private WebView2 webViewer;

        // BYTEBEAT DEFS
        [DllImport("winmm.dll")]
        public static extern int waveOutOpen
        (
            out IntPtr hwo,
            uint uDeviceID,
            ref WaveFormat lpFormat,
            IntPtr dwCallback,
            IntPtr dwInstance,
            uint dwFlags
        );

        [DllImport("winmm.dll")]
        public static extern int waveOutPrepareHeader
        (
            IntPtr hwo,
            ref WaveHeader lpWaveOutHdr,
            uint uSize
        );

        [DllImport("winmm.dll")]
        public static extern int waveOutWrite(IntPtr hwo, ref WaveHeader lpWaveOutHdr, uint uSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct WaveFormat
        {
            public short wFormatTag; public short nChannels; public int nSamplesPerSec;
            public int nAvgBytesPerSec; public short nBlockAlign;
            public short wBitsPerSample; public short cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WaveHeader
        {
            public IntPtr lpData; public uint dwBufferLength; public uint dwBytesRecorded;
            public IntPtr dwUser; public uint dwFlags; public uint dwLoops;
            public IntPtr lpNext; public IntPtr reserved;
        }

        private string currentLang = "en";
        private Dictionary<string, Dictionary<string, string>> translations;
        private readonly Color ieClassicGray = Color.FromArgb(241, 239, 226);

        private const string ServerURL = "http://localhost:4650";

        public Form1()
        {
            Exception ex = null;

            Application.ThreadException += (s, e) =>
            ShowErrorCodeDialog("GUI_THREAD_EXCEPTION", ex.Message);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is null)
                    ShowErrorCodeDialog("UNHANDLED_DOMAIN_EXCEPTION", ex.Message);
            };

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
                { "Title", "GlowStone versão 1.03" },
                { "Back", " ⇦ Voltar" },
                { "Forward", "⇨ Avançar" },
                { "Stop", "✕ Parar" },
                { "Refresh", "↻ Atualizar" },
                { "Home", "⌂ Inicial" },
                { "Go", "⇨ Ir" },
                { "StatusDone", "Concluído" },
                { "StatusLoading", "Abrindo a página {0}..." },
                { "MenuLang", "&Idioma" },
                { "MenuHelp", "&Ajuda" },
                { "LabelAddr", "Endereço" },
                { "MenuSpecial", "Especial" },
                { "LblError",  "Ocorreu um erro no GlowStone!\n\nCódigo de erro:"}
            };

            translations["en"] = new Dictionary<string, string>
            {
                { "Title", "GlowStone version 1.03" },
                { "Back", " ⇦ Back" },
                { "Forward", "⇨ Forward" },
                { "Stop", "✕ Stop" },
                { "Refresh", "↻ Refresh" },
                { "Home", "⌂ Home" },
                { "Go", "⇨ Go" },
                { "StatusDone", "Done" },
                { "StatusLoading", "Opening page {0}..." },
                { "MenuLang", "&Language" },
                { "MenuHelp", "&Help" },
                { "LabelAddr", "Address" },
                { "MenuSpecial", "Special" },
                { "LblError",  "A error ocurred on GlowStone!\n\nError code: "}
            };

            translations["es"] = new Dictionary<string, string>
            {
                { "Title", "GlowStone versión 1.03" },
                { "Back", " ⇦ Atrás" },
                { "Forward", "⇨ Adelante" },
                { "Stop", "✕ Detener" },
                { "Refresh", "↻ Actualizar" },
                { "Home", "⌂ Inicio" },
                { "Go", "⇨ Ir" },
                { "StatusDone", "Listo" },
                { "StatusLoading", "Abriendo la página {0}..." },
                { "MenuLang", "&Idioma" },
                { "MenuHelp", "&Ayuda" },
                { "LabelAddr", "Dirección" },
                { "MenuSpecial", "Especial" },
                { "LblError",  "Un error aconteció en GlowStone\n\nCódigo de error:"}
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

            System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(Form1));

            menuStrip = new MenuStrip { BackColor = ieClassicGray };
            menuLanguage = new ToolStripMenuItem();

            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

            var menuPt = new ToolStripMenuItem("Português", null, (s, e) => ChangeLanguage("pt"));
            var menuEn = new ToolStripMenuItem("English", null, (s, e) => ChangeLanguage("en"));
            var menuEs = new ToolStripMenuItem("Español", null, (s, e) => ChangeLanguage("es"));

            menuLanguage.DropDownItems.AddRange(new ToolStripItem[] { menuPt, menuEn, menuEs });

            //var menuPlaceHolder = new ToolStripMenuItem("placeholder", null, (s, e) => Placeholder());
            var menuBytebeat = new ToolStripMenuItem("Bytebeat", null, (s, e) => StartBytebeats());

            var menuNewThings =
            new ToolStripMenuItem
            (
                "Coisas novas" +
                " | " +
                "New things" +
                " | " +
                "Cosas nuevas",
                null,
                (s, e) => NewThings()
            );

            var menuProject = new ToolStripMenuItem
            (
                "Glowstone Project",
                null, (s, e) => webViewer.CoreWebView2.Navigate("https://github.com/GlowStone-project")
            );

            var menuAbout =
            new ToolStripMenuItem
            (
                "Sobre | About | Sobre",
                null,
                (s, e) => ShowAboutBox()
            );

            menuHelp = new ToolStripMenuItem();
            menuHelp.DropDownItems.AddRange
            (
                new ToolStripItem[]
                {
                    menuProject,
                    menuNewThings,
                    menuAbout
                }
            );

            menuSpecial = new ToolStripMenuItem();
            menuSpecial.DropDownItems.AddRange
            (
                new ToolStripItem[]
                {
                    menuBytebeat
                }
            );

            menuStrip.Items.Add(menuLanguage);
            menuStrip.Items.Add(menuHelp);
            menuStrip.Items.Add(menuSpecial);

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

            btnRefresh = CreateIEButton(240);
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

            btnKotost = new Button
            {
                Location = new Point(150, 10),
                Size = new Size(40, 25),
                Font = new Font("Comic Sans MS", 8, FontStyle.Bold),
                Text = "Kotost",
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.System
            };
            btnKotost.Click += (s, e) => Kotost();

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
                    btnRefresh, btnHome, btnKotost
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
                Size = new Size(85, 28),
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
            this.menuLanguage.Text = langDict["MenuLang"];
            this.menuHelp.Text = langDict["MenuHelp"];
            this.menuSpecial.Text = langDict["MenuSpecial"];
            btnBack.Text = langDict["Back"];
            btnForward.Text = langDict["Forward"];
            btnStop.Text = langDict["Stop"];
            btnRefresh.Text = langDict["Refresh"];
            btnHome.Text = langDict["Home"];

            btnGo.Text = langDict["Go"];
            lblAddress.Text = langDict["LabelAddr"];
            //lblErr.Text = langDict["LblError"];

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
            //webViewer.CoreWebView2.ProcessFailed += OnWebViewProcessFailed;
            await webViewer.EnsureCoreWebView2Async(null);
            webViewer.CoreWebView2.ProcessFailed += OnWebViewProcessFailed;
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


        /*
        private void Placeholder()
        {
            var langDict = translations[currentLang];

            MessageBox.Show
            (
                "test", "test",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        */


        private void NewThings()
        {
            var langDict = translations[currentLang];

            MessageBox.Show
            (
                "POR: Menu sobre, mudanças na UI, mudanças na homepage, essa janela, coisas especiais," +
                "mais adições de ícones, primeiro easter egg (Kotost)" +
                "\n\n" +
                "ENG: Menu about, UI changes, homepage changes, this window, special things," +
                "more icons addictions, first easter egg (Kotost)\n\n" +
                "ESP: Menu sobre, cambios na UI, cambios na homepage, esa ventana, cosas especiais" +
                "primeiro éaster egg (Kotost) ",
                "Novas coisas | New things | Nuevas cosas",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ShowAboutBox()
        {
            var langDict = translations[currentLang];

            MessageBox.Show
            (
                "GlowStone v1.03\n\nBuild 1.03.0022\n\n" +
                "POR: O GlowStone existe para ser uma versão mais estável do Internet Explorer. " +
                "Sendo de código totalmente diferente do Internet Explorer, sendo aberto.\n\n" +
                "ENG: The GlowStone exist to be a version more stable of Internet Explorer. " +
                "Having a code diferent of Internet Explorer, being open-source.\n\n" +
                "ESP: GlowStone existe para ser una versión más estable de Internet Explorer. " +
                "Es de código totalmente diferente al de Internet Explorer y es abierto. ",
                "Sobre | About | Sobre",
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information
            );

            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AboutBox1());
            */
        }

        public void StartBytebeats()
        {
            int sampleRate = 8000;
            int bufferSize = 4000;

            WaveFormat fmt = new WaveFormat
            {
                wFormatTag = 1,
                nChannels = 1,
                nSamplesPerSec = sampleRate,
                nAvgBytesPerSec = sampleRate,
                nBlockAlign = 1,
                wBitsPerSample = 8,
                cbSize = 0
            };

            if (waveOutOpen(out IntPtr hwo, 0xFFFFFFFF, ref fmt, IntPtr.Zero, IntPtr.Zero, 0) == 0)
            {
                uint t = 0;
                while (true)
                {
                    byte[] buffer = new byte[bufferSize];

                    for (int i = 0; i < bufferSize; i++)
                    {
                        // I tried to use this bytebeat (t * t) >> (t / 257)
                        // Is a laser gun
                        buffer[i] = (byte)
                        (
                            (
                                t *
                                (1 + (1 + (t >> 16) % 6) * (t >> 10) *
                                (t >> 11) % 8) ^ t >> 13 ^ t >> 6
                            ) + t
                        );
                        t++;
                    }

                    GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    WaveHeader header = new WaveHeader
                    {
                        lpData = pinnedArray.AddrOfPinnedObject(),
                        dwBufferLength = (uint)bufferSize
                    };

                    waveOutPrepareHeader(hwo, ref header, (uint)Marshal.SizeOf(header));
                    waveOutWrite(hwo, ref header, (uint)Marshal.SizeOf(header));

                    Thread.Sleep(500);
                    pinnedArray.Free();
                }
            }
        }

        private void OnWebViewProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            string errorCode = $"WEBVIEW_CRASH_{e.ProcessFailedKind}";
            string errorDetails = $"Fail type: {e.ProcessFailedKind}\r\n" +
                                  $"Exit Code: {e.ExitCode}\r\n" +
                                  $"Shutdown reason: {e.Reason}";

            ShowErrorCodeDialog(errorCode, errorDetails, null);

            try
            {
                webViewer.Reload();
            }
            catch
            {
                InitializeWebView();
            }
        }

        private void ShowErrorCodeDialog(string errorCode, string details, Exception ex = null)
        {
            string hexCode = $"0x{Math.Abs(errorCode.GetHashCode()):X8}";

            var log = new System.Text.StringBuilder();
            log.AppendLine($"[ERROR CODE]: {hexCode}");
            log.AppendLine($"[IDENTIFIER]: {errorCode}");
            log.AppendLine(new string('-', 60));
            log.AppendLine("[PROCESS DETAILS / CONTEXT]:");
            log.AppendLine(details);

            if (ex != null)
            {
                log.AppendLine(new string('-', 60));
                log.AppendLine($"[EXCEPTION TYPE]: {ex.GetType().FullName}");
                log.AppendLine($"[ERROR MESSAGE]: {ex.Message}");

                if (ex.InnerException != null)
                {
                    log.AppendLine($"[INTERN EXCEPTION]: {ex.InnerException.Message}");
                }

                log.AppendLine(new string('-', 60));
                log.AppendLine("[COMPLETE STACK TRACE]:");
                log.AppendLine(ex.StackTrace ?? "No StackTrace disponible");
            }

            Form crashForm = new Form
            {
                Text = "Erro Crítico | Critical Error | Error Crítico",
                Size = new Size(640, 480),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ieClassicGray
            };

            Label lblIcon = new Label
            {
                Text = "⚠️",
                Font = new Font("Segoe UI Emoji", 28),
                Location = new Point(15, 15),
                AutoSize = true
            };

            lblErr = new Label
            {
                Text = "A error ocurred on GlowStone!\n\n" +
                "Possible causes: Memory burst, a component failed or Chromium/Web crashed.\n\n" +
                "If the error persists, go to GlowStone github repository and make a issue" +
                "reporting this error for creator (Danoni631).",
                Font = new Font("Tahoma", 8.5f, FontStyle.Bold),
                Location = new Point(95, 15),
                Size = new Size(380, 110),
                ForeColor = Color.Black
            };

            TextBox txtDetails = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = log.ToString(),
                Font = new Font("Consolas", 8.5f),
                Location = new Point(15, 130),
                Size = new Size(595, 260)
            };

            Button btnClose = new Button
            {
                Text = "OK",
                Location = new Point(20, 400),
                Size = new Size(80, 25),
                FlatStyle = FlatStyle.System,
                DialogResult = DialogResult.OK
            };

            Button btnCpy = new Button
            {
                Text = "Copy error",
                Location = new Point(90, 400),
                Size = new Size(80, 25),
                FlatStyle = FlatStyle.System,
            };

            btnCpy.Click += (s, e) =>
            {
                Clipboard.SetText(txtDetails.Text);
                MessageBox.Show
                (
                    "Error code copied",
                    "Copied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };

            crashForm.Controls.Add(lblIcon);
            crashForm.Controls.Add(lblErr);
            crashForm.Controls.Add(txtDetails);
            crashForm.Controls.Add(btnClose);
            crashForm.AcceptButton = btnClose;

            crashForm.ShowDialog(this);
        }

        private Form KotostWindows;

        private void Kotost()
        {
            System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(Form1));

            KotostWindows = new Form
            {
                Text = "K O T O S T",
                Size = new Size(800, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ieClassicGray
            };

            Label KotostLabel = new Label
            {
                Text = "KOTOST KOTOST",
                Size = new Size(640, 480),
                Location = new Point(400, 300),
                Font = new Font("Comic Sans MS", 15.0f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            KotostWindows.Controls.Add(KotostLabel);
            KotostWindows.ShowDialog(this);
        }
    }
}
