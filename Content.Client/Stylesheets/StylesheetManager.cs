using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;

namespace Content.Client.Stylesheets
{
    public sealed partial class StylesheetManager : IStylesheetManager
    {
        [Dependency] private IUserInterfaceManager _userInterfaceManager = default!;
        [Dependency] private IResourceCache _resourceCache = default!;
        [Dependency] private IConfigurationManager _configurationManager = default!;

        public Stylesheet SheetNano { get; private set; } = default!;
        public Stylesheet SheetSpace { get; private set; } = default!;

        /// <inheritdoc />
        public event Action? ChatFontChanged;

        public void Initialize()
        {
            StyleNano.SetCrtUiEnabled(_configurationManager.GetCVar(CCVars.CrtUiEnabled));
            StyleNano.SetCrtPalette(_configurationManager.GetCVar(CCVars.CrtUiColor));
            StyleNano.SetChatHousing(_configurationManager.GetCVar(CCVars.CMUChatHousing));
            StyleNano.SetChatReadableFont(_configurationManager.GetCVar(CCVars.CMUChatReadableFont));
            StyleNano.SetChatFontStep(
                StyleNano.ParseChatFontStep(_configurationManager.GetCVar(CCVars.CMUChatBigFont)));
            RefreshNanoSheet();
            SheetSpace = new StyleSpace(_resourceCache).Stylesheet;

            _configurationManager.OnValueChanged(CCVars.CrtUiEnabled, OnCrtUiEnabledChanged);
            _configurationManager.OnValueChanged(CCVars.CrtUiColor, OnCrtUiColorChanged);
            _configurationManager.OnValueChanged(CCVars.CMUChatReadableFont, OnChatReadableFontChanged);
            _configurationManager.OnValueChanged(CCVars.CMUChatBigFont, OnChatBigFontChanged);
            _configurationManager.OnValueChanged(CCVars.CMUChatHousing, OnChatHousingChanged);
        }

        public void PreviewCrtUi(bool enabled, string color)
        {
            StyleNano.SetCrtUiEnabled(enabled);
            StyleNano.SetCrtPalette(color);
            RefreshNanoSheet();
        }

        public void ResetCrtUiPreview()
        {
            StyleNano.SetCrtUiEnabled(_configurationManager.GetCVar(CCVars.CrtUiEnabled));
            StyleNano.SetCrtPalette(_configurationManager.GetCVar(CCVars.CrtUiColor));
            RefreshNanoSheet();
        }

        private void OnCrtUiEnabledChanged(bool enabled)
        {
            StyleNano.SetCrtUiEnabled(enabled);
            RefreshNanoSheet();
        }

        private void OnChatHousingChanged(string tone)
        {
            StyleNano.SetChatHousing(tone);
            RefreshNanoSheet();
            RefreshOpenUi();
        }

        private void OnCrtUiColorChanged(string color)
        {
            StyleNano.SetCrtPalette(color);
            RefreshNanoSheet();
        }

        /// <summary>
        ///     Rebuilding the sheet is only half of it - message rows and the channel prompt bake a
        ///     FontOverride at construction and will not pick a new one up. ChatBox listens to the
        ///     same cvar and rebuilds itself; see ChatBox.OnChatReadableFontChanged. Everything
        ///     outside chat is caught by the full refresh below.
        /// </summary>
        private void OnChatReadableFontChanged(bool enabled)
        {
            StyleNano.SetChatReadableFont(enabled);
            ApplyChatFontChange();
        }

        private void OnChatBigFontChanged(string setting)
        {
            StyleNano.SetChatFontStep(StyleNano.ParseChatFontStep(setting));
            ApplyChatFontChange();
        }

        /// <summary>
        ///     The shared tail of both chat font options.
        /// </summary>
        private void ApplyChatFontChange()
        {
            RefreshNanoSheet();
            RefreshOpenUi();
            ChatFontChanged?.Invoke();
        }

        /// <summary>
        ///     Make every control already on screen re-read the stylesheet.
        /// </summary>
        private void RefreshOpenUi()
        {
            foreach (var root in _userInterfaceManager.AllRoots)
            {
                root.InvalidateStyleSheet();
                root.ForceRunStyleUpdate();
            }
        }

        private void RefreshNanoSheet()
        {
            SheetNano = new StyleNano(_resourceCache).Stylesheet;
            _userInterfaceManager.Stylesheet = SheetNano;
        }
    }
}
