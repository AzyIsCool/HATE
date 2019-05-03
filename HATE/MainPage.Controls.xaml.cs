using Avalonia.Controls;

namespace HATE
{
    internal partial class MainPage
    {
        private Button btnCorrupt;
        private Button btnLaunch;
        private CheckBox chbGarbleText;
        private CheckBox chbHitboxFix;
        private CheckBox chbShowSeed;
        private CheckBox chbShuffleAudio;
        private CheckBox chbShuffleFonts;
        private CheckBox chbShuffleGFX;
        private CheckBox chbShuffleSprites;
        private CheckBox chbShuffleText;
        private TextBlock labGameName;
        private TextBlock labGarbleText;
        private TextBlock labHitboxFix;
        private TextBlock labPower;
        private TextBlock labSeed;
        private TextBlock labShowSeed;
        private TextBlock labShuffleAudio;
        private TextBlock labShuffleFonts;
        private TextBlock labShuffleGFX;
        private TextBlock labShuffleSprites;
        private TextBlock labShuffleText;
        private TextBox txtPower;
        private TextBox txtSeed;

        void SetUpControls()
        {
            btnCorrupt = this.FindControl<Button>("btnCorrupt");
            btnLaunch = this.FindControl<Button>("btnLaunch");
            chbShuffleText = this.FindControl<CheckBox>("chbShuffleText");
            chbShuffleGFX = this.FindControl<CheckBox>("chbShuffleGFX");
            chbHitboxFix = this.FindControl<CheckBox>("chbHitboxFix");
            chbShuffleFonts = this.FindControl<CheckBox>("chbShuffleFonts");
            chbShuffleSprites = this.FindControl<CheckBox>("chbShuffleSprites");
            chbShuffleAudio = this.FindControl<CheckBox>("chbShuffleAudio");
            chbShowSeed = this.FindControl<CheckBox>("chbShowSeed");
            chbGarbleText = this.FindControl<CheckBox>("chbGarbleText");
            labGameName = this.FindControl<TextBlock>("labGameName");
            labShuffleAudio = this.FindControl<TextBlock>("labShuffleAudio");
            labShuffleGFX = this.FindControl<TextBlock>("labShuffleGFX");
            labShuffleFonts = this.FindControl<TextBlock>("labShuffleFonts");
            labHitboxFix = this.FindControl<TextBlock>("labHitboxFix");
            labShuffleSprites = this.FindControl<TextBlock>("labShuffleSprites");
            labShuffleText = this.FindControl<TextBlock>("labShuffleText");
            labGarbleText = this.FindControl<TextBlock>("labGarbleText");
            labShowSeed = this.FindControl<TextBlock>("labShowSeed");
            labPower = this.FindControl<TextBlock>("labPower");
            labSeed = this.FindControl<TextBlock>("labSeed");
            txtPower = this.FindControl<TextBox>("txtPower");
            txtSeed = this.FindControl<TextBox>("txtSeed");
        }
    }
}