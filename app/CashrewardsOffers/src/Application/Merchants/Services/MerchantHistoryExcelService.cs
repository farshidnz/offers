using CashrewardsOffers.Application.Merchants.Models;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;

namespace CashrewardsOffers.Application.Merchants.Services
{
    public interface IMerchantHistoryExcelService
    {
        Stream GetExcelStream(List<MerchantHistoryInfo> changesForYesterday, Stream stream);
    }

    public class MerchantHistoryExcelService : IMerchantHistoryExcelService
    {
        public Stream GetExcelStream(List<MerchantHistoryInfo> changesForYesterday, Stream stream)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Merchant History");
            ConfigureWidths(worksheet);

            int row = 1;
            AddHeader(row++, worksheet);

            foreach (var change in changesForYesterday)
            {
                AddRow(row++, worksheet, change);
            }

            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        private static void AddHeader(int row, IXLWorksheet worksheet)
        {
            worksheet.Cell(row, (int)Columns.ChangeInSydneyTime).Value = "ChangeInSydneyTime";
            worksheet.Cell(row, (int)Columns.Client).Value = "ClientId";
            worksheet.Cell(row, (int)Columns.MerchantId).Value = "MerchantId";
            worksheet.Cell(row, (int)Columns.Name).Value = "Merchant Name";
            worksheet.Cell(row, (int)Columns.HyphenatedString).Value = "Merchant Hyphenated String";
            worksheet.Cell(row, (int)Columns.ClientCommissionString).Value = "New Commission String";
        }

        private static void ConfigureWidths(IXLWorksheet worksheet)
        {
            worksheet.Column((int)Columns.ChangeInSydneyTime).Width = 20;
            worksheet.Column((int)Columns.Client).Width = 10;
            worksheet.Column((int)Columns.MerchantId).Width = 10;
            worksheet.Column((int)Columns.Name).Width = 30;
            worksheet.Column((int)Columns.HyphenatedString).Width = 25;
            worksheet.Column((int)Columns.ClientCommissionString).Width = 22;
        }

        private static void AddRow(int row, IXLWorksheet worksheet, MerchantHistoryInfo info)
        {
            worksheet.Cell(row, (int)Columns.ChangeInSydneyTime).DataType = XLDataType.DateTime;
            worksheet.Cell(row, (int)Columns.Client).DataType = XLDataType.Number;
            worksheet.Cell(row, (int)Columns.MerchantId).DataType = XLDataType.Number;
            worksheet.Cell(row, (int)Columns.Name).DataType = XLDataType.Text;
            worksheet.Cell(row, (int)Columns.HyphenatedString).DataType = XLDataType.Text;
            worksheet.Cell(row, (int)Columns.ClientCommissionString).DataType = XLDataType.Text;

            worksheet.Cell(row, (int)Columns.ChangeInSydneyTime).Value = info.ChangeInSydneyTime.DateTime;
            worksheet.Cell(row, (int)Columns.Client).Value = info.Client;
            worksheet.Cell(row, (int)Columns.MerchantId).Value = info.MerchantId;
            worksheet.Cell(row, (int)Columns.Name).Value = info.Name;
            worksheet.Cell(row, (int)Columns.HyphenatedString).Value = info.HyphenatedString;
            worksheet.Cell(row, (int)Columns.ClientCommissionString).Value = info.ClientCommissionString;
        }

        private enum Columns
        {
            ChangeInSydneyTime = 1,
            Client,
            MerchantId,
            Name,
            HyphenatedString,
            ClientCommissionString
        }
    }
}
