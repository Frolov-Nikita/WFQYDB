using System.Drawing;
using System.Windows.Forms;

namespace WF_svr
{
    public static class RichTextBoxExtentions
    {
        public static void AppendText(this RichTextBox rtb, string text, Color color, Font font = null)
        {
            int bindex = rtb.Text.Length;
            rtb.AppendText(text);
            rtb.Select(bindex, text.Length);
            rtb.SelectionColor = color;
            if (font != null) rtb.SelectionFont = font;
            rtb.DeselectAll();
        }
    }
}
