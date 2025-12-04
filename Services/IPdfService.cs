using COMP2138_ICE.Models;

namespace COMP2138_ICE.Services
{
    public interface IPdfService
    {
        byte[] GenerateTicketPdf(Ticket ticket, Event eventDetails, ApplicationUser user);
    }
}
