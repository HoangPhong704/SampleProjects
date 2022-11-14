using BarcodeLib;
using System.Drawing;
using XMC_Flasher.Models;

namespace XMC_Flasher.Managers
{
    public class BarCodeManager
    {
        const int BARCODE_WIDTH = 120;
        const int BARCODE_HEIGHT = 10;
        public BarCodeManager()
        {

        }
        /// <summary>
        /// Generate barcode image from parameters
        /// </summary>
        /// <param name="firmwareVersion"></param>
        /// <param name="rdmAddress"></param>
        /// <param name="wattage"></param>
        /// <returns></returns>
        public LabelItemModel GenerateBarCode(string firmwareVersion, string rdmAddress, string wattage)
        { 

            var rdmID = int.Parse(rdmAddress.Split(":")[1], System.Globalization.NumberStyles.HexNumber); 
            Color foreColor = Color.Black;
            Color backColor = Color.Transparent;
            var barcodeGenerator = new Barcode();
            var barcode = barcodeGenerator.Encode(TYPE.CODE128A, $"{rdmID}", foreColor, backColor, BARCODE_WIDTH, BARCODE_HEIGHT);
            //barcode.Save(@"D:\bardcode_test39.png", ImageFormat.Png);
            var labelItem = new LabelItemModel()
            {
                Barcode = barcode,
                FirmwareVersion = firmwareVersion,
                RDMAddress = rdmAddress,
                RDMID = $"{rdmID}", 
                Wattage = wattage
            };
            return labelItem;
        }
    }
}
