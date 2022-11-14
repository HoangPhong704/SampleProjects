using System.Drawing;

namespace XMC_Flasher.Models
{
    public class LabelItemModel
    {
        public string? RDMAddress { get; set; }
        public string? RDMID { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? Wattage { get; set; }
        public Image Barcode { get; set; }
    }
}
