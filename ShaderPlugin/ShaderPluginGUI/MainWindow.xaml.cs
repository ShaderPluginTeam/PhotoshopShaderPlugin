using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Folding;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using Clipboard = System.Windows.Forms.Clipboard;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using System.Reflection;

namespace ShaderPluginGUI
{
    public partial class MainWindow : Window
    {
        public static RoutedCommand CopyImage = new RoutedCommand();
        public static RoutedCommand PasteImage = new RoutedCommand();

        SettingsXML Settings;
        SaveFileDialog saveFileDialog;
        OpenFileDialog openFileDialog;
        bool ApplyLastParams_NeedCloseWindow = false; // Used for prevent memory leaks (Free resources after form closing)
        bool TemporaryDisableComboBoxesEvents = false; // Prevent shader compilation at shader loading process
        readonly Regex DigitsCheckerRegex = new Regex("[0-9.]");
        #region Errors Showing and Highlighting
        string LastErrorStr = String.Empty;
        ErrorLineBackgroundRenderer ErrorLineBGRendererImage_VS = new ErrorLineBackgroundRenderer(), ErrorLineBGRendererImage_FS = new ErrorLineBackgroundRenderer();
        ErrorLineBackgroundRenderer ErrorLineBGRendererBufferA_VS = new ErrorLineBackgroundRenderer(), ErrorLineBGRendererBufferA_FS = new ErrorLineBackgroundRenderer();
        ErrorLineBackgroundRenderer ErrorLineBGRendererBufferB_VS = new ErrorLineBackgroundRenderer(), ErrorLineBGRendererBufferB_FS = new ErrorLineBackgroundRenderer();
        ErrorLineBackgroundRenderer ErrorLineBGRendererBufferC_VS = new ErrorLineBackgroundRenderer(), ErrorLineBGRendererBufferC_FS = new ErrorLineBackgroundRenderer();
        ErrorLineBackgroundRenderer ErrorLineBGRendererBufferD_VS = new ErrorLineBackgroundRenderer(), ErrorLineBGRendererBufferD_FS = new ErrorLineBackgroundRenderer();
        #endregion
        #region Folding
        FoldingManager FoldingManager_Image_VS, FoldingManager_Image_FS,
            FoldingManager_BufferA_VS, FoldingManager_BufferA_FS,
            FoldingManager_BufferB_VS, FoldingManager_BufferB_FS,
            FoldingManager_BufferC_VS, FoldingManager_BufferC_FS,
            FoldingManager_BufferD_VS, FoldingManager_BufferD_FS;

        GLSLFoldingStrategy FoldingStrategy_Image_VS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_Image_FS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_BufferA_VS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_BufferA_FS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_BufferB_VS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_BufferB_FS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_BufferC_VS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_BufferC_FS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_BufferD_VS = new GLSLFoldingStrategy();
        GLSLFoldingStrategy FoldingStrategy_BufferD_FS = new GLSLFoldingStrategy();
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            #region Recover window size
            if (Properties.Settings.Default.width > 0)
                this.Width = Properties.Settings.Default.width;

            if (Properties.Settings.Default.height > 0)
                this.Height = Properties.Settings.Default.height;
            #endregion

