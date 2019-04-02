using System.Windows.Forms;

namespace Blacksmith
{
    public class Message
    {
        public static DialogResult Success(string text) => Properties.Settings.Default.hidePopups == 0 || Properties.Settings.Default.hidePopups == 2 ? DialogResult.None : MessageBox.Show(text, "Success");

        public static DialogResult Fail(string text) => Properties.Settings.Default.hidePopups == 1 || Properties.Settings.Default.hidePopups == 2 ? DialogResult.None : MessageBox.Show(text, "Failure");

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons) => MessageBox.Show(text, caption, buttons);
    }
}