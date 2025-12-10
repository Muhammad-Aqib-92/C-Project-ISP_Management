using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Semester_Project.Models;
using System;
using System.IO;

namespace Semester_Project.Services
{
    public class InvoiceService
    {
        public InvoiceService()
        {
            // License key for QuestPDF (Community license)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateInvoice(ISP_user user, PaymentHistory payment)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text($"Invoice #{payment.InvoiceNumber}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Date: {payment.PaymentDate:yyyy-MM-dd}");
                            x.Item().Text($"Customer: {user.Name}");
                            x.Item().Text($"Address: {user.Address ?? "N/A"}");
                            x.Item().Text($"Package: {user.InternetPackage?.PackageName ?? "N/A"}");
                            x.Item().Text($"Speed: {user.InternetPackage?.Speed ?? 0} Mbps");

                            x.Item().LineHorizontal(1);

                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Description");
                                row.ConstantItem(100).AlignRight().Text("Amount");
                            });

                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Internet Service - {payment.PaymentDate:MMMM yyyy}");
                                row.ConstantItem(100).AlignRight().Text($"{payment.Amount:C}");
                            });

                            x.Item().LineHorizontal(1);

                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Total").Bold();
                                row.ConstantItem(100).AlignRight().Text($"{payment.Amount:C}").Bold();
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf();
        }
    }
}
