using DoAn2VADT.Database.Entities;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;
using Aspose.Cells;
using Microsoft.Office.Interop.Excel;

namespace DoAn2VADT.Helpper
{
    public static class ExportToExcelHelper
    {
        public static Stream UpdateDataIntoExcelTemplate(List<OrderDetail> orderDetails,Order order, FileInfo path)
        {
            var stream = new MemoryStream();
            if (path.Exists)
            {
                using (ExcelPackage package = new ExcelPackage(path))
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets["Bill"];

                    sheet.Cells["E2"].Value = order.Code;
                    sheet.Cells["E3"].Value = order.PayStatus == DoAn2VADT.Shared.PayStatusConst.DONE ? "Đã thanh toán" : "Chưa thanh toán";

                    sheet.Cells["C4"].Value = order.Name;
                    sheet.Cells["C5"].Value = order.Address;
                    sheet.Cells["C6"].Value = order.Phone;
                    sheet.Cells["C7"].Value = order.CreatedAt?.ToString("dd/MM/yyyy hh:mm");
                    sheet.Cells["C8"].Value = order.PayWay == DoAn2VADT.Shared.PayConst.OFFLINE ? "Thanh toán khi nhận hàng" : "MoMo";
                    sheet.Cells["C9"].Value = order.ReceiveDate?.ToString("dd/MM/yyyy hh:mm");
                    int rowIndex = 12;
                    int? sumQuantity = 0;
                    decimal? sumPrice = 0;
                    decimal? sumTotal = 0;
                    foreach (var item in orderDetails)
                    {
                        sheet.Cells[rowIndex, 2].Value = item.Product.Name;
                        sheet.Cells[rowIndex, 3].Value = item.Quantity;
                        sheet.Cells[rowIndex, 4].Value = (item.Product.Price - item.Product.Discount)?.ToString("n0");
                        sheet.Cells[rowIndex, 5].Value = item.Total?.ToString("n0");
                        sumQuantity += item.Quantity;
                        sumPrice += item.Product.Price - item.Product.Discount;
                        sumTotal += item.Total;
                        rowIndex += 1;
                    }

                    sheet.Cells["C27"].Value = sumQuantity;
                    sheet.Cells["D27"].Value = sumPrice?.ToString("n0");
                    sheet.Cells["E27"].Value = sumTotal?.ToString("n0");

                    sheet.Cells["E29"].Value = order.ShipFee?.ToString("n0") + " VND";
                    sheet.Cells["E30"].Value = order.Discount?.ToString("n0") + " VND";
                    sheet.Cells["E31"].Value = order.Total?.ToString("n0") + " VND";
                 
                    sheet.Cells["C33"].Value = "Ngày " + DateTime.Now.Day + " tháng " + DateTime.Now.Month + " năm " + DateTime.Now.Year;
                    package.SaveAs(stream);
                    stream.Position = 0;
                }
            }
            return stream;
        }
    }
}
