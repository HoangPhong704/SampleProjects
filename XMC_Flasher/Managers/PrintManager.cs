using XMC_Flasher.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XMC_Flasher.Managers
{
    public class PrintManager
    {
        const int PAGE_WIDTH = 200;
        const int PAGE_HEIGHT = 38;

        public string PrinterName { get; set; }
        public LabelItemModel LabelItem { get; set; }
        public PrintManager(string printerName, LabelItemModel labelItem)
        {
            PrinterName = printerName;
            LabelItem = labelItem;
        }
        public void Print()
        {
            
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = PrinterName;
            pd.DefaultPageSettings.Margins = new Margins(0,0,0,0);
            pd.DefaultPageSettings.PaperSize = new PaperSize("Custom", PAGE_WIDTH, PAGE_HEIGHT);
            pd.PrintPage += Pd_PrintPage;

            //PrintDialog printdlg = new PrintDialog();
            //PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();

            //// preview the assigned document or you can create a different previewButton for it
            //printPrvDlg.Document = pd;
            //printPrvDlg.ShowDialog(); // this shows the preview and then show the Printer Dlg below

            pd.Print();

        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null)
                return;
            Graphics g = e.Graphics;
            var foreColor = new SolidBrush(Color.Black);
            var mainFont = new Font("Arial", 9);
            var secondaryFont = new Font("Arial",6); 
            g.DrawString(LabelItem.FirmwareVersion, mainFont, foreColor, new PointF(3,3));
            g.DrawString(LabelItem.RDMAddress, mainFont, foreColor, new PointF(100, 3));
            g.DrawImage(LabelItem.Barcode, new Point(83,16));
            g.DrawString(LabelItem.RDMID, secondaryFont, foreColor, new PointF(135, 27));            
        }
    }
}