            #region Shader Error and Error Lines
            Engine.ShaderError += Engine_ShaderError;
            textEditorImage_VS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererImage_VS);
            textEditorImage_FS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererImage_FS);
            textEditorBufferA_VS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererBufferA_VS);
            textEditorBufferA_FS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererBufferA_FS);
            textEditorBufferB_VS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererBufferB_VS);
            textEditorBufferB_FS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererBufferB_FS);
            textEditorBufferC_VS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererBufferC_VS);
            textEditorBufferC_FS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererBufferC_FS);
            textEditorBufferD_VS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererBufferD_VS);
            textEditorBufferD_FS.TextArea.TextView.BackgroundRenderers.Add(ErrorLineBGRendererBufferD_FS);
            #endregion

            #region Command binding
            CommandBindings.Add(new CommandBinding(CopyImage));
            CommandBindings.Add(new CommandBinding(PasteImage));
            #endregion

            #region Fix Enums, set it to ComboBoxes as ItemsSource
            var FixedMagFilter = Array.FindAll((TextureMagFilter[])Enum.GetValues(typeof(TextureMagFilter)),
                (TextureMagFilter M) => { return !(M.ToString().EndsWith("Sgis") || M.ToString().EndsWith("Sgix")); });

            var FixedMinFilter = Array.FindAll((TextureMinFilter[])Enum.GetValues(typeof(TextureMinFilter)),
                (TextureMinFilter M) => { return !(M.ToString().EndsWith("Sgis") || M.ToString().EndsWith("Sgix")); });

            var FixedWrapMode = Array.FindAll((TextureWrapMode[])Enum.GetValues(typeof(TextureWrapMode)),
                (TextureWrapMode M) => { return !(M.ToString().EndsWith("Sgis") || M.ToString().EndsWith("Nv") || M.ToString().EndsWith("Arb")); }).Distinct();

            #region Buffer A
            comboBox_BufferA_ImageMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferA_ImageMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferA_ImageWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferA_ImageWrapT.ItemsSource = FixedWrapMode;
            #endregion

            #region Buffer B
            comboBox_BufferB_ImageMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferB_ImageMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferB_ImageWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferB_ImageWrapT.ItemsSource = FixedWrapMode;

            comboBox_BufferB_BufferAMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferB_BufferAMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferB_BufferAWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferB_BufferAWrapT.ItemsSource = FixedWrapMode;
            #endregion

            #region Buffer C
            comboBox_BufferC_ImageMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferC_ImageMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferC_ImageWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferC_ImageWrapT.ItemsSource = FixedWrapMode;

            comboBox_BufferC_BufferAMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferC_BufferAMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferC_BufferAWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferC_BufferAWrapT.ItemsSource = FixedWrapMode;

            comboBox_BufferC_BufferBMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferC_BufferBMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferC_BufferBWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferC_BufferBWrapT.ItemsSource = FixedWrapMode;
            #endregion

            #region Buffer D
            comboBox_BufferD_ImageMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferD_ImageMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferD_ImageWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferD_ImageWrapT.ItemsSource = FixedWrapMode;

            comboBox_BufferD_BufferAMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferD_BufferAMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferD_BufferAWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferD_BufferAWrapT.ItemsSource = FixedWrapMode;

            comboBox_BufferD_BufferBMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferD_BufferBMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferD_BufferBWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferD_BufferBWrapT.ItemsSource = FixedWrapMode;

            comboBox_BufferD_BufferCMagFilter.ItemsSource = FixedMagFilter;
            comboBox_BufferD_BufferCMinFilter.ItemsSource = FixedMinFilter;
            comboBox_BufferD_BufferCWrapS.ItemsSource = FixedWrapMode;
            comboBox_BufferD_BufferCWrapT.ItemsSource = FixedWrapMode;
            #endregion

            #region Image
            comboBox_Image_Buffers.ItemsSource = (MultiPassBuffers[])Enum.GetValues(typeof(MultiPassBuffers));

            comboBox_Image_ImageMagFilter.ItemsSource = FixedMagFilter;
            comboBox_Image_ImageMinFilter.ItemsSource = FixedMinFilter;
            comboBox_Image_ImageWrapS.ItemsSource = FixedWrapMode;
            comboBox_Image_ImageWrapT.ItemsSource = FixedWrapMode;

            comboBox_Image_BufferAMagFilter.ItemsSource = FixedMagFilter;
            comboBox_Image_BufferAMinFilter.ItemsSource = FixedMinFilter;
            comboBox_Image_BufferAWrapS.ItemsSource = FixedWrapMode;
            comboBox_Image_BufferAWrapT.ItemsSource = FixedWrapMode;

            comboBox_Image_BufferBMagFilter.ItemsSource = FixedMagFilter;
            comboBox_Image_BufferBMinFilter.ItemsSource = FixedMinFilter;
            comboBox_Image_BufferBWrapS.ItemsSource = FixedWrapMode;
            comboBox_Image_BufferBWrapT.ItemsSource = FixedWrapMode;

            comboBox_Image_BufferCMagFilter.ItemsSource = FixedMagFilter;
            comboBox_Image_BufferCMinFilter.ItemsSource = FixedMinFilter;
            comboBox_Image_BufferCWrapS.ItemsSource = FixedWrapMode;
            comboBox_Image_BufferCWrapT.ItemsSource = FixedWrapMode;

            comboBox_Image_BufferDMagFilter.ItemsSource = FixedMagFilter;
            comboBox_Image_BufferDMinFilter.ItemsSource = FixedMinFilter;
            comboBox_Image_BufferDWrapS.ItemsSource = FixedWrapMode;
            comboBox_Image_BufferDWrapT.ItemsSource = FixedWrapMode;
            #endregion

            #region Settings
            comboBox_ViewMode.ItemsSource = (DrawModes[])Enum.GetValues(typeof(DrawModes));
            comboBox_ViewMode.SelectedItem = DrawModes.RGBA;

            comboBox_MagFilter.ItemsSource = FixedMagFilter;
            comboBox_MinFilter.ItemsSource = FixedMinFilter;
            #endregion
            #endregion

            #region Highlighting
            using (MemoryStream ms = new MemoryStream(Properties.Resources.glslHightlight))
            {
                using (XmlTextReader reader = new XmlTextReader(ms))
                {
                    IHighlightingDefinition xshd = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    InitTextEditor(xshd, textEditorImage_VS);
                    InitTextEditor(xshd, textEditorImage_FS);
                    InitTextEditor(xshd, textEditorBufferA_VS);
                    InitTextEditor(xshd, textEditorBufferA_FS);
                    InitTextEditor(xshd, textEditorBufferB_VS);
                    InitTextEditor(xshd, textEditorBufferB_FS);
                    InitTextEditor(xshd, textEditorBufferC_VS);
                    InitTextEditor(xshd, textEditorBufferC_FS);
                    InitTextEditor(xshd, textEditorBufferD_VS);
                    InitTextEditor(xshd, textEditorBufferD_FS);
                }
            }
            textEditorImage_VS.Options.EnableHyperlinks = textEditorImage_FS.Options.EnableHyperlinks = false;
            textEditorBufferA_VS.Options.EnableHyperlinks = textEditorBufferA_FS.Options.EnableHyperlinks = false;
            textEditorBufferB_VS.Options.EnableHyperlinks = textEditorBufferB_FS.Options.EnableHyperlinks = false;
            textEditorBufferC_VS.Options.EnableHyperlinks = textEditorBufferC_FS.Options.EnableHyperlinks = false;
            textEditorBufferD_VS.Options.EnableHyperlinks = textEditorBufferD_FS.Options.EnableHyperlinks = false;
            #endregion

            #region Folding
            FoldingManager_Image_VS = FoldingManager.Install(textEditorImage_VS.TextArea);
            FoldingManager_Image_FS = FoldingManager.Install(textEditorImage_FS.TextArea);
            FoldingManager_BufferA_VS = FoldingManager.Install(textEditorBufferA_VS.TextArea);
            FoldingManager_BufferA_FS = FoldingManager.Install(textEditorBufferA_FS.TextArea);
            FoldingManager_BufferB_VS = FoldingManager.Install(textEditorBufferB_VS.TextArea);
            FoldingManager_BufferB_FS = FoldingManager.Install(textEditorBufferB_FS.TextArea);
            FoldingManager_BufferC_VS = FoldingManager.Install(textEditorBufferC_VS.TextArea);
            FoldingManager_BufferC_FS = FoldingManager.Install(textEditorBufferC_FS.TextArea);
            FoldingManager_BufferD_VS = FoldingManager.Install(textEditorBufferD_VS.TextArea);
            FoldingManager_BufferD_FS = FoldingManager.Install(textEditorBufferD_FS.TextArea);
            #endregion

            #region File Dialogs
            openFileDialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                InitialDirectory = Program.ShadersFolderPath,
                Filter = "All Shaders|*.vs;*.fs;*.ps;*.shader|Shader Files (*.shader)|*.shader|Vertex Shader (*.vs)|*.vs|Fragment Shader (*.fs,*.ps)|*.fs;*.ps|All Files (*.*)|*.*",
                FilterIndex = 1
            };

            saveFileDialog = new SaveFileDialog
            {
                RestoreDirectory = true,
                InitialDirectory = Program.ShadersFolderPath,
                Filter = "Shader Files (*.shader)|*.shader|Fragment Shader (*.fs)|*.fs|Vertex Shader (*.vs)|*.vs|All Files (*.*)|*.*",
                FilterIndex = 1
            };
            #endregion

            Settings = SettingsXML.Load(Path.Combine(Program.StartupPath, SettingsXML.XmlFile));
            SettingsApply();
        }

        private void SettingsSave()
        {
            Settings.MagFilter = Engine.MagFilter;
            Settings.MinFilter = Engine.MinFilter;
            Settings.UseMipMaps = Engine.UseMipMaps;

            Settings.BackgroundColor = Engine.GLClearColor;

            Settings.PreviewLine.LineWidth = Engine.PreviewLineWidth;
            Settings.PreviewLine.LineColor = Engine.PreviewLineColor;

            Settings.Grid.GridColor1 = Engine.GridColor1;
            Settings.Grid.GridColor2 = Engine.GridColor2;
            Settings.Grid.GridSize = Engine.GridSize;

            SettingsXML.Save(Settings, Path.Combine(Program.StartupPath, SettingsXML.XmlFile));
        }

        private void SettingsApply()
        {
            comboBox_MagFilter.SelectedItem = Engine.MagFilter = Settings.MagFilter;
            comboBox_MinFilter.SelectedItem = Engine.MinFilter = Settings.MinFilter;
            checkBox_MipMaps.IsChecked = Engine.UseMipMaps = Settings.UseMipMaps;

            Engine.GLClearColor = Settings.BackgroundColor;
            rectangle_BGColor.Fill = new SolidColorBrush(Settings.BackgroundColor.WithoutAlpha);

            Engine.PreviewLineWidth = Settings.PreviewLine.LineWidth;
            Engine.PreviewLineColor = Settings.PreviewLine.LineColor;
            rectangle_PreviewLineColor.Fill = new SolidColorBrush(Settings.PreviewLine.LineColor.WithoutAlpha);

            Engine.GridColor1 = Settings.Grid.GridColor1;
            rectangle_GridColor1.Fill = new SolidColorBrush(Settings.Grid.GridColor1.WithoutAlpha);

            Engine.GridColor2 = Settings.Grid.GridColor2;
            rectangle_GridColor2.Fill = new SolidColorBrush(Settings.Grid.GridColor2.WithoutAlpha);

            Engine.GridSize = Settings.Grid.GridSize;
            tbGridSize.Text = Engine.GridSize.ToString();

            glControl.Invalidate();
        }

        private void SetDefaultTextureParameters()
        {
            TemporaryDisableComboBoxesEvents = true;

            #region Buffer A
            comboBox_BufferA_ImageMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferA_ImageMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferA_ImageWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferA_ImageWrapT.SelectedItem = TextureWrapMode.Repeat;
            #endregion

            #region Buffer B
            comboBox_BufferB_ImageMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferB_ImageMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferB_ImageWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferB_ImageWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_BufferB_BufferAMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferB_BufferAMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferB_BufferAWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferB_BufferAWrapT.SelectedItem = TextureWrapMode.Repeat;
            #endregion

            #region Buffer C
            comboBox_BufferC_ImageMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferC_ImageMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferC_ImageWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferC_ImageWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_BufferC_BufferAMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferC_BufferAMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferC_BufferAWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferC_BufferAWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_BufferC_BufferBMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferC_BufferBMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferC_BufferBWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferC_BufferBWrapT.SelectedItem = TextureWrapMode.Repeat;
            #endregion

            #region Buffer D
            comboBox_BufferD_ImageMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferD_ImageMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferD_ImageWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferD_ImageWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_BufferD_BufferAMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferD_BufferAMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferD_BufferAWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferD_BufferAWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_BufferD_BufferBMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferD_BufferBMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferD_BufferBWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferD_BufferBWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_BufferD_BufferCMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_BufferD_BufferCMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_BufferD_BufferCWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_BufferD_BufferCWrapT.SelectedItem = TextureWrapMode.Repeat;
            #endregion

            #region Image
            comboBox_Image_Buffers.SelectedItem = MultiPassBuffers.NoBuffers;

            comboBox_Image_ImageMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_Image_ImageMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_Image_ImageWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_Image_ImageWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_Image_BufferAMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_Image_BufferAMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_Image_BufferAWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_Image_BufferAWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_Image_BufferBMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_Image_BufferBMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_Image_BufferBWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_Image_BufferBWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_Image_BufferCMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_Image_BufferCMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_Image_BufferCWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_Image_BufferCWrapT.SelectedItem = TextureWrapMode.Repeat;

            comboBox_Image_BufferDMagFilter.SelectedItem = TextureMagFilter.Linear;
            comboBox_Image_BufferDMinFilter.SelectedItem = TextureMinFilter.Linear;
            comboBox_Image_BufferDWrapS.SelectedItem = TextureWrapMode.Repeat;
            comboBox_Image_BufferDWrapT.SelectedItem = TextureWrapMode.Repeat;
            #endregion

            TemporaryDisableComboBoxesEvents = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApplyLastParams_NeedCloseWindow)
                Close();

            ZoomUpdate(false);
            InvalidateVisual();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateVisual();

            Properties.Settings.Default.width = (int)this.Width;
            Properties.Settings.Default.height = (int)this.Height;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void InitTextEditor(IHighlightingDefinition hightlight, TextEditor editor)
        {
            editor.SyntaxHighlighting = hightlight;
            var linenummarg = editor.TextArea.LeftMargins.First() as LineNumberMargin;
            linenummarg.Margin = new Thickness(7, 0, 2, 0);
        }

        private void Engine_ShaderError(string Name, Tuple<ShaderErrorType, string>[] Errors)
        {
            // Save Error for Error Window
            LastErrorStr = String.Join(Environment.NewLine, Errors.Select(err => err.Item1.ToString() + " Info:" + Environment.NewLine + err.Item2 + Environment.NewLine).ToArray());

            // Show error at StatusBar
            ShaderErrorType ErrorType = Errors[0].Item1;
            string InfoLog = Errors[0].Item2;
            int[] ErrorLines = Shader.GetErrorsFromLog(InfoLog);

            if (ErrorLines.Length > 0 && ErrorType != ShaderErrorType.LoadFromString)
            {
                statusBarError.Text = String.Format("{0} shader error at Line: {1}", ErrorType, ErrorLines[0]);
            }
            else
            {
                statusBarError.Text = InfoLog.
                    Replace("\r\n", " ").
                    Replace("\n", " ").
                    Replace("------------- ", String.Empty). // "Fragment info"
                    Replace("----------- ", String.Empty).   // "Vertex info"
                    Replace("  ", " ");
            }

            // Error Lines Highlight
            foreach (var Error in Errors)
            {
                ErrorType = Error.Item1;
                InfoLog = Error.Item2;

                ErrorLines = Shader.GetErrorsFromLog(InfoLog);

                if (ErrorType == ShaderErrorType.Vertex)
                {
                    switch (Name)
                    {
                        default:
                            ErrorLineBGRendererImage_VS.ErrorLines = ErrorLines?.ToList();
                            textEditorImage_VS.TextArea.TextView.InvalidateVisual();
                            break;
                        case "EditingBufferA":
                            ErrorLineBGRendererBufferA_VS.ErrorLines = ErrorLines?.ToList();
                            textEditorBufferA_VS.TextArea.TextView.InvalidateVisual();
                            break;
                        case "EditingBufferB":
                            ErrorLineBGRendererBufferB_VS.ErrorLines = ErrorLines?.ToList();
                            textEditorBufferB_VS.TextArea.TextView.InvalidateVisual();
                            break;
                        case "EditingBufferC":
                            ErrorLineBGRendererBufferC_VS.ErrorLines = ErrorLines?.ToList();
                            textEditorBufferC_VS.TextArea.TextView.InvalidateVisual();
                            break;
                        case "EditingBufferD":
                            ErrorLineBGRendererBufferD_VS.ErrorLines = ErrorLines?.ToList();
                            textEditorBufferD_VS.TextArea.TextView.InvalidateVisual();
                            break;
                    }
                }
                else if (ErrorType == ShaderErrorType.Fragment)
                {
                    switch (Name)
                    {
                        default:
                            ErrorLineBGRendererImage_FS.ErrorLines = ErrorLines?.ToList();
                            textEditorImage_FS.TextArea.TextView.InvalidateVisual();
                            break;
                        case "EditingBufferA":
                            ErrorLineBGRendererBufferA_FS.ErrorLines = ErrorLines?.ToList();
                            textEditorBufferA_FS.TextArea.TextView.InvalidateVisual();
                            break;
                        case "EditingBufferB":
                            ErrorLineBGRendererBufferB_FS.ErrorLines = ErrorLines?.ToList();
                            textEditorBufferB_FS.TextArea.TextView.InvalidateVisual();
                            break;
                        case "EditingBufferC":
                            ErrorLineBGRendererBufferC_FS.ErrorLines = ErrorLines?.ToList();
                            textEditorBufferC_FS.TextArea.TextView.InvalidateVisual();
                            break;
                        case "EditingBufferD":
                            ErrorLineBGRendererBufferD_FS.ErrorLines = ErrorLines?.ToList();
                            textEditorBufferD_FS.TextArea.TextView.InvalidateVisual();
                            break;
                    }
                }
            }
        }

        private bool LoadShaderData(string FileName)
        {
            int Index = openFileDialog.FilterIndex;

            if (Index == 1 && Path.GetExtension(FileName).ToLowerInvariant() == ".shader")
                Index = 2;

            switch (Index)
            {
                case 2:
                    ShaderXML shaderXML = ShaderXML.Load(FileName);
                    if (shaderXML == null)
                        return false;

                    ApplyLoadedShaderXML(shaderXML);
                    break;

                default:
                    string ShaderStr = File.ReadAllText(FileName);

                    switch (tabControlMain.SelectedIndex)
                    {
                        default:
                            (tabControlImage.SelectedIndex == 0 ? textEditorImage_VS : textEditorImage_FS).Text = ShaderStr;
                            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(tabControlImage.SelectedIndex == 0 ? FoldingManager_Image_VS : FoldingManager_Image_FS);
                            break;
                        case 0:
                            (tabControlBufferA.SelectedIndex == 0 ? textEditorBufferA_VS : textEditorBufferA_FS).Text = ShaderStr;
                            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(tabControlBufferA.SelectedIndex == 0 ? FoldingManager_BufferA_VS : FoldingManager_BufferA_FS);
                            break;
                        case 1:
                            (tabControlBufferB.SelectedIndex == 0 ? textEditorBufferB_VS : textEditorBufferB_FS).Text = ShaderStr;
                            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(tabControlBufferB.SelectedIndex == 0 ? FoldingManager_BufferB_VS : FoldingManager_BufferB_FS);
                            break;
                        case 2:
                            (tabControlBufferC.SelectedIndex == 0 ? textEditorBufferC_VS : textEditorBufferC_FS).Text = ShaderStr;
                            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(tabControlBufferC.SelectedIndex == 0 ? FoldingManager_BufferC_VS : FoldingManager_BufferC_FS);
                            break;
                        case 3:
                            (tabControlBufferD.SelectedIndex == 0 ? textEditorBufferD_VS : textEditorBufferD_FS).Text = ShaderStr;
                            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(tabControlBufferD.SelectedIndex == 0 ? FoldingManager_BufferD_VS : FoldingManager_BufferD_FS);
                            break;
                    }
                    break;
            }

            CompileShader();
            return true;
        }

        private bool CompileShader()
        {
            if (!Engine.IsInitialized)
                return false;

            // Clear Errors at StatusBar and ErrorLines Highlight
            statusBarError.Text = String.Empty;
            LastErrorStr = String.Empty;
            ErrorLineBGRendererImage_VS.ErrorLines.Clear();
            ErrorLineBGRendererImage_FS.ErrorLines.Clear();
            ErrorLineBGRendererBufferA_VS.ErrorLines.Clear();
            ErrorLineBGRendererBufferA_FS.ErrorLines.Clear();
            ErrorLineBGRendererBufferB_VS.ErrorLines.Clear();
            ErrorLineBGRendererBufferB_FS.ErrorLines.Clear();
            ErrorLineBGRendererBufferC_VS.ErrorLines.Clear();
            ErrorLineBGRendererBufferC_FS.ErrorLines.Clear();
            ErrorLineBGRendererBufferD_VS.ErrorLines.Clear();
            ErrorLineBGRendererBufferD_FS.ErrorLines.Clear();
            textEditorImage_VS.TextArea.TextView.InvalidateVisual();
            textEditorImage_FS.TextArea.TextView.InvalidateVisual();
            textEditorBufferA_VS.TextArea.TextView.InvalidateVisual();
            textEditorBufferA_FS.TextArea.TextView.InvalidateVisual();
            textEditorBufferB_VS.TextArea.TextView.InvalidateVisual();
            textEditorBufferB_FS.TextArea.TextView.InvalidateVisual();
            textEditorBufferC_VS.TextArea.TextView.InvalidateVisual();
            textEditorBufferC_FS.TextArea.TextView.InvalidateVisual();
            textEditorBufferD_VS.TextArea.TextView.InvalidateVisual();
            textEditorBufferD_FS.TextArea.TextView.InvalidateVisual();

            ShaderXML shaderXML = MakeShaderXMLforSaving();

            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            if (Engine.Compile(shaderXML))
            {
                Engine.Edit(shaderXML);
                glControl.Refresh();
                return true;
            }
            return false;
        }

        private ShaderXML MakeShaderXMLforSaving()
        {
            int MultiPassBuffersCount = (int)Engine.MultiPassBuffers;
            ShaderXML_BufferA BufferA = null;
            ShaderXML_BufferB BufferB = null;
            ShaderXML_BufferC BufferC = null;
            ShaderXML_BufferD BufferD = null;

            ShaderXML_Image ShaderXMLImage = new ShaderXML_Image()
            {
                Shader_VS = textEditorImage_VS.Text,
                Shader_FS = textEditorImage_FS.Text,

                TextureFilter = new ShaderXML_TextureFilters()
                {
                    TextureMagFilter = (TextureMagFilter)comboBox_Image_ImageMagFilter?.SelectedItem,
                    TextureMinFilter = (TextureMinFilter)comboBox_Image_ImageMinFilter?.SelectedItem,
                    TextureWrapModeS = (TextureWrapMode)comboBox_Image_ImageWrapS?.SelectedItem,
                    TextureWrapModeT = (TextureWrapMode)comboBox_Image_ImageWrapT?.SelectedItem
                },
                TextureFilterBufferA = new ShaderXML_TextureFilters()
                {
                    TextureMagFilter = (TextureMagFilter)comboBox_Image_BufferAMagFilter?.SelectedItem,
                    TextureMinFilter = (TextureMinFilter)comboBox_Image_BufferAMinFilter?.SelectedItem,
                    TextureWrapModeS = (TextureWrapMode)comboBox_Image_BufferAWrapS?.SelectedItem,
                    TextureWrapModeT = (TextureWrapMode)comboBox_Image_BufferAWrapT?.SelectedItem
                },
                TextureFilterBufferB = new ShaderXML_TextureFilters()
                {
                    TextureMagFilter = (TextureMagFilter)comboBox_Image_BufferBMagFilter?.SelectedItem,
                    TextureMinFilter = (TextureMinFilter)comboBox_Image_BufferBMinFilter?.SelectedItem,
                    TextureWrapModeS = (TextureWrapMode)comboBox_Image_BufferBWrapS?.SelectedItem,
                    TextureWrapModeT = (TextureWrapMode)comboBox_Image_BufferBWrapT?.SelectedItem
                },
                TextureFilterBufferC = new ShaderXML_TextureFilters()
                {
                    TextureMagFilter = (TextureMagFilter)comboBox_Image_BufferCMagFilter?.SelectedItem,
                    TextureMinFilter = (TextureMinFilter)comboBox_Image_BufferCMinFilter?.SelectedItem,
                    TextureWrapModeS = (TextureWrapMode)comboBox_Image_BufferCWrapS?.SelectedItem,
                    TextureWrapModeT = (TextureWrapMode)comboBox_Image_BufferCWrapT?.SelectedItem
                },
                TextureFilterBufferD = new ShaderXML_TextureFilters()
                {
                    TextureMagFilter = (TextureMagFilter)comboBox_Image_BufferDMagFilter?.SelectedItem,
                    TextureMinFilter = (TextureMinFilter)comboBox_Image_BufferDMinFilter?.SelectedItem,
                    TextureWrapModeS = (TextureWrapMode)comboBox_Image_BufferDWrapS?.SelectedItem,
                    TextureWrapModeT = (TextureWrapMode)comboBox_Image_BufferDWrapT?.SelectedItem
                }
            };
            if (MultiPassBuffersCount > 0)
            {
                BufferA = new ShaderXML_BufferA()
                {
                    Shader_VS = textEditorBufferA_VS.Text,
                    Shader_FS = textEditorBufferA_FS.Text,

                    TextureFilter = new ShaderXML_TextureFilters()
                    {
                        TextureMagFilter = (TextureMagFilter)comboBox_BufferA_ImageMagFilter?.SelectedItem,
                        TextureMinFilter = (TextureMinFilter)comboBox_BufferA_ImageMinFilter?.SelectedItem,
                        TextureWrapModeS = (TextureWrapMode)comboBox_BufferA_ImageWrapS?.SelectedItem,
                        TextureWrapModeT = (TextureWrapMode)comboBox_BufferA_ImageWrapT?.SelectedItem
                    }
                };

                if (MultiPassBuffersCount > 1)
                {
                    BufferB = new ShaderXML_BufferB()
                    {
                        Shader_VS = textEditorBufferB_VS.Text,
                        Shader_FS = textEditorBufferB_FS.Text,

                        TextureFilter = new ShaderXML_TextureFilters()
                        {
                            TextureMagFilter = (TextureMagFilter)comboBox_BufferB_ImageMagFilter?.SelectedItem,
                            TextureMinFilter = (TextureMinFilter)comboBox_BufferB_ImageMinFilter?.SelectedItem,
                            TextureWrapModeS = (TextureWrapMode)comboBox_BufferB_ImageWrapS?.SelectedItem,
                            TextureWrapModeT = (TextureWrapMode)comboBox_BufferB_ImageWrapT?.SelectedItem
                        },
                        TextureFilterBufferA = new ShaderXML_TextureFilters()
                        {
                            TextureMagFilter = (TextureMagFilter)comboBox_BufferB_BufferAMagFilter?.SelectedItem,
                            TextureMinFilter = (TextureMinFilter)comboBox_BufferB_BufferAMinFilter?.SelectedItem,
                            TextureWrapModeS = (TextureWrapMode)comboBox_BufferB_BufferAWrapS?.SelectedItem,
                            TextureWrapModeT = (TextureWrapMode)comboBox_BufferB_BufferAWrapT?.SelectedItem
                        }
                    };

                    if (MultiPassBuffersCount > 2)
                    {
                        BufferC = new ShaderXML_BufferC()
                        {
                            Shader_VS = textEditorBufferC_VS.Text,
                            Shader_FS = textEditorBufferC_FS.Text,

                            TextureFilter = new ShaderXML_TextureFilters()
                            {
                                TextureMagFilter = (TextureMagFilter)comboBox_BufferC_ImageMagFilter?.SelectedItem,
                                TextureMinFilter = (TextureMinFilter)comboBox_BufferC_ImageMinFilter?.SelectedItem,
                                TextureWrapModeS = (TextureWrapMode)comboBox_BufferC_ImageWrapS?.SelectedItem,
                                TextureWrapModeT = (TextureWrapMode)comboBox_BufferC_ImageWrapT?.SelectedItem
                            },
                            TextureFilterBufferA = new ShaderXML_TextureFilters()
                            {
                                TextureMagFilter = (TextureMagFilter)comboBox_BufferC_BufferAMagFilter?.SelectedItem,
                                TextureMinFilter = (TextureMinFilter)comboBox_BufferC_BufferAMinFilter?.SelectedItem,
                                TextureWrapModeS = (TextureWrapMode)comboBox_BufferC_BufferAWrapS?.SelectedItem,
                                TextureWrapModeT = (TextureWrapMode)comboBox_BufferC_BufferAWrapT?.SelectedItem
                            },
                            TextureFilterBufferB = new ShaderXML_TextureFilters()
                            {
                                TextureMagFilter = (TextureMagFilter)comboBox_BufferC_BufferBMagFilter?.SelectedItem,
                                TextureMinFilter = (TextureMinFilter)comboBox_BufferC_BufferBMinFilter?.SelectedItem,
                                TextureWrapModeS = (TextureWrapMode)comboBox_BufferC_BufferBWrapS?.SelectedItem,
                                TextureWrapModeT = (TextureWrapMode)comboBox_BufferC_BufferBWrapT?.SelectedItem
                            }
                        };

                        if (MultiPassBuffersCount > 3)
                        {
                            BufferD = new ShaderXML_BufferD()
                            {
                                Shader_VS = textEditorBufferD_VS.Text,
                                Shader_FS = textEditorBufferD_FS.Text,

                                TextureFilter = new ShaderXML_TextureFilters()
                                {
                                    TextureMagFilter = (TextureMagFilter)comboBox_BufferD_ImageMagFilter?.SelectedItem,
                                    TextureMinFilter = (TextureMinFilter)comboBox_BufferD_ImageMinFilter?.SelectedItem,
                                    TextureWrapModeS = (TextureWrapMode)comboBox_BufferD_ImageWrapS?.SelectedItem,
                                    TextureWrapModeT = (TextureWrapMode)comboBox_BufferD_ImageWrapT?.SelectedItem
                                },
                                TextureFilterBufferA = new ShaderXML_TextureFilters()
                                {
                                    TextureMagFilter = (TextureMagFilter)comboBox_BufferD_BufferAMagFilter?.SelectedItem,
                                    TextureMinFilter = (TextureMinFilter)comboBox_BufferD_BufferAMinFilter?.SelectedItem,
                                    TextureWrapModeS = (TextureWrapMode)comboBox_BufferD_BufferAWrapS?.SelectedItem,
                                    TextureWrapModeT = (TextureWrapMode)comboBox_BufferD_BufferAWrapT?.SelectedItem
                                },
                                TextureFilterBufferB = new ShaderXML_TextureFilters()
                                {
                                    TextureMagFilter = (TextureMagFilter)comboBox_BufferD_BufferBMagFilter?.SelectedItem,
                                    TextureMinFilter = (TextureMinFilter)comboBox_BufferD_BufferBMinFilter?.SelectedItem,
                                    TextureWrapModeS = (TextureWrapMode)comboBox_BufferD_BufferBWrapS?.SelectedItem,
                                    TextureWrapModeT = (TextureWrapMode)comboBox_BufferD_BufferBWrapT?.SelectedItem
                                },
                                TextureFilterBufferC = new ShaderXML_TextureFilters()
                                {
                                    TextureMagFilter = (TextureMagFilter)comboBox_BufferD_BufferCMagFilter?.SelectedItem,
                                    TextureMinFilter = (TextureMinFilter)comboBox_BufferD_BufferCMinFilter?.SelectedItem,
                                    TextureWrapModeS = (TextureWrapMode)comboBox_BufferD_BufferCWrapS?.SelectedItem,
                                    TextureWrapModeT = (TextureWrapMode)comboBox_BufferD_BufferCWrapT?.SelectedItem
                                }
                            };
                        }
                    }
                }
            }

            return new ShaderXML(Engine.MultiPassBuffers, ShaderXMLImage, BufferA, BufferB, BufferC, BufferD);
        }

        private bool ApplyLoadedShaderXML(ShaderXML shaderXML)
        {
            if (shaderXML == null)
            {
                return false;
            }

            tabControlMain.SelectedIndex = 4; // Image;
            Engine.MultiPassBufferDrawMode = MultiPassBuffersDrawMode.NoBuffers;

            textEditorImage_VS.Text = shaderXML.Image?.Shader_VS;
            textEditorImage_FS.Text = shaderXML.Image?.Shader_FS;
            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_Image_VS);
            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_Image_FS);

            SetDefaultTextureParameters();
            TemporaryDisableComboBoxesEvents = true;

            comboBox_Image_Buffers.SelectedItem = shaderXML.MultiPassBuffers;

            comboBox_Image_ImageMagFilter.SelectedItem = shaderXML.Image?.TextureFilter.TextureMagFilter;
            comboBox_Image_ImageMinFilter.SelectedItem = shaderXML.Image?.TextureFilter.TextureMinFilter;
            comboBox_Image_ImageWrapS.SelectedItem = shaderXML.Image?.TextureFilter.TextureWrapModeS;
            comboBox_Image_ImageWrapT.SelectedItem = shaderXML.Image?.TextureFilter.TextureWrapModeT;

            comboBox_Image_BufferAMagFilter.SelectedItem = shaderXML.Image?.TextureFilterBufferA.TextureMagFilter;
            comboBox_Image_BufferAMinFilter.SelectedItem = shaderXML.Image?.TextureFilterBufferA.TextureMinFilter;
            comboBox_Image_BufferAWrapS.SelectedItem = shaderXML.Image?.TextureFilterBufferA.TextureWrapModeS;
            comboBox_Image_BufferAWrapT.SelectedItem = shaderXML.Image?.TextureFilterBufferA.TextureWrapModeT;

            comboBox_Image_BufferBMagFilter.SelectedItem = shaderXML.Image?.TextureFilterBufferB.TextureMagFilter;
            comboBox_Image_BufferBMinFilter.SelectedItem = shaderXML.Image?.TextureFilterBufferB.TextureMinFilter;
            comboBox_Image_BufferBWrapS.SelectedItem = shaderXML.Image?.TextureFilterBufferB.TextureWrapModeS;
            comboBox_Image_BufferBWrapT.SelectedItem = shaderXML.Image?.TextureFilterBufferB.TextureWrapModeT;

            comboBox_Image_BufferCMagFilter.SelectedItem = shaderXML.Image?.TextureFilterBufferC.TextureMagFilter;
            comboBox_Image_BufferCMinFilter.SelectedItem = shaderXML.Image?.TextureFilterBufferC.TextureMinFilter;
            comboBox_Image_BufferCWrapS.SelectedItem = shaderXML.Image?.TextureFilterBufferC.TextureWrapModeS;
            comboBox_Image_BufferCWrapT.SelectedItem = shaderXML.Image?.TextureFilterBufferC.TextureWrapModeT;

            comboBox_Image_BufferDMagFilter.SelectedItem = shaderXML.Image?.TextureFilterBufferD.TextureMagFilter;
            comboBox_Image_BufferDMinFilter.SelectedItem = shaderXML.Image?.TextureFilterBufferD.TextureMinFilter;
            comboBox_Image_BufferDWrapS.SelectedItem = shaderXML.Image?.TextureFilterBufferD.TextureWrapModeS;
            comboBox_Image_BufferDWrapT.SelectedItem = shaderXML.Image?.TextureFilterBufferD.TextureWrapModeT;

            int MultiPassBuffersCount = (int)shaderXML.MultiPassBuffers;
            if (MultiPassBuffersCount > 0)
            {
                textEditorBufferA_VS.Text = shaderXML.BufferA?.Shader_VS;
                textEditorBufferA_FS.Text = shaderXML.BufferA?.Shader_FS;
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferA_VS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferA_FS);

                comboBox_BufferA_ImageMagFilter.SelectedItem = shaderXML.BufferA?.TextureFilter.TextureMagFilter;
                comboBox_BufferA_ImageMinFilter.SelectedItem = shaderXML.BufferA?.TextureFilter.TextureMinFilter;
                comboBox_BufferA_ImageWrapS.SelectedItem = shaderXML.BufferA?.TextureFilter.TextureWrapModeS;
                comboBox_BufferA_ImageWrapT.SelectedItem = shaderXML.BufferA?.TextureFilter.TextureWrapModeT;

                if (MultiPassBuffersCount > 1)
                {
                    textEditorBufferB_VS.Text = shaderXML.BufferB?.Shader_VS;
                    textEditorBufferB_FS.Text = shaderXML.BufferB?.Shader_FS;
                    FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferB_VS);
                    FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferB_FS);

                    comboBox_BufferB_ImageMagFilter.SelectedItem = shaderXML.BufferB?.TextureFilter.TextureMagFilter;
                    comboBox_BufferB_ImageMinFilter.SelectedItem = shaderXML.BufferB?.TextureFilter.TextureMinFilter;
                    comboBox_BufferB_ImageWrapS.SelectedItem = shaderXML.BufferB?.TextureFilter.TextureWrapModeS;
                    comboBox_BufferB_ImageWrapT.SelectedItem = shaderXML.BufferB?.TextureFilter.TextureWrapModeT;

                    comboBox_BufferB_BufferAMagFilter.SelectedItem = shaderXML.BufferB?.TextureFilterBufferA.TextureMagFilter;
                    comboBox_BufferB_BufferAMinFilter.SelectedItem = shaderXML.BufferB?.TextureFilterBufferA.TextureMinFilter;
                    comboBox_BufferB_BufferAWrapS.SelectedItem = shaderXML.BufferB?.TextureFilterBufferA.TextureWrapModeS;
                    comboBox_BufferB_BufferAWrapT.SelectedItem = shaderXML.BufferB?.TextureFilterBufferA.TextureWrapModeT;

                    if (MultiPassBuffersCount > 2)
                    {
                        textEditorBufferC_VS.Text = shaderXML.BufferC?.Shader_VS;
                        textEditorBufferC_FS.Text = shaderXML.BufferC?.Shader_FS;
                        FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferC_VS);
                        FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferC_FS);

                        comboBox_BufferC_ImageMagFilter.SelectedItem = shaderXML.BufferC?.TextureFilter.TextureMagFilter;
                        comboBox_BufferC_ImageMinFilter.SelectedItem = shaderXML.BufferC?.TextureFilter.TextureMinFilter;
                        comboBox_BufferC_ImageWrapS.SelectedItem = shaderXML.BufferC?.TextureFilter.TextureWrapModeS;
                        comboBox_BufferC_ImageWrapT.SelectedItem = shaderXML.BufferC?.TextureFilter.TextureWrapModeT;

                        comboBox_BufferC_BufferAMagFilter.SelectedItem = shaderXML.BufferC?.TextureFilterBufferA.TextureMagFilter;
                        comboBox_BufferC_BufferAMinFilter.SelectedItem = shaderXML.BufferC?.TextureFilterBufferA.TextureMinFilter;
                        comboBox_BufferC_BufferAWrapS.SelectedItem = shaderXML.BufferC?.TextureFilterBufferA.TextureWrapModeS;
                        comboBox_BufferC_BufferAWrapT.SelectedItem = shaderXML.BufferC?.TextureFilterBufferA.TextureWrapModeT;

                        comboBox_BufferC_BufferBMagFilter.SelectedItem = shaderXML.BufferC?.TextureFilterBufferB.TextureMagFilter;
                        comboBox_BufferC_BufferBMinFilter.SelectedItem = shaderXML.BufferC?.TextureFilterBufferB.TextureMinFilter;
                        comboBox_BufferC_BufferBWrapS.SelectedItem = shaderXML.BufferC?.TextureFilterBufferB.TextureWrapModeS;
                        comboBox_BufferC_BufferBWrapT.SelectedItem = shaderXML.BufferC?.TextureFilterBufferB.TextureWrapModeT;

                        if (MultiPassBuffersCount > 3)
                        {
                            textEditorBufferD_VS.Text = shaderXML.BufferD?.Shader_VS;
                            textEditorBufferD_FS.Text = shaderXML.BufferD?.Shader_FS;
                            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferD_VS);
                            FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferD_FS);

                            comboBox_BufferD_ImageMagFilter.SelectedItem = shaderXML.BufferD?.TextureFilter.TextureMagFilter;
                            comboBox_BufferD_ImageMinFilter.SelectedItem = shaderXML.BufferD?.TextureFilter.TextureMinFilter;
                            comboBox_BufferD_ImageWrapS.SelectedItem = shaderXML.BufferD?.TextureFilter.TextureWrapModeS;
                            comboBox_BufferD_ImageWrapT.SelectedItem = shaderXML.BufferD?.TextureFilter.TextureWrapModeT;

                            comboBox_BufferD_BufferAMagFilter.SelectedItem = shaderXML.BufferD?.TextureFilterBufferA.TextureMagFilter;
                            comboBox_BufferD_BufferAMinFilter.SelectedItem = shaderXML.BufferD?.TextureFilterBufferA.TextureMinFilter;
                            comboBox_BufferD_BufferAWrapS.SelectedItem = shaderXML.BufferD?.TextureFilterBufferA.TextureWrapModeS;
                            comboBox_BufferD_BufferAWrapT.SelectedItem = shaderXML.BufferD?.TextureFilterBufferA.TextureWrapModeT;

                            comboBox_BufferD_BufferBMagFilter.SelectedItem = shaderXML.BufferD?.TextureFilterBufferB.TextureMagFilter;
                            comboBox_BufferD_BufferBMinFilter.SelectedItem = shaderXML.BufferD?.TextureFilterBufferB.TextureMinFilter;
                            comboBox_BufferD_BufferBWrapS.SelectedItem = shaderXML.BufferD?.TextureFilterBufferB.TextureWrapModeS;
                            comboBox_BufferD_BufferBWrapT.SelectedItem = shaderXML.BufferD?.TextureFilterBufferB.TextureWrapModeT;

                            comboBox_BufferD_BufferCMagFilter.SelectedItem = shaderXML.BufferD?.TextureFilterBufferC.TextureMagFilter;
                            comboBox_BufferD_BufferCMinFilter.SelectedItem = shaderXML.BufferD?.TextureFilterBufferC.TextureMinFilter;
                            comboBox_BufferD_BufferCWrapS.SelectedItem = shaderXML.BufferD?.TextureFilterBufferC.TextureWrapModeS;
                            comboBox_BufferD_BufferCWrapT.SelectedItem = shaderXML.BufferD?.TextureFilterBufferC.TextureWrapModeT;
                        }
                    }
                }
            }

            Engine.MultiPassBuffers = shaderXML.MultiPassBuffers;
            TemporaryDisableComboBoxesEvents = false;

            return true;
        }

        #region Panel Buttons
        private void Button_New_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Create new shader?", "New shader", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (glControl.IsHandleCreated)
                    glControl.MakeCurrent();

                tabControlMain.SelectedIndex = 4; // Image;
                Engine.MultiPassBuffers = MultiPassBuffers.NoBuffers;
                Engine.MultiPassBufferDrawMode = MultiPassBuffersDrawMode.NoBuffers;

                TemporaryDisableComboBoxesEvents = true;
                SetDefaultTextureParameters();
                TemporaryDisableComboBoxesEvents = false;

                textEditorImage_VS.Text = textEditorBufferA_VS.Text = textEditorBufferB_VS.Text = textEditorBufferC_VS.Text = textEditorBufferD_VS.Text = Properties.Resources.Edit_VS;
                textEditorImage_FS.Text = Properties.Resources.EditImage_FS;
                textEditorBufferA_FS.Text = Properties.Resources.EditBufferA_FS;
                textEditorBufferB_FS.Text = Properties.Resources.EditBufferB_FS;
                textEditorBufferC_FS.Text = Properties.Resources.EditBufferC_FS;
                textEditorBufferD_FS.Text = Properties.Resources.EditBufferD_FS;

                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_Image_VS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_Image_FS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferA_VS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferA_FS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferB_VS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferB_FS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferC_VS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferC_FS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferD_VS);
                FoldingsHelper.TryCollapsePhotoshopUniformsFoldings(FoldingManager_BufferD_FS);

                openFileDialog.FileName = saveFileDialog.FileName = String.Empty;
                CompileShader();
            }
        }

        private void Button_Open_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            string DirectoryName = openFileDialog.InitialDirectory;
            string FileName = String.Empty;

            try
            {
                DirectoryName = Path.GetDirectoryName(openFileDialog.FileName);
                FileName = Path.GetFileName(openFileDialog.FileName);

                if (Directory.Exists(DirectoryName))
                {
                    openFileDialog.InitialDirectory = DirectoryName;
                    openFileDialog.FileName = FileName;
                }
            }
            catch { }

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                saveFileDialog.InitialDirectory = DirectoryName;
                saveFileDialog.FileName = openFileDialog.FileName;

                if (!LoadShaderData(openFileDialog.FileName))
                {
                    MessageBox.Show("Maybe \"" + Path.GetFileName(FileName) + "\" is not shader file!",
                        "File opening error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Button_Save_Click(object sender, ExecutedRoutedEventArgs e)
        {
            string DirectoryName = saveFileDialog.InitialDirectory;
            string FileName = String.Empty;

            try
            {
                DirectoryName = Path.GetDirectoryName(saveFileDialog.FileName);
                FileName = Path.GetFileName(saveFileDialog.FileName);

                if (Directory.Exists(DirectoryName))
                {
                    saveFileDialog.InitialDirectory = DirectoryName;
                    saveFileDialog.FileName = FileName;
                }
            }
            catch { }

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                openFileDialog.InitialDirectory = DirectoryName;
                openFileDialog.FileName = saveFileDialog.FileName;

                switch (saveFileDialog.FilterIndex)
                {
                    default:
                    case 1:
                        if (!ShaderXML.Save(MakeShaderXMLforSaving(), saveFileDialog.FileName))
                        {
                            MessageBox.Show("Error while saving \"" + Path.GetFileName(FileName) + "\" shader file!",
                                "File saving error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;

                    case 2:
                        File.WriteAllText(saveFileDialog.FileName, textEditorImage_FS.Text);
                        break;

                    case 3:
                        File.WriteAllText(saveFileDialog.FileName, textEditorImage_VS.Text);
                        break;

                    case 4:
                        string ShaderStr = String.Empty;
                        switch (tabControlMain.SelectedIndex)
                        {
                            default:
                                ShaderStr = (tabControlImage.SelectedIndex == 0 ? textEditorImage_VS : textEditorImage_FS).Text;
                                break;
                            case 0:
                                ShaderStr = (tabControlBufferA.SelectedIndex == 0 ? textEditorBufferA_VS : textEditorBufferA_FS).Text;
                                break;
                            case 1:
                                ShaderStr = (tabControlBufferB.SelectedIndex == 0 ? textEditorBufferB_VS : textEditorBufferB_FS).Text;
                                break;
                            case 2:
                                ShaderStr = (tabControlBufferC.SelectedIndex == 0 ? textEditorBufferC_VS : textEditorBufferC_FS).Text;
                                break;
                            case 3:
                                ShaderStr = (tabControlBufferD.SelectedIndex == 0 ? textEditorBufferD_VS : textEditorBufferD_FS).Text;
                                break;
                        }
                        File.WriteAllText(saveFileDialog.FileName, ShaderStr);
                        break;
                }
            }
        }

        private void Button_Copy_Click(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetImage(Image);
        }

        private void Button_Paste_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image = Clipboard.GetImage();
                CompileShader();
            }
        }

        private void Button_About_Click(object sender, ExecutedRoutedEventArgs e)
        {
            Version PluginVersion = Assembly.GetExecutingAssembly().GetName().Version;
            
            MessageBox.Show(String.Format("Shader Plugin (for Photoshop) v.{0}.{1}\n\n", PluginVersion.Major, PluginVersion.Minor) +
                "© 2021 Shader Plugin.\n" +
                "All rights reserved.\n\n" +
                "Authors:\n" +
                "Alexandr Zelensky\n" +
                "Vladimir Rymkevich",
                "About...", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Controls
        private void BtnCompile_Click(object sender, RoutedEventArgs e)
        {
            CompileShader();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            if (CompileShader())
            {
                Engine.ApplyToPhotoshop();

                if (!PhotoshopRunLastFilterEnabled && SaveLastShaders())
                    PhotoshopRunLastFilterEnabled = true;

                Program.Result = PSPluginErrorCodes.NoError;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();
            Close();
        }

        private void StatusBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LastErrorStr.Trim() != String.Empty)
            {
                ErrorWindow shaderErrorWindow = new ErrorWindow(LastErrorStr)
                {
                    Owner = this
                };
                shaderErrorWindow.ShowDialog();
            }
        }

        #region Tabs: Image, Buffers
        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.MultiPassBufferDrawMode = (tabControlMain.SelectedIndex < 4 ? MultiPassBuffersDrawMode.BufferA + tabControlMain.SelectedIndex : MultiPassBuffersDrawMode.NoBuffers);

            if (TemporaryDisableComboBoxesEvents)
                return;

            glControl.Refresh();
        }

        private void ComboBox_Image_Buffers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.MultiPassBuffers = (MultiPassBuffers)comboBox_Image_Buffers.SelectedItem;
            int MultiPassBuffersCount = (int)Engine.MultiPassBuffers;
            groupBox_Image_BufferA.Visibility = tabItemBufferA.Visibility = (MultiPassBuffersCount > 0 ? Visibility.Visible : Visibility.Collapsed);
            groupBox_Image_BufferB.Visibility = tabItemBufferB.Visibility = (MultiPassBuffersCount > 1 ? Visibility.Visible : Visibility.Collapsed);
            groupBox_Image_BufferC.Visibility = tabItemBufferC.Visibility = (MultiPassBuffersCount > 2 ? Visibility.Visible : Visibility.Collapsed);
            groupBox_Image_BufferD.Visibility = tabItemBufferD.Visibility = (MultiPassBuffersCount > 3 ? Visibility.Visible : Visibility.Collapsed);
        }

        private void ComboBox_TextureParameters_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (TemporaryDisableComboBoxesEvents)
                return;

            CompileShader();
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (sender == textEditorImage_VS) FoldingStrategy_Image_VS.UpdateFoldings(FoldingManager_Image_VS, textEditorImage_VS.Document);
            else if (sender == textEditorImage_FS) FoldingStrategy_Image_FS.UpdateFoldings(FoldingManager_Image_FS, textEditorImage_FS.Document);
            else if (sender == textEditorBufferA_VS) FoldingStrategy_BufferA_VS.UpdateFoldings(FoldingManager_BufferA_VS, textEditorBufferA_VS.Document);
            else if (sender == textEditorBufferA_FS) FoldingStrategy_BufferA_FS.UpdateFoldings(FoldingManager_BufferA_FS, textEditorBufferA_FS.Document);
            else if (sender == textEditorBufferB_VS) FoldingStrategy_BufferB_VS.UpdateFoldings(FoldingManager_BufferB_VS, textEditorBufferB_VS.Document);
            else if (sender == textEditorBufferB_FS) FoldingStrategy_BufferB_FS.UpdateFoldings(FoldingManager_BufferB_FS, textEditorBufferB_FS.Document);
            else if (sender == textEditorBufferC_VS) FoldingStrategy_BufferC_VS.UpdateFoldings(FoldingManager_BufferC_VS, textEditorBufferC_VS.Document);
            else if (sender == textEditorBufferC_FS) FoldingStrategy_BufferC_FS.UpdateFoldings(FoldingManager_BufferC_FS, textEditorBufferC_FS.Document);
            else if (sender == textEditorBufferD_VS) FoldingStrategy_BufferD_VS.UpdateFoldings(FoldingManager_BufferD_VS, textEditorBufferD_VS.Document);
            else if (sender == textEditorBufferD_FS) FoldingStrategy_BufferD_FS.UpdateFoldings(FoldingManager_BufferD_FS, textEditorBufferD_FS.Document);
        }


        #endregion

        #region Settings
        private void ComboBox_ViewMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.DrawMode = (DrawModes)comboBox_ViewMode?.SelectedItem;
            glControl.Invalidate();
        }

        private void ComboBox_MagFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.MagFilter = (TextureMagFilter)comboBox_MagFilter?.SelectedItem;
            glControl.Invalidate();
        }

        private void ComboBox_MinFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.MinFilter = (TextureMinFilter)comboBox_MinFilter?.SelectedItem;

            if (!Engine.UseMipMaps)
            {
                if (Engine.MinFilter != TextureMinFilter.Nearest && Engine.MinFilter != TextureMinFilter.Linear)
                    checkBox_MipMaps.IsChecked = true;
            }

            glControl.Invalidate();
        }

        private void CheckBox_sRGB_StateChanged(object sender, RoutedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.UseFramebufferSrgb = checkBox_sRGB.IsChecked.Value;
            glControl.Invalidate();
        }

        private void CheckBox_MipMaps_StateChanged(object sender, RoutedEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.UseMipMaps = checkBox_MipMaps.IsChecked.Value;

            if (!Engine.UseMipMaps)
            {
                if (Engine.MinFilter == TextureMinFilter.NearestMipmapNearest || Engine.MinFilter == TextureMinFilter.NearestMipmapLinear)
                    comboBox_MinFilter.SelectedItem = TextureMinFilter.Nearest;
                else if (Engine.MinFilter == TextureMinFilter.LinearMipmapNearest || Engine.MinFilter == TextureMinFilter.LinearMipmapLinear)
                    comboBox_MinFilter.SelectedItem = TextureMinFilter.Linear;
            }

            glControl.Invalidate();
        }

        private void Rectangle_PreviewLineColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            if (Engine.RunColorPicker(Engine.PreviewLineColor, out ColorRGBA ResultColor))
            {
                rectangle_PreviewLineColor.Fill = new SolidColorBrush(ResultColor.WithoutAlpha);
                Engine.PreviewLineColor = ResultColor;
                glControl.Invalidate();
            }
        }

        private void Rectangle_BGColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            if (Engine.RunColorPicker(Engine.GLClearColor, out ColorRGBA ResultColor))
            {
                rectangle_BGColor.Fill = new SolidColorBrush(ResultColor.WithoutAlpha);
                Engine.GLClearColor = ResultColor;
                glControl.Invalidate();
            }
        }

        private void Rectangle_GridColor1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            if (Engine.RunColorPicker(Engine.GridColor1, out ColorRGBA ResultColor))
            {
                rectangle_GridColor1.Fill = new SolidColorBrush(ResultColor.WithoutAlpha);
                Engine.GridColor1 = ResultColor;
                glControl.Invalidate();
            }
        }

        private void Rectangle_GridColor2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            if (Engine.RunColorPicker(Engine.GridColor2, out ColorRGBA ResultColor))
            {
                rectangle_GridColor2.Fill = new SolidColorBrush(ResultColor.WithoutAlpha);
                Engine.GridColor2 = ResultColor;
                glControl.Invalidate();
            }
        }

        private void TbGridSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(tbGridSize.Text, out float GridSize))
                Engine.GridSize = GridSize;

            glControl.Invalidate();
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsSave();
        }

        private void BtnDefaultSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings = new SettingsXML();
            SettingsApply();
        }

        private void TbGridSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsCheckerRegex.IsMatch(e.Text);
            if ((sender as TextBox).Text.Length >= 2)
                e.Handled = true;
        }

        // Use the DataObject.Pasting Handler
        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!DigitsCheckerRegex.IsMatch(text))
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }
        #endregion

        #endregion
    }
}