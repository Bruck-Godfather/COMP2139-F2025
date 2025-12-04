using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using COMP2138_ICE.Models;

namespace COMP2138_ICE.Services
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
        }

        public byte[] GenerateTicketPdf(Ticket ticket, Event eventDetails, ApplicationUser user)
        {
            // Convert Base64 QR Code to bytes
            var base64Data = ticket.QRCodeData;
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Split(',')[1];
            }
            
            byte[] qrCodeBytes;
            try 
            {
                qrCodeBytes = Convert.FromBase64String(base64Data);
            }
            catch
            {
                qrCodeBytes = Array.Empty<byte>();
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Custom ticket size
                    page.Size(new PageSize(800, 300));
                    page.Margin(0);

                    page.Content().Row(row =>
                    {
                        // Left Section (Main Ticket) - Dark Theme
                        row.RelativeItem(2.5f).Background("#3B4252").Padding(25).Column(col =>
                        {
                            // Ticket ID at top left corner
                            col.Item().AlignLeft().Text($"ID: {ticket.TicketNumber}")
                                .FontSize(9).FontColor("#E5E7EB");

                            // Spacer
                            col.Item().PaddingTop(15);

                            // Event Title - Centered
                            col.Item().AlignCenter().Text(eventDetails.Title.ToUpper())
                                .FontSize(24).Bold().FontColor("#F472B6"); // Pink
                            
                            // Decorative Stars - Centered
                            col.Item().PaddingVertical(5).AlignCenter().Text("* * * * *")
                                .FontColor("#FBBF24").FontSize(12); // Gold

                            // Subtitle - Centered
                            col.Item().AlignCenter().Text("MOVIE NIGHT")
                                .FontSize(16).FontColor("#FDE68A"); // Light yellow

                            // Spacer
                            col.Item().PaddingTop(20);

                            // Details Section
                            col.Item().Row(details =>
                            {
                                // Date Column
                                details.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("DATE").FontSize(9).FontColor("#9CA3AF");
                                    c.Item().Text($"{eventDetails.DateTime:MMM dd}").FontSize(16).Bold().FontColor(Colors.White);
                                    c.Item().Text($"{eventDetails.DateTime:HH:mm}").FontSize(14).Bold().FontColor("#F472B6");
                                });

                                // Price Column
                                details.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("PRICE").FontSize(9).FontColor("#9CA3AF");
                                    c.Item().Text($"{eventDetails.Price:C}").FontSize(16).Bold().FontColor(Colors.White);
                                });
                                
                                // Attendee Column
                                details.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("ATTENDEE").FontSize(9).FontColor("#9CA3AF");
                                    c.Item().Text(user.FullName ?? user.UserName ?? "Guest")
                                        .FontSize(12).Bold().FontColor(Colors.White);
                                });
                            });

                            // Organizer Section
                            col.Item().PaddingTop(12).Row(orgRow =>
                            {
                                orgRow.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("ORGANIZER").FontSize(9).FontColor("#9CA3AF");
                                    c.Item().Text(eventDetails.Organizer?.FullName ?? eventDetails.Organizer?.UserName ?? "Event Organizer")
                                        .FontSize(11).FontColor("#D1D5DB");
                                });
                            });
                        });

                        // Perforation Line (Visual)
                        row.ConstantItem(3).Background("#000000"); // Dark divider

                        // Right Section (Stub with QR) - Light Theme
                        row.RelativeItem(1).Background("#FDE68A").Padding(20).Column(col =>
                        {
                            // Ticket ID at top
                            col.Item().AlignLeft().Text($"ID: {ticket.TicketNumber}")
                                .FontSize(9).FontColor("#1F2937");
                            
                            // QR Code - Vertically centered
                            if (qrCodeBytes.Length > 0)
                            {
                                col.Item().PaddingVertical(20).AlignCenter().AlignMiddle().Width(110).Height(110).Image(qrCodeBytes);
                            }
                            
                            // Ticket ID below QR code - Centered
                            col.Item().AlignCenter().Text($"ID: {ticket.TicketNumber}")
                                .FontSize(9).FontColor("#1F2937");

                            // Spacer
                            col.Item().PaddingTop(10);

                            // ADMIT ONE at bottom
                            col.Item().AlignCenter().Text("ADMIT ONE")
                                .FontSize(20).Black().Bold().FontColor("#1F2937");
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
